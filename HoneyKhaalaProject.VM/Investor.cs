using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace HoneyKhaalaProject.VM
{
    public partial class Investor : ObservableObject
    {
        // stable unique id for an investor
        [ObservableProperty]
        string id = Guid.NewGuid().ToString();

        [ObservableProperty]
        string name = "";

        [ObservableProperty]
        double amount; // legacy / optional total field

        [ObservableProperty]
        double percentage;

        // total profit across businesses (computed)
        [ObservableProperty]
        double profitShare;

        [ObservableProperty]
        string displayProfit = "$0.00";

        // contributions per business
        public ObservableCollection<Contribution> Contributions { get; set;  } = new();
    }
}