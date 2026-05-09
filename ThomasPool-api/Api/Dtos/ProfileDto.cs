using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ThomasPool.Domain.Entities;

namespace ThomasPool.Api.Dtos;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SingleContentDto), "string")]
[JsonDerivedType(typeof(BoolContentDto), "bool")]
[JsonDerivedType(typeof(MultipleContentDto), "multiple")]
public abstract record ProfileContentDto
{
    public abstract ProfileContent ToDomain();

    public static ProfileContentDto FromDomain(ProfileContent content) => content switch
    {
        SingleContent s   => new SingleContentDto(s.Value),
        BoolContent b     => new BoolContentDto(b.Value),
        MultipleContent m => new MultipleContentDto(m.Values),
        _ => throw new ArgumentException($"Unknown ProfileContent type: {content.GetType()}")
    };
}

public record SingleContentDto(
    [property: JsonPropertyName("value")] string Value
) : ProfileContentDto
{
    public override ProfileContent ToDomain() => new SingleContent { Value = Value };
}

public record BoolContentDto(
    [property: JsonPropertyName("value")] bool Value
) : ProfileContentDto
{
    public override ProfileContent ToDomain() => new BoolContent { Value = Value };
}

public record MultipleContentDto(
    [property: JsonPropertyName("values")] string[] Values
) : ProfileContentDto
{
    public override ProfileContent ToDomain() => new MultipleContent { Values = Values };
}

public record ProfileBaseDto(
    [property: JsonPropertyName("name")]
    [Required, StringLength(100, MinimumLength = 1)]
    string Name,

    [property: JsonPropertyName("gender")]
    [Required]
    bool Gender,

    [property: JsonPropertyName("phoneNumber")]
    [Required, RegularExpression(@"^\d{11}$", ErrorMessage = "PhoneNumber must be 11 digits.")]
    string PhoneNumber,

    [property: JsonPropertyName("birthYear")]
    [Range(1900, 2100)]
    int BirthYear,

    [property: JsonPropertyName("region")]
    [Required, StringLength(100, MinimumLength = 1)]
    string Region
)
{
    public ProfileBase ToDomain() => new(Name, Gender, PhoneNumber, BirthYear, Region);
}

public record ProfileDto(
    [property: JsonPropertyName("name")]
    [Required, StringLength(100, MinimumLength = 1)]
    string Name,

    [property: JsonPropertyName("gender")]
    [Required]
    bool Gender,

    [property: JsonPropertyName("phoneNumber")]
    [Required, RegularExpression(@"^\d{11}$", ErrorMessage = "PhoneNumber must be 11 digits.")]
    string PhoneNumber,

    [property: JsonPropertyName("birthYear")]
    [Range(1900, 2100)]
    int BirthYear,

    [property: JsonPropertyName("region")]
    [Required, StringLength(100, MinimumLength = 1)]
    string Region,

    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("info")] ProfileContentDto[] Info
)
{
    public Profile ToProfile() => new(
        Name, Gender, PhoneNumber, BirthYear, Region,
        [.. Info.Select(c => c.ToDomain())],
        Version
    );

    public static ProfileDto FromDomain(Profile profile) => new(
        profile.Name,
        profile.Gender,
        profile.PhoneNumber,
        profile.BirthYear,
        profile.Region,
        profile.Version,
        [.. profile.Info.Select(ProfileContentDto.FromDomain)]
    );
}
