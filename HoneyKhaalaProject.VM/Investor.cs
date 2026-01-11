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
    }
}
