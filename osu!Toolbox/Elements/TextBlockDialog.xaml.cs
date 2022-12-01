using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
    /// TextboxDialog.xaml 的交互逻辑
    /// </summary>
    public partial class TextBlockDialog : UserControl
    {
        public Action<string> Action { get; set; }
        public bool NoCheck { get; set; }

        public TextBlockDialog(string message)
        {
            InitializeComponent();
            textBlock_Message.Text = message;
        }
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CloseDialog();
        }
    }
}
