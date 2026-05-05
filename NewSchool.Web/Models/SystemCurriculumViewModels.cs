namespace NewSchool.Web.Models;

public class SystemCurriculumTrackViewModel
{
    public string Title { get; set; } = string.Empty;
    public string DomainLabel { get; set; } = string.Empty;
    public string DomainChipClass { get; set; } = "neutral";
    public string AgeBandLabel { get; set; } = string.Empty;
    public string SchoolPlacementLabel { get; set; } = string.Empty;
    public string CurrentPhaseLabel { get; set; } = string.Empty;
    public string YearGoal { get; set; } = string.Empty;
    public string CurrentFocus { get; set; } = string.Empty;
    public string ParentMethod { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public List<string> ExampleSkills { get; set; } = new();
    public List<string> Materials { get; set; } = new();
    public SystemCurriculumUnitViewModel? CurrentUnit { get; set; }
    public List<SystemCurriculumPhaseSequenceViewModel> PhaseSequences { get; set; } = new();
}

public class SystemCurriculumPhaseSequenceViewModel
{
    public string PhaseLabel { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public int UnitCount { get; set; }
    public int LessonCount { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<SystemCurriculumUnitViewModel> Units { get; set; } = new();
}

public class SystemCurriculumUnitViewModel
{
    public string PhaseLabel { get; set; } = string.Empty;
    public string SchoolPlacementLabel { get; set; } = string.Empty;
    public string SubjectLabel { get; set; } = string.Empty;
    public int UnitNumber { get; set; }
    public string UnitLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string TaskTitle { get; set; } = string.Empty;
    public string TaskPrompt { get; set; } = string.Empty;
    public string CompletionSignal { get; set; } = string.Empty;
    public string OptionalEvidenceNote { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public List<string> SkillCodes { get; set; } = new();
    public List<string> LessonTitles { get; set; } = new();
    public List<string> Materials { get; set; } = new();
}
