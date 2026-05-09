using Moq;
using ThomasPool.Application.Dtos;
using ThomasPool.Application.Services;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool_api.Tests.Services;

public class AdminServiceTests
{
    private readonly Mock<IAdminRepository> _adminRepo = new();
    private readonly Mock<IJwtProvider> _jwtProvider = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly AdminService _sut;

    public AdminServiceTests()
    {
        _sut = new AdminService(_adminRepo.Object, _jwtProvider.Object, _passwordHasher.Object);
    }

    #region RegisterAsync

    [Fact]
    public async Task RegisterAsync_Success_ReturnsSuccess()
    {
        _passwordHasher.Setup(x => x.Hash("pw")).Returns("hashed");
        _adminRepo.Setup(x => x.AddUserAsync("홍길동", "admin1", "hashed")).Returns(Task.CompletedTask);

        var result = await _sut.RegisterAsync("홍길동", "admin1", "pw");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateId_ReturnsConflict()
    {
        _passwordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed");
        _adminRepo.Setup(x => x.AddUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                  .ThrowsAsync(new InvalidOperationException());

        var result = await _sut.RegisterAsync("홍길동", "admin1", "pw");

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Admin.Conflict, result.Error);
    }

    [Fact]
    public async Task RegisterAsync_UnexpectedException_ReturnsUnknownError()
    {
        _passwordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed");
        _adminRepo.Setup(x => x.AddUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                  .ThrowsAsync(new Exception("db error"));

        var result = await _sut.RegisterAsync("홍길동", "admin1", "pw");

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.General.Unknown, result.Error);
    }

    #endregion

    #region LoginAsync

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var admin = new Admin { Name = "홍길동", AdminId = "admin1", Password = "hashed", Role = Role.Admin };
        _adminRepo.Setup(x => x.FindUserAsync("admin1")).ReturnsAsync(admin);
        _passwordHasher.Setup(x => x.Verify("pw", "hashed")).Returns(true);
        _jwtProvider.Setup(x => x.GenerateToken(admin)).Returns("jwt-token");

        var result = await _sut.LoginAsync("admin1", "pw");

        Assert.True(result.IsSuccess);
        Assert.Equal("jwt-token", result.Value);
    }

    [Fact]
    public async Task LoginAsync_AdminNotFound_ReturnsNotFound()
    {
        _adminRepo.Setup(x => x.FindUserAsync("admin1")).ReturnsAsync((Admin?)null);

        var result = await _sut.LoginAsync("admin1", "pw");

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Admin.NotFound, result.Error);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsInvalid()
    {
        var admin = new Admin { Name = "홍길동", AdminId = "admin1", Password = "hashed", Role = Role.Admin };
        _adminRepo.Setup(x => x.FindUserAsync("admin1")).ReturnsAsync(admin);
        _passwordHasher.Setup(x => x.Verify("wrong", "hashed")).Returns(false);

        var result = await _sut.LoginAsync("admin1", "wrong");

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Admin.Invalid, result.Error);
    }

    [Fact]
    public async Task LoginAsync_PendingAdmin_ReturnsNotApproved()
    {
        var admin = new Admin { Name = "홍길동", AdminId = "admin1", Password = "hashed", Role = Role.Pending };
        _adminRepo.Setup(x => x.FindUserAsync("admin1")).ReturnsAsync(admin);
        _passwordHasher.Setup(x => x.Verify("pw", "hashed")).Returns(true);

        var result = await _sut.LoginAsync("admin1", "pw");

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Admin.NotApproved, result.Error);
    }

    [Fact]
    public async Task LoginAsync_UserRoleAdmin_ReturnsNotApproved()
    {
        var admin = new Admin { Name = "홍길동", AdminId = "admin1", Password = "hashed", Role = Role.User };
        _adminRepo.Setup(x => x.FindUserAsync("admin1")).ReturnsAsync(admin);
        _passwordHasher.Setup(x => x.Verify("pw", "hashed")).Returns(true);

        var result = await _sut.LoginAsync("admin1", "pw");

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Admin.NotApproved, result.Error);
    }

    [Fact]
    public async Task LoginAsync_UnexpectedException_ReturnsUnknownError()
    {
        _adminRepo.Setup(x => x.FindUserAsync(It.IsAny<string>())).ThrowsAsync(new Exception("db error"));

        var result = await _sut.LoginAsync("admin1", "pw");

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.General.Unknown, result.Error);
    }

    #endregion

    #region PendingListAsync

    [Fact]
    public async Task PendingListAsync_ReturnsMappedAdminInfos()
    {
        var admins = new[]
        {
            new Admin { Name = "홍길동", AdminId = "admin1", Password = "h1", Role = Role.Pending },
            new Admin { Name = "이순신", AdminId = "admin2", Password = "h2", Role = Role.Pending },
        };
        _adminRepo.Setup(x => x.FindUsersAsync(0, 10, null, null, Role.Pending)).ReturnsAsync(admins);

        var result = await _sut.PendingListAsync(0, 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Length);
        Assert.Equal("admin1", result.Value[0].AdminId);
        Assert.Equal(Role.Pending, result.Value[1].Role);
    }

    [Fact]
    public async Task PendingListAsync_EmptyList_ReturnsEmptyArray()
    {
        _adminRepo.Setup(x => x.FindUsersAsync(0, 10, null, null, Role.Pending)).ReturnsAsync([]);

        var result = await _sut.PendingListAsync(0, 10);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task PendingListAsync_UnexpectedException_ReturnsUnknownError()
    {
        _adminRepo.Setup(x => x.FindUsersAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, Role.Pending))
                  .ThrowsAsync(new Exception("db error"));

        var result = await _sut.PendingListAsync(0, 10);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.General.Unknown, result.Error);
    }

    #endregion

    #region ApproveAsync

    [Fact]
    public async Task ApproveAsync_Success_ReturnsSuccess()
    {
        var ids = new[] { "admin1", "admin2" };
        _adminRepo.Setup(x => x.ApproveUsersAsync(ids)).Returns(Task.CompletedTask);

        var result = await _sut.ApproveAsync(ids);

        Assert.True(result.IsSuccess);
        _adminRepo.Verify(x => x.ApproveUsersAsync(ids), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_UnexpectedException_ReturnsUnknownError()
    {
        _adminRepo.Setup(x => x.ApproveUsersAsync(It.IsAny<string[]>())).ThrowsAsync(new Exception("db error"));

        var result = await _sut.ApproveAsync(["admin1"]);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.General.Unknown, result.Error);
    }

    #endregion
}
