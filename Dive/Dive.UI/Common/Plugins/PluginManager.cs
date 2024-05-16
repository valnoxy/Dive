using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dive.PluginContext;

namespace Dive.UI.Common.Plugins
{
    public class PluginManager
    {
        private readonly List<IPlugin> _plugins = [];

        public int LoadPlugins(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return 0;

            var count = 0;

            foreach (var file in Directory.GetFiles(folderPath, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    var pluginTypes = assembly.GetTypes()
                        .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface);

                    foreach (var type in pluginTypes)
                    {
                        var plugin = (IPlugin)Activator.CreateInstance(type)!;
                        _plugins.Add(plugin);
                        count++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($@"Failed to load assembly '{file}': {ex.Message}");
                }
            }
            return count;
        }

        public void InitPlugins()
        {
            foreach (var plugin in _plugins)
            {
                plugin.Initialize();
            }
        }

        public List<IPlugin> GetPlugins()
        {
            return _plugins;
        }
    }
}
