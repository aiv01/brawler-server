using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlerServer.Utilities
{
    public class GenericTraceListener : IListener
    {
        public void Write(string message)
        {
            Write(message, "");
        }

        public void Write(string message, string category)
        {
            Console.WriteLine($"[{category}]{message}");
        }
    }
    

    public static class Logs
    {
        [Flags]
        public enum DebugLevel
        {
            None, ErrorsOnly, WarningsOnly, Full
        }
        public static DebugLevel Level = DebugLevel.Full;

        public static IListener Listener = new GenericTraceListener();

        public static void Log(string message)
        {
            if (Level != DebugLevel.Full) return;
            Listener.Write(message, "Debug");
        }

        public static void LogWarning(string message)
        {
            if (Level < DebugLevel.WarningsOnly) return;
            Listener.Write(message, "Warning");
        }

        public static void LogError(string message)
        {
            if (Level < DebugLevel.ErrorsOnly) return;
            Listener.Write(message, "Error");
        }
    }
}


public interface IListener
{
    void Write(string text);
    void Write(string text, string category);
}

