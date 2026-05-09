using System.ComponentModel.DataAnnotations;
using ThomasPool.Application.Dtos;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;
using ThomasPool.Domain.Services;

namespace ThomasPool.Application.Services;

public class ProfileService
{
    private readonly IProfileFormRepository _profileFormRepository;
    private readonly IProfileRepository _profileRepository;
    public ProfileService(IProfileFormRepository profileFormRepository, IProfileRepository profileRepository)
    {
        _profileFormRepository = profileFormRepository;
        _profileRepository = profileRepository;
    }

    public async Task<Result> UpdateProfileFormAsync(ProfileForm profileForm)
    {
        try
        {
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(profileForm, new ValidationContext(profileForm), results, true))
                return Result.Failure(Errors.ProfileForm.Invalid);

            await _profileFormRepository.UpdateFormAsync(profileForm);
            return Result.Success();
        }
        catch
        {
            return Result.Failure(Errors.General.Unknown);
        }
    }

    public async Task<Result<ProfileForm>> GetProfileFormAsync(int? version)
    {
        try
        {
            ProfileForm? profileForm = await _profileFormRepository.GetFormAsync(version);
            if (profileForm != null) return Result<ProfileForm>.Success(profileForm);
        }
        catch
        {
            return Result<ProfileForm>.Failure(Errors.General.Unknown);
        }
        return Result<ProfileForm>.Failure(Errors.ProfileForm.NotFound);
    }

    public async Task<Result> UpdateProfileAsync(Profile profile)
    {
        try
        {
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(profile, new ValidationContext(profile), results, true))
                return Result.Failure(Errors.Profile.Invalid);

            ProfileForm? validForm = await _profileFormRepository.GetFormAsync(profile.Version);
            if (validForm == null || validForm.Version == null)
                return Result.Failure(Errors.ProfileForm.NotFound);

            if (!validForm.ValidateProfile(profile))
                return Result.Failure(Errors.Profile.Invalid);
            
            // TODO: validate password
            var prevProfile = await _profileRepository.GetInfoAsync(profile);
            if (prevProfile != null) await _profileRepository.UpdateInfoAsync(profile);            
            else await _profileRepository.SaveInfoAsync(profile);
            
            return Result.Success();
        }
        catch
        {
            return Result.Failure(Errors.General.Unknown);
        }
    }

    public async Task<Result<Profile[]>> GetProfilesAsync(string name, int skip, int limit)
    {
        try
        {
            Profile[] profiles = await _profileRepository.GetInfosAsync(name, skip, limit);
            return Result<Profile[]>.Success(profiles);
        }
        catch
        {
            return Result<Profile[]>.Failure(Errors.General.Unknown);
        }
    }
}