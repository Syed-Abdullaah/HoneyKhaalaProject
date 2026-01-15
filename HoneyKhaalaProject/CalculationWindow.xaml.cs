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
using System.Windows.Input;
using System.Windows.Media;

namespace HoneyKhaalaProject
{
    public partial class CalculationWindow : Window, INotifyPropertyChanged
    {
        readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-US");

        string totalInvestmentDisplay = "$0.00";
        public string TotalInvestmentDisplay
        {
            get => totalInvestmentDisplay;
            set { totalInvestmentDisplay = value; OnPropertyChanged(nameof(TotalInvestmentDisplay)); }
        }

        public ObservableCollection<Investor> Investors { get; } = new ObservableCollection<Investor>();

        // fixed business list (not editable per your requirement)
        public ObservableCollection<BusinessEntry> Businesses { get; } = new ObservableCollection<BusinessEntry>();

        private readonly string _storage_folder;
        private DateTime _editingMonth;
        private string selectedMonthDisplay = "";
        public string SelectedMonthDisplay
        {
            get => selectedMonthDisplay;
            set { selectedMonthDisplay = value; OnPropertyChanged(nameof(SelectedMonthDisplay)); }
        }

        public CalculationWindow()
        {
            _storage_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoneyKhaalaProject", "Months");
            Directory.CreateDirectory(_storage_folder);

            InitializeComponent();
            this.DataContext = this;

            InitializeBusinesses();
            _editingMonth = DateTime.Now;
            SelectedMonthDisplay = _editingMonth.ToString("MMMM yyyy", DisplayCulture);
            CreateDummyInvestors();
        }

        public CalculationWindow(string storageFolder) : this()
        {
            if (!string.IsNullOrWhiteSpace(storageFolder))
            {
                Directory.CreateDirectory(storageFolder);
            }
        }

