using NewSchool.Web.Domain;

namespace NewSchool.Web.Services;

public static class CurriculumStructure
{
    public static IReadOnlyList<LearningDomain> AnnualSubjectOrder { get; } =
    [
        LearningDomain.Language,
        LearningDomain.Math,
        LearningDomain.Science,
        LearningDomain.History,
        LearningDomain.Geography,
        LearningDomain.ExecutiveFunction
    ];

    public static bool IsWorldKnowledgeDomain(LearningDomain domain) =>
        domain is LearningDomain.World or LearningDomain.Science or LearningDomain.History or LearningDomain.Geography;

    public static LearningDomain GetAnalyticsDomain(LearningDomain domain) =>
        IsWorldKnowledgeDomain(domain) ? LearningDomain.World : domain;

    public static bool DomainMatches(LearningDomain left, LearningDomain right)
    {
        if (left == right)
        {
            return true;
        }

        return GetAnalyticsDomain(left) == GetAnalyticsDomain(right);
    }

    public static string FormatDomainLabel(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "Linguagem",
        LearningDomain.Math => "Matemática",
        LearningDomain.Science => "Ciências",
        LearningDomain.History => "História",
        LearningDomain.Geography => "Geografia",
        LearningDomain.World => "Ciências, história e geografia",
        LearningDomain.ExecutiveFunction => "Autonomia",
        _ => "Geral"
    };

    public static string GetDomainChipClass(LearningDomain domain) => domain switch
    {
        LearningDomain.Language => "track-communication",
        LearningDomain.Math => "track-academic",
        LearningDomain.Science => "success",
        LearningDomain.History => "success",
        LearningDomain.Geography => "success",
        LearningDomain.World => "success",
        LearningDomain.ExecutiveFunction => "track-dailyliving",
        _ => "neutral"
    };

    public static string GetSchoolPlacementLabel(int age) => age switch
    {
        3 => "Educação Infantil • 3 anos",
        4 => "Educação Infantil • 4 anos",
        5 => "Educação Infantil • 5 anos",
        6 => "1º ano do Ensino Fundamental",
        7 => "2º ano do Ensino Fundamental",
        8 => "3º ano do Ensino Fundamental",
        9 => "4º ano do Ensino Fundamental",
        10 => "5º ano do Ensino Fundamental",
        11 => "6º ano do Ensino Fundamental",
        12 => "7º ano do Ensino Fundamental",
        13 => "8º ano do Ensino Fundamental",
        14 => "9º ano do Ensino Fundamental",
        _ when age <= 5 => "Educação Infantil",
        _ => "Ensino Fundamental"
    };

    public static string GetEducationStageLabel(int age) => age switch
    {
        <= 5 => "Educação Infantil",
        _ => "Ensino Fundamental"
    };

    public static string GetCurriculumBandLabel(int age) => age switch
    {
        3 => "Currículo autoral • Educação Infantil 3",
        4 => "Currículo autoral • Educação Infantil 4",
        5 => "Currículo autoral • Educação Infantil 5",
        6 => "Currículo autoral • 1º ano",
        7 => "Currículo autoral • 2º ano",
        8 => "Currículo autoral • 3º ano",
        9 => "Currículo autoral • 4º ano",
        10 => "Currículo autoral • 5º ano",
        11 => "Currículo autoral • 6º ano",
        12 => "Currículo autoral • 7º ano",
        13 => "Currículo autoral • 8º ano",
        14 => "Currículo autoral • 9º ano",
        _ => "Currículo autoral"
    };

    public static LearningDomain GetKnowledgeDomainForDay(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Monday => LearningDomain.Science,
        DayOfWeek.Tuesday => LearningDomain.History,
        DayOfWeek.Wednesday => LearningDomain.Geography,
        DayOfWeek.Thursday => LearningDomain.Science,
        DayOfWeek.Friday => LearningDomain.History,
        _ => LearningDomain.Science
    };

    public static LearningDomain GetPreferredLibraryDomain(string familyGoalTrack) => familyGoalTrack switch
    {
        "math_foundations" => LearningDomain.Math,
        "science_discovery" => LearningDomain.Science,
        "autonomy" => LearningDomain.ExecutiveFunction,
        _ => LearningDomain.Language
    };
}
