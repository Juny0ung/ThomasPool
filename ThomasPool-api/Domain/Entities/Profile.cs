
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace ThomasPool.Domain.Entities;

public class ProfileBase
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    // true: male / false: female
    [Required]
    public required bool Gender { get; set; }

    [Required, RegularExpression(@"^\d{11}$", ErrorMessage = "PhoneNumber must be 11 digits.")]
    public required string PhoneNumber { get; set; }

    [Range(1900, 2100)]
    public int BirthYear { get; set; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Region { get; set; }

    public string? PhotoId { get; set; }

    protected ProfileBase() {}

    [SetsRequiredMembers]
    public ProfileBase(string name, bool gender, string phoneNumber, int birthYear, string region)
    {
        Name = name;
        Gender = gender;
        PhoneNumber = phoneNumber;
        BirthYear = birthYear;
        Region = region;
    }

    public override bool Equals(object? obj)
    {
        if (obj is ProfileBase other)
        {
            return PhoneNumber == other.PhoneNumber;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PhoneNumber);
    }
}

public class MultipleContent : ProfileContent
{
    public required string[] Values { get; set; }
}

public class SingleContent : ProfileContent
{
    public required string Value { get; set; }
}

public class BoolContent : ProfileContent
{
    public required bool Value { get; set; }
}

public abstract class ProfileContent
{
}

public class Profile : ProfileBase
{
    public int Version { get; set; }
    public ProfileContent[] Info { get; set; }

    private Profile()
    {
        Info = [];
    }

    [SetsRequiredMembers]
    public Profile(string name, bool gender, string phoneNumber, int birthYear, string region,
                   ProfileContent[] info, int version = -1)
        : base(name, gender, phoneNumber, birthYear, region)
    {
        if (version > -1) Version = version;
        Info = info;
    }
}