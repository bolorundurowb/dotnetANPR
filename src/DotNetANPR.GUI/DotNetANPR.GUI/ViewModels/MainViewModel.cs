using CommunityToolkit.Mvvm.ComponentModel;

namespace DotNetANPR.GUI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";
}
