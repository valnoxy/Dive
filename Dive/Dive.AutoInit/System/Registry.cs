namespace Dive.AutoInit.System
{
    internal class Registry
    {
        internal static bool SetKey(string path, string name, string value)
        {
            var parentKey = Microsoft.Win32.Registry.LocalMachine;
            var subKey = parentKey.OpenSubKey(path, true);

            try
            {
                subKey.SetValue(name, value);
            }
            catch { return false; }
            return true;
        }
    }
}
