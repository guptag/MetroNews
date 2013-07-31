using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Linq;
using System.IO;

namespace NewsExtractor
{
    class RequestExecutor
    {
        private string url;
        HttpWebRequest webRequest;

        public string ResponseContent
        { 
          get; 
          set; 
        }    

        public RequestExecutor(string url)
        {
            this.url = url;
        }

        public IAsyncResult BeginRequest(Object context)
        {
            webRequest = (HttpWebRequest)WebRequest.Create(url);            
            webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            webRequest.Headers["Accept-Language"] = "en-us";
            webRequest.Headers["Accept-Encoding"] = "gzip, deflate";
            webRequest.Headers["Accept-Charset"] = "ISO-8859-1,utf-8;q=0.7,*;q=0.7";
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:2.0) Gecko/20100101 Firefox/4.0";                       
            
            return webRequest.BeginGetResponse(EndRequest, null);            
        }       

        public void EndRequest(IAsyncResult ar)
        {
            WebResponse response = webRequest.EndGetResponse(ar);
            StreamReader str = new StreamReader(response.GetResponseStream());
            ResponseContent = str.ReadToEnd();
        }
    }
}
