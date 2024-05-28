namespace Dive.PluginContext
{
    public class DebugConsole
    {
        public static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("*");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("] ");

            Console.ForegroundColor = color;
            Console.WriteLine(message);
        }

        public static void Write(string message, bool continueLine = false, ConsoleColor color = ConsoleColor.White)
        {
            if (!continueLine)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("*");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("] ");
            }

            Console.ForegroundColor = color;
            Console.Write(message);
        }
    }
}
