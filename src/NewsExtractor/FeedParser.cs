using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml.Linq;
using System.IO;
using System.Configuration;

namespace NewsExtractor
{
    public class FeedParser
    {
        public List<NewsItem> GetNewsItemsFromFeeds(List<string> feedUrls, int maxArticlesToReturn)
        {
            Logger logger = Logger.GetInstance();

            DateTime startTime = DateTime.Now;
            List<NewsItem> newsItemCollection = new List<NewsItem>();

            
            logger.AddLogEntry("Scheduling Feed Requests");
            Task continuation = Task.Factory.ContinueWhenAll((from url in feedUrls
                                                             select GetNewsItems(url)).ToArray(),
                                                             tasks =>
                                                             {
                                                                  foreach (Task<List<NewsItem>> tsk in tasks)
                                                                  {
                                                                      if (tsk.Status == TaskStatus.RanToCompletion)
                                                                      {
                                                                          newsItemCollection.AddRange(tsk.Result);
                                                                      }
                                                                  }
                                                             });
            DateTime feedScheduledTime = DateTime.Now;
            try
            {
                continuation.Wait();
                logger.AddLogEntry(String.Format("Finished parsing {0} feeds", feedUrls.Count));
            }
            catch (AggregateException ex)
            {
                logger.AddLogEntry(ex);
                foreach (Exception e in ex.InnerExceptions)
                {
                    logger.AddLogEntry(e);
                }
            }

            if (newsItemCollection.Count == 0)
            {
                logger.AddLogEntry("No news articles from feeds");
                return new List<NewsItem>();
            }

            logger.AddLogEntry("Scheduling requests to get Images");
            Task continuation1 = Task.Factory.ContinueWhenAll((from newsItem in newsItemCollection
                                                               select GetArticleImage(newsItem)).ToArray(),
                                                               tasks =>
                                                               {
                                                                   foreach (Task<ArticleImage> tsk in tasks)
                                                                   {
                                                                       if (tsk.Status != TaskStatus.RanToCompletion)
                                                                       {
                                                                           //log the details
                                                                           if (tsk.Exception != null)
                                                                           {
                                                                               logger.AddLogEntry(tsk.Exception);
                                                                           }
                                                                       }
                                                                   }
                                                               });


            try
            {
                continuation1.Wait();
                logger.AddLogEntry("Finished retrieving images");
            }
            catch (AggregateException ex)
            {
                logger.AddLogEntry(ex);
                foreach (Exception e in ex.InnerExceptions)
                {
                    logger.AddLogEntry(e);
                }
            }

            //Get the newsitems that have images
            List<NewsItem> newsWithImages = newsItemCollection.Where(ni => ni.Image != null).ToList();
            //Shuffle the Articles (so that articles from first url won't always show at the top)
            newsWithImages = ShuffleArticles(newsWithImages);
            
           /* if (newsWithImages.Count < maxArticlesToReturn)
            {
                //try to get the images from Bing if there aren't enough articles with Images                
                List<NewsItem> newsWithNoImages = newsItemCollection.Where(ni => ni.Image == null).Take((maxArticlesToReturn - newsWithImages.Count) + 10).ToList();

                if (newsWithNoImages.Count > 0)
                {
                    logger.AddLogEntry("Scheduling requests to get Images from Bing");
                    Task continuation2 = Task.Factory.ContinueWhenAll((from newsItem in newsWithNoImages
                                                                       select GetArticleImageFromBing(newsItem)).ToArray(),
                                                                       tasks =>
                                                                       {
                                                                           foreach (Task<ArticleImage> tsk in tasks)
                                                                           {
                                                                               if (tsk.Status != TaskStatus.RanToCompletion)
                                                                               {
                                                                                   //log the details
                                                                                   if (tsk.Exception != null)
                                                                                   {
                                                                                       logger.AddLogEntry(tsk.Exception);
                                                                                   }
                                                                               }
                                                                           }
                                                                       });



                    try
                    {
                        continuation2.Wait();
                        logger.AddLogEntry("Finished retrieving images from Bing");
                    }
                    catch (AggregateException ex)
                    {
                        logger.AddLogEntry(ex);
                        foreach (Exception e in ex.InnerExceptions)
                        {
                            logger.AddLogEntry(e);
                        }
                    }

                    newsWithImages.AddRange(newsWithNoImages.Where(ni => ni.Image != null).ToList());
                }
                else
                {
                    logger.AddLogEntry("No news articles without images");
                }
            }*/

            return NewsItem.FilterNewsItems(newsWithImages).Take(maxArticlesToReturn).ToList();
        }

        private List<NewsItem> ShuffleArticles(List<NewsItem> inputList)
        {
            //http://www.vcskicks.com/randomize_array.php
            List<NewsItem> randomList = new List<NewsItem>();

            Random r = new Random();
            int randomIndex = 0;
            while (inputList.Count > 0)
            {
                randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return randomList; //return the new random list
        }

        private Task<List<NewsItem>> GetNewsItems(string feedUrl)
        {
            TaskCompletionSource<List<NewsItem>> tc = new TaskCompletionSource<List<NewsItem>>();

            var request = (HttpWebRequest)WebRequest.Create(feedUrl);
            //request.KeepAlive = false;

            request.BeginGetResponse(ar =>
            {
                try
                {
                    var response = (HttpWebResponse)request.EndGetResponse(ar);
                    tc.SetResult(NewsItem.ParseNewsItems(response.GetResponseStream()));
                }
                catch (Exception ex)
                {
                    tc.SetException(ex);
                }
            }, feedUrl);

            return tc.Task;
        }        

        static Task<ArticleImage> GetArticleImage(NewsItem newsItem)
        {
            TaskCompletionSource<ArticleImage> tc = new TaskCompletionSource<ArticleImage>();

            var request = (HttpWebRequest)WebRequest.Create(newsItem.Link);
            //request.KeepAlive = false;
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 50;
            request.BeginGetResponse(ar =>
            {
                try
                {
                    var response = (HttpWebResponse)request.EndGetResponse(ar);
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    newsItem.Image = ImageParserUtility.ParseImageTagsFromHtml(newsItem, reader.ReadToEnd());
                    tc.SetResult(newsItem.Image);
                }
                catch (Exception ex)
                {
                    tc.SetException(ex);
                }
            }, null);

            return tc.Task;
        }

        static Task<ArticleImage> GetArticleImageFromBing(NewsItem newsItem)
        {
            TaskCompletionSource<ArticleImage> tc = new TaskCompletionSource<ArticleImage>();

            var request = (HttpWebRequest)WebRequest.Create(BingSearchUtility.GetImageSearchUrl(newsItem.Title));
           // request.KeepAlive = false;
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 50;
            request.BeginGetResponse(ar =>
            {
                try
                {
                    var response = (HttpWebResponse)request.EndGetResponse(ar);
                    newsItem.Image = BingSearchUtility.GetImagesFromStream(newsItem, response.GetResponseStream());
                    tc.SetResult(newsItem.Image);
                }
                catch (Exception ex)
                {
                    tc.SetException(ex);
                }
            }, null);

            return tc.Task;
        }
    }
}
