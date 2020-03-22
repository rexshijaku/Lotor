using System;

namespace Lotor.Calculations
{
    class QualityFeatures
    {
        //public string url;

        /// <summary>
        /// number of visible terms on the document (as rendered by a web browser)
        /// </summary>
        public double numVisTerms;

        /// <summary>
        /// number of terms in the document <title>
        /// </summary>
        public double numTitleTerms;

        /// <summary>
        /// average length (number of characters) of visible terms on the document.
        /// </summary>
        public double avgTermLen;

        /// <summary>
        /// fraction of anchor text on the document.
        /// </summary>
        public double ratioAnchorText;

        /// <summary>
        /// fraction of visible text on the document compared to the full source 
        /// </summary>
        public double ratioVisText;

        /// <summary>
        /// the entropy of the document content.
        /// </summary>
        public double entropy;

        /// <summary>
        /// stopword/non-stopword ratio of the document   
        /// </summary>
        public double ratioStops;

        /// <summary>
        /// fraction of terms in the stopword list that appear on the document.
        /// </summary>
        public double stopCover;

        /// <summary>
        /// the depth of the URL path (number of back-slashes in the URL).
        /// </summary>
        public double urlDepth;

        public double urlLength;

        /// <summary>
        /// fraction of table text on the document.
        /// </summary>
        public double ratioTableText;

        public double rankV;
    }
}
