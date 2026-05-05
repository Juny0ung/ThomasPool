using ThomasPool.Domain.Entities;

namespace ThomasPool.Domain.Interfaces;

public interface IProfileFormRepository
{
    Task UpdateFormAsync(ProfileForm form);
    Task<ProfileForm?> GetFormAsync(int? version);
}