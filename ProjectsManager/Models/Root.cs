namespace ProjectsManager.Models
{
    public class Root
    {
        public GitConfig GitConfig { get; set; } = new();
        public List<Folder> Folders { get; set; } = new();
    }
}
