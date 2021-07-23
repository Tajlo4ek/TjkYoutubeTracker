using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TjkYoutubeTracker.LinkUtils
{
    public class Playlist
    {
        [JsonProperty("videos")]
        private readonly List<LinkInfo> videos;

        [JsonProperty("playlist_id")]
        public string Id { get; private set; }

        public Playlist(string url)
        {
            videos = new List<LinkInfo>();
            this.Id = url;
        }

        public void AddVideo(TjkYoutubeDL.VideoInfo info)
        {
            var link = new LinkInfo(info.Url, info.Title);
            videos.Add(link);
        }

        public void AddVideo(LinkInfo info)
        {
            videos.Add(info.Copy());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Id);

            foreach (var video in videos)
            {
                sb.Append("    ");
                sb.Append(video.ToString());
            }

            return sb.ToString();
        }


        public bool Contains(LinkInfo linkInfo)
        {
            var data = videos.Find((x) => x.Url == linkInfo.Url);
            var res = data != null;
            return res;
        }

        public void Remove(LinkInfo linkInfo)
        {
            videos.Remove(linkInfo);
        }

        public List<LinkInfo> GetVideos()
        {
            var list = new List<LinkInfo>();

            foreach (var video in videos)
            {
                list.Add(video.Copy());
            }

            return list;
        }


        public string GetJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static Playlist FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Playlist>(json);
        }

        public void Clear()
        {
            videos.Clear();
        }
    }
}
