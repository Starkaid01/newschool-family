using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NewSchool.Web.Models;

public class LogSessionViewModel
{
    public Guid ChildId { get; set; }
    public Guid DailyPlanId { get; set; }

    [Range(5, 240)]
    [Display(Name = "Minutos realizados")]
    public int MinutesCompleted { get; set; } = 30;

    [Display(Name = "Pontos fortes")]
    public string Wins { get; set; } = string.Empty;

    [Display(Name = "Pontos de atencao")]
    public string Challenges { get; set; } = string.Empty;

    [Display(Name = "Observacoes")]
    public string Notes { get; set; } = string.Empty;

    [Display(Name = "Imagem ou video da atividade")]
    public IFormFile? EvidenceFile { get; set; }

    [Display(Name = "O que veio antes da dificuldade ou da mudanca?")]
    public string Antecedent { get; set; } = string.Empty;

    [Display(Name = "Como a crianca reagiu?")]
    public string ChildReaction { get; set; } = string.Empty;

    [Display(Name = "O que ajudou de verdade?")]
    public string WhatHelped { get; set; } = string.Empty;

    [Display(Name = "Apoio usado")]
    public string SupportUsed { get; set; } = string.Empty;

    [Range(1, 5)]
    [Display(Name = "Intensidade do desconforto")]
    public int DistressLevel { get; set; } = 2;

    [Range(1, 120)]
    [Display(Name = "Quanto tempo tolerou antes de travar?")]
    public int TaskToleranceMinutes { get; set; } = 10;

    [Display(Name = "Precisou de plano B")]
    public bool NeededPlanB { get; set; }

    [Display(Name = "Apoio visual ajudou")]
    public bool VisualSupportHelped { get; set; }

    [Display(Name = "Pausa ajudou")]
    public bool BreakHelped { get; set; }

    [Display(Name = "Co-regulacao ajudou")]
    public bool CoRegulationHelped { get; set; }

    public List<BlockFeedbackInputViewModel> BlockFeedbacks { get; set; } = new();
}

public class BlockFeedbackInputViewModel
{
    public Guid DailyPlanBlockId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SkillCode { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string DomainLabel { get; set; } = string.Empty;

    [Display(Name = "Como a crianca foi neste bloco?")]
    public int Rating { get; set; } = 2;
}
