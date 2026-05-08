using ThomasPool.Domain.Entities;

namespace ThomasPool.Domain.Services;

public static class ProfileValidator
{
    public static bool ValidateProfile(this ProfileForm profileForm, Profile profile)
    {
        if (profileForm.Version != null && profileForm.Version != profile.Version) return false;

        int cnt = profileForm.Questions.Length;
        if (cnt != profile.Info.Length) return false;
        for (int i = 0; i < cnt; i++)
        {
            switch (profileForm.Questions[i].Type)
            {
                case QuestionType.ShortAnswer:
                case QuestionType.Essay:
                case QuestionType.MultipleChoice:
                    if (profile.Info[i] is not SingleContent) return false;
                    break;
                case QuestionType.MultipleSelection:
                    if (profile.Info[i] is not MultipleContent) return false;
                    break;
                case QuestionType.TrueFalse:
                    if (profile.Info[i] is not BoolContent) return false;
                    break;
                default:
                    return false;
            }
        }
        return true;
    }
}