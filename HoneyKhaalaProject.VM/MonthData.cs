using CommunityToolkit.Mvvm.ComponentModel;

namespace HoneyKhaalaProject.VM
{
    public partial class MonthData : ObservableObject
    {
        [ObservableProperty]
        private DateTime month;

        [ObservableProperty]
        private BusinessEntry[]? businesses;

        [ObservableProperty]
        private Investor[]? investors;
    }
}
