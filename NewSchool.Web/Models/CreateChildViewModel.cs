using System.ComponentModel.DataAnnotations;

namespace NewSchool.Web.Models;

public class CreateChildViewModel
{
    [Display(Name = "Nome da crianca")]
    public string FullName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Data de nascimento exata")]
    public DateTime? BirthDate { get; set; }

    [Range(3, 10)]
    [Display(Name = "Idade aproximada")]
    public int EstimatedAgeYears { get; set; } = 6;

    [Range(20, 120)]
    [Display(Name = "Ritmo diario")]
    public int DailyStudyMinutes { get; set; } = 45;

    [Display(Name = "Observacoes livres")]
    public string Notes { get; set; } = string.Empty;

    [Display(Name = "Perfil de suporte")]
    public string SupportProfile { get; set; } = "General";

    [Display(Name = "Objetivo principal da familia")]
    public string FamilyGoalTrack { get; set; } = "balanced_growth";

    [Display(Name = "Metodologia pedagogica")]
    public string TeachingMethodology { get; set; } = "eclectic";

    [Display(Name = "Perfil de aprendizagem")]
    public string LearningProfile { get; set; } = "balanced";

    [Display(Name = "Como essa crianca aprende melhor")]
    public string GuidanceStyle { get; set; } = "guided";

    [Display(Name = "Linguagem")]
    public int LanguageLevel { get; set; } = 3;

    [Display(Name = "Matematica")]
    public int MathLevel { get; set; } = 3;

    [Display(Name = "Mundo real")]
    public int WorldLevel { get; set; } = 3;

    [Display(Name = "Funcao executiva")]
    public int ExecutiveFunctionLevel { get; set; } = 3;

    [Display(Name = "Pontos fortes percebidos")]
    public string StrengthsSummary { get; set; } = string.Empty;

    [Display(Name = "O que mais precisa de reforco")]
    public string SupportSummary { get; set; } = string.Empty;

    [Display(Name = "Como a crianca se comunica melhor")]
    public string CommunicationProfile { get; set; } = string.Empty;

    [Display(Name = "Observacoes sobre comunicacao")]
    public string CommunicationNotes { get; set; } = string.Empty;

    [Display(Name = "Ansiedade")]
    public int AnxietyLevel { get; set; } = 3;

    [Display(Name = "Rigidez cognitiva")]
    public int CognitiveRigidityLevel { get; set; } = 3;

    [Display(Name = "Sensibilidade sensorial")]
    public int SensorySensitivityLevel { get; set; } = 3;

    [Display(Name = "Dificuldade em transicoes")]
    public int TransitionDifficultyLevel { get; set; } = 3;

    [Display(Name = "Intensidade de apoio necessaria")]
    public int SupportIntensityLevel { get; set; } = 3;

    [Display(Name = "Precisa de rotina visual")]
    public bool NeedsVisualRoutine { get; set; }

    [Display(Name = "Precisa de first-then")]
    public bool NeedsFirstThen { get; set; }

    [Display(Name = "Precisa de timer visual")]
    public bool NeedsTimer { get; set; }

    [Display(Name = "Precisa de plano B")]
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

    [Display(Name = "Entende melhor linguagem direta")]
    public bool CommunicationDirectLanguage { get; set; }

    [Display(Name = "Responde melhor com apoio visual")]
    public bool CommunicationVisualSupport { get; set; }

    [Display(Name = "Precisa de modelagem antes de tentar")]
    public bool CommunicationModeling { get; set; }

    [Display(Name = "Precisa de mais tempo para processar")]
    public bool CommunicationProcessingTime { get; set; }

    [Display(Name = "Curiosa e investigativa")]
    public bool StrengthCurious { get; set; }

    [Display(Name = "Comunicativa")]
    public bool StrengthCommunicative { get; set; }

    [Display(Name = "Criativa")]
    public bool StrengthCreative { get; set; }

    [Display(Name = "Memoriza com facilidade")]
    public bool StrengthMemorizes { get; set; }

    [Display(Name = "Gosta de logica e padroes")]
    public bool StrengthLogical { get; set; }

    [Display(Name = "Precisa reforcar foco")]
    public bool SupportNeedsFocus { get; set; }

    [Display(Name = "Precisa reforcar transicoes")]
    public bool SupportNeedsTransitions { get; set; }

    [Display(Name = "Precisa reforcar linguagem")]
    public bool SupportNeedsLanguage { get; set; }

    [Display(Name = "Precisa reforcar autorregulacao")]
    public bool SupportNeedsRegulation { get; set; }

    [Display(Name = "Mudancas inesperadas pesam")]
    public bool TriggerUnexpectedChange { get; set; }

    [Display(Name = "Barulho pesa")]
    public bool TriggerNoise { get; set; }

    [Display(Name = "Escrita ou tarefas no papel pesam")]
    public bool TriggerWritingDemand { get; set; }

    [Display(Name = "Esperar pesa")]
    public bool TriggerWaiting { get; set; }

    [Display(Name = "Exigencia social pesa")]
    public bool TriggerSocialDemand { get; set; }

    [Display(Name = "Pausa curta ajuda")]
    public bool CalmingPause { get; set; }

    [Display(Name = "Movimento ajuda")]
    public bool CalmingMovement { get; set; }

    [Display(Name = "Lugar silencioso ajuda")]
    public bool CalmingQuietCorner { get; set; }

    [Display(Name = "Desenhar ajuda")]
    public bool CalmingDrawing { get; set; }

    [Display(Name = "Musica calma ajuda")]
    public bool CalmingMusic { get; set; }

    [Display(Name = "A escola gera crise de ansiedade")]
    public bool BarrierAnxietyCrises { get; set; }

    [Display(Name = "A escola gera sobrecarga sensorial")]
    public bool BarrierSchoolOverload { get; set; }

    [Display(Name = "A rigidez esta alta")]
    public bool BarrierRigidity { get; set; }

    [Display(Name = "As demandas sociais estao pesando")]
    public bool BarrierSocialDifficulty { get; set; }
}
