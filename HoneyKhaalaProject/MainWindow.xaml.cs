using HoneyKhaalaProject.VM;
using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;

namespace HoneyKhaalaProject
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer? monthTimer;
        private MainVM? VM => this.DataContext as MainVM;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainVM();

            // schedule update at next midnight and then every 24h
            StartMonthRolloverTimer();
        }

        void StartMonthRolloverTimer()
        {
            var vm = VM;
            if (vm == null) return;

            var nextMidnight = DateTime.Today.AddDays(1);
            var initial = nextMidnight - DateTime.Now;

            monthTimer = new DispatcherTimer { Interval = initial };
            monthTimer.Tick += (s, e) =>
            {
                vm.RefreshMonths();
                monthTimer!.Interval = TimeSpan.FromDays(1); // subsequent ticks daily
            };
            monthTimer.Start();
        }

        private void CurrentMonth_Click(object sender, RoutedEventArgs e)
        {
            if (VM != null)
            {
                var calc = new CalculationWindow(VM.StorageFolder);
                calc.Owner = this;
                calc.ShowDialog();
                VM.RefreshMonths(); // refresh after dialog in case user saved
            }
        }

        // New handler: open CalculationWindow for selected previous month so user can view/edit investors and profit
        private void PreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is DateTime month && VM != null)
            {
                var calc = new CalculationWindow(VM.StorageFolder, month)
                {
                    Owner = this
                };
                calc.ShowDialog();
                VM.RefreshMonths(); // refresh UI after possible save
            }
        }
    }
}