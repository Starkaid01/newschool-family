namespace NewSchool.Web.Models;

public class TrackOfferViewModel
{
    public string TrackCode { get; set; } = string.Empty;
    public string TrackLabel { get; set; } = string.Empty;
    public string Eyebrow { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Lead { get; set; } = string.Empty;
    public string Promise { get; set; } = string.Empty;
    public string RegisterUrl { get; set; } = string.Empty;
    public string SecondaryUrl { get; set; } = string.Empty;
    public string SocialProof { get; set; } = string.Empty;
    public string ConversionHook { get; set; } = string.Empty;
    public List<string> Outcomes { get; set; } = new();
    public List<string> ProofBlocks { get; set; } = new();
    public List<string> ParentWins { get; set; } = new();
}
