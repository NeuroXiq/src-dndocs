using System;

namespace DNDocs.ConsoleTools
{
    class ConsoleOut
    {
        const ConsoleColor ColorError = ConsoleColor.Red;
        const ConsoleColor ColorSuccess = ConsoleColor.Green;

        internal static void Error(string msg) => ConsoleWriteline(msg, ColorError);
        internal static void Success(string msg) => ConsoleWriteline(msg, ColorSuccess);

        static void ConsoleWriteline(string msg, ConsoleColor color)
        {
            var orgcolor = System.Console.ForegroundColor;

            Console.ForegroundColor = color;
            System.Console.WriteLine(msg);
            
            Console.ForegroundColor = orgcolor;
        }

        public static void Debug(string msg)
        {
            ConsoleWriteline($"\tRobinia-DEBUG: {msg}", ConsoleColor.DarkGray);
        }
    }
}