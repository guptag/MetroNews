using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace NewsExtractor
{
    //http://api.bing.net/xml.aspx?AppId=F969214916187B3AA3E23ABDF56A2C276CDDC86E&Query=Obama%20takes%20re-election%20campaign%20to%20Coast&Sources=Image&Version=2.0&Market=en-us&Adult=strict&Image.Count=10&Image.Offset=0&Image.Filters=Size:Medium&Size:Large
    class BingSearchUtility
    {
        static readonly string AppId = "F969214916187B3AA3E23ABDF56A2C276CDDC86E";
        static readonly string BingImageSearchUrl = "http://api.bing.net/xml.aspx?AppId={0}&Query={1}&Sources=Image&Version=2.0&Market=en-us&Adult=strict&Image.Count=5&Image.Offset=0&Image.Filters=Size:Medium&Size:Large";

        public static string GetImageSearchUrl(string article)
        {
            return String.Format(BingImageSearchUrl, AppId, article);
        }

        public static ArticleImage GetImagesFromStream(NewsItem newsItem, Stream imageStream)
        {
            XDocument xDoc = XDocument.Load(imageStream);

            XNamespace ns = "http://schemas.microsoft.com/LiveSearch/2008/04/XML/multimedia";
            ArticleImage articleImage = (from imageResult in xDoc.Descendants(ns + "ImageResult")
                                        select new ArticleImage()
                                        {
                                            Url = (imageResult.Element(ns + "MediaUrl") ?? new XElement(ns + "MediaUrl", String.Empty)).Value,
                                            AltText = newsItem.Title + " (powered by Bing Image Search)",
                                            ClickUrl = newsItem.Link,
                                            Width = Int32.Parse((imageResult.Element(ns + "Width") ?? new XElement(ns + "Width", "-1")).Value),
                                            Height = Int32.Parse((imageResult.Element(ns + "Height") ?? new XElement(ns + "Height", "-1")).Value),
                                        }).Where(img => !String.IsNullOrEmpty(img.Url) && img.Width > 300 && img.Height > 150).FirstOrDefault();

            return articleImage;
        }

    }
}
