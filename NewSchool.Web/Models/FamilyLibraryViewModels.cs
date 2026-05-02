namespace NewSchool.Web.Models;

public class FamilyLibraryHomeViewModel
{
    public int TotalBooks { get; set; }
    public int ProprietaryBooks { get; set; }
    public int ProprietaryPrintables { get; set; }
    public int FavoriteBooks { get; set; }
    public int CompletedBooks { get; set; }
    public int InProgressBooks { get; set; }
    public Guid? SelectedChildId { get; set; }
    public string SelectedChildName { get; set; } = string.Empty;
    public string PersonalizationHeadline { get; set; } = string.Empty;
    public string PersonalizationSummary { get; set; } = string.Empty;
    public FamilyLibraryBookCardViewModel? CurrentBook { get; set; }
    public FamilyLibraryRecommendationViewModel? SpotlightBook { get; set; }
    public FamilyLibraryRecommendationViewModel? SpotlightPrintable { get; set; }
    public FamilyLibraryWeeklyReadingViewModel? WeeklyReading { get; set; }
    public FamilyLibraryAnnualSpineViewModel? AnnualReadingSpine { get; set; }
    public List<SystemCurriculumTrackViewModel> SystemCurriculumTracks { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public string SelectedCollection { get; set; } = string.Empty;
    public string SelectedStage { get; set; } = string.Empty;
    public string SelectedAgeFilter { get; set; } = string.Empty;
    public List<string> CollectionFilters { get; set; } = new();
    public List<string> StageFilters { get; set; } = new();
    public List<string> AgeFilters { get; set; } = new();
    public List<FamilyLibraryShelfViewModel> CurriculumShelves { get; set; } = new();
    public List<FamilyLibraryBookCardViewModel> FavoriteItems { get; set; } = new();
    public List<FamilyLibraryBookCardViewModel> CompletedItems { get; set; } = new();
    public List<FamilyLibraryBookCardViewModel> Books { get; set; } = new();
}

public class FamilyLibraryBookCardViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string EducationStage { get; set; } = string.Empty;
    public int RecommendedMinAge { get; set; }
    public int RecommendedMaxAge { get; set; }
    public string SkillFocus { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CollectionLabel { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public int CurrentPageNumber { get; set; } = 1;
    public bool IsStarted { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? LastReadAtUtc { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = "Novo";
    public string StatusCssClass { get; set; } = "new";
    public string AgeLabel { get; set; } = string.Empty;
}

public class FamilyLibraryReaderViewModel
{
    public Guid Id { get; set; }
    public Guid? SelectedChildId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string EducationStage { get; set; } = string.Empty;
    public string CollectionLabel { get; set; } = string.Empty;
    public string SkillFocus { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public int CurrentPageNumber { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsCompleted { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public string BackLibraryUrl { get; set; } = string.Empty;
    public string BackPrintablesUrl { get; set; } = string.Empty;
    public List<FamilyLibraryPageViewModel> Pages { get; set; } = new();
    public FamilyLibraryPageViewModel CurrentPage => Pages.First(page => page.PageNumber == CurrentPageNumber);
}

public class FamilyLibraryPageViewModel
{
    public int PageNumber { get; set; }
    public string TextContent { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl);
    public bool HasText => !string.IsNullOrWhiteSpace(TextContent);
}

public class FamilyLibraryPrintablesViewModel
{
    public Guid? SelectedChildId { get; set; }
    public string SelectedChildName { get; set; } = string.Empty;
    public string PersonalizationSummary { get; set; } = string.Empty;
    public string SearchTerm { get; set; } = string.Empty;
    public string SelectedCategory { get; set; } = string.Empty;
    public string SelectedStage { get; set; } = string.Empty;
    public string SelectedAgeFilter { get; set; } = string.Empty;
    public List<string> CategoryFilters { get; set; } = new();
    public List<string> StageFilters { get; set; } = new();
    public List<string> AgeFilters { get; set; } = new();
    public List<FamilyLibraryPrintableCardViewModel> Materials { get; set; } = new();
}

public class FamilyLibraryPrintableCardViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string EducationStage { get; set; } = string.Empty;
    public string CollectionLabel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SkillFocus { get; set; } = string.Empty;
    public int RecommendedMinAge { get; set; }
    public int RecommendedMaxAge { get; set; }
    public int PageCount { get; set; }
    public string AgeLabel { get; set; } = string.Empty;
}

public class FamilyLibraryCurriculumBridgeViewModel
{
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string LibraryUrl { get; set; } = string.Empty;
    public FamilyLibraryRecommendationViewModel? RecommendedBook { get; set; }
    public FamilyLibraryRecommendationViewModel? RecommendedPrintable { get; set; }
    public FamilyLibraryWeeklyReadingViewModel? WeeklyReading { get; set; }
}

public class FamilyLibraryShelfViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ActionLabel { get; set; } = string.Empty;
    public string ActionUrl { get; set; } = string.Empty;
    public List<FamilyLibraryRecommendationViewModel> Items { get; set; } = new();
}

public class FamilyLibraryRecommendationViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string EducationStage { get; set; } = string.Empty;
    public string CollectionLabel { get; set; } = string.Empty;
    public string SkillFocus { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AgeLabel { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public string ProgressLabel { get; set; } = string.Empty;
    public string FitReason { get; set; } = string.Empty;
    public string AccessLabel { get; set; } = string.Empty;
    public string AccessUrl { get; set; } = string.Empty;
    public string SecondaryActionLabel { get; set; } = string.Empty;
    public string SecondaryActionUrl { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public int CurrentPageNumber { get; set; }
    public bool IsPrintable { get; set; }
    public bool IsStarted { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsFavorite { get; set; }
}

public class FamilyLibraryWeeklyReadingViewModel
{
    public Guid ChildId { get; set; }
    public Guid? MaterialId { get; set; }
    public int PhaseNumber { get; set; }
    public string PhaseLabel { get; set; } = string.Empty;
    public string PeriodKey { get; set; } = string.Empty;
    public int WeekNumber { get; set; }
    public string WeekLabel { get; set; } = string.Empty;
    public string MonthLabel { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string WeeklyGoal { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string CompletionSignal { get; set; } = string.Empty;
    public string ReflectionPrompt { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public string CompletionStatusLabel { get; set; } = string.Empty;
    public string CompletionActionLabel { get; set; } = string.Empty;
    public string CompletedAtLabel { get; set; } = string.Empty;
    public int CompletedReadingsCount { get; set; }
    public string CompletedReadingsLabel { get; set; } = string.Empty;
    public List<string> WeekSteps { get; set; } = new();
    public FamilyLibraryRecommendationViewModel? Book { get; set; }
    public FamilyLibraryRecommendationViewModel? Printable { get; set; }
    public FamilyLibraryMonthlyGoalViewModel MonthlyGoal { get; set; } = new();
    public FamilyLibraryLiteratureUnitViewModel? CurrentUnit { get; set; }
    public List<ChildReadingHistoryItemViewModel> RecentHistory { get; set; } = new();
}

public class FamilyLibraryAnnualSpineViewModel
{
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string CurrentPhaseLabel { get; set; } = string.Empty;
    public int CompletedReadingsCount { get; set; }
    public string CompletedReadingsLabel { get; set; } = string.Empty;
    public List<ChildReadingHistoryItemViewModel> RecentHistory { get; set; } = new();
    public List<FamilyLibraryAnnualSpinePhaseViewModel> Phases { get; set; } = new();
}

public class FamilyLibraryAnnualSpinePhaseViewModel
{
    public int PhaseNumber { get; set; }
    public string PhaseLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string WeeklyRhythm { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string CompletionSignal { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public int CompletedCount { get; set; }
    public string CompletedCountLabel { get; set; } = string.Empty;
    public int TargetReadingsToClose { get; set; }
    public string AutoCloseLabel { get; set; } = string.Empty;
    public bool IsClosed { get; set; }
    public FamilyLibraryRecommendationViewModel? Book { get; set; }
    public FamilyLibraryRecommendationViewModel? Printable { get; set; }
    public List<FamilyLibraryLiteratureUnitViewModel> UnitSequences { get; set; } = new();
    public List<ChildReadingHistoryItemViewModel> CompletedItems { get; set; } = new();
}

public class ChildReadingHistoryItemViewModel
{
    public Guid MaterialId { get; set; }
    public int PhaseNumber { get; set; }
    public string PhaseLabel { get; set; } = string.Empty;
    public string PeriodKey { get; set; } = string.Empty;
    public int WeekNumber { get; set; }
    public string WeekLabel { get; set; } = string.Empty;
    public string MonthLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string GoalLabel { get; set; } = string.Empty;
    public DateTime CompletedAtUtc { get; set; }
    public string CompletedAtLabel { get; set; } = string.Empty;
    public string AccessUrl { get; set; } = string.Empty;
}

public class FamilyLibraryMonthlyGoalViewModel
{
    public string MonthLabel { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int TargetReadings { get; set; }
    public int CompletedReadings { get; set; }
    public bool IsCompleted { get; set; }
    public string ProgressLabel { get; set; } = string.Empty;
    public string AutoCloseProjectionLabel { get; set; } = string.Empty;
    public List<string> GoalBullets { get; set; } = new();
    public List<FamilyLibraryWeeklySequenceItemViewModel> WeeklySequence { get; set; } = new();
}

public class FamilyLibraryWeeklySequenceItemViewModel
{
    public int WeekNumber { get; set; }
    public string WeekLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public bool IsCompleted { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
}

public class FamilyLibraryLiteratureUnitViewModel
{
    public int PhaseNumber { get; set; }
    public int UnitNumber { get; set; }
    public string UnitLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ParentGuide { get; set; } = string.Empty;
    public string WritingTaskTitle { get; set; } = string.Empty;
    public string WritingTaskPrompt { get; set; } = string.Empty;
    public string WritingCompletionSignal { get; set; } = string.Empty;
    public string OptionalEvidencePrompt { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public List<string> UnitFlow { get; set; } = new();
    public FamilyLibraryRecommendationViewModel? Book { get; set; }
    public FamilyLibraryRecommendationViewModel? Printable { get; set; }
}
