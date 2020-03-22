using Lotor.Globals;
using Lotor.Helpers;
using Lotor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Caches
{
    /// <summary>
    /// Caches frequently used lists
    /// </summary>
    public class MainCache
    {
        /// <summary>
        /// UrlList contains urls that are being crawled (domain names) which are fetched from a given input file (Paths.URL_LIST)
        /// </summary>
        private static List<Domain> _UrlList = null;
        public static List<Domain> UrlList
        {
            get
            {
                if (_UrlList == null)
                {
                    Report.info(Paths.URL_LIST + Report.separator + "Reading the given list of domains...");
                    _UrlList = new List<Domain>();
                    try
                    {
                        string[] lines = File.ReadAllLines(FileOperations.getFilePath(Paths.URL_LIST));

                        if (lines.Length == 0)
                            Report.info("No url found to crawl!", ConsoleColor.Yellow, true);

                        for (int i = 0; i < lines.Length; i++)
                        {
                            string domainUrl = String.Empty;
                            if (lines[i].Contains(Constants.DOMAIN_SEPARATOR))
                                domainUrl = lines[i].Split(Constants.DOMAIN_SEPARATOR)[0];
                            else
                                domainUrl = lines[i];

                            domainUrl = domainUrl.Trim().ToLower();

                            if (!domainUrl.Equals(String.Empty))
                                _UrlList.Add(new Domain(domainUrl));
                            else
                                Report.error(lines[i] + " was skipped!");
                        }
                    }
                    catch (FileNotFoundException ex)
                    {
                        Report.error(Paths.URL_LIST + Report.separator + "Url List file was not found! Please provide one!", ex, true);
                    }
                    catch (Exception ex)
                    {
                        Report.error(Paths.URL_LIST + Report.separator + "Something went wrong while reading the given Url List file! Make sure you have a url list and it is located properly!", ex, true);
                    }
                }
                return _UrlList;
            }
        }


        /// <summary>
        /// keeps the most common Albanian Words list, which plays crucial role in determining the Albanian value of a document
        /// </summary>
        private static Dictionary<string, int> _MostCommonAlbanianWords = null;
        public static Dictionary<string, int> MostCommonAlbanianWords
        {
            get
            {
                if (_MostCommonAlbanianWords == null)
                {
                    _MostCommonAlbanianWords = new Dictionary<string, int>();
                    try
                    {
                        string[] lines = File.ReadAllLines(FileOperations.getFilePath(Paths.ALB_WORDS));
                        for (int i = 0; i < lines.Length; i++)
                        {
                            string[] lineArray = lines[i].Split(Constants.WORD_SEPARATOR);
                            string xWord = lineArray[0].Trim().ToLower();
                            if (!String.IsNullOrEmpty(xWord) && !_MostCommonAlbanianWords.ContainsKey(xWord))
                            {
                                int wordWeight = Convert.ToInt32(lineArray[1]);
                                _MostCommonAlbanianWords.Add(xWord, wordWeight);
                            }
                        }
                        #region Info
                        if (_MostCommonAlbanianWords.Count > 0 && _MostCommonAlbanianWords.Count < 1000)
                        {
                            Report.info("The accuracy of the analyzer will not be as expected! ", ConsoleColor.Yellow);
                            Report.info("Contact us on rexhepshijaku@gmail.com to get access in our 30000 most common Albanian words on web and to all stop words as well!", ConsoleColor.Yellow);
                            Console.ReadKey();
                        }
                        #endregion
                    }
                    catch (FileNotFoundException ex)
                    {
                        Report.error(Paths.ALB_WORDS + Report.separator + "Most Common Albaninan file was not found! Please provide one!", ex, true);
                    }
                    catch (Exception ex)
                    {
                        Report.error(Paths.ALB_WORDS + Report.separator + "Something went wrong while reading the Most Common Albaninan Words file!", ex, true);
                    }
                }
                return _MostCommonAlbanianWords;
            }
        }

        /// <summary>
        /// ignore crawling documents which have these extensions
        /// you can specify correctly in EXCLUDED_EXTENTIONS
        /// </summary>
        private static List<string> _ExcludedFileFormats = null;
        public static List<string> ExcludedFileFormats
        {
            get
            {
                if (_ExcludedFileFormats == null)
                {
                    _ExcludedFileFormats = new List<string>();
                    try
                    {
                        string[] lines = File.ReadAllLines(FileOperations.getFilePath(Paths.EXCLUDED_EXTENTIONS));
                        for (int i = 0; i < lines.Length; i++)
                        {
                            string format = lines[i].Trim().ToLower();
                            _ExcludedFileFormats.Add(format);
                        }
                    }
                    catch (FileNotFoundException ex)
                    {
                        Report.error(Paths.EXCLUDED_EXTENTIONS + Report.separator + "Excluded Formats file was not found!", ex, true);
                    }
                    catch (Exception ex)
                    {
                        Report.error(Paths.EXCLUDED_EXTENTIONS + Report.separator + "Something went wrong while reading Excluded Formats file!", ex, true);
                    }
                }
                return _ExcludedFileFormats;
            }
        }

        /// <summary>
        /// specify chars to replace, e,g. in Albanian there are words like ë and ç which should be replaced by e and c respectively
        /// replacing such characters (normalizing) helps us to make more accurate operations on text
        /// </summary>
        private static Dictionary<char, char> _charsToReplace = null;
        public static Dictionary<char, char> charsToReplace
        {
            get
            {
                if (_charsToReplace == null)
                {
                    _charsToReplace = new Dictionary<char, char>();
                    _charsToReplace.Add('ë', 'e');
                    _charsToReplace.Add('ç', 'c');
                }
                return _charsToReplace;
            }
        }

        /// <summary>
        /// stop words which will be ignored by the quality estimator
        /// </summary>
        private static List<string> stopWords_ = null;
        public static List<string> stopWords
        {
            get
            {
                if (stopWords_ == null)
                {
                    try
                    {
                        stopWords_ = new List<string>();
                        string[] stopWordsRaw = File.ReadAllLines(FileOperations.getFilePath(Paths.ALB_STOPWORDS));
                        for (int i = 0; i < stopWordsRaw.Length; i++)
                        {
                            string word = stopWordsRaw[i].Trim().ToLower();
                            stopWords_.Add(word);
                        }
                    }
                    catch (FileNotFoundException ex)
                    {
                        Report.error(Paths.ALB_STOPWORDS + Report.separator + "Stop Words file was not found! It must be included!", ex, true);
                    }
                    catch (Exception ex)
                    {
                        Report.error(Paths.ALB_STOPWORDS + Report.separator + "Something went wrong while reading Stop Words file!", ex, true);
                    }
                }
                return stopWords_;
            }
        }

        public static List<string> invalidUrlStarts = new List<string>() { "mailto:", "javascript:", "skype:", "callto:" };

    }
}
