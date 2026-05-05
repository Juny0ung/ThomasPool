
using System.Text.Json.Nodes;
using ThomasPool.Domain.Entities;

namespace ThomasPool.Domain.Interfaces;

public interface IProfileRepository
{
    Task RegisterAsync(ProfileBase profileBase);
    Task UnregisterAsync(ProfileBase profileBase);
    Task SaveInfoAsync(ProfileBase profileBase, int version, JsonNode info);
    Task<JsonNode?> GetInfoAsync(ProfileBase profileBase);
    Task<Profile[]> GetInfosAsync(string name, int skip = 0, int limit = 20);
}