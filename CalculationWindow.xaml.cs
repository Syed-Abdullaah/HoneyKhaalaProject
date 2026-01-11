// Plan (pseudocode):
// 1. Provide a public constructor CalculationWindow(string storageFolder) so MainWindow can create the window.
// 2. Call InitializeComponent() to ensure XAML is loaded.
// 3. Store the provided storageFolder in a private readonly field for later use in this window.
// 4. Do not change other types or behavior; keep this file minimal and focused on resolving CS1729.

using System.Windows;

namespace HoneyKhaalaProject
{
    public partial class CalculationWindow : Window
    {
        private readonly string storageFolder;

        public CalculationWindow(string storageFolder)
        {
            InitializeComponent();
            this.storageFolder = storageFolder;
            // You can use storageFolder as needed in this window
        }

        private void AddInvestor_Click(object sender, RoutedEventArgs e)
        {
            int next = Investors.Count + 1;
            Investors.Add(new Investor { Name = $"Investor {next}", AmountString = "0" });
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
            // parse amounts (silent zero for invalid)
            foreach (var inv in Investors)
            {
                if (!double.TryParse(inv.AmountString, out double v))
                    v = 0;
                inv.Amount = Math.Max(0, v);
            }

            double total = Investors.Sum(i => i.Amount);
            TotalDisplay = total.ToString("C2");

            // update each investor's percentage, formatted strings and bar width
            foreach (var inv in Investors)
            {
                if (total > 0)
                    inv.Percentage = inv.Amount / total * 100.0;
                else
                    inv.Percentage = 0;

                inv.DisplayAmount = inv.Amount.ToString("C2");
                inv.DisplayPercentage = inv.Percentage.ToString("F1") + " %";
                inv.BarWidth = (inv.Percentage / 100.0) * MaxBarPixelWidth;
            }
        }
    }
}
