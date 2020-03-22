using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Net;
using Lotor.Caches;
using Lotor.Helpers;
using Lotor.Models;
using Lotor.Globals;
using Lotor.Calculations;

namespace Lotor
{
    class Crawl
    {
        /// <summary>
        /// The initial point
        /// </summary>
        public Crawl()
        {
            if (FileOperations.checkDirs())
                this.start();
            else
                Environment.Exit(0);
        }


        /// <summary>
        /// Crawls the entire set of given domains.
        /// </summary>
        public void start()
        {
            Report.turnOn();
            Report.info("Starting...");

            InternetOperations.init();
            foreach (var domain in MainCache.UrlList)
            {
                domain.initCrawl();
                if (domain.indexPage.isFound())
                {
                    DomainCache.zeroLevel.Add(domain.name); // this level will be used only to extract urls from index page
                    domain.crawl(Level.Index, DomainCache.zeroLevel, DomainCache.firstLevelUrls);

                    if (!domain.hasValidIndexPage())
                        domain.crawlWhenInvalidIndexPage();

                    if (domain.has1stLevel())
                    {
                        domain.crawl(Level.First, DomainCache.firstLevelUrls, DomainCache.secondLevelUrls);
                        if (domain.isAlbanian()) // if domain is Albanian continue to crawl its second and third levels
                        {
                            Report.reportLang(domain, true);
                            if (domain.has2ndLevel())
                            {
                                domain.crawl(Level.Second, DomainCache.secondLevelUrls, DomainCache.thirdLevelUrls);
                                if (domain.has3rdLevel())
                                    domain.crawl(Level.Third, DomainCache.thirdLevelUrls);
                                else
                                    Report.info(String.Format("{0} has no {1} level!", DomainCache.activeDomain.name, GlobalHelper.levelStr(Level.Third)));
                            }
                            else
                                Report.info(String.Format("{0} has no {1} level!", DomainCache.activeDomain.name, GlobalHelper.levelStr(Level.Second)));

                            Report.success(DomainCache.activeDomain.name + " has been successfully processed.");
                            domain.save();
                            domain.checkIfIsMultiLingual(); // check whether the domain has any other language except Albanian
                        }
                        else
                        {
                            Report.reportLang(domain);
                            domain.save();
                            domain.checkAlbAsAlternative(); // check whether the domain has Albanian language as an alternative
                        }
                    }
                    else
                        Report.info(String.Format("{0} has no {1} level!", DomainCache.activeDomain.name, GlobalHelper.levelStr(Level.First)));

                    domain.cleanCache();
                    GlobalHelper.animateCrawlEnd();
                }
            }

            Report.info("Domain list was finished!", ConsoleColor.Green);
            if (!Configs.CLOSE_CONSOLE)
                Console.Read();
        }
    }
}
