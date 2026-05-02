using System.ComponentModel.DataAnnotations;

namespace NewSchool.Web.Models;

public class ChildTeaProfileViewModel
{
    public Guid ChildId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string SupportProfileLabel { get; set; } = string.Empty;
    public string ChildUrl { get; set; } = string.Empty;
    public string TeaTracksUrl { get; set; } = string.Empty;
    public string AdaptiveRoutineUrl { get; set; } = string.Empty;
    public string DossierUrl { get; set; } = string.Empty;
    public AdaptiveRoutineSnapshotViewModel Snapshot { get; set; } = new();
    public List<TeaProfileInsightViewModel> Insights { get; set; } = new();
    public ChildTeaProfileInputViewModel Form { get; set; } = new();
}

public class ChildTeaProfileInputViewModel
{
    public Guid ChildId { get; set; }

    [Display(Name = "Como a crianca se comunica melhor")]
    public string CommunicationProfile { get; set; } = string.Empty;

    [Display(Name = "Observacoes sobre comunicacao")]
    public string CommunicationNotes { get; set; } = string.Empty;

    [Range(1, 5)]
    [Display(Name = "Ansiedade")]
    public int AnxietyLevel { get; set; } = 3;

    [Range(1, 5)]
    [Display(Name = "Rigidez cognitiva")]
    public int CognitiveRigidityLevel { get; set; } = 3;

    [Range(1, 5)]
    [Display(Name = "Sensibilidade sensorial")]
    public int SensorySensitivityLevel { get; set; } = 3;

    [Range(1, 5)]
    [Display(Name = "Dificuldade em transicoes")]
    public int TransitionDifficultyLevel { get; set; } = 3;

    [Range(1, 5)]
    [Display(Name = "Intensidade de apoio necessaria")]
    public int SupportIntensityLevel { get; set; } = 3;

    [Display(Name = "Precisa de rotina visual")]
    public bool NeedsVisualRoutine { get; set; }

    [Display(Name = "Precisa de first-then")]
    public bool NeedsFirstThen { get; set; }

    [Display(Name = "Precisa de timer visual")]
    public bool NeedsTimer { get; set; }

    [Display(Name = "Precisa de plano B previsto")]
    public bool NeedsPlanB { get; set; }

    [Display(Name = "Interesses especiais / hiperfocos")]
    public string SpecialInterests { get; set; } = string.Empty;

    [Display(Name = "Reforcadores que funcionam")]
    public string EffectiveReinforcers { get; set; } = string.Empty;

    [Display(Name = "Gatilhos comuns")]
    public string CommonTriggers { get; set; } = string.Empty;

    [Display(Name = "Sinais de sobrecarga")]
    public string OverloadSignals { get; set; } = string.Empty;

    [Display(Name = "O que costuma acalmar")]
    public string CalmingStrategies { get; set; } = string.Empty;

    [Display(Name = "Apoios para transicao")]
    public string TransitionSupports { get; set; } = string.Empty;

    [Display(Name = "Prioridades de vida diaria")]
    public string DailyLivingPriorities { get; set; } = string.Empty;

    [Display(Name = "Objetivo principal da familia agora")]
    public string ParentPrimaryGoal { get; set; } = string.Empty;

    [Display(Name = "O que esta tornando a escola inviavel hoje")]
    public string SchoolBarrierSummary { get; set; } = string.Empty;

    [Display(Name = "Notas para documentacao")]
    public string DocumentationNotes { get; set; } = string.Empty;
}

public class TeaProfileInsightViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ChipClass { get; set; } = "neutral";
}
