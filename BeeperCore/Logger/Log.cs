using System;

namespace BeeperCore
{
    public class Log
    {

        public readonly byte INFO = 0;
        public readonly byte WARN = 1;
        public readonly byte ERROR = 2;
        public readonly byte VERBOSE = 3;
        public readonly byte DEBUG = 4;
        public Boolean VERBOSEON { get; set; } = false;
        public Boolean DEBUGON { get; set; } = false;
        public String TAG = "INTERNAL";
        private ConsoleColor ForcedColor;
        private Boolean ForceColor;

        public DateTime GetDateTimeNow()
        {
            return DateTime.Now;
        }

        public String GetDateTimeNowString()
        {
            return GetDateTimeNow().ToString("yyyy/MM/dd hh:mm:ss.ffff tt");
        }

        public String GetLogHeader()
        {
            if (TAG != "") return "[" + GetDateTimeNowString() + "][" + TAG + "]" + " ";
            else return "[" + GetDateTimeNowString() + "]" + " ";
        }

        public void ForceConsoleColor(ConsoleColor color)
        {
            ForceColor = true;
            ForcedColor = color;
        }

        public void ClearForcedColor()
        {
            ForceColor = false;
        }

        private String GetLogHeader(byte logLevel)
        {
            String LEVELTAG = "INFO";
            switch (logLevel)
            {
                case 0: LEVELTAG = "INFO"; break;
                case 1: LEVELTAG = "WARN"; break;
                case 2: LEVELTAG = "ERROR"; break;
                case 3: LEVELTAG = "VERBOSE"; break;
                case 4: LEVELTAG = "DEBUG"; break;
            }
            if (TAG != "") return "[" + GetDateTimeNowString() + "][" + TAG + "][" + LEVELTAG + "]" + " ";
            else return "[" + GetDateTimeNowString() + "][" + LEVELTAG + "]" + " ";
        }

        public Log()
        {
            TAG = "";
        }

        public Log(String tag)
        {
            TAG = tag;
        }

        public void Write(byte LogLevel, String content)
        {
            if (!DEBUGON && LogLevel == DEBUG) return;
            if (!VERBOSEON && LogLevel == VERBOSE) return;
            switch (LogLevel)
            {
                case 0: Console.ForegroundColor = ConsoleColor.Gray; break;
                case 1: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case 2: Console.ForegroundColor = ConsoleColor.Red; break;
                case 3: Console.ForegroundColor = ConsoleColor.Blue; break;
                case 4: Console.ForegroundColor = ConsoleColor.Magenta; break;
            }
            if (ForceColor) Console.ForegroundColor = ForcedColor;
            Console.Write(GetLogHeader(LogLevel) + content);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void Write(byte LogLevel, String[] content)
        {
            if (!DEBUGON && LogLevel == DEBUG) return;
            if (!VERBOSEON && LogLevel == VERBOSE) return;
            switch (LogLevel)
            {
                case 0: Console.ForegroundColor = ConsoleColor.Gray; break;
                case 1: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case 2: Console.ForegroundColor = ConsoleColor.Red; break;
                case 3: Console.ForegroundColor = ConsoleColor.Blue; break;
                case 4: Console.ForegroundColor = ConsoleColor.Magenta; break;
            }
            if (ForceColor) Console.ForegroundColor = ForcedColor;
            foreach (String line in content)
            {
                Console.Write(GetLogHeader(LogLevel) + line);
            }
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void WriteLine(byte LogLevel, String content)
        {
            if (!DEBUGON && LogLevel == DEBUG) return;
            if (!VERBOSEON && LogLevel == VERBOSE) return;
            switch (LogLevel)
            {
                case 0: Console.ForegroundColor = ConsoleColor.Gray; break;
                case 1: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case 2: Console.ForegroundColor = ConsoleColor.Red; break;
                case 3: Console.ForegroundColor = ConsoleColor.Blue; break;
                case 4: Console.ForegroundColor = ConsoleColor.Magenta; break;
            }
            if (ForceColor) Console.ForegroundColor = ForcedColor;
            Console.WriteLine(GetLogHeader(LogLevel) + content);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void WriteLine(byte LogLevel, String[] content)
        {
            if (!DEBUGON && LogLevel == DEBUG) return;
            if (!VERBOSEON && LogLevel == VERBOSE) return;
            switch (LogLevel)
            {
                case 0: Console.ForegroundColor = ConsoleColor.Gray; break;
                case 1: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case 2: Console.ForegroundColor = ConsoleColor.Red; break;
                case 3: Console.ForegroundColor = ConsoleColor.Blue; break;
                case 4: Console.ForegroundColor = ConsoleColor.Magenta; break;
            }
            if (ForceColor) Console.ForegroundColor = ForcedColor;
            foreach (String line in content)
            {
                Console.WriteLine(GetLogHeader(LogLevel) + line);
            }
            Console.ForegroundColor = ConsoleColor.Gray;
        }

    }
}
