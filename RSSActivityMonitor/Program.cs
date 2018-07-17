using System;

namespace RSSActivityMonitor
{
    class Program
    {
        /// <summary>
        /// Accepts a CSV-formatted list of companies and their RSS feeds, as well as a given number of days,
        /// and will output any companies with no RSS activity within that given number.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var monitor = new RSSActivityMonitor();
            var output = monitor.Start(args);

            Console.WriteLine(output);
        }
    }
}
