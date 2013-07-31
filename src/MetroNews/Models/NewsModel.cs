using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using MetroNews.Common;
using NewsExtractor;
using System.Web.Script.Serialization;
using System.Net;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace MetroNews.Models
{
    public class NewsModelResult
    {
        public string[] Categories;
        public Dictionary<string, List<NewsItem>> News;
    }

    public class NewsModel
    {
        public string GetNewsContentJSON()
        {
            //var newsContent = {
            //         "categories" : ["Headlines", "Technology"],
            //         "Headlines" : [{
            //                            "title" : "Damaged Washington Monument deemed \'structurally sound\'",
            //                            "description" : "Damaged Washington Monument deemed \'structurally sound\'",
            //                            "link" : "http://www.cnn.com",
            //                            "source" : { "title" : "Cnn", "link" : "http://www.cnn.com"},
            //                            "image" : { "url" : "http://i2.cdn.turner.com/cnn/dam/assets/110926085234-vo-washington-monument-quake-00001902-story-top.jpg", "width" : "", "height" : "", "alt" : ""}
            //                        },
            //                        {
            //                            "title" : "Test1 Test2 Test3 Test4 Test5",
            //                            "description" : "Test1 Test2 Test3 Test4 Test5",
            //                            "link" : "http://www.cnn.com",
            //                            "source" : { "title" : "NY Times", "link" : "http://www.cnn.com"},
            //                            "image" : { "url" : "http://graphics8.nytimes.com/images/2011/09/27/world/europe/27russia2/27russia2-articleLarge.jpg", "width" : "", "height" : "", "alt" : ""}
            //                        },
            //                       ]
            //      };

            Dictionary<string, List<NewsItem>> newsForAllCategories = NewsContentManager.GetNewsForAllCategories();
            NewsModelResult result = new NewsModelResult()
            {
                Categories = newsForAllCategories.Keys.ToArray(),
                News = newsForAllCategories
            };
            return new JavaScriptSerializer().Serialize(result);

            //download images

            /*int total = 0;
            foreach (KeyValuePair<string, List<NewsItem>> kvp in newsForAllCategories)
            {
                string category = kvp.Key;
                List<NewsItem> articles = kvp.Value;

                foreach(NewsItem item in articles)
                {
                   Stream stream = null;
                    Bitmap bitmap;

                   try
                   {
                       string imageName =item.Image.Url.Substring(item.Image.Url.LastIndexOf("/") + 1);

                       WebClient client = new WebClient();
                       stream = client.OpenRead(item.Image.Url);
                       bitmap = new Bitmap(stream);
                       try
                       {
                           bitmap.Save(@"C:\Users\guptag\Documents\Visual Studio 2010\Projects\MetroNews\MetroNews\Content\News\" + imageName);
                           item.Image.Url = "../Content/News/" + imageName;
                       }
                       catch
                       {
                           imageName = item.Title.Replace(" ", "").Replace(":","").Replace(",","").Replace + ".jpg";
                           bitmap.Save(@"C:\Users\guptag\Documents\Visual Studio 2010\Projects\MetroNews\MetroNews\Content\News\" + imageName);
                           item.Image.Url = "../Content/News/" + imageName;
                       }

                       total += 1;

                      // if (total >= 5) break;
                   }
                   finally
                   {
                       if (stream != null)
                       {
                           stream.Flush();
                           stream.Close();
                       }
                   }
                }
            }*/

            
        }        
    }
}