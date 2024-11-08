namespace Dive.PluginContext
{
    public interface IPlugin
    {
        void Initialize();
        void OnStartup();
        PluginInfo GetPluginInfo();
    }

    public class PluginInfo
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Author { get; set; }
        public string? Version { get; set; }
    }
}
