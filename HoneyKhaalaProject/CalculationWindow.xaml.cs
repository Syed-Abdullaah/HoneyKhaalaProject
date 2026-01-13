// Plan (pseudocode):
// 1. Provide a public constructor CalculationWindow(string storageFolder) so MainWindow can create the window.
// 2. Call InitializeComponent() to ensure XAML is loaded.
// 3. Store the provided storageFolder in a private readonly field for later use in this window.
// 4. Do not change other types or behavior; keep this file minimal and focused on resolving CS1729.

using HoneyKhaalaProject.VM;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace HoneyKhaalaProject
{
    public partial class CalculationWindow : Window
    {
        string totalDisplay = "$0.00";

        public string TotalDisplay                  
        {
            get => totalDisplay;
            set {
                totalDisplay = value;
              //  OnPropertyChanged(nameof(TotalDisplay));
            }
        }

        public ObservableCollection<Investor> Investors { get; } = new ObservableCollection<Investor>();

        private readonly string _storageFolder;

        [Obsolete("For designer support only. Use other overload for normal construction.")]
        public CalculationWindow()
        {
            _storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoneyKhaalaProject", "Months");
            Directory.CreateDirectory(_storageFolder);

            InitializeComponent();
            this.DataContext = this;
            CreateDummyInvestors();
        }

        public CalculationWindow(string storageFolder)
        {
            _storageFolder = string.IsNullOrWhiteSpace(storageFolder)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoneyKhaalaProject", "Months")
                : storageFolder;
            Directory.CreateDirectory(_storageFolder);

            InitializeComponent();
            this.DataContext = this;

            CreateDummyInvestors();
        }

        private void CreateDummyInvestors()
        {
            // start with two example rows to make UI friendlier
            Investors.Add(new Investor { Name = "Investor A", Amount = 10000 });
            Investors.Add(new Investor { Name = "Investor B", Amount = 10000 });
            Investors.Add(new Investor { Name = "Investor C", Amount = 10000 });
            Investors.Add(new Investor { Name = "Investor D", Amount = 10000 });
            Investors.Add(new Investor { Name = "Investor E", Amount = 10000 });
            Investors.Add(new Investor { Name = "Investor F", Amount = 10000 });
            Investors.Add(new Investor { Name = "Investor G", Amount = 10000 });
        }

        private void AddInvestor_Click(object sender, RoutedEventArgs e)
        {
            int next = Investors.Count + 1;
            Investors.Add(new Investor { Name = $"Investor {next}", Amount = 0 });
        }

        private void RemoveInvestor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is Investor inv)
            {
                Investors.Remove(inv);
            }
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            double total = Investors.Sum(i => i.Amount);
            TotalDisplay = total.ToString("C2");

            // update each investor's percentage, formatted strings and bar width
            foreach (var inv in Investors)
            {
                if (total > 0)
                    inv.Percentage = inv.Amount / total * 100.0;
                else
                    inv.Percentage = 0;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var file = Path.Combine(_storageFolder, $"{DateTime.Now:yyyy-MM}.txt");
            JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };
            var data = System.Text.Json.JsonSerializer.Serialize(Investors.ToArray(), options);
            File.WriteAllText(file, data);
        }

        // Show a context menu with current + previous months; open selected month's text file.
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement placementTarget) return;

            var menu = new ContextMenu();

            // include current month (i = 0) and previous 11 months (adjust count as desired)
            for (int i = 0; i <= 11; i++)
            {
                var m = DateTime.Now.AddMonths(-i);
                var mi = new MenuItem
                {
                    Header = m.ToString("MMMM yyyy"),
                    Tag = m
                };
                mi.Click += MonthMenuItem_Click;
                menu.Items.Add(mi);
            }

            menu.PlacementTarget = placementTarget;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        }

        private void MonthMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.Tag is DateTime month)
            {
                OpenMonthFile(month);
            }
        }

        private void OpenMonthFile(DateTime month)
        {
            try
            {
                var file = Path.Combine(_storageFolder, $"{month:yyyy-MM}.txt");
                if (!File.Exists(file))
                {
                    // create an empty file so the OS can open it
                    File.WriteAllText(file, string.Empty);
                }

                Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not open file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
