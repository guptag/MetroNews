using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml.Linq;
using NewsExtractor;
using System.Reflection;
using System.IO;


namespace NewsExtractorService
{
    public class NewsFeedConfiguration
    {
        public Dictionary<string, List<string>> FeedUrlsPerCategory { get; set;}
        public int MaxArticlesPerCategory { get; set;}
    }

    public static class NewsFeedConfigurationManager
    {
        static NewsFeedConfiguration configuration = null;
        private static object lockObject = new object();
        
        public static NewsFeedConfiguration GetNewsFeedConfiguration()
        {
            if (configuration == null)
            {
                lock (lockObject)
                {
                    if (configuration == null)
                    {
                        configuration = ParseConfigurationFromFile();
                    }
                }
            }

            return configuration;
        }

        private static NewsFeedConfiguration ParseConfigurationFromFile()
        {
            Logger logger = Logger.GetInstance();

            string filePath = ConfigurationManager.AppSettings["NewsFeedConfigurationFullPath"].Replace("~", Utility.GetCurrentDirectory());

            XDocument xDoc = XDocument.Load(filePath);

            var feedConfiguration = from rootNode in xDoc.Elements("FeedConfiguration")
                                    let generalNode = rootNode.Element("General") ?? new XElement("General")
                                    let feedsListNode = rootNode.Element("FeedsList") ?? new XElement("FeedsList")
                                    select new NewsFeedConfiguration()
                                    {
                                          MaxArticlesPerCategory = generalNode.ParseChildElementValue("MaxArticlesPerCategory", 25),
                                          FeedUrlsPerCategory = (from feedsNode in feedsListNode.Elements("Feeds")
                                                                  let feeds = feedsNode.Elements("Feed")
                                                                  select new KeyValuePair<string, List<string>>
                                                                          (
                                                                            feedsNode.ParseAttributeValue("category", String.Empty), 
                                                                            (from fd in feeds
                                                                             select fd.Value).Where(url => !String.IsNullOrEmpty(url)).ToList()
                                                                           ))
                                                                  .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase)
                                    };

            return feedConfiguration.First();
        }

       
    }
}
