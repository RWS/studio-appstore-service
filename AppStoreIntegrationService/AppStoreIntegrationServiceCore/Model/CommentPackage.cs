namespace AppStoreIntegrationServiceCore.Model
{
    public class CommentPackage
    {
        public IEnumerable<Comment> PluginComments { get; set; }
        public IDictionary<string, IEnumerable<Comment>> VersionComments { get; set; }
    }
}
