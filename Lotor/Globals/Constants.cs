using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Globals
{
    class Constants
    {
        /// <summary>
        /// change this if your separator is different
        /// separates the url of domain from its score
        /// </summary>
        public static readonly char DOMAIN_SEPARATOR = '\t';

        /// <summary>
        /// changes this if your word separator is different
        /// </summary>
        public static readonly char WORD_SEPARATOR = ' ';
        
        /// <summary>
        /// keep the code clean xD
        /// </summary>
        public static readonly string HTTP = "http";

        /// <summary>
        /// when a duplicate is found 
        /// its content is saved as the value of this variable
        /// </summary>
        public static readonly string DUPLICATE_DOCUMENT_CONTENT = "DUPLICATE";

        /// <summary>
        /// when a document is not found
        /// its content is saved as the value of this variable
        /// </summary>
        public static readonly string DOCUMENT_NOT_FOUND_CONTENT = "404";
    }
}
