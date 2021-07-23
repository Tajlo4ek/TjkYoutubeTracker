using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using TjkYoutubeDL;
using TjkYoutubeTracker.LinkUtils;
using TjkYoutubeTracker.MyControls;
using TjkYoutubeTracker.Utils;

namespace TjkYoutubeTracker
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string applicationName = "TjkYoutubeTracker";

        private readonly string savePath;
        private readonly string linksFileName;
        private readonly string playlistSavePath;
        private readonly string configSavePath;

        private readonly YoutubeDL yudl;

        private readonly List<Playlist> playlists;
        private int videoScannedCount;
        private readonly ConfigData config;

        public MainWindow()
        {
            InitializeComponent();

            savePath = Path.GetFullPath("save/");
            linksFileName = Path.GetFullPath(savePath + "links.txt");
            playlistSavePath = Path.GetFullPath(savePath + "playlists/");
            configSavePath = Path.GetFullPath(savePath + "config.json");

            CreateDirectory(savePath);
            CreateDirectory(playlistSavePath);

            LoadLinks();

            yudl = new YoutubeDL();
            yudl.SetYoutubeDLPath("Resources/youtube-dl.exe");
            //yudl.EnableLog = true;

            playlists = new List<Playlist>();
            config = LoadConfig();

            cbAutoScan.IsChecked = config.AutoScanAtStart;
            cbAutoStart.IsChecked = config.StartWithSystem;
            if (config.AutoScanAtStart)
            {
                this.Dispatcher.InvokeAsync(Scan);
            }
        }

        private void CreateDirectory(string path)
        {
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
        }


        public void LoadLinks()
        {
            var isReaded = TryReadFile(linksFileName, out string links);

            if (isReaded == false)
            {
                SaveLinks();
                return;
            }

            rtbLinks.AppendText(links);
        }

        public void SaveLinks()
        {
            var links = new TextRange(rtbLinks.Document.ContentStart, rtbLinks.Document.ContentEnd).Text;
            StringBuilder sb = new StringBuilder();
            foreach (var link in links.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                sb.AppendLine(link);
            }
            SaveFile(sb.ToString(), linksFileName);
        }

        private void SaveConfig(ConfigData config)
        {
            SaveFile(config.GetJson(), configSavePath);
        }

        private ConfigData LoadConfig()
        {
            var isExist = TryReadFile(configSavePath, out string data);

            if (isExist == false)
            {
                var config = ConfigData.GetDefault();
                SaveConfig(config);
                return config;
            }

            return ConfigData.Parse(data);
        }

        private bool TryReadFile(string path, out string data)
        {
            if (File.Exists(path) == false)
            {
                data = "";
                return false;
            }

            using (StreamReader sr = new StreamReader(path))
            {
                data = sr.ReadToEnd();
                return true;
            }

        }

        private void SaveFile(string data, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(data);
            }
        }



        private void OnVideoGet(VideoInfo videoInfo)
        {
            var find = playlists.Find((x) => x.Id == videoInfo.Playlist);

            if (find == null)
            {
                find = new Playlist(videoInfo.Playlist);
                playlists.Add(find);
            }

            find.AddVideo(videoInfo);

            this.Dispatcher.Invoke(() =>
            {
                videoScannedCount++;
                labelResult.Content = string.Format("Scanned {0} video", videoScannedCount);
            });
        }

        private void Scan()
        {
            playlists.Clear();
            videoScannedCount = 0;
            spRemovedVideos.Children.Clear();
            btnScan.IsEnabled = false;
            labelResult.Content = "scanning...";

            var links = new TextRange(rtbLinks.Document.ContentStart, rtbLinks.Document.ContentEnd).Text;
            var linksArray = links.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);


            yudl.GetInfo(linksArray, OnVideoGet, OnScanEnd);
        }

        private void OnScanEnd(bool isOk)
        {
            foreach (var playlist in playlists)
            {
                var path = GetPlayListPath(playlist);

                var isExist = TryReadFile(path, out string readData);
                if (isExist)
                {
                    var prevData = Playlist.FromJson(readData);

                    foreach (var prevVideo in prevData.GetVideos())
                    {
                        if (playlist.Contains(prevVideo) == false)
                        {
                            playlist.AddVideo(prevVideo);
                            AddRemovedVideo(prevVideo);
                        }
                    }
                }

                SaveFile(playlist.GetJson(), path);
            }

            this.Dispatcher.Invoke(() =>
            {
                btnScan.IsEnabled = true;
                labelResult.Content = string.Format("Find {0} removed video. Total: {1}", spRemovedVideos.Children.Count, videoScannedCount);
            });
        }

        private void AddRemovedVideo(LinkInfo linkInfo)
        {
            this.Dispatcher.Invoke(() =>
            {
                var control = new RemoveVideoControl();
                control.SetInfo(linkInfo);
                control.OnRemoveClick += RemoveVideo;
                spRemovedVideos.Children.Add(control);
            });

            Console.WriteLine("video removed " + linkInfo.ToString());
        }

        private void RemoveVideo(RemoveVideoControl control)
        {
            var info = control.GetCurrentInfo();
            spRemovedVideos.Children.Remove(control);

            foreach (var playlist in playlists)
            {
                playlist.Remove(info);
                SaveFile(playlist.GetJson(), GetPlayListPath(playlist));
            }

        }

        private string GetPlayListPath(Playlist playlist)
        {
            return playlistSavePath + PathUtils.RemoveillegalChars(playlist.Id) + ".txt";
        }




        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            Scan();
        }

        private void CbEnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (cbEnEdit.IsChecked == false)
            {
                SaveLinks();
                rtbLinks.IsReadOnly = true;
            }
            else
            {
                rtbLinks.IsReadOnly = false;
            }
        }

        private void CbAutoStart_Click(object sender, RoutedEventArgs e)
        {
            const string pathRegistryKeyStartup = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            if (cbAutoStart.IsChecked == false)
            {
                config.StartWithSystem = false;

                using (Microsoft.Win32.RegistryKey registryKeyStartup =
                            Microsoft.Win32.Registry.CurrentUser.OpenSubKey(pathRegistryKeyStartup, true))
                {
                    registryKeyStartup.DeleteValue(applicationName, false);
                }
            }
            else
            {
                config.StartWithSystem = true;

                using (Microsoft.Win32.RegistryKey registryKeyStartup =
                            Microsoft.Win32.Registry.CurrentUser.OpenSubKey(pathRegistryKeyStartup, true))
                {
                    registryKeyStartup.SetValue(
                        applicationName,
                        string.Format("\"{0}\"", System.Reflection.Assembly.GetExecutingAssembly().Location));
                }
            }

            SaveConfig(config);
        }

        private void CbAutoScan_Click(object sender, RoutedEventArgs e)
        {
            if (cbAutoScan.IsChecked == false)
            {
                config.AutoScanAtStart = false;
            }
            else
            {
                config.AutoScanAtStart = true;
            }

            SaveConfig(config);
        }

    }
}
