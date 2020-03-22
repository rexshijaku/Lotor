using Lotor.Caches;
using Lotor.Helpers;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lotor.Models;

namespace Lotor.Calculations
{
    public class QualityCalculator
    {
        private int documentCount;
        private double domainQuality;

        public QualityCalculator()
        {
            this.setInitialValues();
        }

        public void setInitialValues()
        {
            this.documentCount = 0;
            this.domainQuality = 0.0;
        }
      
        public void addToQualitySum(double documentQuality)
        {
            if (double.IsNaN(this.domainQuality))
                this.domainQuality = 0.0;

            this.domainQuality += documentQuality;
            this.documentCount++;
        }

        public double getDocumentQuality(Document document)
        {
            Report.info(document.url + " estimating document quality...");
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(document.html);
            string[] terms = QualityCalculations.parseWords(document.text).ToArray();

            QualityFeatures qualityFeatureO = new QualityFeatures();
            qualityFeatureO.numVisTerms = QualityCalculations.calNumVisTerms(terms);
            qualityFeatureO.numTitleTerms = QualityCalculations.calNumTitleTerms(document.html);
            qualityFeatureO.avgTermLen = QualityCalculations.calAvgTermLen(terms);
            qualityFeatureO.urlDepth = QualityCalculations.calUrlDepth(document.url);
            qualityFeatureO.urlLength = QualityCalculations.calUrlLength(document.url);
            qualityFeatureO.ratioVisText = QualityCalculations.calRatioVisText(document.html, document.text);
            qualityFeatureO.entropy = QualityCalculations.calEntropy(terms);
            qualityFeatureO.ratioStops = QualityCalculations.calRatioStops(terms, MainCache.stopWords);
            qualityFeatureO.stopCover = QualityCalculations.calStopCover(terms, MainCache.stopWords);
            qualityFeatureO.ratioAnchorText = QualityCalculations.calRatioAnchorText(htmlDoc, terms.Length);
            qualityFeatureO.ratioTableText = QualityCalculations.calRatioTableText(htmlDoc, terms.Length);
            qualityFeatureO.rankV = (QualityFreeParams.stopCoverC * qualityFeatureO.stopCover) + (QualityFreeParams.ratioStops * qualityFeatureO.ratioStops)
                + (QualityFreeParams.entropy * qualityFeatureO.entropy)
                + (QualityFreeParams.numVisTermsC * qualityFeatureO.numVisTerms)
                + (QualityFreeParams.numTitleTermsC * qualityFeatureO.numTitleTerms)
                + (QualityFreeParams.avgTermLenC * qualityFeatureO.avgTermLen)
                + (QualityFreeParams.urlDepthC * qualityFeatureO.urlDepth)
                + (QualityFreeParams.ratioVisTextC * qualityFeatureO.ratioVisText)
                + (QualityFreeParams.urlLengthC * qualityFeatureO.urlLength)
                + (QualityFreeParams.ratioAnchorTextC * qualityFeatureO.ratioAnchorText)
                + (QualityFreeParams.ratioTableTextC * qualityFeatureO.ratioTableText);

            if (!double.IsNaN(qualityFeatureO.rankV))
                return qualityFeatureO.rankV;
            else
                return 0.0;
        }

        public double getDomainQuality()
        {
            if (this.documentCount == 0)
                return 0.0;
          
            var result = Math.Round(this.domainQuality / this.documentCount, 3);

            if (result > 1.0)
                return 1.0;
            else
                return result;
        }
    }
}
