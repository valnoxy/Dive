using System;
using System.Text;
using Spectre.Console;

namespace Dive.UI.Common.Diagnostics
{
    /// <summary>
    /// Writes log entries to the console using Spectre.Console.
    /// Format: <c>HH:mm:ss LVL message</c>
    /// </summary>
    public sealed class ConsoleLogSink : ILogSink
    {
        public void Write(LogEntry entry)
        {
            var levelMarkup = entry.Level switch
            {
                LogLevel.Debug => "[grey]DBG[/]",
                LogLevel.Info => "[blue]INF[/]",
                LogLevel.Warning => "[yellow]WRN[/]",
                LogLevel.Error => "[red]ERR[/]",
                _ => "[grey]???[/]"
            };

            var sb = new StringBuilder();
            sb.Append($"[grey]{entry.Timestamp:HH:mm:ss}[/] {levelMarkup} ");

            foreach (var segment in entry.Segments)
            {
                var text = Markup.Escape(segment.Text);
                var color = MapColor(segment.Color);
                sb.Append(color is null ? text : $"[{color}]{text}[/]");
            }

            AnsiConsole.MarkupLine(sb.ToString());
        }

        private static string? MapColor(ConsoleColor? color) => color switch
        {
            null or ConsoleColor.White or ConsoleColor.Gray => null,
            ConsoleColor.Black => "black",
            ConsoleColor.DarkBlue => "navy",
            ConsoleColor.DarkGreen => "green",
            ConsoleColor.DarkCyan => "teal",
            ConsoleColor.DarkRed => "maroon",
            ConsoleColor.DarkMagenta => "purple",
            ConsoleColor.DarkYellow => "olive",
            ConsoleColor.DarkGray => "grey",
            ConsoleColor.Blue => "blue",
            ConsoleColor.Green => "lime",
            ConsoleColor.Cyan => "aqua",
            ConsoleColor.Red => "red",
            ConsoleColor.Magenta => "fuchsia",
            ConsoleColor.Yellow => "yellow",
            _ => null
        };
    }
}
