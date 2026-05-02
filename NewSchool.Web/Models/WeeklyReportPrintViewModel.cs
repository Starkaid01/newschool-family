namespace NewSchool.Web.Models;

public class WeeklyReportPrintViewModel
{
    public string ParentName { get; set; } = string.Empty;
    public string WeekLabel { get; set; } = string.Empty;
    public WeeklyFamilyReportViewModel WeeklyReport { get; set; } = new();
}
