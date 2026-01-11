using HoneyKhaalaProject.VM;
using System.ComponentModel;
using System.Windows;

namespace HoneyKhaalaProject
{
    public partial class MainWindow : Window
    {
        private MainVM? VM => this.DataContext as MainVM;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainVM();
        }

        private void CurrentMonth_Click(object sender, RoutedEventArgs e)
        {
            if (VM != null)
            {
                var calc = new CalculationWindow(VM.StorageFolder);
                calc.Owner = this;
                calc.ShowDialog();
            }
        }
    }
}