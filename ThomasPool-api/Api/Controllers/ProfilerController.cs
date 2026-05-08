using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThomasPool.Api.Dtos;
using ThomasPool.Application.Services;
using ThomasPool.Application.Dtos;
using ThomasPool.Domain.Entities;

namespace ThomasPool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ApiControllerBase
{
    private readonly ProfileService _profileService;

    public ProfileController(ProfileService profileService)
    {
        _profileService = profileService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("profileform")]
    public async Task<IActionResult> UpdateProfileForm(ProfileFormRequest profileFormRequest)
    {
        Result result = await _profileService.UpdateProfileFormAsync(profileFormRequest.ToDomain());
        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [HttpGet("profileform")]
    public async Task<IActionResult> GetProfileForm(int? version)
    {
        Result<ProfileForm> result = await _profileService.GetProfileFormAsync(version);
        return result.IsSuccess ? Ok(ProfileFormResponse.FromDomain(result.Value)) : HandleFailure(result.Error);
    }

    [HttpPost("profile")]
    public async Task<IActionResult> UpdateProfile(ProfileDto profileDto)
    {
        if (profileDto.Info == null || profileDto.Version <= -1) return HandleFailure(Errors.Profile.Invalid);
        Result result = await _profileService.UpdateProfileAsync(profileDto.ToProfile());
        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("profiles")]
    public async Task<IActionResult> GetProfiles(string name, int skip, int limit)
    {
        Result<Profile[]> result = await _profileService.GetProfilesAsync(name, skip, limit);
        return result.IsSuccess ? Ok(result.Value.Select(p => ProfileDto.FromDomain(p))) : HandleFailure(result.Error);
    }
}