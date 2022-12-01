using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu_Toolbox.Elements.Import;
using osu_Toolbox.Views.Dialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static osu_Toolbox.SampleOsuDbReaderExtensions;
using Collection = OsuParsers.Database.Objects.Collection;

namespace osu_Toolbox
{
    /// <summary>
    /// BeatmapShare.xaml 的交互逻辑
    /// </summary>
    public partial class CollectionManager : UserControl
    {
        private static ObservableCollection<CollectionNode> collectionNodes;

        public static ObservableCollection<CollectionNode> CollectionNodes { get => collectionNodes; set => collectionNodes = value; }

        public static Dispatcher UIDispatcher { get; set; }
        public CollectionManager()
        {
            InitializeComponent();
            UIDispatcher = Dispatcher;
        }
        
        public void BuildTree()
        {
            CollectionNodes = new ObservableCollection<CollectionNode>();
            MainWindow.ShowDialog(new ProgressDialog(() => {
                Toolbox.OsuData.LoadCollectionData();
                var listBeatmaps = new List<SimpleBeatmap>(Toolbox.OsuData.GetBeatmapEnumerator());
                foreach (var item in Toolbox.OsuData.Collections)
                {
                    var collectionNode = new CollectionNode() { Name = item.Name };
                    var result = item.MD5Hashes.Join(listBeatmaps, hash => hash, map => map.Md5Hash, (p, map) => map);
                    foreach (var map in result)
                    {
                        collectionNode.Children.Add(new BeatmapNode(map, collectionNode));
                    }
                    UpdateUI(() => CollectionNodes.Add(collectionNode));
                }
            }));
            UpdateUI(() => TreeView_Main.ItemsSource = collectionNodes);
        }

        private void Button_AddMap_Click(object sender, RoutedEventArgs e)
        {
            if (TreeView_Main.SelectedItem is CollectionNode) 
            {
                MainWindow.ShowDialog(new TextboxDialog("请输入BeatmapID", (text) => {
                    var enumerator = Toolbox.OsuData.GetBeatmapEnumerator();
                    var result = enumerator.Where((map) => {
                        return map.BeatmapId == Convert.ToInt32(text);
                    });
                    foreach (var item in result)
                    {
                        var node = TreeView_Main.SelectedItem as CollectionNode;
                        node.Children.Add(new BeatmapNode(item, node));
                    }
                }));
            }
        }

        private void Button_AddCollection_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowDialog(new TextboxDialog("请输入收藏夹名称", (text) => {
                CollectionNodes.Add(new CollectionNode() { Name = text });
            }));
        }

        private void Buttom_RemoveCollection_Click(object sender, RoutedEventArgs e)
        {
            if (TreeView_Main.SelectedItem is CollectionNode)
            {
                var collectionNode = TreeView_Main.SelectedItem as CollectionNode;
                MainWindow.ShowDialog(new DualButtonDialog("确认要删除 " + collectionNode.Name +  " 吗", () => {
                    CollectionNodes.Remove(collectionNode);
                }));
            }
        }

