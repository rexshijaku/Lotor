using Lotor.Caches;
using Lotor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Helpers
{
    public class UrlHelper
    {
        /// <summary>
        /// tries to add (to list) a url which is found in a document
        /// it will not be added in certain situations: e.g when it was found previously
        /// </summary>
        /// <param name="lotorUrl">url to be added (processed)</param>
        /// <param name="levelTo">list of next level</param>
        /// <param name="rawDom">url before processed (as it was found)</param>
        public static int tryToAddUrl(LotorUrl lotorUrl, List<string> levelTo, string rawDom)
        {
            if (lotorUrl.hasFragmentPrefix()) // urls like mydomain.smth/#home or just #home
            {
                string[] urlSegments = lotorUrl.urlFragmentSegments();
                string firstSegment = urlSegments[0];
                if (!DomainCache.isAddedBefore(firstSegment))
                {
                    addUrlToQueue(firstSegment, levelTo);
                    return 1;
                }
                else
                    Report.info(lotorUrl.Url + " was not added to queue! | Reason : Same location from the source found previously!", ConsoleColor.DarkGreen);
            }
            else
            {
                if (!DomainCache.isAddedBefore(lotorUrl.Url))
                {
                    addUrlToQueue(lotorUrl.Url, levelTo);
                    return 1;
                }
                else
                    Report.error(lotorUrl.Url + " was not added to queue! | Reason : It was added before.");
            }
            return 0;
        }

        private static void addUrlToQueue(string documentUrl, List<string> level)
        {
            level.Add(documentUrl);
            Report.success(documentUrl + " added to queue.");
        }
    }
}
