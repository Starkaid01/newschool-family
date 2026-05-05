namespace NewSchool.Web.Models;

public class EvidenceStorageOptions
{
    public const string SectionName = "EvidenceStorage";

    public bool Enabled { get; set; }
    public bool UseDirectUpload { get; set; }
    public string AzureConnectionString { get; set; } = string.Empty;
    public string AzureContainerName { get; set; } = "evidences";
    public int UploadSasMinutes { get; set; } = 15;
    public int ReadSasMinutes { get; set; } = 120;
}
