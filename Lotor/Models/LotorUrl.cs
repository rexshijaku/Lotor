using Lotor.Caches;
using Lotor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Models
{
    public class LotorUrl
    {
        public string Url { get; set; }
        public string Domain { get; set; }
        public string CurrentUrl { get; set; }
        private string DomainHost { get; set; }
        public bool UrlIsValid { get; set; }
        public string Message { get; set; }

        public LotorUrl(string Url, string CurrentUrl)
        {
            this.Url = Url;
            this.Domain = DomainCache.activeDomain.name;
            this.CurrentUrl = CurrentUrl;
            this.DomainHost = (new Uri(this.Domain)).Host;
            Report.info("Grabbing " + this.Url + " from " + this.CurrentUrl, ConsoleColor.Yellow);
            this.processAndCombine();
        }

        //todo
        public void processAndCombine()
        {
            this.clean();

            if (!this.checkIsValidFormat())
            {
                this.UrlIsValid = false;
                this.Message = "Invalid format!";
                return;
            }

            if (!this.isValid())
            {
                this.UrlIsValid = false;
                this.Message = "Invalid url!";
                return;
            }

            if (this.sameHostNoHttp())
                this.setHttp();
            else
            {
                if (this.isRelative())
                    this.formatUrl();
                else
                {
                    if (!this.hasHttp())
                    {
                        //index.php
                        //~/Sayfa
                        if (this.hasDot())
                        {
                            if (this.someCond())
                                this.whenSomeCond(); //todo
                            else
                                this.findAndSet();
                        }
                        else
                            this.findAndSet();
                    }
                }
            }
            //if (this.isRelative())
            //     this.formatUrl();
            //else
            //{
            //    if (this.sameHostNoHttp())
            //        this.setHttp();

            //}

            try
            {
                if (this.isFomValidDomain())
                    this.UrlIsValid = true;
                else
                {
                    this.UrlIsValid = false;
                    this.Message = "It is not a part of the domain that is being craled!";
                }
            }
            catch (Exception ex)
            {
                this.UrlIsValid = false;
                this.Message = ex.ToString();
            }
        }

        #region Url variables
        private const string HTTP = "http://";
        private const string HTTPS = "https://";
        private const string ROOT_DIR_PREFIX = "/";
        private const string PARENT_DIR_PREFIX = "../";
        private const string CURRENT_DIR_PREFIX = "./";
        private const string QUERY_DIR_PREFIX = "?";
        private const string FRAGMENT_DIR_PREFIX = "#";
        #endregion

        #region Questions


        public bool checkIsValidFormat()
        {
            if (String.IsNullOrEmpty(this.Url))
                return false;

            foreach (var invalidStart in MainCache.invalidUrlStarts)
                if (this.Url.ToLower().StartsWith(invalidStart))
                    return false;

            foreach (var invalidEnd in MainCache.ExcludedFileFormats)
                if (this.Url.ToLower().EndsWith("." + invalidEnd))
                    return false;

            return true;
        }

        private bool hasDot()
        {
            return this.Url.Contains('.');
        }

        private bool isValid()
        {
            return !this.Domain.Equals(this.Url);
        }

        private bool sameHostNoHttp()
        {
            return this.Url.Contains(this.DomainHost) && (!this.Url.StartsWith(HTTP) && !this.Url.StartsWith(HTTPS));
        }

        public bool isRelative()
        {
            return this.inRootDir() || this.inCurrentDir() || this.inParentOfCurrentDir() || this.isQuery() || this.isFragment() || !this.Url.Contains("/");
        }

        public bool hasHttp()
        {
            return this.Url.StartsWith(HTTP) || this.Url.StartsWith(HTTPS);
        }

        public bool someCond()
        {
            return this.Url.Split('.').Length == 2 && !this.Url.Contains("/");
        }
        public bool isFomValidDomain()
        {
            try
            {
                Uri urlUri = new Uri(this.Url);

                if (!urlUri.Host.Contains('.'))
                    return false;

                string[] hostArray = urlUri.Host.Split('.');
                string name = hostArray[hostArray.Length - 2];
                string suffix = hostArray[hostArray.Length - 1];

                if (this.DomainHost.Contains(name + "." + suffix))
                    return true;
            }
            catch (Exception ex)
            {
                Report.error("Invalid uri for " + this.Url, ex);
            }
            return false;
        }
        public bool hasFragmentPrefix()
        {
            return this.Url.Contains(FRAGMENT_DIR_PREFIX);
        }
        #endregion

        #region Check if url found
        public bool inRootDir()
        {
            return this.Url.StartsWith(ROOT_DIR_PREFIX);
        }

        public bool inCurrentDir()
        {
            return this.Url.StartsWith(CURRENT_DIR_PREFIX);
        }

        public bool inParentOfCurrentDir()
        {
            return this.Url.StartsWith(PARENT_DIR_PREFIX);
        }

        public bool isQuery()
        {
            return this.Url.StartsWith(QUERY_DIR_PREFIX);
        }

        public bool isFragment()
        {
            return this.Url.StartsWith(FRAGMENT_DIR_PREFIX);
        }
        #endregion

        #region Format found url
        public void setWhenCurrentDir()
        {
            string bound = CURRENT_DIR_PREFIX.Remove(0, 1);
            this.Url = this.Url.Remove(0, 1);
            glueUrl(bound);
        }
       
        public void setWhenFragment()
        {
            this.Url = this.CurrentUrl + this.Url;
        }

        public void setWhenOther()
        {
            if (this.CurrentUrl.Contains("/"))
                while (!this.CurrentUrl.EndsWith("/"))
                    this.CurrentUrl = this.CurrentUrl.Remove(this.CurrentUrl.Length-1, 1);
            this.Url = this.CurrentUrl + this.Url;
        }

        public void setWhenQ()
        {
            if (this.CurrentUrl.Contains("?"))
                while (!this.CurrentUrl.EndsWith("?"))
                    this.CurrentUrl = this.CurrentUrl.Remove(this.CurrentUrl.Length - 1, 1);

            this.Url = this.CurrentUrl + this.Url;
        }
        #endregion

        public void formatUrl()
        {
            if (this.inRootDir())
                this.setWhenInRootDir();
            else if (this.isQuery())
                glueUrl(QUERY_DIR_PREFIX);
            else if (this.isFragment())
                this.setWhenFragment();
            else if (this.inCurrentDir())
                this.setWhenCurrentDir();
            else if (this.inParentOfCurrentDir())
                glueWhenParent();
            else
                this.setWhenOther();
        }

        public void whenSomeCond()
        {
            if (this.Url.StartsWith(ROOT_DIR_PREFIX) && this.CurrentUrl.EndsWith("/"))
                this.Url = this.Url.Remove(0, ROOT_DIR_PREFIX.Length);

            if (this.Url.Contains("/"))
            //this.Url = mergeUrl('/');
            {
                int a = 1;
            }
            else
                this.Url = "http://www." + update(this.CurrentUrl) + "/" + this.Url;
        }

        public void findAndSet() //todo important joqalbania
        {
            string temp = "";

            if (this.Url.Contains("/"))
            {
                string[] urlSegments = this.Url.Split('/');

                for (int i = 0; i < urlSegments.Length; i++)
                {
                    if (this.CurrentUrl.IndexOf(urlSegments[i] + "/") != -1 && i == 0)
                    {
                        temp = "ok";
                        this.Url = this.CurrentUrl.Substring(0, this.CurrentUrl.IndexOf(urlSegments[i] + "/")) + this.Url;
                        break;
                    }
                }
                if (String.IsNullOrEmpty(temp))
                    this.Url = update(this.CurrentUrl) + this.Url;
            }
            else
            {
                if (this.hasDot())
                    this.Url = update(this.CurrentUrl) + this.Url;
            }

        }

        #region Format
        private void setHttp()
        {
            if (!this.Domain.Contains(HTTPS))
                this.Url = HTTP + this.Url;
            else
                this.Url = HTTPS + this.Url;
        }

        public void setWhenInRootDir()
        {
            if (this.Domain.EndsWith("/"))
                this.Domain = this.Domain.Remove(this.Domain.Length - 1, 1);

            //Uri domainUri = new Uri(this.Domain);
            //this.Url = this.Domain.Substring(0, this.Domain.IndexOf(domainUri.Host) + domainUri.Host.Length) + this.Url;
            this.Url = this.Domain + this.Url;
        }

        /// <summary>
        /// example 
        // CurrentUrl = myexample.com/test/test.php
        // this.Url = ./test2.php
        // outputs = myexample.com/test/test2.php
        //
        // CurrentUrl = myexample.com/test/test.php
        // this.Url = ?q=2
        // outputs = myexample.com/test/test.php?q=2
        //
        // CurrentUrl = myexample.com/test/test.php?q=1
        // this.Url = ?q=3
        // outputs = myexample.com/test/test.php?q=3
        /// </summary>
        /// <param name="karakteri"></param>
        /// <returns></returns>
        //private string mergeUrl(string removeFromUrl, string compareChar)
        //{
        //    this.Url = this.Url.Remove(0, removeFromUrl.Length);

        //    int startToCutFrom = 0;
        //    string tempCurrent = this.CurrentUrl;
        //    for (int r = 0; r < tempCurrent.Length; r++)
        //        if (tempCurrent[r].ToString().Equals(compareChar))
        //            startToCutFrom = r;

        //    startToCutFrom++;
        //    tempCurrent = tempCurrent.Remove(startToCutFrom, tempCurrent.Length - startToCutFrom);
        //    return tempCurrent + this.Url;
        //}

        private void glueUrl(string boundary)
        {
            if (this.CurrentUrl.Contains(boundary))
                while (!this.CurrentUrl.EndsWith(boundary))
                    this.CurrentUrl = this.CurrentUrl.Remove(this.CurrentUrl.Length - 1, 1);
            if (this.CurrentUrl.EndsWith(boundary))
                this.CurrentUrl = this.CurrentUrl.Remove(this.CurrentUrl.Length - 1, 1);

            this.Url = this.CurrentUrl + this.Url;
        }

        /// <summary>
        /// this.CurrentUrl = example.com/test/test2/test3
        /// this.Url = ../../test4
        /// formats url to => example.com/test4
        /// </summary>
        public void glueWhenParent()
        {
            int stepsBack = 0;
            string downloadingUrlCopy = this.CurrentUrl;

            if (!this.CurrentUrl.EndsWith("/"))
                stepsBack++;

            while (this.Url.StartsWith(PARENT_DIR_PREFIX))
            {
                stepsBack++;
                this.Url = this.Url.Remove(0, PARENT_DIR_PREFIX.Length);
            }

            string[] downloadingUrlSegments = this.CurrentUrl.Split('/');
            this.CurrentUrl = "";

            for (int i = 0; i < downloadingUrlSegments.Length - stepsBack; i++)
                this.CurrentUrl += downloadingUrlSegments[i] + "/";

            this.Url = this.CurrentUrl + this.Url;
        }

        private string update(string xurl)
        {
            Uri u = new Uri(xurl);
            string uhost = u.Host;
            if (uhost.Contains("www"))
                uhost = uhost.Replace("www.", "");
            return uhost;
        }
        private void clean()
        {
            if (this.Url.Contains("&amp;"))
                this.Url = this.Url.Replace("&amp;", "&");
            if (this.Url.StartsWith("~"))
                this.Url.Remove(0, 1);
            if (this.Url.StartsWith("//"))
                this.Url = this.Url.Remove(0, 2);
            if (this.Url.EndsWith("/") && !this.Url.StartsWith(".") && this.Url.Length > 1)
                this.Url = this.Url.Remove(this.Url.Length - 1, 1);
        }
        #endregion

        public string[] urlFragmentSegments()
        {
            return this.Url.Split('#');
        }
    }
}
