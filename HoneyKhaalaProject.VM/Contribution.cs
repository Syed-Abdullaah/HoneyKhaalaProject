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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof (HalfAmount))]
        double profitShare;

        public double HalfAmount => ProfitShare / 2;
    }
}