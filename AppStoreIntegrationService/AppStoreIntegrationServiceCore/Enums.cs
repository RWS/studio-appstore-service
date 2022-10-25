namespace AppStoreIntegrationServiceCore
{
    public class Enums
    {
        public enum DeployMode
        {
            AzureBlob,
            ServerFilePath,
            NetworkFilePath
        }
        public enum SortType
        {
            None,
            DownloadCount,
            ReviewCount,
            TopRated,
            LastUpdated,
            NewlyAdded
        }
        public enum StatusValue
        {
            Active,
            Inactive,
            All
        }

    }
}
