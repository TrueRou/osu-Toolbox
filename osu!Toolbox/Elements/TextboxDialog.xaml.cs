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
    public partial class TextboxDialog : UserControl
    {
        public Action<string> Action { get; set; }
        public bool NoCheck { get; set; }

        public TextboxDialog(string message, Action<string> action)
        {
            InitializeComponent();
            textBlock_Message.Text = message;
            Action = action;
        }
        public TextboxDialog(string message, Action<string> action, bool noCheck)
        {
            InitializeComponent();
            textBlock_Message.Text = message;
            Action = action;
            NoCheck = noCheck;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Regex regex = new Regex(@"^([a-zA-Z]:\\)?[^\/\:\*\?\""\<\>\|\,]*$");
            if (textBox.Text != "" && (regex.Match(textBox.Text).Success || NoCheck))
            {
                Action.Invoke(textBox.Text);
                MainWindow.CloseDialog();
            }
        }
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CloseDialog();
        }
    }
}
