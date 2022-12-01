using MaterialDesignThemes.Wpf;
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
    /// SectionDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SectionDialog : UserControl
    {
        public SectionDialog(string text1, string text2)
        {
            InitializeComponent();
            textBlock_1.Text = text1;
            textBlock_2.Text = text2;
        }

        public void AddSection(string text, RoutedEventHandler action)
        {
            var button = new Button
            {
                Margin = new Thickness(5),
                Content = text,
            };
            button.Click += action;
            stackPanel.Children.Add(button);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CloseDialog();
        }
    }
}
