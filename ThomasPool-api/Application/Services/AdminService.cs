using ThomasPool.Application.Dtos;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool.Application.Services;

public class AdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;
    public AdminService(IAdminRepository adminRepository, IJwtProvider jwtProvider, IPasswordHasher passwordHasher)
    {
        _adminRepository = adminRepository;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> RegisterAsync(string name, string id, string password)
    {
        try
        {
            await _adminRepository.AddUserAsync(name, id, _passwordHasher.Hash(password));
            return Result.Success();
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(Errors.Admin.Conflict);
        }
        catch
        {
            return Result.Failure(Errors.General.Unknown);
        }
    }

    public async Task<Result<string>> LoginAsync(string id, string password)
    {
        try
        {
            var admin = await _adminRepository.FindUserAsync(id);
            if (admin == null) return Result<string>.Failure(Errors.Admin.NotFound);

            if (!_passwordHasher.Verify(password, admin.Password)) return Result<string>.Failure(Errors.Admin.Invalid);

            if (admin.Role != Role.Admin) return Result<string>.Failure(Errors.Admin.NotApproved);

            return Result<string>.Success(_jwtProvider.GenerateToken(admin));
        }
        catch
        {
            return Result<string>.Failure(Errors.General.Unknown);
        }
    }

    public async Task<Result<AdminInfo[]>> PendingListAsync(int skip, int limit)
    {
        try
        {
            var admins = await _adminRepository.FindUsersAsync(skip, limit, role: Role.Pending);
            return Result<AdminInfo[]>.Success([.. admins.Select(a => new AdminInfo { Name = a.Name, AdminId = a.AdminId, Role = a.Role })]);
        }
        catch
        {
            return Result<AdminInfo[]>.Failure(Errors.General.Unknown);
        }
    }

    public async Task<Result> ApproveAsync(string[] ids)
    {
        try
        {
            await _adminRepository.ApproveUsersAsync(ids);
            return Result.Success();
        }
        catch
        {
            return Result.Failure(Errors.General.Unknown);
        }
    }
}