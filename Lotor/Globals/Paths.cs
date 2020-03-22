using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Globals
{
    /// <summary>
    /// stores file locations
    /// </summary>
    class Paths
    {
        /// <summary>
        /// the main directory where all other folders reside
        /// </summary>
        public static readonly string MAIN_DIR = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;

        #region Inputs
        /// <summary>
        /// url list that contains all domains to be crawled
        /// </summary>
        public static readonly string URL_LIST = @"\lotor_input\seed.txt";

        /// <summary>
        /// Albanian words which are composed by maximum by four characters
        /// </summary>
        public static readonly string ALB_WORDS = @"\lotor_input\albTerms4.txt";

        /// <summary>
        /// list of Albanian stop words
        /// </summary>
        public static readonly string ALB_STOPWORDS = @"\lotor_input\al_stopwords.txt";

        /// <summary>
        /// excluded extensions e.g .pdf or .ppt
        /// </summary>
        public static readonly string EXCLUDED_EXTENTIONS = @"\lotor_input\excluded_extentions.txt"; 
        #endregion

        #region Outputs
        /// <summary>
        /// in this folder are written temporary documents of a domain that is being crawled
        /// </summary>
        public static readonly string TEMP = @"\lotor_output\temp\";

        /// <summary>
        /// html documents saved for diagnostic purposes
        /// </summary>
        public static readonly string HTML_DOCS = @"\lotor_output\htmlpages\";

        /// <summary>
        /// in this folder are stored results 
        /// such as: list of Albanian and non albanina domains, 
        /// list of  duplicted documents, 
        /// list of domains that are likely Albanian 
        /// and list of domains which may consist of another language alongside with Albanian 
        /// </summary>
        public static readonly string RESULTS_DIR = @"\lotor_output\results\";

        /// <summary>
        /// file in which are stored duplicate documents detected by crawler
        /// </summary>
        public static readonly string DUPLICATES = @"\lotor_output\results\duplicated.txt";

        /// <summary>
        /// list of domains which are not Albanian but may contain Albanian
        /// </summary>
        public static readonly string LIKELY_ALB = @"\lotor_output\results\likely_albanian_domains.html";

        /// <summary>
        /// list of Albanian domains
        /// </summary>
        public static readonly string ALB_RESULTS = @"\lotor_output\results\albanian_domains.csv";

        /// <summary>
        /// list of non Albanian domains
        /// </summary>
        public static readonly string NON_ALBRESULTS = @"\lotor_output\results\nonalbanian_domains.csv";

        /// <summary>
        /// list of domains that are Albanian and may include another non Albanian language as alternative
        /// </summary>
        public static readonly string LIKELY_MULTILANG = @"\lotor_output\results\likely_multilingual.htm";


        
        #endregion
    }
}
