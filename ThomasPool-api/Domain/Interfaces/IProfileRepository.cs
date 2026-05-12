
using System.Text.Json.Nodes;
using ThomasPool.Domain.Entities;

namespace ThomasPool.Domain.Interfaces;

public interface IProfileRepository
{
    Task SaveInfoAsync(Profile profile);
    Task UpdateInfoAsync(Profile profile);
    Task DeleteProfileAsync(ProfileBase profileBase);
    Task<Profile?> GetInfoAsync(ProfileBase profileBase);
    Task<Profile[]> GetInfosAsync(int skip = 0, int limit = 20, string[]? name = null, bool[]? gender = null, int[]? birthYear = null, string[]? region = null);
}