        public CalculationWindow(string storageFolder, DateTime month) : this(storageFolder)
        {
            _editingMonth = month;
            SelectedMonthDisplay = _editingMonth.ToString("MMMM yyyy", DisplayCulture);
            LoadMonthFile(month);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Select the text in a TextBox when it receives focus.
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(SelectivelyIgnoreMouseButton));
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotKeyboardFocusEvent,
                new RoutedEventHandler(SelectAllText));
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.MouseDoubleClickEvent,
                new RoutedEventHandler(SelectAllText));
        }

        void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focused, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }

        void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }

        void InitializeBusinesses()
        {
            var list = new[]
            {
                "Right Choice Enterprise ltd",
                "Clinical Diagnostic Services Ltd",
                "Evergreen Estates Manchester ltd",
                "Multinational Foods ltd",
                "Programmers hut"
            };

            Businesses.Clear();
            foreach (var b in list)
                Businesses.Add(new BusinessEntry { Id = Guid.NewGuid().ToString(), Name = b, Profit = 0 });
        }

        private void CreateDummyInvestors()
        {
            AddInvestor("Investor A", 10000);
            AddInvestor("Investor B", 10000);
            AddInvestor("Investor C", 10000);
            UpdateTotalsAndDisplays();
        }

        void AddInvestor(string name, double defaultAmount)
        {
            var inv = new Investor { Name = name, Amount = defaultAmount };
            foreach (var b in Businesses)
            {
                inv.Contributions.Add(new Contribution { BusinessId = b.Id, BusinessName = b.Name, Amount = 0 });
            }
            Investors.Add(inv);

            foreach (var b in Businesses)
            {
                b.Total = Investors.SelectMany(i => i.Contributions.Where(c => c.BusinessId == b.Id)).Sum(c => c.Amount);
            }
        }

        private void AddInvestor_Click(object sender, RoutedEventArgs e)
        {
            int next = Investors.Count + 1;
            AddInvestor($"Investor {next}", 0);
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
            // 1) parse contributions' text values into numeric Amount before calculations
            foreach (var inv in Investors)
            {
                // NEW: update investor legacy Amount to the sum of their contributions so UI and percentage use the correct total
                inv.Amount = inv.Contributions.Sum(c => c.Amount);
            }

            // 3) reset investor totals
            foreach (var inv in Investors)
            {
                inv.ProfitShare = 0;
                inv.DisplayProfit = "$0.00";
            }

            // 4) for each business, compute contributions total and distribute that business's profit
            foreach (var b in Businesses)
            {
                double totalContrib = Investors.Sum(inv => inv.Contributions.First(c => c.BusinessId == b.Id).Amount);

                if (totalContrib <= 0)
                {
                    foreach (var inv in Investors)
                    {
                        var c = inv.Contributions.First(cc => cc.BusinessId == b.Id);
                        c.ProfitShare = 0;
                    }
                    continue;
                }

                foreach (var inv in Investors)
                {
                    var c = inv.Contributions.First(cc => cc.BusinessId == b.Id);
                    var percent = c.Amount / totalContrib; // 0..1
                    c.ProfitShare = percent * b.Profit;

                    // accumulate to investor total
                    inv.ProfitShare += c.ProfitShare;
                }

                b.Total = Investors.SelectMany(i => i.Contributions.Where(c => c.BusinessId == b.Id)).Sum(c => c.Amount);
            }

            // 5) update investor display profit
            foreach (var inv in Investors)
            {
                inv.DisplayProfit = inv.ProfitShare.ToString("C2", DisplayCulture);
            }

            UpdateTotalsAndDisplays();
        }

        private void UpdateTotalsAndDisplays()
        {
            double total = Investors.Sum(inv => inv.Contributions.Sum(c => c.Amount));
            TotalInvestmentDisplay = total.ToString("C2", DisplayCulture);

            // Ensure investor % values reflect updated legacy Amount (sum of contributions)
            foreach (var inv in Investors)
            {
                inv.Percentage = total > 0 ? inv.Amount / total * 100.0 : 0;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Calculate_Click(this, new RoutedEventArgs());

            var file = Path.Combine(_storage_folder, $"{_editingMonth:yyyy-MM}.json");
            Directory.CreateDirectory(_storage_folder);
            JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };

            var monthData = new MonthData
            {
                Month = _editingMonth,
                Businesses = Businesses.Select(b => new BusinessEntry { Id = b.Id, Name = b.Name, Profit = b.Profit }).ToArray(),
                Investors = Investors.ToArray()
            };

            var data = System.Text.Json.JsonSerializer.Serialize(monthData, options);
            File.WriteAllText(file, data);

            MessageBox.Show(this, $"Saved data for {SelectedMonthDisplay}.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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
                var file = Path.Combine(_storage_folder, $"{month:yyyy-MM}.json");
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

                var md = JsonSerializer.Deserialize<MonthData>(data);
                if (md != null)
                {
                    var businessByName = Businesses.ToDictionary(b => b.Name, StringComparer.OrdinalIgnoreCase);

                    foreach (var bd in md.Businesses ?? Array.Empty<BusinessEntry>())
                    {
                        if (businessByName.TryGetValue(bd.Name, out var target))
                        {
                            target.Profit = bd.Profit;
                        }
                    }

                    Investors.Clear();
                    foreach (var inv in md.Investors ?? Array.Empty<Investor>())
                    {
                        var remapped = new List<Contribution>();

                        foreach (var c in inv.Contributions.ToList())
                        {
                            if (!string.IsNullOrWhiteSpace(c.BusinessName) && businessByName.TryGetValue(c.BusinessName, out var bMatch))
                            {
                                c.BusinessId = bMatch.Id;
                                c.BusinessName = bMatch.Name;
                                remapped.Add(c);
                            }
                            else
                            {
                                var byId = Businesses.FirstOrDefault(b => b.Id == c.BusinessId);
                                if (byId != null)
                                {
                                    c.BusinessName = byId.Name;
                                    remapped.Add(c);
                                }
                            }
                        }

                        foreach (var b in Businesses)
                        {
                            if (!remapped.Any(x => x.BusinessId == b.Id))
                            {
                                remapped.Add(new Contribution { BusinessId = b.Id, BusinessName = b.Name, Amount = 0 });
                            }
                            else
                            {
                                var c = remapped.First(cc => cc.BusinessId == b.Id);
                                c.BusinessName = b.Name;
                            }
                        }

                        inv.Contributions.Clear();
                        foreach (var c in remapped)
                            inv.Contributions.Add(c);

                        Investors.Add(inv);
                    }

                    Calculate_Click(this, new RoutedEventArgs());
                    return;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not open file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}
