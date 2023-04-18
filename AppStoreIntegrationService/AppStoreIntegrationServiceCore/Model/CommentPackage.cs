namespace AppStoreIntegrationServiceCore.Model
{
    public class CommentPackage
    {
        public IEnumerable<Comment> PluginComments { get; set; } = new List<Comment>();
        public IDictionary<string, IEnumerable<Comment>> VersionComments { get; set; } = new Dictionary<string, IEnumerable<Comment>>();
    }
}