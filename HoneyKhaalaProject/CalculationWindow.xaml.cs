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
using System.Globalization;
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
        // force dollar formatting for display/parsing
        readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-US");

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
        public string SelectedMonthDisplay
        {
            get => selectedMonthDisplay;
            set
            {
                if (selectedMonthDisplay != value)
                {
                    selectedMonthDisplay = value;
                    OnPropertyChanged(nameof(SelectedMonthDisplay));
                }
            }
        }
        private readonly string _storageFolder;
        private DateTime _editingMonth;
        // Add this property to the CalculationWindow class to fix CS0103
        private string selectedMonthDisplay = "";

        [Obsolete("For designer support only. Use other overload for normal construction.")]
        public CalculationWindow()
        {
            _storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoneyKhaalaProject", "Months");
            Directory.CreateDirectory(_storageFolder);

            InitializeComponent();
            this.DataContext = this;

            _editingMonth = DateTime.Now;
            SelectedMonthDisplay = _editingMonth.ToString("MMMM yyyy", DisplayCulture);
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

            _editingMonth = DateTime.Now;
            // Replace this line in the obsolete constructor:
            // SelecteditMonthDisplay = _editingMonth.ToString("MMMM yyyy", DisplayCulture);
            // with:
            SelectedMonthDisplay = _editingMonth.ToString("MMMM yyyy", DisplayCulture);
            CreateDummyInvestors();
        }

        public CalculationWindow(string storageFolder, DateTime month) : this(storageFolder)
        {
            _editingMonth = month;
            SelectedMonthDisplay = _editingMonth.ToString("MMMM yyyy", DisplayCulture);
            LoadMonthFile(month);
        }

        private void CreateDummyInvestors()
        {
            Investors.Add(new Investor { Name = "Investor A", Amount = 10000 });
            Investors.Add(new Investor { Name = "Investor B", Amount = 10000 });
            Investors.Add(new Investor { Name = "Investor C", Amount = 10000 });

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

            // parse profit allowing currency symbol (use explicit culture)
            if (!double.TryParse(ProfitString, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, DisplayCulture, out double profit))
                profit = 0;

            // distribute profit based on percentage (percentage is 0..100)
            foreach (var inv in Investors)
            {
                inv.ProfitShare = inv.Percentage > 0 ? inv.Percentage / 100.0 * profit : 0;
                inv.DisplayProfit = inv.ProfitShare.ToString("C2", DisplayCulture);
            }
        }

        private void UpdateTotalsAndDisplays()
        {
            double total = Investors.Sum(i => i.Amount);
            TotalDisplay = total.ToString("C2", DisplayCulture);
            TotalInvestmentDisplay = total.ToString("C2", DisplayCulture);

            // update percentages
            foreach (var inv in Investors)
            {
                inv.Percentage = total > 0 ? inv.Amount / total * 100.0 : 0;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // ensure profit distribution is up-to-date before saving
            Calculate_Click(this, new RoutedEventArgs());

            // save MonthData for the currently edited month (_editingMonth)
            var file = Path.Combine(_storageFolder, $"{_editingMonth:yyyy-MM}.txt");
            Directory.CreateDirectory(_storageFolder);
            JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };

            // parse profit using same culture
            double.TryParse(ProfitString, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, DisplayCulture, out double profit);

            var monthData = new MonthData
            {
                Profit = profit,
                Investors = Investors.ToArray()
            };

            var data = System.Text.Json.JsonSerializer.Serialize(monthData, options);
            File.WriteAllText(file, data);

            MessageBox.Show(this, $"Saved data for {SelectedMonthDisplay}.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Show a context menu with current + previous months; load selected month's text file into UI.
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement placementTarget) return;

            var menu = new ContextMenu();

            for (int i = 0; i <= 11; i++)
            {
                var m = DateTime.Now.AddMonths(-i);
                var mi = new MenuItem
                {
                    Header = m.ToString("MMMM yyyy", DisplayCulture),
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
                _editingMonth = month;
                SelectedMonthDisplay = _editingMonth.ToString("MMMM yyyy", DisplayCulture);
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
                            inv.DisplayProfit = inv.ProfitShare.ToString("C2", DisplayCulture);
                            Investors.Add(inv);
                        }

                        ProfitString = md.Profit.ToString("N", DisplayCulture);
                        UpdateTotalsAndDisplays();
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
                        foreach (var inv in arr) Investors.Add(inv);
                        UpdateTotalsAndDisplays();
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
