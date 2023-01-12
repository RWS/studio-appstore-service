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
            Query,
            FromDate,
            ToDate
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
            Comment,
            Categories,
            ParentProducts,
            Products,
            Profile,
            Password,
            Register
        }
    }
}
