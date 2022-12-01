using Newtonsoft.Json.Linq;
using osu_Toolbox.Views.Dialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using static osu_Toolbox.DownloadManager;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using UserControl = System.Windows.Controls.UserControl;

namespace osu_Toolbox.Elements.Import
{
    /// <summary>
    /// BeatmapsImport.xaml 的交互逻辑
    /// </summary>
    public partial class JsonImport : UserControl
    {
        public ObservableCollection<BeatmapNode> BeatmapNodes = new();
        private List<QueueBeatmap> QueueBeatmaps = new();
        private readonly IBeatmapSource beatmapSource = new SayoBeatmapSource();
        private string CollectionName;

        private JsonImport()
        {
            InitializeComponent();
            TreeView_Main.ItemsSource = QueueBeatmaps;
        }

        public static void CreateInstanceAndShow()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择谱面列表",
                Filter = "谱面列表(.json)|*.json",
                FilterIndex = 1
            };
            if ((bool)openFileDialog.ShowDialog())
            {
                MainWindow.CloseDialog();
                JsonImport jsonImport = new();
                MainWindow.ShowDialog(new ProgressDialog(() => {
                    var collectionNodes = JObject.Parse(File.ReadAllText(openFileDialog.FileName)).ToObject<CollectionNode>();
                    jsonImport.BeatmapNodes = collectionNodes.Children;
                    jsonImport.CollectionName = collectionNodes.Name + " 导入";
                    jsonImport.BuildQueue();
                }, () =>
                {
                    MainWindow.ShowDialog(jsonImport);
                }).SetTitle("正在从Sayobot获取谱面信息"));
            }
        }

        private void BuildQueue()
        {
            var result = BeatmapNodes.Join(Toolbox.OsuData.GetBeatmapEnumerator(), node => node.Md5, map => map.Md5Hash, (p, map) => p);
            foreach (var item in BeatmapNodes.Except(result))
            {
                item.Title = "(需要下载) " + item.Title;
                QueueBeatmaps.Add(beatmapSource.GetBeatmapInformation(item.BeatmapID));
            }
            var list = BeatmapNodes.OrderBy(w => w.Title).ToList();
            BeatmapNodes.Clear();
            foreach (var item in list)
            {
                BeatmapNodes.Add(item);
            }
            UpdateUI(() => TreeView_Main.ItemsSource = BeatmapNodes);
        }

        private void InjectCollection()
        {
            var collectionNode = CreateCollectionNode();
            CollectionManager.UpdateUI(() => CollectionManager.CollectionNodes.Add(collectionNode));
        }

        private CollectionNode CreateCollectionNode()
        {
            var result = new CollectionNode();
            var children = new ObservableCollection<BeatmapNode>();
            foreach (var item in BeatmapNodes)
            {
                children.Add(item.Clone(result, true));
            }
            result.Name = CollectionName;
            result.Children = children;
            return result;
        }

        private void Button_Build_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowDialog(new ProgressDialog(() => {
                InjectCollection();
                if (QueueBeatmaps.Count > 0)
                {
                    AddQueueBeatmaps(QueueBeatmaps);
                    MainWindow.ShowMessage($"缺失的{QueueBeatmaps.Count}个谱面已经开始下载");
                }
                MainWindow.ShowMessage($"\"{CollectionName}\" 已创建");
            }));
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CloseDialog();
        }

        public void UpdateUI(Action func)
        {
            Dispatcher.Invoke(func);
        }
    }
}
