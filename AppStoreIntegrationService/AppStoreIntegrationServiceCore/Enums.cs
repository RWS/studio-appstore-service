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

        public enum Status
        {
            Active = 0,
            Inactive,
            Draft,
            InReview,
            All
        }

        public enum FilterType
        {
            Status = 0,
            Product,
            Query
        }

        public enum ProductType
        {
            Child,
            Parent
        }

        public enum Page
        {
            None = 0,
            Plugin,
            Version,
            Comment
        }
    }
}
