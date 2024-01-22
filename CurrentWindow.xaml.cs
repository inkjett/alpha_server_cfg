using System;
using System.Collections.Generic;
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

namespace alphaserver_cfg
{
    /// <summary>
    /// Логика взаимодействия для CurrentWindow.xaml
    /// </summary>
    public partial class CurrentWindow : Window
    {
        public CurrentWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("WPF на METANIT.COM");
        }
    }
}
