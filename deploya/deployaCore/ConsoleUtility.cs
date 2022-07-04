using System;

namespace deploya_core
{
    static class ConsoleUtility
    {
        private const char _block = '■';
        private const string _back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";
        private const string _twirl = "-\\|/";

        public static void WriteProgressBar(int percent, bool update = false)
        {
            if (update)
                Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b");
            Console.Write("[");
            int num = (int)(percent / 10.0 + 0.5);
            for (int index = 0; index < 10; ++index)
            {
                if (index >= num)
                    Console.Write(' ');
                else
                    Console.Write('■');
            }
            Console.Write("] {0,3:##0}%", percent);
        }

        public static void WriteProgress(int progress, bool update = false)
        {
            if (update)
                Console.Write("\b");
            Console.Write("-\\|/"[progress % "-\\|/".Length]);
        }
    }
}
