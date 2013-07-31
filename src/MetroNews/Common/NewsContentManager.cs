using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NewsExtractor;
using System.IO;

namespace MetroNews.Common
{
    public class NewsContentManager
    {
        public static object lockObj = new object();

        public static Dictionary<string, List<NewsItem>> LastDisplayedNewsItems =
            new Dictionary<string, List<NewsItem>>();

        public static DateTime LastRefreshed = DateTime.MinValue;

        public static Dictionary<string, List<NewsItem>> GetNewsForAllCategories()
        {
            string contentFolderPath = ConfigurationManager.AppSettings["ContentFolder"];
            string[] categories = ConfigurationManager.AppSettings["Categories"].Split(';');

            TimeSpan timeDiff = DateTime.Now.Subtract(LastRefreshed);
            if (DateTime.Now >
                LastRefreshed +
                TimeSpan.FromMilliseconds(Int32.Parse(ConfigurationManager.AppSettings["CacheExpirationInMSec"])))
            {
                lock (lockObj)
                {
                    try
                    {
                        Logger.GetInstance().AddLogEntry("Cache expired");
                        Dictionary<string, List<NewsItem>> newsContent = new Dictionary<string, List<NewsItem>>();
                        foreach (string category in categories)
                        {
                            if (File.Exists(contentFolderPath + category + ".xml"))
                            {
                                List<NewsItem> newsItems = NewsItem.GetNewsItemsFromXml(contentFolderPath + category + ".xml", Int32.Parse(ConfigurationManager.AppSettings["MaxArticlesPerCategory"]));

                                if (newsItems.Count > 0)
                                {
                                    newsContent[category] = newsItems;
                                }
                            }
                        }

                        //Only update if the new data has some articles
                        if (newsContent.Count != 0)
                        {
                            LastDisplayedNewsItems = newsContent;
                        }
                        LastRefreshed = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        Logger.GetInstance().AddLogEntry(ex);
                        Logger.GetInstance().WriteEventlogEntry(ex, EventLogEntryType.Error);
                    }
                }
            }

            if (LastDisplayedNewsItems != null)
            {
                return LastDisplayedNewsItems;
            }
            else
            {
                return new Dictionary<string, List<NewsItem>>();
            }
        }
    }
}