using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace osu_Toolbox.Views.Dialog
{
    /// <summary>
    /// ProgressDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressDialog : UserControl
    {
        public ProgressDialog(Action work, Action callback)
        {
            InitializeComponent();
            new Thread(new ThreadStart(() =>
            {
                work.Invoke();
                MainWindow.CloseDialog();
                callback.Invoke();
            })).Start();
        }

        public ProgressDialog SetTitle(string title)
        {
            TextBlock_Main.Text = title;
            TextBlock_Main.Visibility = Visibility.Visible;
            return this;
        }

        public ProgressDialog(Action work)
        {
            InitializeComponent();
            new Thread(new ThreadStart(() =>
            {
                work.Invoke();
                MainWindow.CloseDialog();
            })).Start();
        }
    }
}
