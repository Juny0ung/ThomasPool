using Moq;
using ThomasPool.Application.Dtos;
using ThomasPool.Application.Services;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool_api.Tests.Services;

public class ProfileServiceTests
{
    private readonly Mock<IProfileFormRepository> _formRepo = new();
    private readonly Mock<IProfileRepository> _profileRepo = new();
    private readonly Mock<IPhotoRepository> _photoRepo = new();
    private readonly ProfileService _sut;

    private static readonly byte[] SamplePhoto = [0xFF, 0xD8, 0xFF];

    public ProfileServiceTests()
    {
        _sut = new ProfileService(_formRepo.Object, _profileRepo.Object, _photoRepo.Object);
    }

    private static ProfileForm ValidForm(int version = 1) => new ProfileForm
    {
        Version = version,
        Questions = [new QuestionForm { Question = "자기소개", Type = QuestionType.ShortAnswer }]
    };

    private static Profile ValidProfile(int version = 1) => new Profile(
        name: "홍길동",
        gender: true,
        phoneNumber: "01012345678",
        birthYear: 1990,
        region: "서울",
        info: [new SingleContent { Value = "안녕하세요" }],
        version: version
    );

    #region UpdateProfileFormAsync

    [Fact]
    public async Task UpdateProfileFormAsync_ValidForm_ReturnsSuccess()
    {
        var form = ValidForm();
        _formRepo.Setup(x => x.UpdateFormAsync(form)).Returns(Task.CompletedTask);

        var result = await _sut.UpdateProfileFormAsync(form);

        Assert.True(result.IsSuccess);
        _formRepo.Verify(x => x.UpdateFormAsync(form), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileFormAsync_EmptyQuestions_ReturnsInvalid()
    {
        var form = new ProfileForm { Questions = [] };

        var result = await _sut.UpdateProfileFormAsync(form);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ProfileForm.Invalid, result.Error);
        _formRepo.Verify(x => x.UpdateFormAsync(It.IsAny<ProfileForm>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileFormAsync_MultipleChoiceWithoutOptions_ReturnsInvalid()
    {
        var form = new ProfileForm
        {
            Questions = [new QuestionForm { Question = "질문", Type = QuestionType.MultipleChoice }]
        };

        var result = await _sut.UpdateProfileFormAsync(form);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ProfileForm.Invalid, result.Error);
    }

    [Fact]
    public async Task UpdateProfileFormAsync_VersionConflict_ReturnsConflict()
    {
        var form = ValidForm();
        _formRepo.Setup(x => x.UpdateFormAsync(form)).ThrowsAsync(new InvalidOperationException());

        var result = await _sut.UpdateProfileFormAsync(form);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ProfileForm.Conflict, result.Error);
    }

    [Fact]
    public async Task UpdateProfileFormAsync_UnexpectedException_ReturnsUnknownError()
    {
        var form = ValidForm();
        _formRepo.Setup(x => x.UpdateFormAsync(form)).ThrowsAsync(new Exception("db error"));

        var result = await _sut.UpdateProfileFormAsync(form);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.General.Unknown, result.Error);
    }

    #endregion

    #region GetProfileFormAsync

    [Fact]
    public async Task GetProfileFormAsync_FormExists_ReturnsForm()
    {
        var form = ValidForm(version: 3);
        _formRepo.Setup(x => x.GetFormAsync(3)).ReturnsAsync(form);

        var result = await _sut.GetProfileFormAsync(3);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Version);
    }

