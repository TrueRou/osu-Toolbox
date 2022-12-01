using osu_Toolbox.Views.Dialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static osu_Toolbox.DownloadManager;

namespace osu_Toolbox.Elements.Import
{
    /// <summary>
    /// ManualImport.xaml 的交互逻辑
    /// </summary>
    public partial class ManualImport : UserControl
    {
        private readonly List<QueueBeatmap> QueueBeatmaps = new();
        private readonly IBeatmapSource beatmapSource = new SayoBeatmapSource();
        private static List<int> MapIDs;
        private static List<int> Lacks;


        public ManualImport()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox.Text == "") return;
            List<int> list = GetBeatmapIDs();
            if (list == null) return;
            MainWindow.CloseDialog();
            MainWindow.ShowDialog(new ProgressDialog(() => {
                MapIDs = list.Join(Toolbox.OsuData.GetBeatmapEnumerator(), a => a, m => m.BeatmapId, (p, map) => p).ToList();
                Lacks = list.Except(MapIDs).ToList();
                foreach (var item in Lacks)
                {
                    QueueBeatmaps.Add(beatmapSource.GetBeatmapInformation(item));
                }
            }, () =>
            {
                if (Lacks.Any())
                {
                    AddQueueBeatmaps(QueueBeatmaps);
                    MainWindow.ShowMessage($"缺失的{QueueBeatmaps.Count}个谱面已经开始下载");
                    MainWindow.ShowDialog(new TextBlockDialog("由于无法获知缺失谱面的Md5, 你的收藏没有被创建.\n请待缺失谱面下载完成后进入游戏.\n进入游戏过之后, 请重试这个步骤, 收藏即可正常创建"));
                }
                else
                {
                    InjectCollection();
                }
            }).SetTitle("正在获取谱面信息"));
        }

        private List<int> GetBeatmapIDs()
        {
            var maps = TextBox.Text.Split(Environment.NewLine.ToCharArray());
            List<int> returnValue = new();
            foreach (var map in maps)
            {
                if (string.IsNullOrEmpty(map)) continue;
                try { returnValue.Add(int.Parse(map)); }
                catch{ return null; }
            }
            return returnValue;
        }

        private static void InjectCollection()
        {
            MainWindow.CloseDialog();
            MainWindow.UpdateUI(() =>
            {
                var dialog = new TextboxDialog("请输入收藏夹名称", (name) =>
                {
                    MainWindow.ShowDialog(new ProgressDialog(() =>
                    {
                        var node = CreateCollectionNode(name);
                        CollectionManager.UpdateUI(() => CollectionManager.CollectionNodes.Add(node));
                    }));

                });
                MainWindow.ShowDialog(dialog);
            });
            
        }

        private static CollectionNode CreateCollectionNode(string name)
        {
            var result = new CollectionNode();
            var children = new ObservableCollection<BeatmapNode>();
            var simpleBeatmaps = MapIDs.Join(Toolbox.OsuData.GetBeatmapEnumerator(), a => a, m => m.BeatmapId, (p, map) => map);
            foreach (var item in simpleBeatmaps)
            {
                children.Add(new BeatmapNode(item, result));
            }
            result.Name = name;
            result.Children = children;
            return result;
        }
    }
}
