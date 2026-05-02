namespace NewSchool.Web.Models;

public class PortuguesePlanningViewModel
{
    public string SubjectName { get; set; } = "Lingua Portuguesa";
    public string ScopeLabel { get; set; } = "Planejamento pedagogico BNCC";
    public string ScopeSummary { get; set; } = string.Empty;
    public Guid? SelectedChildId { get; set; }
    public string? SelectedChildName { get; set; }
    public int? SelectedChildAge { get; set; }
    public string? SelectedStageLabel { get; set; }
    public string? SelectedStageSummary { get; set; }
    public List<PlanningChildOptionViewModel> Children { get; set; } = new();
    public List<PortuguesePlanningStageViewModel> Stages { get; set; } = new();
}

public class PlanningChildOptionViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string StageLabel { get; set; } = string.Empty;
}

public class PortuguesePlanningStageViewModel
{
    public int Age { get; set; }
    public string StageLabel { get; set; } = string.Empty;
    public string SchoolPlacement { get; set; } = string.Empty;
    public string BnccAnchor { get; set; } = string.Empty;
    public string AnnualObjective { get; set; } = string.Empty;
    public string DiagnosticFocus { get; set; } = string.Empty;
    public string PedagogicalApproach { get; set; } = string.Empty;
    public List<string> BnccReferences { get; set; } = new();
    public List<string> ExpectedOutcomes { get; set; } = new();
    public List<string> SuggestedEvidence { get; set; } = new();
    public List<string> FamilyRoutineActions { get; set; } = new();
    public List<PortuguesePlanningTermViewModel> Terms { get; set; } = new();
}

public class PortuguesePlanningTermViewModel
{
    public string TermLabel { get; set; } = string.Empty;
    public string MainFocus { get; set; } = string.Empty;
    public List<string> LearningGoals { get; set; } = new();
    public List<string> SuggestedGenres { get; set; } = new();
    public List<string> AssessmentSignals { get; set; } = new();
}
