namespace NewSchool.Web.Domain;

public class ChildRoutineObservation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChildId { get; set; }
    public ChildProfile Child { get; set; } = null!;
    public Guid? SessionId { get; set; }
    public LearningSession? Session { get; set; }
    public Guid? DailyPlanId { get; set; }
    public DailyPlan? DailyPlan { get; set; }
    public DateTime ObservedAt { get; set; } = DateTime.UtcNow;
    public string ContextPeriod { get; set; } = string.Empty;
    public string Antecedent { get; set; } = string.Empty;
    public string ChildReaction { get; set; } = string.Empty;
    public string WhatHelped { get; set; } = string.Empty;
    public string SupportUsed { get; set; } = string.Empty;
    public int DistressLevel { get; set; } = 2;
    public int TaskToleranceMinutes { get; set; } = 10;
    public bool NeededPlanB { get; set; }
    public bool VisualSupportHelped { get; set; }
    public bool BreakHelped { get; set; }
    public bool CoRegulationHelped { get; set; }
    public string Notes { get; set; } = string.Empty;
}
