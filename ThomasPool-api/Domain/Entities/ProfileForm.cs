using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace ThomasPool.Domain.Entities;

public enum QuestionType
{
    ShortAnswer,
    Essay,
    MultipleChoice,
    MultipleSelection,
    TrueFalse
}

public class MultipleForm : QuestionForm
{
    [Required, MinLength(2)]
    public required string[] Options { get; set; }
}

[BsonDiscriminator(RootClass = true)]
[BsonKnownTypes(typeof(MultipleForm))]
public class QuestionForm
{
    [Required, StringLength(500, MinimumLength = 1)]
    public required string Question { get; set; }
    public QuestionType Type { get; set; }
}

public class ProfileForm : IValidatableObject
{
    public int? Version { get; set; }

    [Required, MinLength(1)]
    public required QuestionForm[] Questions { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var (question, i) in Questions.Select((q, i) => (q, i)))
        {
            var results = new List<ValidationResult>();
            switch (question.Type)
            {
                case QuestionType.MultipleChoice:
                case QuestionType.MultipleSelection:
                    if (question is not MultipleForm) yield return new ValidationResult($"Questions[{i}]: Invalid type");
                    break;
                default:
                    if (question is MultipleForm) yield return new ValidationResult($"Questions[{i}]: Invalid type");
                    break;
            }
            if (!Validator.TryValidateObject(question, new ValidationContext(question), results, true))
                foreach (var result in results)
                    yield return new ValidationResult($"Questions[{i}]: {result.ErrorMessage}");
        }
    }
}