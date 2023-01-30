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

        public enum Status
        {
            Active = 0,
            Inactive,
            Draft,
            InReview,
            All
        }

        public enum Page
        {
            None = 0,
            Details,
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
