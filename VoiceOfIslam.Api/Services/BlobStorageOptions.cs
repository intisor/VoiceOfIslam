namespace VoiceOfIslam.Api.Services
{
    public class BlobStorageOptions
    {
        public const string SectionName = "BlobStorage";

        public string ConnectionString { get; set; } = string.Empty;
        public int SasTokenMinutes { get; set; } = 30;
    }
}
