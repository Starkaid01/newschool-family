namespace NewSchool.Web.Models;

public class HomeIndexViewModel
{
    public int TotalFamilies { get; set; }
    public int TotalChildren { get; set; }
    public int CurriculumItems { get; set; }
    public int ActiveSubscribers { get; set; }
    public int SessionsDelivered { get; set; }
    public int EvidenceCaptured { get; set; }
    public int MonthlySnapshotsRecorded { get; set; }
    public int FamiliesTrackingEvolution { get; set; }
    public List<TrackOfferLinkViewModel> TrackOffers { get; set; } = new();
    public List<HomeResourceSourceViewModel> ResourceSources { get; set; } = new();
}

public class TrackOfferLinkViewModel
{
    public string TrackCode { get; set; } = string.Empty;
    public string TrackLabel { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class HomeResourceSourceViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ActionLabel { get; set; } = string.Empty;
}
