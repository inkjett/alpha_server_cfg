using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using alphaserver_cfg;



namespace alphaserver_cfg
{
    /// <summary>
    /// Логика взаимодействия для CurrentWindow.xaml
    /// </summary>
    //static FuncData FuncData = new FuncData();
    public partial class CurrentWindow : Window
    {
        public CurrentWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_install(object sender, RoutedEventArgs e)
        {
            FuncData.regData("install", Program.CurrentDir);
            Close();
        }

        private void Button_Click_uninstall(object sender, RoutedEventArgs e)
        {
            FuncData.regData("uninstall", Program.CurrentDir);
            Close();
        }
        private void Button_Click_info(object sender, RoutedEventArgs e)
        {
            //FuncData.regData("uninstall", Program.CurrentDir);
            Close();
        }

    }
}
