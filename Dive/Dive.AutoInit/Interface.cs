using Dive.PluginContext;
using Newtonsoft.Json;

namespace Dive.AutoInit
{
    public class AutoInitAddon : IPlugin
    {
        private bool _isCalled = false;
        private Common.Configuration? _config = null;

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
            var args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (arg.Contains("--autoinit"))
                    _isCalled = true;
                if (arg.Contains("--autoinit-config"))
                {
                    // Split the argument to get the path of the configuration file. The configuration file should be in JSON format.
                    var configPath = arg.Split(':')[1];
                    var configJson = File.ReadAllText(configPath);
                    _config = JsonConvert.DeserializeObject<Common.Configuration>(configJson);
                    
                }
                if (arg.Contains("--create-config"))
                {
                    var config = Common.ConfigurationLoader.CreateConfiguration();
                    File.WriteAllText("D:\\shitty_config.json", config);
                    DebugConsole.WriteLine("Created config successfully.");
                }
            }
        }

        public void OnStartup()
        {
            if (_isCalled)
            {
                DebugConsole.WriteLine(@"Running AutoInit...");
                if (_config != null)
                {
                    Action.Perform(_config);
                }
                else
                {
                    DebugConsole.WriteLine("Configuration is null. Exiting...");
                }
            }
        }
    }
}