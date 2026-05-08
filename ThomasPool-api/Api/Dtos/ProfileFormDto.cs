using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ThomasPool.Domain.Entities;

namespace ThomasPool.Api.Dtos;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(QuestionFormDto), "question")]
[JsonDerivedType(typeof(MultipleFormDto), "multiple")]
public record QuestionFormDto
{
    [JsonPropertyName("question")]
    [Required, StringLength(500, MinimumLength = 1)]
    public required string Question { get; init; }

    [JsonPropertyName("questionType")]
    public required QuestionType QuestionType { get; init; }

    public virtual QuestionForm ToDomain() => new() { Question = Question, Type = QuestionType };

    public static QuestionFormDto FromDomain(QuestionForm q) => q is MultipleForm m
        ? new MultipleFormDto { Question = m.Question, QuestionType = m.Type, Options = m.Options }
        : new QuestionFormDto { Question = q.Question, QuestionType = q.Type };
}

public record MultipleFormDto : QuestionFormDto
{
    [JsonPropertyName("options")]
    [Required, MinLength(2)]
    public required string[] Options { get; init; }

    public override QuestionForm ToDomain() =>
        new MultipleForm { Question = Question, Type = QuestionType, Options = Options };
}

public record ProfileFormRequest(
    [property: JsonPropertyName("questions")]
    [property: Required, MinLength(1)]
    QuestionFormDto[] Questions
)
{
    public ProfileForm ToDomain() => new()
    {
        Questions = [.. Questions.Select(q => q.ToDomain())]
    };
}

public record ProfileFormResponse(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("questions")] QuestionFormDto[] Questions
)
{
    public static ProfileFormResponse FromDomain(ProfileForm form) => new(
        form.Version!.Value,
        [.. form.Questions.Select(QuestionFormDto.FromDomain)]
    );
}
