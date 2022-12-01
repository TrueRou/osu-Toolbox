using Downloader;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static osu_Toolbox.DownloadManager;

namespace osu_Toolbox.Elements
{
    /// <summary>
    /// MapCard.xaml 的交互逻辑
    /// </summary>
    public partial class MapCard : UserControl
    {
        private readonly QueueBeatmap queueBeatmap;
        private readonly IBeatmapSource beatmapSource;

        public MapCard(QueueBeatmap beatmap, IBeatmapSource beatmapSource)
        {
            InitializeComponent();
            queueBeatmap = beatmap;
            this.beatmapSource = beatmapSource;
            BuildContents();
            BeginDownload();
        }

        public void BuildContents()
        {
            image.Source = GetBitImage(queueBeatmap.CoverLink);
            ratingBar.Value = queueBeatmap.StarValue;
            diffName.Text = string.Format("{0} by {1}", queueBeatmap.Difficulty, queueBeatmap.Creator);
            textName.Text = queueBeatmap.Title;
        }

        public void BeginDownload()
        {
            var downloader = new DownloadService();
            var link = beatmapSource.GetDownloadLink(queueBeatmap.BeatmapSetID);
            var path = Path.Combine(Toolbox.ClientPath, "Songs", queueBeatmap.BeatmapSetID.ToString() + ".osz");
            downloader.DownloadFileTaskAsync(link, path);
            downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
            downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
        }

        private void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            UpdateUI(() => MapCards.Remove(this));
        }

        private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UpdateUI(() => bar.Value = e.ProgressPercentage);
        }

        private void OpenLink_Button(object sender, RoutedEventArgs e)
        {
            var page = beatmapSource.GetPage(queueBeatmap.BeatmapID);
            Process.Start("explorer.exe", page);
        }

        public void UpdateUI(Action func)
        {
            Dispatcher.Invoke(func);
        }

        private static BitmapImage GetBitImage(string url)
        {
            BitmapImage bitImage = new();
            bitImage.BeginInit();
            bitImage.UriSource = new Uri(url, UriKind.Absolute);
            bitImage.EndInit();
            return bitImage;
        }
    }
}
