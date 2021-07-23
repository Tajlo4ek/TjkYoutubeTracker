using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TjkYoutubeTracker.LinkUtils
{
    static class PathUtils
    {
        public static string RemoveillegalChars(string filename)
        {
            if (filename == null)
            {
                filename = "noPlayList";
            }

            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(filename, "");
        }
    }
}
