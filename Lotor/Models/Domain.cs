using Lotor.Caches;
using Lotor.Calculations;
using Lotor.Globals;
using Lotor.Helpers;
using Lotor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Models
{
    public class Domain
    {
        #region Vars
        /// <summary>
        /// domain name - modified
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// original name given in url list
        /// </summary>
        public string listName { get; set; }

        /// <summary>
        /// domains index page 
        /// </summary>
        public Document indexPage { get; set; }

        /// <summary>
        /// a new instance of this class contains information about the document counts in each level
        /// </summary>
        public LevelInfo levelCountInfo;

        /// <summary>
        /// calculates the quality of a document that is found in domain and keeps information about the entire quality of a domain
        /// </summary>
        public QualityCalculator qualityCalculator;

        /// <summary>
        /// the albanian value of the first level in a domain
        /// </summary>
        public double firstLevelAlbVal { get; set; }

        /// <summary>
        /// Albanian value of the index page  in a domain
        /// </summary>
        public double indexPageAlbVal { get; set; }

        /// <summary>
        /// final Albanian val calucated by a specific formula
        /// </summary>
        private double finalAlbVal { get; set; }

        /// <summary>
        /// the size of domain
        /// </summary>
        private double size { get; set; }

        /// <summary>
        /// the quality of domain
        /// </summary>
        private double quality { get; set; }

        /// <summary>
        /// keeps information wether domain is Albanian or not after applying formula
        /// </summary>
        private bool isAlb { get; set; }
        #endregion

        public Domain(string url)
        {
            this.name = url;
            this.listName = url;
        }

        /// <summary>
        /// initializes domain crawling
        /// </summary>
        public void initCrawl()
        {
            Report.info(String.Format("{0} is prepearing to be crwaled...", this.name), ConsoleColor.Green);
            DomainCache.activeDomain = this; // set as active domain
            FileOperations.cleanDir(FileOperations.getFilePath(Paths.TEMP));
            this.setDomainName();
            this.qualityCalculator = new QualityCalculator();
            Report.turnOff();
            this.indexPage = new Document(this.name);
            Report.turnOn();
            Report.info("Started crawling of " + this.name);
        }

        /// <summary>
        /// modifies the given domain name if is incomplete 
        /// </summary>
        private void setDomainName()
        {
            if (!DomainCache.activeDomain.name.Contains("://"))
                DomainCache.activeDomain.name = "http://" + DomainCache.activeDomain.name;

            DomainCache.activeDomain.name = InternetOperations.geDomainsUrl(); // checks for the final url

            if (DomainCache.activeDomain.name.EndsWith("/"))
                DomainCache.activeDomain.name = DomainCache.activeDomain.name.Remove(DomainCache.activeDomain.name.Length - 1, 1);

        }

        #region Check if domain
        /// <summary>
        /// checks whether the domain has a valid index page or not
        /// </summary>
        /// <returns></returns>
        public bool hasValidIndexPage()
        {
            bool response = DomainCache.firstLevelUrls.Count >= Configs.MIN_INDEX_ANCHORS; // we consider an index page valid if it has more than MIN_INDEX_ANCHORS anchors 
            if (!response)
                Report.error("Invalid index page!");
            return response;
        }

        /// <summary>
        /// checks if domains first level exists
        /// </summary>
        /// <returns>true if first level exists, false otherwise</returns>
        public bool has1stLevel()
        {
            Report.info("Checking first level...");
            return DomainCache.firstLevelUrls.Count > 0;
        }

        /// <summary>
        /// checks if domains second level exists
        /// </summary>
        /// <returns>true if the second level exists, false otherwise</returns>
        public bool has2ndLevel()
        {
            Report.info("Checking second level...");
            return DomainCache.secondLevelUrls.Count > 0;
        }

        /// <summary>
        /// checks if domains third level exists
        /// </summary>
        /// <returns>true if the third level exists, false otherwise</returns>
        public bool has3rdLevel()
        {
            Report.info("Checking third level...");
            return DomainCache.thirdLevelUrls.Count > 0;
        }

        /// <summary>
        /// checks if domains is Albanian
        /// </summary>
        /// <returns>true if it is wrriten in Albanian, false otherwise</returns>
        public bool isAlbanian()
        {
            this.isAlb = this.finalAlbVal > Configs.ALB_THRESHOLD;
            return this.isAlb;
        }
        #endregion

        #region Get domains
        public double getQuality()
        {
            return this.quality;
        }

        public double getSize()
        {
            return this.size;
        }

        public string getHost()
        {
            return new Uri(this.name).Host;
        }

        public double getScore()
        {
            double score = 1;
            if (this.levelCountInfo.getTotalDocuments() > 0)
                score = ((this.levelCountInfo.getTotalDocuments() + this.getSize()) * this.finalAlbVal) * this.quality;
            return score;
        }
        #endregion

        /// <summary>
        /// this method is invoked when index page is found to be invalid 
        /// it finds links in the page which is refered from the index page
        /// </summary>
        public void crawlWhenInvalidIndexPage()
        {
            List<string> firstLevelCopy = new List<string>(DomainCache.firstLevelUrls);
            this.cleanWhenIndexPageIsInvalid();
            this.crawl(Level.Index, firstLevelCopy, DomainCache.firstLevelUrls);
        }

        /// <summary>
        /// crawls level 
        /// </summary>
        /// <param name="crawlingLevel">which level is crawling</param>
        /// <param name="levelFrom">urls of crawling level</param>
        /// <param name="levelTo">found new urls will be added on next level</param>
        /// <returns></returns>

        public void crawl(Level crawlingLevel, List<string> levelFrom, List<string> levelTo = null)
        {
            double documentCount = 0; // total successfully processed documents
            double albValAcc = 0.0; // sum of processed documents Albanian values

            List<Task> TaskList = new List<Task>(); // stores tasks (to download & process) created for each document in a particular level
            GlobalHelper.reportCrawlingLevel(crawlingLevel);
            string levelStr = GlobalHelper.levelStr(crawlingLevel);

            for (int i = 0; i < levelFrom.Count; i++)
            {
                var task = Task.Factory.StartNew(listIndex =>
                 {
                     string documentUrl = levelFrom.ElementAt((int)listIndex);
                     Report.info(String.Format("{0} LEVEL | Running task for url : {1} ", levelStr, documentUrl), ConsoleColor.Cyan);
                     Document document = new Document(documentUrl);
                     try
                     {
                         if (document.isFound())
                         {
                             document.process(levelFrom, (int)listIndex);

                             if (document.isValid())
                             {
                                 if (!document.isDuplicate() && crawlingLevel != Level.Third) // extract links only if page is not duplicate and level hasn't reached third
                                 {
                                     if (crawlingLevel == Level.Index || crawlingLevel == Level.First) // only in index and first level we do calculations about the language and quality of domains
                                     {
                                         double albVal = document.getAlbVal();
                                         albValAcc += albVal;
                                         if (crawlingLevel == Level.First) // because only in the first levels formula we divide by document count
                                             documentCount++;

                                         if (!DomainCache.likelyAlbanian.ContainsKey(documentUrl)) // if domain is not Albanian, we will try to find Albanian part from these urls
                                             DomainCache.likelyAlbanian.Add(documentUrl, albVal);
                                     }

                                     List<string> newLinks = TextOperations.extractLinks(document);
                                     int totalValid = GlobalHelper.addLinksToList(documentUrl, newLinks, levelTo);
                                   
                                     Report.info(document.url + " | " + totalValid + " new valid links from " + newLinks.Count + " total found.", ConsoleColor.Green);
                                     this.qualityCalculator.addToQualitySum(this.qualityCalculator.getDocumentQuality(document));
                                     Report.success(document.url + " has been successfully processed.");
                                 }
                             }
                             else
                                 document.html = levelFrom[(int)listIndex] = Constants.DOCUMENT_NOT_FOUND_CONTENT;
                         }
                         else
                             document.html = levelFrom[(int)listIndex] = Constants.DOCUMENT_NOT_FOUND_CONTENT;
                     }
                     catch (Exception ex)
                     {
                         Report.error(documentUrl + " could not be processed! | Reason : " + ex.ToString());
                         document.weight = -404;
                     }
                     document.save();
                     Report.reportCrawled();
                 }, i);
                TaskList.Add(task);
            }

            Task levelTasks = Task.WhenAll(TaskList);
            try
            {
                Report.info(String.Format("{0} | {1} {2} running for {3} level.",
                    DomainCache.activeDomain.name,
                    levelFrom.Count,
                    levelFrom.Count > 1 ? "tasks are" : "task is",
                    GlobalHelper.levelStr(crawlingLevel)));
                levelTasks.Wait();
            }
            catch (Exception ex)
            {
                Report.error(DomainCache.activeDomain.name + " | Level task are not completed...", ex);
            }

            if (levelTasks.Status == TaskStatus.RanToCompletion)
            {

                if (Level.Third == crawlingLevel)
                {

                    foreach (var lvl in DomainCache.firstLevelUrls)
                    {
                        if (lvl.Equals(Constants.DOCUMENT_NOT_FOUND_CONTENT) || lvl.Equals(Constants.DUPLICATE_DOCUMENT_CONTENT))
                            continue;

                        if (!DomainCache.documentsAndWeights.ContainsKey(lvl))
                        {
                            Console.WriteLine("Missing - > " + lvl);
                        }
                    }
                    foreach (var lvl in DomainCache.secondLevelUrls)
                    {
                        if (lvl.Equals(Constants.DOCUMENT_NOT_FOUND_CONTENT) || lvl.Equals(Constants.DUPLICATE_DOCUMENT_CONTENT))
                            continue;
                        if (!DomainCache.documentsAndWeights.ContainsKey(lvl))
                        {
                            Console.WriteLine("Missing - > " + lvl);
                        }
                    }
                    foreach (var lvl in DomainCache.thirdLevelUrls)
                    {
                        if (lvl.Equals(Constants.DOCUMENT_NOT_FOUND_CONTENT) || lvl.Equals(Constants.DUPLICATE_DOCUMENT_CONTENT))
                            continue;
                        if (!DomainCache.documentsAndWeights.ContainsKey(lvl))
                        {
                            Console.WriteLine("Missing - > " + lvl);
                        }
                    }

                    Console.WriteLine("again");
                    Report.reportCrawled();
                    Console.WriteLine("again");
                }
                Report.success(DomainCache.activeDomain.name + " | Level task are completed...");
                this.calculateAlbVal(crawlingLevel, albValAcc, documentCount);
            }
            else
                Report.error(DomainCache.activeDomain.name + " | Error happened running tasks!");
        }

        /// <summary>
        /// calculates albanian value from a given parameters
        /// set LANG_DECIMAL_PLACES in Configs.cs file based on how many decimals you need
        /// </summary>
        /// <param name="level">in which level is calculated</param>
        /// <param name="albVal">cumulative Albanian value</param>
        /// <param name="pagesFound">total pages in level</param>
        /// <returns></returns>
        private void calculateAlbVal(Level level, double albVal, double pagesFound)
        {
            if (level == Level.Index)
                this.indexPageAlbVal = Math.Round(albVal, Configs.LANG_DECIMAL_PLACES);
            else if (level == Level.First)
            {
                this.firstLevelAlbVal = Math.Round((albVal / pagesFound), Configs.LANG_DECIMAL_PLACES);
                this.calculateFinalAlbVal();
            }
        }

        /// <summary>
        /// calculates the Albanian value of a domain - based on our defined formula
        /// </summary>
        private void calculateFinalAlbVal()
        {
            this.finalAlbVal = (Configs.INDEX_WEIGHT * this.indexPageAlbVal) + (Configs.LEVEL1_WEIGHT * this.firstLevelAlbVal);
        }

        public bool checkIfIsMultiLingual()
        {
            bool isMultiLingual = false;
            var likelyAlbanian = DomainCache.likelyAlbanian.OrderBy(i => i.Key.Length);
            for (int r = 0; r < likelyAlbanian.Count(); r++)
            {
                double albVal = likelyAlbanian.ElementAt(r).Value;
                if (albVal < Configs.ALB_THRESHOLD)
                {
                    string url = likelyAlbanian.ElementAt(r).Key;//?
                    double anchorAlbVal = TextOperations.getAnchorsAlbRatio(InternetOperations.downloadDocument(url));

                    if (double.IsNaN(anchorAlbVal) || anchorAlbVal == -1.0)
                        continue;

                    if (anchorAlbVal < Configs.ALB_THRESHOLD)
                    {
                        string anchHtml = LanguageHelper.createAnchHtml(url);
                        string path = FileOperations.getFilePath(Paths.LIKELY_MULTILANG);
                        if (!File.Exists(path)) // then to create
                            anchHtml = LanguageHelper.createHtml(anchHtml, "Documents which may be multilingual");
                        FileOperations.writeToFile(path, anchHtml);
                        Report.info("This domain may be multilingual.", ConsoleColor.Green);
                        isMultiLingual = true;
                        break;
                    }
                }
            }

            if (!isMultiLingual)
                Report.info("This domain is monolingual.", ConsoleColor.Green);

            return isMultiLingual;
        }

        public bool checkAlbAsAlternative()
        {
            bool hasAlbAsAlternative = false;
            var likelyAlbOrdered = DomainCache.likelyAlbanian.OrderBy(i => i.Key.Length).ToList();

            foreach (var likelyAlb in likelyAlbOrdered)
            {
                string documentUrl = likelyAlb.Key;
                double albVal = likelyAlb.Value;

                if (albVal >= Configs.ALB_THRESHOLD)
                {
                    double langResult = TextOperations.getAnchorsAlbRatio(InternetOperations.downloadDocument(documentUrl));
                    if (langResult == -1.0 || double.IsNaN(langResult))
                        continue;

                    if (langResult >= Configs.ALB_THRESHOLD)
                    {
                        Report.success(documentUrl + " supposedly is written in Albanian!");
                        string path = FileOperations.getFilePath(Paths.LIKELY_ALB);
                        string anchHtml = LanguageHelper.createAnchHtml(documentUrl);
                        if (!File.Exists(path))
                            anchHtml = LanguageHelper.createHtml(anchHtml, "Documents which may be written in Albanian");
                        FileOperations.writeToFile(path, anchHtml);
                        hasAlbAsAlternative = true;
                        break;
                    }
                }
            }

            return hasAlbAsAlternative;
        }

        #region Clean
        /// <summary>
        /// cleans domain cache after the domain crawl is finished
        /// </summary>
        public void cleanCache()
        {
            Report.info(String.Format("{0} crawl was completed...Removing cached files for domain...", DomainCache.activeDomain.name));
            this.CleanLikelyAlbanian();
            DomainCache.zeroLevel.Clear();
            DomainCache.firstLevelUrls.Clear();
            DomainCache.secondLevelUrls.Clear();
            DomainCache.thirdLevelUrls.Clear();
            DomainCache.documentsAndWeights.Clear();
            DomainCache.likelyAlbanian.Clear();
            DomainCache.successfullyProcessed = 0;
            FileOperations.cleanDir(FileOperations.getFilePath(Paths.TEMP));
        }

        /// <summary>
        /// resets domain after the index page is invalid 
        /// </summary>
        public void cleanWhenIndexPageIsInvalid()
        {
            this.CleanLikelyAlbanian();
            this.qualityCalculator.setInitialValues();
            DomainCache.firstLevelUrls.Clear();
            DomainCache.documentsAndWeights.Clear();
            DomainCache.zeroLevel.Add(this.name);
        }

        /// <summary>
        /// cleans the li
        /// </summary>
        private void CleanLikelyAlbanian()
        {
            DomainCache.likelyAlbanian.Clear();
        }
        #endregion

        #region Finally
        /// <summary>
        /// evaluates the size and the quality of the domain
        /// </summary>
        private void setFinals()
        {
            levelCountInfo = new LevelInfo(this.isAlb);
            if (this.isAlb)
            {
                double[] levels = { (int)Level.First, (int)Level.Second, (int)Level.Third };
                double[] yvals = { levelCountInfo.FirstLevel, levelCountInfo.SecondLevel, levelCountInfo.ThirdLevel };
                this.size = RegressionCalculator.getDomainSize(levels, yvals);
                this.quality = this.qualityCalculator.getDomainQuality();
            }
        }

        /// <summary>
        /// saves the domain score
        /// </summary>
        public void save()
        {
            this.setFinals();
            if (this.isAlb)
            {
                this.saveAlb();
                FileOperations.updateDomainsScoreOnSeed(this);
            }
            else
            {
                this.saveNoNAlb();
                FileOperations.removeDomainFromSeed();
            }
        }

        /// <summary>
        /// saves albainan domains (detailed) in a file and informations
        /// </summary>
        private void saveAlb()
        {
            Report.success("  1st level documents: " + this.levelCountInfo.FirstLevel
                    + "   |   2nd level documents: " + this.levelCountInfo.SecondLevel
                    + "   |   3rd level documents: " + this.levelCountInfo.ThirdLevel
                    + "   |   Total documents: " + this.levelCountInfo.getTotalDocuments()
                    + "   |   Domain size: = " + this.size
                    + "   |   Domain is ALBANIAN");

            string path = FileOperations.getFilePath(Paths.ALB_RESULTS);
            if (!File.Exists(path))
            {
                List<string> head = new List<string>()
                {
                    "Url in list",
                    "Url processed",
                    "First Level document count",
                    "Second Level document count",
                    "Third Level document count",
                    "Total document count",
                    "Domain Size",
                    "Domain Quality",
                    "First Level Albanian ratio",
                    "Domain Albanian ratio"
                };
                FileOperations.writeToFile(path, String.Join(";", head.ToArray()));
            }

            List<string> toWrite_ = new List<string>()
            {
                this.listName,
                this.name,
                this.levelCountInfo.FirstLevel.ToString(),
                this.levelCountInfo.SecondLevel.ToString(),
                this.levelCountInfo.ThirdLevel.ToString(),
                this.levelCountInfo.getTotalDocuments().ToString(),
                this.getSize().ToString(),
                this.getQuality().ToString(),
                this.indexPageAlbVal.ToString(),
                this.firstLevelAlbVal.ToString(),
                this.finalAlbVal.ToString()

            };
            FileOperations.writeToFile(path, String.Join(";", toWrite_.ToArray()));
        }

        private void saveNoNAlb()
        {
            Report.success( this.levelCountInfo.FirstLevel
                + "     " + this.indexPageAlbVal
                + "     " + this.firstLevelAlbVal
                + "     SLOPE = " + this.size + " NONALBANIAN");


            string path = FileOperations.getFilePath(Paths.NON_ALBRESULTS);
            if (!File.Exists(path))
            {
                List<string> head = new List<string>()
                {
                    "FirstLevel Page Count",
                    "IndexPage",
                    "First Level Alb Val",
                    "ListName",
                    
                };
                FileOperations.writeToFile(path, String.Join(";", head.ToArray()));
            }

            List<string> toWrite = new List<string>() 
            { 
                this.levelCountInfo.FirstLevel.ToString(),
                this.indexPageAlbVal.ToString(),
                this.firstLevelAlbVal.ToString(),
                this.listName
            };
            FileOperations.writeToFile(path, String.Join(";", toWrite.ToArray()));
        }
        #endregion
    }
}
