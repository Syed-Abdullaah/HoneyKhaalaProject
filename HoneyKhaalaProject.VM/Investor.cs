using CommunityToolkit.Mvvm.ComponentModel;

namespace HoneyKhaalaProject.VM
{
    public partial class Investor : ObservableObject
    {
        [ObservableProperty]
        string name = "";

        [ObservableProperty]
        string amountString = "0";

        [ObservableProperty]
        double amount;

        [ObservableProperty]
        double percentage;

        [ObservableProperty]
        double barWidth;

        [ObservableProperty]
        string displayAmount = "$0.00";

        [ObservableProperty]
        string displayPercentage = "0.0 %";
    }
}
