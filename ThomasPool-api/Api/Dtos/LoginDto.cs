
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThomasPool.Api.Dtos;

public record LoginDto(
    [property: JsonPropertyName("adminId")]
    [Required]
    string AdminId,

    [property: JsonPropertyName("password")]
    [Required]
    string Password
);
