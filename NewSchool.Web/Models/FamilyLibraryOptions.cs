namespace NewSchool.Web.Models;

public class FamilyLibraryOptions
{
    public const string SectionName = "FamilyLibrary";

    public bool Enabled { get; set; }
    public string SourceCatalogConnection { get; set; } = string.Empty;
    public string SourceAssetsRootPath { get; set; } = string.Empty;
    public string TargetAssetsRootPath { get; set; } = "wwwroot\\family-library-assets";
}
