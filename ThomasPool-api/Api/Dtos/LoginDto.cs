
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThomasPool.Api.Dtos;

public record LoginDto(
    [property: JsonPropertyName("id")]
    [property: Required]
    string AdminId,

    [property: JsonPropertyName("password")]
    [property: Required]
    string Password
);