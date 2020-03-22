using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Globals
{
    class Configs
    {
        /// <summary>
        /// the url which helps to check the availability of internet
        /// </summary>
        public const string CHECK_INTERNET_LINK = "http://google.com/generate_204";

        /// <summary>
        /// at the end of crawling close console or keep it open
        /// </summary>
        public const bool CLOSE_CONSOLE = false;

        /// <summary>
        /// on each run cleans results created in previous crawl
        /// </summary>
        public const bool CLEAN_RESULTS_ON_EACH_RUN = false;

        #region Time
        /// <summary>
        /// web request read timeout 
        /// in miliseconds
        /// </summary>
        public const int READ_TIMEOUT = 1500;

        /// <summary>
        /// web request response time
        /// in miliseconds
        /// </summary>
        public const int REQUEST_TIMEOUT = 60000;

        /// <summary>
        /// the delay between 
        /// re-download tries
        /// in miliseconds
        /// </summary>
        public const int DELAY_BETWEEN_TRIES = 1500;

        /// <summary>
        /// internet reconnect delay
        /// in miliseconds
        /// </summary>
        public const int DELAY_BETWEEN_INTERNET_RECONNECTION_TRIES = 1500;
        #endregion

        #region Min
        /// <summary>
        /// minimum word length in order to process the word
        /// created model best words with minimum four letters
        /// </summary>
        public const int MIN_WORD_LENGTH = 4;

        /// <summary>
        /// in order to a page to be considered as index how many links it should contain?
        /// note that there are few pages which are fake indexes, and serve to redirect to original index page 
        /// from our empirical experience we determined it to be 2
        /// </summary>
        public const int MIN_INDEX_ANCHORS = 2;
        #endregion

        #region Max
        /// <summary>
        /// how many tries until the web page is downloaded
        /// </summary>
        public const int MAX_TRIES = 3;

        /// <summary>
        /// when similar pages found how many of them should be compared to current page until duplicate is found
        /// </summary>
        public const int MAX_DUPLICATE_COMPARISONS = 10;

        /// <summary>
        /// decimals in quality result
        /// </summary>
        public const int QUALITY_DECIMAL_PLACES = 3;

        /// <summary>
        /// decimals in lang result
        /// </summary>
        public const int LANG_DECIMAL_PLACES = 3;
        #endregion

        #region Weights
        /// <summary>
        /// how important is index page of a domain in order to determine its language?
        /// based on our work and experiments, we suggest index weight as 0.6
        /// </summary>
        public const double INDEX_WEIGHT = 0.6;

        /// <summary>
        /// how important is the first level  of a domain in order to determine its language?
        /// based on our work and experiments, we suggest first levels weight as 0.4
        /// </summary>
        public const double LEVEL1_WEIGHT = 0.4;

        /// <summary>
        /// when a domain is considered Albanian?
        /// 0.2 is the ideal value based on our experiments
        /// </summary>
        public const double ALB_THRESHOLD = 0.2;
        #endregion
    }
}
