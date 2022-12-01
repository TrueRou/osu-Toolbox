using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using osu_Toolbox.Elements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Net.Http;
using System;
using System.Linq;
using System.Windows.Threading;

namespace osu_Toolbox
{
    /// <summary>
    /// MapDownload.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadManager : UserControl
    {
        public static readonly ObservableCollection<MapCard> MapCards = new();
        private static Dispatcher UIDispatcher;

        public DownloadManager()
        {
            UIDispatcher = Dispatcher;
            InitializeComponent();
            ItemsControl_Main.ItemsSource = MapCards;
        }

        public static void AddQueueBeatmaps(List<QueueBeatmap> queueBeatmaps)
        {
            foreach (var item in queueBeatmaps)
            {
                UpdateUI(() => MapCards.Add(new MapCard(item, new SayoBeatmapSource())));
            }
        }

        public static void UpdateUI(Action func)
        {
            UIDispatcher.Invoke(func);
        }

        public class QueueBeatmap
        {
            public string CoverLink;
            public int StarValue;
            public string Title;
            public string Difficulty;
            public int BeatmapID;
            public int BeatmapSetID;
            public string Creator;
            public string Md5Hash;
        }

        public interface IBeatmapSource
        {
            string GetPage(int bid);
            string GetDownloadLink(int sid);
            QueueBeatmap GetBeatmapInformation(int bid);
        }

        public class SayoBeatmapSource : IBeatmapSource
        {
            public QueueBeatmap GetBeatmapInformation(int bid)
            {
                var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "http://api.sayobot.cn/v2/beatmapinfo?K=" + bid + "?T=1");
                var response = httpClient.Send(request);
                if (!response.IsSuccessStatusCode) return null;
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var responseBody = reader.ReadToEnd();
                var json = (JObject)JsonConvert.DeserializeObject(responseBody);
                var beatmapSid = json["data"]["sid"].ToString();
                var beatmaps = json["data"]["bid_data"];
                var singleBeatmap = (from beatmap in beatmaps where beatmap["bid"].ToString() == bid.ToString() select beatmap).First();
                if (singleBeatmap == null) return null;
                return new QueueBeatmap()
                {
                    CoverLink = "https://cdn.sayobot.cn:25225/beatmaps/" + beatmapSid + "/covers/cover.jpg",
                    BeatmapID = bid,
                    BeatmapSetID = int.Parse(beatmapSid),
                    StarValue = (int)double.Parse(singleBeatmap["star"].ToString()),
                    Title = json["data"]["title"].ToString(),
                    Difficulty = singleBeatmap["version"].ToString(),
                    Creator = json["data"]["creator"].ToString()
                };
            }

            public string GetDownloadLink(int sid)
            {
                return "https://txy1.sayobot.cn/beatmaps/download/novideo/" + sid;
            }

            public string GetPage(int bid)
            {
                return "https://osu.ppy.sh/b/" + bid;
            }
        }
    }
}
