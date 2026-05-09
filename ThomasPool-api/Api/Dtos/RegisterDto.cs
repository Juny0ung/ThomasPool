
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThomasPool.Api.Dtos;

public record RegisterDto(
    [property: JsonPropertyName("name")]
    [Required, StringLength(100, MinimumLength = 1)]
    string Name,

    [property: JsonPropertyName("adminId")]
    [Required, StringLength(30, MinimumLength = 2)]
    string AdminId,

    [property: JsonPropertyName("password")]
    [Required]
    string Password
);
