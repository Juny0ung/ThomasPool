using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ThomasPool.Domain.Entities;

namespace ThomasPool.Api.Dtos;

public record ProfileBaseDto(
    [property: JsonPropertyName("name")]
    [property: Required, StringLength(100, MinimumLength = 1)]
    string Name,

    [property: JsonPropertyName("phoneNumber")]
    [property: Required, RegularExpression(@"^\d{11}$", ErrorMessage = "PhoneNumber must be 11 digits.")]
    string PhoneNumber,

    [property: JsonPropertyName("birthYear")]
    [property: Range(1900, 2100)]
    int BirthYear,

    [property: JsonPropertyName("region")]
    [property: Required, StringLength(100, MinimumLength = 1)]
    string Region
)
{
    public ProfileBase ToDomain() => new(Name, PhoneNumber, BirthYear, Region);
}

public record ProfileDto(
    [property: JsonPropertyName("name")]
    [property: Required, StringLength(100, MinimumLength = 1)]
    string Name,

    [property: JsonPropertyName("phoneNumber")]
    [property: Required, RegularExpression(@"^\d{11}$", ErrorMessage = "PhoneNumber must be 11 digits.")]
    string PhoneNumber,

    [property: JsonPropertyName("birthYear")]
    [property: Range(1900, 2100)]
    int BirthYear,

    [property: JsonPropertyName("region")]
    [property: Required, StringLength(100, MinimumLength = 1)]
    string Region,

    [property: JsonPropertyName("version")] int? Version,
    [property: JsonPropertyName("info")] JsonNode? Info
)
{
    public ProfileBase ToProfileBase() => new(Name, PhoneNumber, BirthYear, Region);

    public static ProfileDto FromDomain(Profile profile) => new(
        profile.Name,
        profile.PhoneNumber,
        profile.BirthYear,
        profile.Region,
        profile.Version,
        profile.Info
    );
}
