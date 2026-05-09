using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Services;

namespace ThomasPool_api.Tests.Domain;

public class ProfileValidatorTests
{
    private static ProfileForm FormWith(params (string question, QuestionType type)[] questions)
    {
        var forms = questions.Select(q =>
        {
            if (q.type is QuestionType.MultipleChoice or QuestionType.MultipleSelection)
                return (QuestionForm)new MultipleForm { Question = q.question, Type = q.type, Options = ["A", "B"] };
            return new QuestionForm { Question = q.question, Type = q.type };
        }).ToArray();

        return new ProfileForm { Version = 1, Questions = forms };
    }

    private static Profile ProfileWith(int version, params ProfileContent[] contents) =>
        new("홍길동", true, "01012345678", 1990, "서울", contents, version);

    [Fact]
    public void ValidateProfile_AllShortAnswer_ReturnsTrue()
    {
        var form = FormWith(("q1", QuestionType.ShortAnswer), ("q2", QuestionType.Essay));
        var profile = ProfileWith(1,
            new SingleContent { Value = "a1" },
            new SingleContent { Value = "a2" });

        Assert.True(form.ValidateProfile(profile));
    }

    [Fact]
    public void ValidateProfile_TrueFalseWithBoolContent_ReturnsTrue()
    {
        var form = FormWith(("yes or no?", QuestionType.TrueFalse));
        var profile = ProfileWith(1, new BoolContent { Value = true });

        Assert.True(form.ValidateProfile(profile));
    }

    [Fact]
    public void ValidateProfile_MultipleSelectionWithMultipleContent_ReturnsTrue()
    {
        var form = FormWith(("choose", QuestionType.MultipleSelection));
        var profile = ProfileWith(1, new MultipleContent { Values = ["A", "B"] });

        Assert.True(form.ValidateProfile(profile));
    }

    [Fact]
    public void ValidateProfile_MultipleChoiceWithSingleContent_ReturnsTrue()
    {
        var form = FormWith(("pick one", QuestionType.MultipleChoice));
        var profile = ProfileWith(1, new SingleContent { Value = "A" });

        Assert.True(form.ValidateProfile(profile));
    }

    [Fact]
    public void ValidateProfile_VersionMismatch_ReturnsFalse()
    {
        var form = FormWith(("q1", QuestionType.ShortAnswer));
        var profile = ProfileWith(version: 99, new SingleContent { Value = "a" });

        Assert.False(form.ValidateProfile(profile));
    }

    [Fact]
    public void ValidateProfile_CountMismatch_ReturnsFalse()
    {
        var form = FormWith(("q1", QuestionType.ShortAnswer), ("q2", QuestionType.Essay));
        var profile = ProfileWith(1, new SingleContent { Value = "only one" });

        Assert.False(form.ValidateProfile(profile));
    }

    [Fact]
    public void ValidateProfile_TrueFalseWithWrongType_ReturnsFalse()
    {
        var form = FormWith(("yes?", QuestionType.TrueFalse));
        var profile = ProfileWith(1, new SingleContent { Value = "yes" });

        Assert.False(form.ValidateProfile(profile));
    }

    [Fact]
    public void ValidateProfile_MultipleSelectionWithSingleContent_ReturnsFalse()
    {
        var form = FormWith(("choose", QuestionType.MultipleSelection));
        var profile = ProfileWith(1, new SingleContent { Value = "A" });

        Assert.False(form.ValidateProfile(profile));
    }

    [Fact]
    public void ValidateProfile_ShortAnswerWithBoolContent_ReturnsFalse()
    {
        var form = FormWith(("intro", QuestionType.ShortAnswer));
        var profile = ProfileWith(1, new BoolContent { Value = false });

        Assert.False(form.ValidateProfile(profile));
    }

    [Fact]
    public void ValidateProfile_NullFormVersion_IgnoresVersionCheck()
    {
        var form = new ProfileForm
        {
            Version = null,
            Questions = [new QuestionForm { Question = "q", Type = QuestionType.ShortAnswer }]
        };
        var profile = ProfileWith(version: 42, new SingleContent { Value = "a" });

        Assert.True(form.ValidateProfile(profile));
    }
}
