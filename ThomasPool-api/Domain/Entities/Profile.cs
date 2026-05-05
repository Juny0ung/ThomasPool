
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace ThomasPool.Domain.Entities;

public class ProfileBase
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    [Required, RegularExpression(@"^\d{10,11}$", ErrorMessage = "PhoneNumber must be 10 or 11 digits.")]
    public required string PhoneNumber { get; set; }

    [Range(1900, 2100)]
    public int BirthYear { get; set; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Region { get; set; }

    protected ProfileBase() {}

    [SetsRequiredMembers]
    public ProfileBase(ProfileBase other)
    {
        Name = other.Name;
        PhoneNumber = other.PhoneNumber;
        BirthYear = other.BirthYear;
        Region = other.Region;
    }

    public override bool Equals(object? obj)
    {
        if (obj is ProfileBase other)
        {
            return Name == other.Name 
                && PhoneNumber == other.PhoneNumber;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, PhoneNumber);
    }
}

public class Profile : ProfileBase
{
    public int? Version { get; set; }
    public JsonNode? Info { get; set; }

    private Profile() {}

    [SetsRequiredMembers]
    public Profile(ProfileBase other, int version = -1, JsonNode? info = null) : base(other)
    {
        if (version > -1) Version = version;
        if (info != null) Info = info;
    }
}