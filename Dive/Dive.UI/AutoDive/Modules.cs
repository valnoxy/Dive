using System;

namespace Dive.UI.AutoDive
{
    internal class Modules
    {
        internal static void ExceptionMessage(string v)
        {
            const string title = "AutoDive";
            const string btn1 = "Exit";

            var w = new MessageUI(title, v, btn1);
            if (w.ShowDialog() != false) return;
            var summary = w.Summary;
            if (summary == "Btn1")
            {
                Environment.Exit(1);
            }
        }

        internal static string convertSize(double size)
        {
            String[] units = new String[] { "B", "KB", "MB", "GB", "TB", "PB" };

            double mod = 1024.0;

            int i = 0;

            while (size >= mod)
            {
                size /= mod;
                i++;
            }
            return Math.Round(size, 2) + units[i];//with 2 decimals
        }
    }
}