    [Fact]
    public async Task GetProfileFormAsync_NoVersion_ReturnsLatest()
    {
        var form = ValidForm(version: 5);
        _formRepo.Setup(x => x.GetFormAsync(null)).ReturnsAsync(form);

        var result = await _sut.GetProfileFormAsync(null);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Version);
    }

    [Fact]
    public async Task GetProfileFormAsync_FormNotFound_ReturnsEmptyForm()
    {
        _formRepo.Setup(x => x.GetFormAsync(It.IsAny<int?>())).ReturnsAsync((ProfileForm?)null);

        var result = await _sut.GetProfileFormAsync(999);

        Assert.True(result.IsSuccess);
        Assert.Equal(-1, result.Value.Version);
        Assert.Empty(result.Value.Questions);
    }

    [Fact]
    public async Task GetProfileFormAsync_UnexpectedException_ReturnsUnknownError()
    {
        _formRepo.Setup(x => x.GetFormAsync(It.IsAny<int?>())).ThrowsAsync(new Exception("db error"));

        var result = await _sut.GetProfileFormAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.General.Unknown, result.Error);
    }

    #endregion

    #region UpdateProfileAsync

    [Fact]
    public async Task UpdateProfileAsync_NewProfile_SavesAndReturnsSuccess()
    {
        var profile = ValidProfile(version: 1);
        var form = ValidForm(version: 1);
        _formRepo.Setup(x => x.GetFormAsync(1)).ReturnsAsync(form);
        _profileRepo.Setup(x => x.GetInfoAsync(profile)).ReturnsAsync((Profile?)null);
        _photoRepo.Setup(x => x.SavePhotoAsync(It.IsAny<string>(), SamplePhoto)).Returns(Task.CompletedTask);
        _profileRepo.Setup(x => x.SaveInfoAsync(profile)).Returns(Task.CompletedTask);

        var result = await _sut.UpdateProfileAsync(profile, SamplePhoto);

        Assert.True(result.IsSuccess);
        _profileRepo.Verify(x => x.SaveInfoAsync(profile), Times.Once);
        _profileRepo.Verify(x => x.UpdateInfoAsync(It.IsAny<Profile>()), Times.Never);
        _photoRepo.Verify(x => x.SavePhotoAsync(It.IsAny<string>(), SamplePhoto), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ExistingProfile_ReusesPhotoIdAndUpdates()
    {
        var profile = ValidProfile(version: 1);
        var existing = ValidProfile(version: 1);
        existing.PhotoId = "existing-photo-id";
        var form = ValidForm(version: 1);
        _formRepo.Setup(x => x.GetFormAsync(1)).ReturnsAsync(form);
        _profileRepo.Setup(x => x.GetInfoAsync(profile)).ReturnsAsync(existing);
        _photoRepo.Setup(x => x.SavePhotoAsync("existing-photo-id", SamplePhoto)).Returns(Task.CompletedTask);
        _profileRepo.Setup(x => x.UpdateInfoAsync(profile)).Returns(Task.CompletedTask);

        var result = await _sut.UpdateProfileAsync(profile, SamplePhoto);

        Assert.True(result.IsSuccess);
        Assert.Equal("existing-photo-id", profile.PhotoId);
        _profileRepo.Verify(x => x.UpdateInfoAsync(profile), Times.Once);
        _profileRepo.Verify(x => x.SaveInfoAsync(It.IsAny<Profile>()), Times.Never);
        _photoRepo.Verify(x => x.SavePhotoAsync("existing-photo-id", SamplePhoto), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_InvalidPhoneNumber_ReturnsInvalid()
    {
        var profile = new Profile("홍길동", true, "0101234", 1990, "서울", [new SingleContent { Value = "hello" }], 1);

        var result = await _sut.UpdateProfileAsync(profile, SamplePhoto);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Profile.Invalid, result.Error);
        _formRepo.Verify(x => x.GetFormAsync(It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_FormNotFound_ReturnsFormNotFound()
    {
        var profile = ValidProfile(version: 99);
        _formRepo.Setup(x => x.GetFormAsync(99)).ReturnsAsync((ProfileForm?)null);

        var result = await _sut.UpdateProfileAsync(profile, SamplePhoto);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ProfileForm.NotFound, result.Error);
    }

    [Fact]
    public async Task UpdateProfileAsync_MismatchedInfoCount_ReturnsInvalid()
    {
        var form = new ProfileForm
        {
            Version = 1,
            Questions =
            [
                new QuestionForm { Question = "q1", Type = QuestionType.ShortAnswer },
                new QuestionForm { Question = "q2", Type = QuestionType.Essay }
            ]
        };
        // profile.Info has only 1 entry but form has 2 questions
        var profile = ValidProfile(version: 1);
        _formRepo.Setup(x => x.GetFormAsync(1)).ReturnsAsync(form);

        var result = await _sut.UpdateProfileAsync(profile, SamplePhoto);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Profile.Invalid, result.Error);
    }

    [Fact]
    public async Task UpdateProfileAsync_WrongContentType_ReturnsInvalid()
    {
        var form = new ProfileForm
        {
            Version = 1,
            Questions = [new QuestionForm { Question = "True or false?", Type = QuestionType.TrueFalse }]
        };
        // TrueFalse expects BoolContent, but profile sends SingleContent
        var profile = new Profile("홍길동", true, "01012345678", 1990, "서울",
            [new SingleContent { Value = "answer" }], version: 1);
        _formRepo.Setup(x => x.GetFormAsync(1)).ReturnsAsync(form);

        var result = await _sut.UpdateProfileAsync(profile, SamplePhoto);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Profile.Invalid, result.Error);
    }

    [Fact]
    public async Task UpdateProfileAsync_Conflict_ReturnsConflict()
    {
        var profile = ValidProfile(version: 1);
        var form = ValidForm(version: 1);
        _formRepo.Setup(x => x.GetFormAsync(1)).ReturnsAsync(form);
        _profileRepo.Setup(x => x.GetInfoAsync(profile)).ReturnsAsync((Profile?)null);
        _photoRepo.Setup(x => x.SavePhotoAsync(It.IsAny<string>(), SamplePhoto)).Returns(Task.CompletedTask);
        _profileRepo.Setup(x => x.SaveInfoAsync(profile)).ThrowsAsync(new InvalidOperationException());

        var result = await _sut.UpdateProfileAsync(profile, SamplePhoto);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Profile.Conflict, result.Error);
    }

    [Fact]
    public async Task UpdateProfileAsync_UnexpectedException_ReturnsUnknownError()
    {
        var profile = ValidProfile(version: 1);
        _formRepo.Setup(x => x.GetFormAsync(1)).ThrowsAsync(new Exception("db error"));

        var result = await _sut.UpdateProfileAsync(profile, SamplePhoto);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.General.Unknown, result.Error);
    }

    #endregion

    #region GetPhotoAsync

    [Fact]
    public async Task GetPhotoAsync_PhotoExists_ReturnsPhoto()
    {
        _photoRepo.Setup(x => x.GetPhotoAsync("photo-1")).ReturnsAsync(SamplePhoto);

        var result = await _sut.GetPhotoAsync("photo-1");

        Assert.True(result.IsSuccess);
        Assert.Equal(SamplePhoto, result.Value);
    }

    [Fact]
    public async Task GetPhotoAsync_PhotoNotFound_ReturnsNotFound()
    {
        _photoRepo.Setup(x => x.GetPhotoAsync("missing")).ReturnsAsync((byte[]?)null);

        var result = await _sut.GetPhotoAsync("missing");

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Photo.NotFound, result.Error);
    }

    [Fact]
    public async Task GetPhotoAsync_UnexpectedException_ReturnsUnknownError()
    {
        _photoRepo.Setup(x => x.GetPhotoAsync(It.IsAny<string>())).ThrowsAsync(new Exception("db error"));

        var result = await _sut.GetPhotoAsync("photo-1");

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.General.Unknown, result.Error);
    }

    #endregion

    #region GetProfilesAsync

    [Fact]
    public async Task GetProfilesAsync_ReturnsProfiles()
    {
        var profiles = new[] { ValidProfile(), ValidProfile() };
        _profileRepo.Setup(x => x.GetInfosAsync("홍길동", 0, 10)).ReturnsAsync(profiles);

        var result = await _sut.GetProfilesAsync("홍길동", 0, 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Length);
    }

    [Fact]
    public async Task GetProfilesAsync_EmptyResult_ReturnsEmptyArray()
    {
        _profileRepo.Setup(x => x.GetInfosAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync([]);

        var result = await _sut.GetProfilesAsync("없는사람", 0, 10);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetProfilesAsync_UnexpectedException_ReturnsUnknownError()
    {
        _profileRepo.Setup(x => x.GetInfosAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(new Exception("db error"));

        var result = await _sut.GetProfilesAsync("홍길동", 0, 10);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.General.Unknown, result.Error);
    }

    #endregion
}
