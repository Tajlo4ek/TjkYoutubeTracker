using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TjkYoutubeTracker.Utils
{
    public class ConfigData
    {
        [JsonProperty("autoStart")]
        public bool StartWithSystem { get; set; }


        [JsonProperty("autoScan")]
        public bool AutoScanAtStart { get; set; }

        private ConfigData()
        {
        }

        public static ConfigData Parse(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<ConfigData>(json);
            }
            catch (Exception)
            {
                return GetDefault();
            }
        }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static ConfigData GetDefault()
        {
            return new ConfigData()
            {
                StartWithSystem = false,
                AutoScanAtStart = false,
            };
        }
    }

}
