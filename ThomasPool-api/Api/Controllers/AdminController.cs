
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ThomasPool.Api.Dtos;
using ThomasPool.Application.Dtos;
using ThomasPool.Application.Services;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ApiControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        Result result = await _adminService.RegisterAsync(registerDto.Name, registerDto.AdminId, registerDto.Password);
        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        Result<string> result = await _adminService.LoginAsync(loginDto.AdminId, loginDto.Password);
        return result.IsSuccess ? Ok(new { Token = result.Value} ) : HandleFailure(result.Error);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("pendinglist")]
    public async Task<IActionResult> PendingList(int skip, int limit)
    {
        Result<AdminInfo[]> result = await _adminService.PendingListAsync(skip, limit);
        return result.IsSuccess ? Ok(result.Value.Select(a => AdminDto.FromDomain(a))) : HandleFailure(result.Error);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("approve")]
    public async Task<IActionResult> Approve([FromBody] string[] ids)
    {
        Result result = await _adminService.ApproveAsync(ids);
        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }
}