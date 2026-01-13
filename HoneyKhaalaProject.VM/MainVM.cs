using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace HoneyKhaalaProject.VM
{
    public partial class MainVM : ObservableRecipient
    {
        [ObservableProperty]
        private ObservableCollection<DateTime> previousMonths = new();

        [ObservableProperty]
        string storageFolder = "";

        [ObservableProperty]
        string currentMonth = "";

        public MainVM()
        {
            StorageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoneyKhaalaProject", "Months");
            if (!Directory.Exists(storageFolder)) Directory.CreateDirectory(storageFolder);

            RefreshMonths();
        }

        public void RefreshMonths()
        {
            CurrentMonth = $"{DateTime.Now.ToString("MMMM yyyy", CultureInfo.CurrentCulture)} — Open Calculation";

            PreviousMonths.Clear();
            for (int i = 1; i <= 11; i++)
            {
                PreviousMonths.Add(DateTime.Now.AddMonths(-i));
            }
        }

        [RelayCommand]
        private void OpenMonthFile(DateTime month)
        {
            var file = Path.Combine(StorageFolder, $"{month:yyyy-MM}.txt");
            if (!File.Exists(file)) File.WriteAllText(file, "");
            Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
        }
    }
}
