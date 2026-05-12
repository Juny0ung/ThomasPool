using System.Text.Json;
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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProfile([FromForm] string profile, IFormFile photo)
    {
        if (photo == null) return HandleFailure(Errors.Profile.Invalid);

        var profileRequest = JsonSerializer.Deserialize<ProfileRequest>(profile);
        if (profileRequest == null) return HandleFailure(Errors.Profile.Invalid);

        using var ms = new MemoryStream();
        await photo.CopyToAsync(ms);
        byte[] photoBytes = ms.ToArray();

        Result result = await _profileService.UpdateProfileAsync(profileRequest.ToProfile(), photoBytes);
        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [HttpGet("photo/{photoId}")]
    public async Task<IActionResult> GetPhoto(string photoId)
    {
        Result<byte[]> result = await _profileService.GetPhotoAsync(photoId);
        return result.IsSuccess ? File(result.Value, "image/jpeg") : HandleFailure(result.Error);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("profiles")]
    public async Task<IActionResult> GetProfiles(int skip, int limit, [FromQuery] string[]? name = null, [FromQuery] bool[]? gender = null, [FromQuery] int[]? birthYear = null, [FromQuery] string[]? region = null)
    {
        Result<Profile[]> result = await _profileService.GetProfilesAsync(skip, limit, name, gender, birthYear, region);
        return result.IsSuccess ? Ok(result.Value.Select(ProfileResponse.FromDomain)) : HandleFailure(result.Error);
    }
}