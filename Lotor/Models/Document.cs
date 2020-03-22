using Lotor.Caches;
using Lotor.Globals;
using Lotor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Models
{
    /// <summary>
    /// Document or a webpage
    /// </summary>
    public class Document
    {
        /// <summary>
        /// html of a downloaded document
        /// </summary>
        public string html { get; set; }

        /// <summary>
        /// text of a document, without html tags
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// url of the document
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// keeps the information whether the document is duplicate or not
        /// </summary>
        private bool duplicate { get; set; }

        /// <summary>
        /// keeps the information whether the document is valid or not
        /// </summary>
        private bool validity { get; set; }

        /// <summary>
        /// keeps the information whether the document has unordered list or not
        /// </summary>
        private bool hasUnorderedList_ { get; set; }

        public long weight { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="url">the raw url of a document</param>
        public Document(string url)
        {
            this.url = url;
            this.html = InternetOperations.downloadDocument(this.url);
            this.text = TextOperations.getDocumentsText(this.html, this.url);
            this.hasUnorderedList_ = this.html.Contains("<ul"); // important for language processing
        }

        /// <summary>
        /// calculates Albanian value of the document
        /// </summary>
        /// <returns>Albanian value</returns>
        public double getAlbVal()
        {
            Report.info(this.url + " calculating Albanian value...");
            return TextOperations.getAlbValue(this.text);
        }

        /// <summary>
        /// checks whether the document was found or not
        /// </summary>
        /// <returns>true if it is found / false otherwise</returns>
        public bool isFound()
        {
            bool isFound = !this.html.Equals(Constants.DOCUMENT_NOT_FOUND_CONTENT);
            if (isFound)
                Report.success(this.url + " exists!");
            else
            {
                this.weight = -404;
                Report.error(this.url + " was not found!");
            }
            return isFound;
        }

        /// <summary>
        /// checks whether the document is valid
        /// </summary>
        /// <returns>true if it is valid / false otherwise</returns>
        public bool isValid()
        {
            if (this.validity)
                Report.success(this.url + " is a valid document.");
            else
                Report.error(this.url + " is not a valid document!");
            return this.validity;
        }

        /// <summary>
        /// checks whether the document is duplicate
        /// </summary>
        /// <returns>true if it is duplicate / false otherwise</returns>
        public bool isDuplicate()
        {
            return this.duplicate;
        }

        /// <summary>
        /// checks whether the document contains unordered list html tag
        /// </summary>
        /// <returns>true if it contains / false otherwise</returns>
        public bool hasUnorderedList()
        {
            return this.hasUnorderedList_;
        }

        /// <summary>
        /// saves documents to file (temporary) and to cache
        /// </summary>
        /// <param name="documentWeight">to store in cache</param>
        public void save()
        {
            if (this.weight == -1)
                this.html = "-1";

            Report.success(this.url + " is being saved...");
            DomainCache.successfullyProcessed++;
            DomainCache.documentsAndWeights[this.url] = this.weight;
            FileOperations.saveDocumentTemporary(this);
        }

        /// <summary>
        /// processes the document 
        /// calculates its weight
        /// checks its validity
        /// checks for uniqueness
        /// lastly saves the document
        /// </summary>
        /// <param name="level">level is needed in order to set its weight to -1</param>
        public void process(List<string> level,int index)
        {
            Report.info(this.url + " is processing...");
            this.validity = true;
            if (!this.html.Contains("head") && !this.url.Equals(DomainCache.activeDomain))
            {
                this.validity = false;
                return;
            }

            Report.info(this.url + " checking for duplicates...");
            this.duplicate = false;
            long documentWeight = this.getWeight();
            if (DomainCache.documentsAndWeights.ContainsValue(documentWeight))
            {
                var similar = DomainCache.documentsAndWeights.Reverse().Where(p => p.Value == documentWeight).Select(p => p.Key).ToList();
                Report.info(similar.Count + " similar documents were found. Weight: " + documentWeight, ConsoleColor.Yellow);
                int totalCompared = 0;
                foreach (var similarDocumentsUrl in similar)
                {
                    Report.info(this.url + " is comparing with " + similarDocumentsUrl, ConsoleColor.Yellow);
                    if (totalCompared == Configs.MAX_DUPLICATE_COMPARISONS)
                        break;
                    else
                    {
                        if (GlobalHelper.sameContent(this, similarDocumentsUrl))
                        {
                            Report.error(this.url + " is duplicate of " + similarDocumentsUrl + "!");
                            this.duplicate = true;
                            this.weight = -1;
                            level[index] = Constants.DUPLICATE_DOCUMENT_CONTENT;
                            FileOperations.writeToFile(FileOperations.getFilePath(Paths.DUPLICATES), this.url + Constants.DOMAIN_SEPARATOR + similarDocumentsUrl);
                            break;
                        }
                        else
                            totalCompared++;
                    }
                }
            }
            else
                Report.info(this.url + " | No duplicates found!");
        }

        /// <summary>
        /// calculates the weight of document
        /// </summary>
        /// <returns>weight</returns>
        private long getWeight()
        {
            int weight = 0;
            string[] words = this.text.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (MainCache.MostCommonAlbanianWords.ContainsKey(word))
                    weight += MainCache.MostCommonAlbanianWords[word];
            }
            this.weight = weight;
            return weight;
        }
    }
}
