using Dive.PluginContext;

namespace Dive.AutoInit
{
    public class AutoInitAddon : IPlugin
    {
        public PluginInfo GetPluginInfo()
        {
            return new PluginInfo
            {
                Name = "AutoInit Plugin",
                Description = "Automatic initial setup for Windows operating systems",
                Author = "Exploitox",
                Version = "2.0"
            };
        }

        public void Initialize()
        {
            Console.WriteLine(@"AutoInit Plugin initialized");
        }
    }
}