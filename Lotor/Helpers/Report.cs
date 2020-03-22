using Lotor.Caches;
using Lotor.Helpers;
using Lotor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Helpers
{
    /// <summary>
    /// used to report to console
    /// </summary>
    class Report
    {
        private static bool writeToConsole = false;
        public const string separator = " | "; // delimiter in console messages
        /// <summary>
        /// invoked to start reporting
        /// </summary>
        public static void turnOn()
        {
            writeToConsole = true;
        }

        /// <summary>
        /// invoked to stop reporting
        /// </summary>
        public static void turnOff()
        {
            writeToConsole = false;
        }

        /// <summary>
        /// reports errors
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool error(string message, Exception e = null, bool stop = false)
        {
            if (writeToConsole)
            {
                switchColorTo(ConsoleColor.Red);
                Console.WriteLine(String.Format("{0}{1} {2}", GlobalHelper.getTime(), separator, message));
                switchBackColor();
            }

            if (stop) // do not allow to continue in certain situations errors
            {
                Console.ReadKey();
                Environment.Exit(0);
            }
            return false;
        }

        /// <summary>
        /// reports success message
        /// </summary>
        /// <param name="message"></param>
        public static void success(string message)
        {
            if (writeToConsole)
            {
                switchColorTo(ConsoleColor.Green);
                Console.WriteLine(String.Format("{0}{1}{2}", GlobalHelper.getTime(), separator, message));
                switchBackColor();
            }
        }

        /// <summary>
        /// reports any given message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        public static void info(string message, ConsoleColor color = ConsoleColor.White, bool stop = false)
        {
            if (writeToConsole)
            {
                switchColorTo(color);
                Console.WriteLine(String.Format("{0}{1}{2}", GlobalHelper.getTime(), separator, message));
                switchBackColor();
            }

            if (stop) // do not allow to continue in certain situations errors
            {
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// at the end of every domain crawl
        /// reports stats e.g number of urls processed
        /// </summary>
        public static void reportCrawled()
        {
            if (writeToConsole && DomainCache.totalUrls > 0)
            {
                string message = String.Format("{0} {1} {2}{3}Crawled {4}/{5} about {6}%", 
                   DomainCache.firstLevelUrls.Count,
                   DomainCache.secondLevelUrls.Count,
                   DomainCache.thirdLevelUrls.Count,
                   separator,
                   DomainCache.successfullyProcessed,
                   DomainCache.totalUrls,
                   GlobalHelper.getCrawledPercentage()
                   );
                info(message, ConsoleColor.DarkGreen);
            }
        }

        /// <summary>
        /// reports detailed albanian
        /// </summary>
        /// <param name="domain"></param>
        public static void reportLang(Domain domain, bool isAlbanian = false)
        {
            string message = String.Format("This domain is{0}Albanian{1}{2} index page{1}and{1}{3} the first level.", isAlbanian ? " " : " not ", separator, domain.indexPageAlbVal, domain.firstLevelAlbVal);
            success(message);
        }

        private static ConsoleColor lastColor;
        private static void switchColorTo(ConsoleColor color)
        {
            lastColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
        }
        private static void switchBackColor()
        {
            Console.ForegroundColor = lastColor;
        }
    }
}
