using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace TjkYoutubeTracker.LinkUtils
{
    public class LinkInfo
    {
        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("title")]
        public string Title { get; private set; }

        public LinkInfo(string url, string title)
        {
            Regex r = new Regex("http.?://");
            url = "http://" + r.Replace(url, "");

            this.Url = url;
            this.Title = title;
        }

        private LinkInfo()
        {
        }

        public override string ToString()
        {
            return string.Format("\r\n Title: {0}\r\n url: {1}\r\n", Title, Url);
        }

        public LinkInfo Copy()
        {
            return new LinkInfo()
            {
                Title = this.Title,
                Url = this.Url,
            };
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                LinkInfo info = (LinkInfo)obj;
                return this.Url == info.Url;
            }
        }

        public override int GetHashCode()
        {
            return this.Url.GetHashCode();
        }

    }
}
