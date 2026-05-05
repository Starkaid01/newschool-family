using NewSchool.Web.Domain;

namespace NewSchool.Web.Models;

public class ProprietaryCurriculumSubjectBlueprintViewModel
{
    public int Age { get; set; }
    public LearningDomain Domain { get; set; }
    public string SchoolPlacementLabel { get; set; } = string.Empty;
    public string SubjectLabel { get; set; } = string.Empty;
    public string TrackTitle { get; set; } = string.Empty;
    public string YearGoal { get; set; } = string.Empty;
    public string ParentMethod { get; set; } = string.Empty;
    public List<ProprietaryCurriculumPhaseBlueprintViewModel> Phases { get; set; } = new();
}

public class ProprietaryCurriculumPhaseBlueprintViewModel
{
    public int PhaseNumber { get; set; }
    public string PhaseLabel { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<ProprietaryCurriculumUnitBlueprintViewModel> Units { get; set; } = new();
}

public class ProprietaryCurriculumUnitBlueprintViewModel
{
    public string SchoolPlacementLabel { get; set; } = string.Empty;
    public string SubjectLabel { get; set; } = string.Empty;
    public int PhaseNumber { get; set; }
    public string PhaseLabel { get; set; } = string.Empty;
    public int UnitNumber { get; set; }
    public string UnitLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string TaskPrompt { get; set; } = string.Empty;
    public string CompletionSignal { get; set; } = string.Empty;
    public string OptionalEvidenceNote { get; set; } = string.Empty;
    public string CompanionBookTitle { get; set; } = string.Empty;
    public string PrintableTitle { get; set; } = string.Empty;
    public string AssessmentTitle { get; set; } = string.Empty;
    public List<string> LessonTitles { get; set; } = new();
    public List<ProprietaryCurriculumLessonBlueprintViewModel> Lessons { get; set; } = new();
    public List<string> Materials { get; set; } = new();
}

public class ProprietaryCurriculumLessonBlueprintViewModel
{
    public string TaskSlug { get; set; } = string.Empty;
    public int LessonNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string OpeningForAdult { get; set; } = string.Empty;
    public string AnchorQuestion { get; set; } = string.Empty;
    public string CoreMaterialLabel { get; set; } = "Leia agora";
    public string CoreMaterialTitle { get; set; } = string.Empty;
    public List<string> CoreMaterialParagraphs { get; set; } = new();
    public List<string> AdultSteps { get; set; } = new();
    public List<string> AdultQuestions { get; set; } = new();
    public List<string> AcceptableAnswers { get; set; } = new();
    public string PracticeTask { get; set; } = string.Empty;
    public string CompletionDefinition { get; set; } = string.Empty;
    public string EvidencePrompt { get; set; } = string.Empty;
    public string ExpectedOutcome { get; set; } = string.Empty;
    public int SuggestedMinutes { get; set; } = 20;
    public string MatchKeywords { get; set; } = string.Empty;
    public string PrimaryMaterialTitle { get; set; } = string.Empty;
    public string SupportMaterialTitle { get; set; } = string.Empty;
}
