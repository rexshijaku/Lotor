using Lotor.Caches;
using Lotor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Models
{
    public enum Level
    {
        Index = 0,
        First = 1,
        Second = 2,
        Third = 3
    }

    /// <summary>
    /// the new instance provides information about document count in each level
    /// </summary>
    public class LevelInfo
    {
        public LevelInfo(bool isAlb)
        {
            this.CountAndSave();
        }
        private bool isAlb { get; set; }
        public int FirstLevel { get; set; }
        public int SecondLevel { get; set; }
        public int ThirdLevel { get; set; }
        private void CountAndSave()
        {
            this.FirstLevel = GlobalHelper.saveLevelDocuments(DomainCache.firstLevelUrls, this.isAlb, Level.First);
            this.SecondLevel = GlobalHelper.saveLevelDocuments(DomainCache.secondLevelUrls, this.isAlb, Level.Second);
            this.ThirdLevel = GlobalHelper.saveLevelDocuments(DomainCache.thirdLevelUrls, this.isAlb, Level.Third);
        }
        public int getTotalDocuments()
        {
            return (this.FirstLevel + this.SecondLevel + this.ThirdLevel);
        }
    }
}
