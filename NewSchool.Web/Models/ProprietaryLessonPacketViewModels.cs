namespace NewSchool.Web.Models;

public class ProprietaryLessonPacketViewModel
{
    public string CurriculumPlacement { get; set; } = string.Empty;
    public string UnitTitle { get; set; } = string.Empty;
    public string UnitSummary { get; set; } = string.Empty;
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
}
