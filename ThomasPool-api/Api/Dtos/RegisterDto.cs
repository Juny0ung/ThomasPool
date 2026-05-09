
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThomasPool.Api.Dtos;

public record RegisterDto(
    [property: JsonPropertyName("name")]
    [property: Required, StringLength(100, MinimumLength = 1)]
    string Name,

    [property: JsonPropertyName("id")]
    [property: Required, StringLength(30, MinimumLength = 2)]
    string Id,

    [property: JsonPropertyName("password")]
    [property: Required]
    string Password
);