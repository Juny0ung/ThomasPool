
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ThomasPool.Domain.Entities;

namespace ThomasPool.Api.Dtos;

public record AdminDto(
    [property: JsonPropertyName("id")]
    [property: Required]
    string Id,

    [property: JsonPropertyName("name")]
    [property: Required]
    string Name,

    [property: JsonPropertyName("role")]
    [property: Required]
    Role Role
)
{
    public static AdminDto FromDomain(AdminInfo adminInfo) => new(
        adminInfo.Id,
        adminInfo.Name,
        adminInfo.Role
    );
}