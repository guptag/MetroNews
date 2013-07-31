using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using NewsExtractor;
using System.Configuration;
using System.Threading;
using System.Reflection;
using System.IO;


namespace NewsExtractorService
{
    public partial class NewsExtractorService : ServiceBase
    {		
        private Timer serviceTimer;
        private Timer serviceTimer1;
        private bool isFirstTime = true;

        public NewsExtractorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Logger.GetInstance().WriteEventlogEntry("NewsExtractorService Started", EventLogEntryType.Information);                

                // start processing within 10 secs (for the first time)
                serviceTimer = new Timer(new TimerCallback(FetchNews), null, 0, 10000);
            }
            catch (Exception ex)
            {
                Logger.GetInstance().WriteEventlogEntry("Exception in OnStart: " + ex.Message + ex.Source + ex.StackTrace, EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            serviceTimer1.Dispose();
            Logger.GetInstance().WriteEventlogEntry("NewsExtractorService stopped", EventLogEntryType.Information);
        }

        private void FetchNews(object obj)
        {
            if (isFirstTime)
            {
                Logger.GetInstance().WriteEventlogEntry("Bootstrapping the process....", EventLogEntryType.Information);
                serviceTimer.Dispose();
                serviceTimer1 = new Timer(new TimerCallback(FetchNewsItems), null, 240000, Int32.Parse(ConfigurationManager.AppSettings["RefreshContentInMSec"]));
                isFirstTime = false;
            }

            FetchNewsItems(null);            
        }

        private void FetchNewsItems(object obj)
        {
            try
            {                
                Logger.GetInstance().AddLogEntry("Started fetching data");
                Logger.GetInstance().WriteEventlogEntry("Started fetching data", EventLogEntryType.Information);

                ServicePointManager.DefaultConnectionLimit = 15;
                TaskScheduler.UnobservedTaskException += (sender, e) =>
                {
                    Logger.GetInstance().AddLogEntry(e.Exception);
                    e.SetObserved();
                };

                DateTime dt = DateTime.Now;

                NewsFeedConfiguration feedConfig = NewsFeedConfigurationManager.GetNewsFeedConfiguration();
                Dictionary<string, List<NewsItem>> newsItemsPerCategory = new Dictionary<string, List<NewsItem>>();
                FeedParser feedParser = new FeedParser();
                string outputFilePath = ConfigurationManager.AppSettings["OutputFilePath"].Replace("~", Utility.GetCurrentDirectory());

                foreach (string category in feedConfig.FeedUrlsPerCategory.Keys)
                {
                    Logger.GetInstance().AddLogEntry("begin processing " + category);
                    List<NewsItem> newsItem = feedParser.GetNewsItemsFromFeeds(feedConfig.FeedUrlsPerCategory[category], feedConfig.MaxArticlesPerCategory);
                    newsItemsPerCategory[category] = newsItem;
                    Logger.GetInstance().AddLogEntry("finished processing " + category);
                    Logger.GetInstance().Flush();

                    try
                    {
                        NewsItem.SerializeToXml(newsItem, outputFilePath + category + ".xml");
                        Logger.GetInstance().AddLogEntry("finished writing datafile: " + category);
                        Logger.GetInstance().Flush();
                    }
                    catch (Exception ex)
                    {
                        Logger.GetInstance().WriteEventlogEntry("Error while generating the datafile: " + ex.Message + ex.Source + ex.StackTrace, EventLogEntryType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.GetInstance().WriteEventlogEntry("Exception from NewsExtractorService: " + ex.Message + ex.Source + ex.StackTrace, EventLogEntryType.Error);
            }
        }        
    }
}
