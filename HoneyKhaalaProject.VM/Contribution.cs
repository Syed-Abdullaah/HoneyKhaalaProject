using CommunityToolkit.Mvvm.ComponentModel;

namespace HoneyKhaalaProject.VM
{
    public partial class Contribution : ObservableObject
    {
        [ObservableProperty]
        string businessId = "";

        [ObservableProperty]
        string businessName = "";

        // keep numeric Amount for calculations
        [ObservableProperty]
        double amount;

        // string-backed input so the TextBox always shows/edit text reliably
        [ObservableProperty]
        string amountString = "0";

        [ObservableProperty]
        double profitShare;

        [ObservableProperty]
        string displayProfit = "$0.00";
    }
}