        private void Button_ImportCollection_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SectionDialog("选择一个导入方式", "缺失的谱面会调用Sayobot源进行补全");
            dialog.AddSection("手动批量导入", (sender, e) => {
                MainWindow.CloseDialog();
                MainWindow.ShowDialog(new ManualImport());
            });
            dialog.AddSection("从谱面列表(.json)导入", (sender, e) => {
                JsonImport.CreateInstanceAndShow();
            });
            dialog.AddSection("从收藏数据(.db)导入", (sender, e) => {

            });
            dialog.AddSection("从压缩包(.zip)导入", (sender, e) => {

            });
            MainWindow.ShowDialog(dialog);
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            var newCollections = CollectionNode.GetDbCollections(CollectionNodes);
            Toolbox.OsuData.SaveCollections(newCollections);
        }

        private void Button_ExportCollection_Click(object sender, RoutedEventArgs e)
        {
            if (TreeView_Main.SelectedItem is CollectionNode)
            {
                var collectionNode = TreeView_Main.SelectedItem;
                
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "选择谱面列表的保存位置",
                    Filter = "谱面列表(.json)|*.json",
                    FilterIndex = 1
                };
                if ((bool)saveFileDialog.ShowDialog())
                {
                    File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(JObject.FromObject(collectionNode), Formatting.Indented));
                }
            }
        }

        private void TreeView_Main_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();
            if (TreeView_Main.SelectedItem is CollectionNode)
            {
                var node = TreeView_Main.SelectedItem as CollectionNode;

                var itemRemove = new MenuItem() { Header = "删除" };
                itemRemove.Click += Buttom_RemoveCollection_Click;
                contextMenu.Items.Add(itemRemove);

                var itemRename = new MenuItem() { Header = "重命名" };
                itemRename.Click += (sender, e) => {
                    MainWindow.ShowDialog(new TextboxDialog("请输入新的收藏夹名称", (text) => {
                        node.Name = text;
                    }, true));
                };
                contextMenu.Items.Add(itemRename);

                var itemsCutTo = new MenuItem() { Header = "全部剪切到" };
                itemsCutTo.Click += (sender, e) =>
                {
                    SelectParent((newParent) => {
                        foreach (var item in node.Children)
                        {
                            newParent.Children.Add(item.Clone(newParent));
                        }
                        node.Children.Clear();
                    });
                };
                contextMenu.Items.Add(itemsCutTo);

                var itemsCopyTo = new MenuItem() { Header = "全部复制到" };
                itemsCopyTo.Click += (sender, e) =>
                {
                    SelectParent((newParent) => {
                        foreach (var item in node.Children)
                        {
                            newParent.Children.Add(item.Clone(newParent));
                        }
                    });
                };
                contextMenu.Items.Add(itemsCopyTo);

                contextMenu.Items.Add(new Separator());

                var itemCopy = new MenuItem() { Header = "制作副本" };
                itemCopy.Click += (sender, e) => {
                    var copy = new CollectionNode() { Name = node.Name + " 副本" };
                    foreach (var item in node.Children)
                    {
                        copy.Children.Add(item.Clone(copy));
                    }
                    CollectionNodes.Add(copy);
                };
                contextMenu.Items.Add(itemCopy);

                var itemExport = new MenuItem() { Header = "导出为Json" };
                itemExport.Click += Button_ExportCollection_Click;
                contextMenu.Items.Add(itemExport);
            }
            if (TreeView_Main.SelectedItem is BeatmapNode)
            {
                var node = TreeView_Main.SelectedItem as BeatmapNode;

                var itemRemove = new MenuItem() { Header = "删除" };
                itemRemove.Click += (sender, e) =>
                {
                    MainWindow.ShowDialog(new DualButtonDialog("确认要从收藏夹中移除: " + node.Title + "("+ node.Difficulty + ") 吗？", () => {
                        node.Parent.Children.Remove(node);
                    }));
                };
                contextMenu.Items.Add(itemRemove);

                var itemCutTo = new MenuItem() { Header = "剪切到" };
                itemCutTo.Click += (sender, e) =>
                {
                    SelectParent((newParent) => {
                        newParent.Children.Add(node.Clone(newParent));
                        node.Parent.Children.Remove(node);
                    });
                };
                contextMenu.Items.Add(itemCutTo);

                var itemCopyTo = new MenuItem() { Header = "复制到" };
                itemCopyTo.Click += (sender, e) =>
                {
                    SelectParent((newParent) => {
                        newParent.Children.Add(node.Clone(newParent));
                    });
                };
                contextMenu.Items.Add(itemCopyTo);
            }
            TreeView_Main.ContextMenu = contextMenu;
        }
        
        private void SelectParent(Action<CollectionNode> action)
        {
            var dialog = new SectionDialog("选择一个目标收藏夹", "选中的该谱面会移动或复制到目标收藏夹中");
            CollectionNode parent;
            foreach (var item in CollectionNodes)
            {
                dialog.AddSection(item.Name, (sender, e) => {
                    parent = item;
                    action.Invoke(parent);
                    MainWindow.CloseDialog();
                });
            }
            MainWindow.ShowDialog(dialog);
        }
        public static void UpdateUI(Action func)
        {
            UIDispatcher.Invoke(func);
        }
    }
    public class CollectionNode : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Name"));     
            }
        }
        public ObservableCollection<BeatmapNode> Children { get; set; }

        public CollectionNode()
        {
            Children = new ObservableCollection<BeatmapNode>();
        }

        public static List<Collection> GetDbCollections(Collection<CollectionNode> collectionNodes)
        {
            var list = new List<Collection>();
            foreach (var node in collectionNodes)
            {
                var collection = new Collection
                {
                    Name = node.Name
                };
                foreach (var item in node.Children)
                {
                    collection.MD5Hashes.Add(item.Md5);
                }
                collection.Count = collection.MD5Hashes.Count;
                list.Add(collection);
            }
            return list;
        }
    }

    public class BeatmapNode
    {
        public string Title { get; set; }
        public string Difficulty { get; set; }
        public int BeatmapID { get; set; }
        public int BeatmapSID { get; set; }
        public string Md5 { get; set; }
        [JsonIgnore]
        public CollectionNode Parent { get; set; }
        [JsonIgnore]
        public string Background { get; set; }

        public BeatmapNode(string title, string difficulty, int beatmapID, int beatmapSID, string md5, CollectionNode parent)
        {
            Title = title;
            Difficulty = difficulty;
            BeatmapID = beatmapID;
            BeatmapSID = beatmapSID;
            Md5 = md5;
            Parent = parent;
        }

        public BeatmapNode() { }

        public BeatmapNode(SimpleBeatmap beatmap, CollectionNode parent) : this(beatmap.Title, beatmap.Difficulty, beatmap.BeatmapId, beatmap.BeatmapSetId, beatmap.Md5Hash, parent)
        {
            var files = Directory.GetFiles(Path.Combine(Toolbox.ClientPath, "Songs", beatmap.FolderName), "*.jpg");
            if (files.Length != 0)
            {
                Background = files[0];
            }
        }
        public BeatmapNode Clone(CollectionNode newParent, bool removeHeader = false)
        {
            var newTitle = Title;
            if (removeHeader) newTitle = newTitle.Replace("(需要下载) ", "");
            return new BeatmapNode(newTitle, Difficulty, BeatmapID, BeatmapSID, Md5, newParent);
        }
    }
}
