using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NewsExtractor
{
    class ImageParserUtility
    {
        static Regex ImageParserRegEx = new Regex("<img(.*?)>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex ImageAltParserRegEx = new Regex("alt=\"(.*?)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex ImageSrcParserRegEx = new Regex("src=\"(.*?)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex ImageWidthParserRegEx = new Regex("width=\"(.*?)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex ImageHeightParserRegEx = new Regex("height=\"(.*?)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static ArticleImage ParseImageTagsFromHtml(NewsItem newsItem, string content)
        {
            List<ArticleImage> imageList = new List<ArticleImage>();

            Uri siteUri = new Uri(newsItem.Link);
            MatchCollection matchCollection = ImageParserRegEx.Matches(content);

            foreach (Match match in matchCollection)
            {
                //Group0 ---> <a href=....><img src........../></a>
                //Group1 ---> Anchor's href value
                //Group2 ---> Image meta data (src="<val>" alt="<val" height="<value>" width="<value>")

                if (match.Groups.Count >= 2)
                {
                    ArticleImage imageData = new ArticleImage();
                    imageData.ClickUrl = newsItem.Link;
                    imageData.AltText = newsItem.Title;

                    string imageMetaData = match.Groups[1].Value;

                    Match imgAttrMatch = ImageAltParserRegEx.Match(imageMetaData);
                    if (imgAttrMatch != null && imgAttrMatch.Groups.Count >= 2)
                    {
                        imageData.AltText = imgAttrMatch.Groups[1].Value;
                    }

                    imgAttrMatch = ImageSrcParserRegEx.Match(imageMetaData);
                    if (imgAttrMatch != null && imgAttrMatch.Groups.Count >= 2)
                    {
                        imageData.Url = imgAttrMatch.Groups[1].Value;

                        Uri uri = new Uri(imageData.Url, UriKind.RelativeOrAbsolute);

                        if (!uri.IsAbsoluteUri)
                        {
                            imageData.Url = siteUri.Scheme + "://" + siteUri.Host + (imageData.Url.StartsWith("/") ? "" : "/") + imageData.Url;
                        }

                    }

                    imgAttrMatch = ImageWidthParserRegEx.Match(imageMetaData);
                    bool couldParseWidth = false;
                    if (imgAttrMatch != null && imgAttrMatch.Groups.Count >= 2)
                    {
                        int width = 0;
                        couldParseWidth = Int32.TryParse(imgAttrMatch.Groups[1].Value, out width);
                        imageData.Width = width;
                    }

                    imgAttrMatch = ImageHeightParserRegEx.Match(imageMetaData);
                    bool couldParseHeight = false;
                    if (imgAttrMatch != null && imgAttrMatch.Groups.Count >= 2)
                    {
                        int height = 0;
                        couldParseHeight = Int32.TryParse(imgAttrMatch.Groups[1].Value, out height);
                        imageData.Height = height;
                    }

                    if (imageData.Width >= 300 && 
                        imageData.Height >= 150 &&
                        !imageData.Url.Contains(".gif") &&
                        !(imageData.Width == 300 && imageData.Height == 250))
                    {
                        return imageData;
                    }
                }
            }

            return null;
        }
    }
}
