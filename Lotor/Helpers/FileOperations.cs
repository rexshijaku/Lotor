using Lotor.Caches;
using Lotor.Globals;
using Lotor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Helpers
{
    class FileOperations
    {
        /// <summary>
        /// creates full location of a given file
        /// </summary>
        /// <param name="file">file name with extension</param>
        /// <returns>full location of a given file</returns>
        public static string getFilePath(string file)
        {
            return Paths.MAIN_DIR + file;
        }

        /// <summary>
        /// writes to any path (file) any given content
        /// </summary>
        /// <param name="path">path to write</param>
        /// <param name="content">content to write</param>
        public static void writeToFile(string path, string content)
        {
            StreamWriter strw = new StreamWriter(path, true);
            strw.WriteLine(content);
            strw.Close();
        }

        /// <summary>
        /// saves document html in order to reuse (if needed later)
        /// </summary>
        /// <param name="doc"></param>
        public static void saveDocumentTemporary(Document doc)
        {
            string pathHash = doc.url.GetHashCode().ToString();
            writeToFile(getFilePath(Paths.TEMP + pathHash + ".txt"), doc.html);
        }

        /// <summary>
        /// reads the html stored in a file (temp cache)
        /// used to prevent redownload of the same document
        /// </summary>
        /// <param name="path">source from where to read</param>
        /// <returns>html content of document</returns>
        public static string getTemporaryDocumentContent(string path)
        {
            string pathHash = path.GetHashCode().ToString();
            return File.ReadAllText(getFilePath(Paths.TEMP + pathHash + ".txt"));
        }

        /// <summary>saves urls found in a domain
        /// for diagnostic purposes
        /// </summary>
        public static void saveUrl(string content, string lang)
        {
            string path = FileOperations.getFilePath(Paths.HTML_DOCS) + lang + DomainCache.activeDomain.getHost().Replace(".", "") + ".txt";
            writeToFile(path, content);
        }

        /// <summary>when a domain is not Albanian it is removed from the seed
        /// invoked for non albanian domains
        /// invoked after domaincrawl
        /// </summary>
        public static void removeDomainFromSeed()
        {
            string tempFile = Path.GetTempFileName();
            string seedPath = FileOperations.getFilePath(Paths.URL_LIST);
            using (var sr = new StreamReader(seedPath))
            using (var sw = new StreamWriter(tempFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                    if (line != DomainCache.activeDomain.listName)
                        sw.WriteLine(line);
            }
            File.Delete(seedPath);
            File.Move(tempFile, seedPath);
        }

        /// <summary>when crawled domain is Albanian, its score is updated in seed
        /// invoked for every Albanian domain 
        /// invoked after domaincrawl
        /// </summary>
        public static void updateDomainsScoreOnSeed(Domain domain)
        {
            Dictionary<string, double?> seed = new Dictionary<string, double?>();
            string[] lines = File.ReadAllLines(FileOperations.getFilePath(Paths.URL_LIST));
            //read all list from file
            foreach (string line in lines)
            {
                string domainUrl = line;
                double? domainScore;
                if (line.Contains(Constants.DOMAIN_SEPARATOR))
                {
                    string[] domainListName = line.Split(Constants.DOMAIN_SEPARATOR);
                    domainUrl = domainListName[0];
                    if (domainListName.Length > 0 && !String.IsNullOrEmpty(domainListName[1]))
                        domainScore = Convert.ToDouble(domainListName[1]);
                    else
                        domainScore = null;
                }
                else
                    domainScore = null;

                if (!seed.ContainsKey(domainUrl))
                    seed.Add(domainUrl.Trim().ToLower(), domainScore);
            }

            //add domain to list or update it's score
            if (seed.ContainsKey(domain.listName))
                seed[domain.listName] = domain.getScore();
            else
                seed.Add(domain.listName, domain.getScore());

            //order domains by score
            var orderedDomains = seed.OrderByDescending(pair => pair.Value).ToDictionary(k => k.Key, v => v.Value);

            //write ordered domains to list
            string tempFile = Path.GetTempFileName(); // write to temp file first
            string frontierPath = FileOperations.getFilePath(Paths.URL_LIST);
            StreamWriter sw = new StreamWriter(tempFile);
            foreach (var orderedDomain in orderedDomains)
            {
                if (orderedDomain.Value == null)
                    sw.WriteLine(orderedDomain.Key);
                else
                    sw.WriteLine(orderedDomain.Key + Constants.DOMAIN_SEPARATOR + orderedDomain.Value.ToString());
            }
            sw.Close();
            File.Delete(frontierPath);
            File.Move(tempFile, frontierPath); // move temp file as a new frontier list
        }

        /// <summary>cleans all temporary files created when a domain was downloading
        /// it is called in every iteration (for each domain)
        /// </summary>
        public static void cleanDir(string path)
        {
            try
            { 
                Report.info("Removing files from " + path + "...");
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Report.error("Something went wrong while removing files from " + path + "!", ex);
            }
        }

        /// <summary>checks whether the necessary directories are ready or not
        /// if a particular directory does not exists it creates that (if possible)
        /// if a problem happens (e.g has no permission to write), then the crawling process can not start
        /// </summary>
        public static bool checkDirs()
        {
            try
            {
                Report.info("Checking the input directories...");
                List<string> dirsToCheck = new List<string>() { Paths.TEMP, Paths.HTML_DOCS, Paths.RESULTS_DIR };
                foreach (var dirToCheck in dirsToCheck)
                {
                    string dir = FileOperations.getFilePath(dirToCheck);
                    if (!Directory.Exists(dir))
                    {
                        Report.info(dirToCheck + " does not exists!");
                        Directory.CreateDirectory(dir);
                        Report.info(dirToCheck + " was created...");
                    }
                    else
                    {
                        if (dirToCheck.Equals(Paths.RESULTS_DIR) && !Configs.CLEAN_RESULTS_ON_EACH_RUN)
                            continue;
                    }
                    FileOperations.cleanDir(dir);
                }
            }
            catch (Exception ex)
            {
                Report.error("The crawling process could not start due to the following exception: " + ex.ToString(),ex, true);
                return false;
            }
            Report.info("Checking the input directories was completed.");
            return true;
        }
    }
}
