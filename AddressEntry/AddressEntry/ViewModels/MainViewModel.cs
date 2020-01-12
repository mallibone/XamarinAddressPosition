using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AddressEntry.ViewModels
{
    internal class MainViewModel : ReactiveObject
    {
        public MainViewModel()
        {
            HandleOpenMap = ReactiveCommand.Create(() => IsPickingLocationFromMap = true);
        }

        [Reactive] public string Street { get; set; }
        [Reactive] public int PostalCode { get; set; }
        [Reactive] public string City { get; set; }
        [Reactive] public string Country { get; set; }
        [Reactive] public bool IsPickingLocationFromMap { get; set; }
        public IReactiveCommand HandleOpenMap { get; set; } 
    }
}
