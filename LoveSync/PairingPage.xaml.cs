using LoveSync.ViewModels;

namespace LoveSync;

public partial class PairingPage : ContentPage
{
    public PairingPage(PairingViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}