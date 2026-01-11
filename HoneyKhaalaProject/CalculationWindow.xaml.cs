// Plan (pseudocode):
// 1. Provide a public constructor CalculationWindow(string storageFolder) so MainWindow can create the window.
// 2. Call InitializeComponent() to ensure XAML is loaded.
// 3. Store the provided storageFolder in a private readonly field for later use in this window.
// 4. Do not change other types or behavior; keep this file minimal and focused on resolving CS1729.

using HoneyKhaalaProject.VM;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

        [Obsolete("For designer support only. Use other overload for normal construction.")]
        public CalculationWindow()
        {
            InitializeComponent();
            CreateDummyInvestors();
        }

        public CalculationWindow(string storageFolder)
        {
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
            var storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoneyKhaalaProject", "Months");
            var file = Path.Combine(storageFolder, $"{DateTime.Now:yyyy-MM}.txt");
            JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };
            var data = System.Text.Json.JsonSerializer.Serialize(Investors.ToArray(), options);
            File.WriteAllText(file, data);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoneyKhaalaProject", "Months");
            var file = Path.Combine(storageFolder, $"{DateTime.Now:yyyy-MM}.txt");
            var data = File.ReadAllText(file);
            var data2 = JsonSerializer.Deserialize<Investor[]>(data);


            if (data2 != null)
            {
                Investors.Clear();
                foreach (var item in data2)
                {
                    Investors.Add(item);
                }
            }
        }
    }
}
