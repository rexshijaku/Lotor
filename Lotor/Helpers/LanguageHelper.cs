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
    public class LanguageHelper
    {
        /// <summary>
        /// for diagnostic purposes we create html
        /// </summary>
        /// <param name="pageUrl">url for which html element is creating</param>
        /// <returns>html anchor element for a page</returns>
        public static string createAnchHtml(string pagetUrl)
        {
            return "<a target='_blank' href='" + DomainCache.activeDomain.name + "'>'"
                            + DomainCache.activeDomain.name + "'</a> in <a  target='_blank' href='"
                            + pagetUrl + "'>'" + pagetUrl + "'</a> <br>";
        }

        /// <summary>
        /// for diagnostic purposes we create html
        /// </summary>
        /// <param name="anchorsHtml"></param>
        /// <param name="message"></param>
        /// <returns>html document</returns>
        public static string createHtml(string anchorsHtml, string message)
        {
            return "<html><head><title>" + message + "</title></head><body>" + anchorsHtml + "</body></html>";
        }
    }
}
