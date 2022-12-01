using System;
using System.Collections.Generic;
using System.Text;
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
    /// DualButtonDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DualButtonDialog : UserControl
    {
        readonly Action Action1, Action2;

        public DualButtonDialog(string message, Action action1, Action action2)
        {
            InitializeComponent();
            textBlock_Message.Text = message;
            Action1 = action1;
            Action2 = action2;
        }

        public DualButtonDialog(string message, Action action1)
        {
            InitializeComponent();
            textBlock_Message.Text = message;
            Action1 = action1;
        }

        public void Rename(string name1, string name2)
        {
            button_1.Content = name1;
            button_2.Content = name2;
        }

        private void Button_1_Click(object sender, RoutedEventArgs e)
        {
            Action1.Invoke();
        }

        private void Button_2_Click(object sender, RoutedEventArgs e)
        {
            if (Action2 != null)
            {
                Action2.Invoke();
            }
        }
    }
}
