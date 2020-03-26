namespace TwilioMediaStreams.Models
{
    public class ProjectSettings
    {
        public string TwilioMediaStreamWebhookUri { get; set; }
        public string AzureStorageAccountConnectionString { get; set; }
        public string AzureStorageAccountContainerName { get; set; }
    }
}
