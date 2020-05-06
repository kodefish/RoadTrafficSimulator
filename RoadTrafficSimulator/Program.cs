using System;
using System.Diagnostics;

namespace RoadTrafficSimulator
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Outputs Debug.WriteLine to console
            TextWriterTraceListener myWriter = new TextWriterTraceListener(System.Console.Out);
            // Debug.Listeners.Add(myWriter);
            using (var game = new RoadTrafficSimulator())
                game.Run();
        }
    }
}
