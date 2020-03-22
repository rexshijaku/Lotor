using Lotor.Caches;
using Lotor.Globals;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Lotor.Helpers
{
    class InternetOperations
    {
        public static void init()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                    | SecurityProtocolType.Tls11
                                    | SecurityProtocolType.Tls12;

            ServicePointManager.Expect100Continue = false;
        }
        /// <summary>
        /// creates a web request to download the document of a given url
        /// </summary>
        /// <param name="documentUrl">the url of a document for which request is being created</param>
        /// <returns>webrequest</returns>
        private static HttpWebRequest createWebRequest(string documentUrl)
        {
           
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(documentUrl);
            webRequest.UserAgent = "*";
            webRequest.CookieContainer = new CookieContainer();
            webRequest.Timeout = Configs.REQUEST_TIMEOUT;
            webRequest.ProtocolVersion = HttpVersion.Version10;
            webRequest.AllowAutoRedirect = true;
            webRequest.KeepAlive = false;
            webRequest.AllowWriteStreamBuffering = false;
            return webRequest;
        }

        /// <summary>
        /// checks whether the internet is available or not
        /// </summary>
        /// <returns>
        /// boolean
        /// true when internet is available
        /// false when internet is not available</returns>
        private static bool hasInternet()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead(Configs.CHECK_INTERNET_LINK))
                    return true;
            }
            catch (Exception ex)
            {
                Report.error("Error while connecting to internet...", ex);
                return false;
            }
        }

        /// <summary>
        /// tries to connect to internet
        /// invoked when internet is not available
        /// </summary>
        private static void checkInternetConnection()
        {
            Report.error("Internet connection was lost...");
            do
            {
                Report.info("Waiting to connect to the internet to crawl " + DomainCache.activeDomain.listName + "...", ConsoleColor.Gray);
                Thread.Sleep(Configs.DELAY_BETWEEN_INTERNET_RECONNECTION_TRIES);
            }
            while (!hasInternet());
        }


        /// <summary>
        /// tries to download a given document for a number of trials specified in Configs.MAX_TRIALS
        /// </summary>
        /// <param name="url">the url of the document</param>
        /// <returns>
        /// documentHtml - which is html of a downloaded document, and is returned when download is sucessful
        /// NOTFOUNDCONTENT - which is a constant returned when download is not successful
        /// </returns>
        public static string downloadDocument(string url)
        {
            for (int i = 0; i < Configs.MAX_TRIES; i++)
            {
                string documentHtml = tryToDownload(url);
                if (!documentHtml.Equals(Constants.DOCUMENT_NOT_FOUND_CONTENT)) // if everything was ok, return html content of that page
                    return documentHtml;
                Thread.Sleep(Configs.DELAY_BETWEEN_TRIES);
            }
            return Constants.DOCUMENT_NOT_FOUND_CONTENT;
        }

        /// <summary>
        /// Downloads a web document for given url
        /// </summary>
        /// <param name="url">the url of the document that is intended to download</param>
        /// <returns>
        /// html - html of the document (when request completed successfully
        /// NOTFOUNDCONTENT - when there is a problem downloading the given web document
        /// </returns>
        private static string tryToDownload(string url)
        {
            string html = String.Empty;
            bool requestCompleted = false;
            while (!requestCompleted)
            {
                try
                {
                    Report.info(url + " | Creating request to download...");
                    HttpWebRequest webRequest = createWebRequest(url);
                    HttpWebResponse response = null;
                    HttpStatusCode statusCode = HttpStatusCode.Created;
                    try
                    {
                        response = (HttpWebResponse)webRequest.GetResponse();
                        if (response != null && !(response.Headers["content-type"]).Contains("html"))
                            Report.error(url + " | Could not downloaded this document! | Reason: Invalid format. [" + response.Headers["content-type"] + "]");
                        statusCode = response.StatusCode;
                        requestCompleted = true;
                    }
                    catch (WebException ex)
                    {
                        if (!hasInternet()) // keep request alive if disconnected from internet
                        {
                            checkInternetConnection(); // this will be terminated after it re-connects to internet
                            continue;
                        }
                        else // otherwise we can skip request (count as done) 
                        {
                            statusCode = HttpStatusCode.NotFound;
                            requestCompleted = true;
                        }
                    }

                    if (statusCode == HttpStatusCode.NotFound)
                    {
                        Report.error(url + " | Error on getting response! | Reason: Document was not found or Wrong Url!");
                        return Constants.DOCUMENT_NOT_FOUND_CONTENT;
                    }
                    else if (statusCode == HttpStatusCode.BadRequest)
                    {
                        Report.error(url + " | Error on getting response! | Reason: Not identified!");
                        return Constants.DOCUMENT_NOT_FOUND_CONTENT;
                    }
                    else
                    {
                        Report.success(url + " | Document was downloaded successfully.");
                        Stream dataStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        reader.BaseStream.ReadTimeout = Configs.REQUEST_TIMEOUT;
                        html = reader.ReadToEnd();
                        if (url.Equals(DomainCache.activeDomain.name)) // if the downloaded page is the index page of domain
                        {
                            string urlInMetaData = getMetaDataUrl(html); // then check if it redirects to some other domain
                            if (!String.IsNullOrEmpty(urlInMetaData))
                            {
                                DomainCache.activeDomain.name = urlInMetaData;
                                html = tryToDownload(urlInMetaData);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Report.error(url + " | Error on getting response! | Reason: Document was not found or Wrong Url!", ex);
                    return Constants.DOCUMENT_NOT_FOUND_CONTENT;
                }
            }
            return html;
        }

        /// <summary>
        /// can be improve
        /// </summary>
        /// <param name="documentHtml"></param>
        /// <returns></returns>
        private static string getMetaDataUrl(string documentHtml)
        {
            if (!String.IsNullOrEmpty(documentHtml))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(documentHtml);

                HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//meta");
                if (collection != null)
                {
                    foreach (HtmlNode link in collection)
                    {
                        if (link.Attributes["content"] != null)
                        {
                            string content = link.Attributes["content"].Value;
                            if (content.Contains("url="))
                                return content.Split('=')[1];
                            else
                                return String.Empty;
                        }
                    }
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// tries to get an original url of a given url, if one exist
        /// </summary>
        /// <returns>domain name either modified or not</returns>
        public static string geDomainsUrl()
        {
            bool REQUEST_COMPLETED = false;
            do
            {
                try
                {
                    Report.info(DomainCache.activeDomain.name + " | Trying to get the full url...");
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(DomainCache.activeDomain.name);
                    request.Method = "HEAD";
                    request.AllowAutoRedirect = false;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.MovedPermanently) // handle these status codes
                    {
                        string responseUrl = response.GetResponseHeader("Location");
                        Report.info(DomainCache.activeDomain.name + " | Moved Permanently or Redirected to " + responseUrl);
                        if (responseUrl.StartsWith("http"))
                            DomainCache.activeDomain.name = responseUrl;
                        else
                        {
                            if (!responseUrl.StartsWith("/"))
                                DomainCache.activeDomain.name += "/";
                            DomainCache.activeDomain.name += responseUrl;
                        }
                    }
                    REQUEST_COMPLETED = true;
                }
                catch (WebException ex)
                {
                    Report.error(DomainCache.activeDomain.name + " | Request was not successful... Not found or Wrong Url!", ex);
                    if (!hasInternet())
                    {
                        checkInternetConnection(); // keep checking internet connection until it reconnects
                        continue; // do another request
                    }
                    else
                    {
                        // if internet was not problem of exception then, consider as it done
                        Report.info(DomainCache.activeDomain.name + " | Url was not modified!", ConsoleColor.Gray);
                        REQUEST_COMPLETED = true;
                    }
                }
            }
            while (!REQUEST_COMPLETED);
            return DomainCache.activeDomain.name;
        }
    }
}
