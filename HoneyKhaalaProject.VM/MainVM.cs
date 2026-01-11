using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (!Directory.Exists(storageFolder))
                Directory.CreateDirectory(storageFolder);


            CurrentMonth = $"{DateTime.Now.ToString("MMMM yyyy", CultureInfo.CurrentCulture)} — Open Calculation";

            // previous 5 months (most recent first)
            for (int i = 1; i <= 7; i++)
            {
                var m = DateTime.Now.AddMonths(-i);
                previousMonths.Add(m);
            }
        }

        [RelayCommand]
        private void OpenMonthFile(DateTime month)
        {
            var file = Path.Combine(StorageFolder, $"{month:yyyy-MM}.txt");

            if (!File.Exists(file))
            {
                File.WriteAllText(file,"");
            }
                Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
        }

    }
}
