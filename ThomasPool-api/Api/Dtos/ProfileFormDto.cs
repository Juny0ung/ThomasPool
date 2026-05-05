using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ThomasPool.Domain.Entities;

namespace ThomasPool.Api.Dtos;

public record QuestionFormDto(
    [property: JsonPropertyName("question")]
    [property: Required, StringLength(500, MinimumLength = 1)]
    string Question,

    [property: JsonPropertyName("type")]
    QuestionType Type,

    [property: JsonPropertyName("options")]
    string[]? Options
);

public record ProfileFormRequest(
    [property: JsonPropertyName("questions")]
    [property: Required, MinLength(1)]
    QuestionFormDto[] Questions
)
{
    public ProfileForm ToDomain() => new()
    {
        Questions = [.. Questions.Select(q =>
            q.Type is QuestionType.MultipleChoice or QuestionType.MultipleSelection
                ? new MultipleForm { Question = q.Question, Type = q.Type, Options = q.Options ?? [] }
                : new QuestionForm { Question = q.Question, Type = q.Type })]
    };
}

public record ProfileFormResponse(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("questions")] QuestionFormDto[] Questions
)
{
    public static ProfileFormResponse FromDomain(ProfileForm form) => new(
        form.Version!.Value,
        [.. form.Questions.Select(q => q is MultipleForm m
            ? new QuestionFormDto(m.Question, m.Type, m.Options)
            : new QuestionFormDto(q.Question, q.Type, null))]
    );
}
