using CommunityToolkit.Mvvm.ComponentModel;

namespace HoneyKhaalaProject.VM
{
    public partial class Investor : ObservableObject
    {
        [ObservableProperty]
        string name = "";

        [ObservableProperty]
        double amount;

        [ObservableProperty]
        double percentage;

        // new properties for profit distribution and display
        [ObservableProperty]
        double profitShare;

        [ObservableProperty]
        string displayProfit = "$0.00";
    }
}
