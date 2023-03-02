namespace AppStoreIntegrationServiceCore.Model
{
    public class Log : IEquatable<Log>
    {
        public string Author { get; set; }
        public DateTime Date { get; set; }

        public string Description { get; set; }

        public bool Equals(Log other)
        {
            return Author == other?.Author &&
                   Description == other?.Description;
        }
    }
}
