using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace osu_Toolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Dispatcher UIDispatcher;
        private static MainWindow Instance;
        

        public MainWindow()
        {
            Instance = this;
            UIDispatcher = Dispatcher;
            InitializeComponent();
        }

        private void Drag(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GoHome(object sender, RoutedEventArgs e)
        {
            Transitioner_Main.SelectedIndex = 0;
        }

        private void GoCollection(object sender, RoutedEventArgs e)
        {
            if (CollectionManager.CollectionNodes == null) CollectionManager.BuildTree();
            Transitioner_Main.SelectedIndex = 1;
        }

        private void GoDownload(object sender, RoutedEventArgs e)
        {
            Transitioner_Main.SelectedIndex = 2;
        }

        private void Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        public static void ShowDialog(object obj)
        {
            UpdateUI(() => Instance.DialogHost_Main.ShowDialog(obj));
        }

        public static void CloseDialog()
        {
            UpdateUI(() => Instance.DialogHost_Main.IsOpen = false);
        }

        public static void ShowMessage(object obj)
        {
            UpdateUI(() => Instance.Snackbar_Main.MessageQueue.Enqueue(obj));
        }

        public static void UpdateUI(Action func) {
            UIDispatcher.Invoke(func);
        }
    }
}
