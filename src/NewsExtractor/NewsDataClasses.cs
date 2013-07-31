using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace NewsExtractor
{
    public class NewsItem
    {
        public string Link { get; set; }
        public string Title { get; set; }
        public string Desciption { get; set; }
        public DateTime PubDate { get; set; }
        public ArticleImage Image { get; set; }
        public Source Source { get; set; }

        public static void SerializeToXml(List<NewsItem> newsItems, string filePath)
        {

            XElement newsItemsXML = new XElement("NewsItems",
                                from newsItem in newsItems.Where(ni => ni.Image != null && !String.IsNullOrEmpty(ni.Title) && !String.IsNullOrEmpty(ni.Desciption) && !String.IsNullOrEmpty(ni.Link))
                                select new XElement("NewsItem",
                                            new XElement("Title", newsItem.Title),
                                            new XElement("Link", newsItem.Link),
                                            new XElement("Description", newsItem.Desciption),
                                            new XElement("PubDate", newsItem.PubDate.ToUniversalTime().ToString()),
                                            new XElement("Source",
                                                    new XElement("Title", newsItem.Source.Title),
                                                    new XElement("Logo",
                                                        new XAttribute("Url", newsItem.Source.SourceLogo.Url),
                                                        new XAttribute("AltText", newsItem.Source.SourceLogo.AltText),
                                                        new XAttribute("ClickUrl", newsItem.Source.SourceLogo.ClickUrl),
                                                        new XAttribute("Height", newsItem.Source.SourceLogo.Height)),
                                                        new XAttribute("Width", newsItem.Source.SourceLogo.Width)),
                                            new XElement("Image",
                                            new XAttribute("Url", newsItem.Image.Url),
                                            new XAttribute("AltText", newsItem.Image.AltText),
                                            new XAttribute("ClickUrl", newsItem.Image.ClickUrl),
                                            new XAttribute("Height", newsItem.Image.Height),
                                            new XAttribute("Width", newsItem.Image.Width))));

            newsItemsXML.Save(filePath, SaveOptions.None);

        }

        public static List<NewsItem> GetNewsItemsFromXml(string filePath, int maxArticlesPerCategory)
        {
            //Let the code throw exception if the file doesn't exist
            XDocument xDoc = XDocument.Load(filePath);

            List<NewsItem> newsItems = (from newsItemNode in xDoc.Element("NewsItems").Elements("NewsItem")
                                        let imageNode = newsItemNode.Element("Image") ?? new XElement("Image")
                                        let sourceNode = newsItemNode.Element("Source") ?? new XElement("Source")
                                        let sourceLogoNode = sourceNode.Element("Logo") ?? new XElement("Logo")
                                        select new NewsItem()
                                        {
                                            Title = newsItemNode.ParseChildElementValue("Title", String.Empty),
                                            Desciption = string.Empty /*newsItemNode.ParseChildElementValue("Description", String.Empty)*/,
                                            PubDate = newsItemNode.ParseChildElementValue("PubDate", DateTime.MinValue),
                                            Link = newsItemNode.ParseChildElementValue("Link", String.Empty),
                                            Source = new Source()
                                            {
                                                Title = sourceNode.ParseChildElementValue("Title", String.Empty),
                                                Link = sourceLogoNode.ParseAttributeValue("ClickUrl", String.Empty)/*,
                                                SourceLogo = new ArticleImage()
                                                {
                                                    ClickUrl = sourceLogoNode.ParseAttributeValue("ClickUrl", String.Empty),
                                                    AltText = sourceLogoNode.ParseAttributeValue("AltText", String.Empty),
                                                    Url = sourceLogoNode.ParseAttributeValue("Url", String.Empty),
                                                    Height = sourceLogoNode.ParseAttributeValue("Height", -1),
                                                    Width = sourceLogoNode.ParseAttributeValue("Width", -1),
                                                }*/
                                            },
                                            Image = new ArticleImage()
                                            {
                                                ClickUrl = imageNode.ParseAttributeValue("ClickUrl", String.Empty),
                                                AltText = imageNode.ParseAttributeValue("AltText", String.Empty),
                                                Url = imageNode.ParseAttributeValue("Url", String.Empty),
                                                Height = imageNode.ParseAttributeValue("Height", -1),
                                                Width = imageNode.ParseAttributeValue("Width", -1)
                                            }
                                        }).Take(maxArticlesPerCategory).ToList();
            return newsItems;
        }

        public static List<NewsItem> ParseNewsItems(Stream feedStream)
        {
            XDocument xDoc = XDocument.Load(feedStream);

            List<NewsItem> newsItems = (from itemNode in xDoc.Element("rss").Element("channel").Elements("item")
                                        let channelNode = xDoc.Element("rss").Element("channel")
                                        let channelLogoNode = channelNode.Element("image")
                                        select new NewsItem()
                                        {
                                            Title = itemNode.ParseChildElementValue("title", String.Empty),
                                            Desciption = itemNode.ParseChildElementValue("description", String.Empty),
                                            PubDate = itemNode.ParseChildElementValue("pubDate", DateTime.MinValue),
                                            Link = itemNode.ParseChildElementValue("link", String.Empty),
                                            Source = new Source()
                                            {
                                                Title = channelNode.ParseChildElementValue("title", String.Empty),
                                                SourceLogo = new ArticleImage()
                                                {
                                                    ClickUrl = channelLogoNode.ParseChildElementValue("link", String.Empty),
                                                    AltText = channelLogoNode.ParseChildElementValue("title", String.Empty),
                                                    Url = channelLogoNode.ParseChildElementValue("url", String.Empty),
                                                    Height = channelLogoNode.ParseChildElementValue("height", -1),
                                                    Width = channelLogoNode.ParseChildElementValue("width", -1),
                                                }
                                            }
                                        })
                                        .Where(ni => (!String.IsNullOrEmpty(ni.Title) && !String.IsNullOrEmpty(ni.Link) && !String.IsNullOrEmpty(ni.Title) && !String.IsNullOrEmpty(ni.Desciption)))
                                        .ToList();

            return newsItems;
        }

        public static List<NewsItem> FilterNewsItems(List<NewsItem> newsItems)
        {
            return newsItems.Where(n => n.Image != null &&
                                        n.Image.Url != null &&
                                        !n.Title.ToLower().Contains("technoni") &&
                                        !n.Title.ToLower().Contains("sponser") &&
                                        !n.Link.ToLower().Contains("ad.") &&
                                        !n.Link.ToLower().Contains("ads.") &&
                                        !n.Link.ToLower().Contains("ad/") &&
                                        !n.Link.ToLower().Contains("ads/") &&
                                        !n.Link.ToLower().Contains("unicast") &&
                                        !n.Link.ToLower().Contains("doubleclick") &&
                                        !n.Image.Url.Contains("technoni") &&
                                        !n.Image.Url.Contains("ads") &&
                                        !n.Image.Url.Contains("ads.") &&
                                        !n.Image.Url.Contains("ads/") &&
                                        !n.Image.Url.Contains("ad") &&
                                        !n.Image.Url.Contains("ad.") &&
                                        !n.Image.Url.Contains("ad/") &&
                                        !n.Image.Url.Contains("unicast") &&
                                        !n.Image.Url.Contains("doubleclick")).ToList();
        }
    }

    public class ArticleImage
    {
        public string AltText { get; set; }
        public string ClickUrl { get; set; }
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Source
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public ArticleImage SourceLogo { get; set; }
    }
}
