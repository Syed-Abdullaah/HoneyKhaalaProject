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
    public partial class CalculationWindow : Window, INotifyPropertyChanged
    {
        string totalDisplay = "$0.00";
        public string TotalDisplay
        {
            get => totalDisplay;
            set { totalDisplay = value; OnPropertyChanged(nameof(TotalDisplay)); }
        }

        string totalInvestmentDisplay = "$0.00";
        public string TotalInvestmentDisplay
        {
            get => totalInvestmentDisplay;
            set { totalInvestmentDisplay = value; OnPropertyChanged(nameof(TotalInvestmentDisplay)); }
        }

        string profitString = "0";
        public string ProfitString
        {
            get => profitString;
            set { profitString = value; OnPropertyChanged(nameof(ProfitString)); }
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

        public CalculationWindow(string storageFolder, DateTime month) : this(storageFolder)
        {
            // load the selected month's data into the window (shows data if file exists)
            LoadMonthFile(month);
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

            // update totals for initial data
            UpdateTotalsAndDisplays();
        }

        private void AddInvestor_Click(object sender, RoutedEventArgs e)
        {
            int next = Investors.Count + 1;
            Investors.Add(new Investor { Name = $"Investor {next}", Amount = 0 });
            UpdateTotalsAndDisplays();
        }

        private void RemoveInvestor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is Investor inv)
            {
                Investors.Remove(inv);
                UpdateTotalsAndDisplays();
            }
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            // compute totals
            UpdateTotalsAndDisplays();

            // parse profit (silent zero for invalid)
            if (!double.TryParse(ProfitString, out double profit))
                profit = 0;

            // distribute profit based on percentage (percentage is 0..100)
            foreach (var inv in Investors)
            {
                if (inv.Percentage > 0)
                    inv.ProfitShare = inv.Percentage / 100.0 * profit;
                else
                    inv.ProfitShare = 0;

                inv.DisplayProfit = inv.ProfitShare.ToString("C2");
            }
        }

        private void UpdateTotalsAndDisplays()
        {
            double total = Investors.Sum(i => i.Amount);
            TotalDisplay = total.ToString("C2");
            TotalInvestmentDisplay = total.ToString("C2");

            // update percentages
            foreach (var inv in Investors)
            {
                inv.Percentage = total > 0 ? inv.Amount / total * 100.0 : 0;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // save a small wrapper so profit & investor profit shares are persisted
            var file = Path.Combine(_storageFolder, $"{DateTime.Now:yyyy-MM}.txt");
            JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };

            // try parse profit, default to 0
            double.TryParse(ProfitString, out double profit);

            var monthData = new MonthData
            {
                Profit = profit,
                Investors = Investors.ToArray()
            };

            var data = System.Text.Json.JsonSerializer.Serialize(monthData, options);
            File.WriteAllText(file, data);
        }

        // Show a context menu with current + previous months; load selected month's text file into UI.
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
                LoadMonthFile(month);
            }
        }

        private void LoadMonthFile(DateTime month)
        {
            try
            {
                var file = Path.Combine(_storageFolder, $"{month:yyyy-MM}.txt");
                if (!File.Exists(file))
                {
                    // nothing to load
                    MessageBox.Show(this, $"No saved data for {month:MMMM yyyy}.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var data = File.ReadAllText(file);
                if (string.IsNullOrWhiteSpace(data))
                {
                    MessageBox.Show(this, $"File is empty for {month:MMMM yyyy}.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // try read wrapper first
                try
                {
                    var md = JsonSerializer.Deserialize<MonthData>(data);
                    if (md != null && md.Investors != null)
                    {
                        Investors.Clear();
                        foreach (var inv in md.Investors)
                        {
                            // populate display profit string if profit share exists
                            inv.DisplayProfit = inv.ProfitShare.ToString("C2");
                            Investors.Add(inv);
                        }

                        ProfitString = md.Profit.ToString();
                        UpdateTotalsAndDisplays();
                        // recompute profit shares if they were not serialized
                        Calculate_Click(this, new RoutedEventArgs());
                        return;
                    }
                }
                catch
                {
                    // ignore, try fallback
                }

                // fallback: old format - array of investors
                try
                {
                    var arr = JsonSerializer.Deserialize<Investor[]>(data);
                    if (arr != null)
                    {
                        Investors.Clear();
                        foreach (var inv in arr)
                            Investors.Add(inv);

                        UpdateTotalsAndDisplays();
                        // clear profit if none present
                        ProfitString = "0";
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not parse file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not open file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private record MonthData
        {
            public double Profit { get; init; }
            public Investor[]? Investors { get; init; }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}
