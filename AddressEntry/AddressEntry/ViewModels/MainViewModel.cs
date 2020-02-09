using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace AddressEntry.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private readonly Geocoder _geocoder = new Geocoder();
        public MainViewModel()
        {
            ExecuteSetAddress = ReactiveCommand.CreateFromTask<Position>((position) => SetAddress(position));

            ExecuteSetPosition = ReactiveCommand.CreateFromTask(() => SetPosition(Street, City, Country));

            ExecuteNavigateTo = ReactiveCommand.CreateFromTask(() => NavigateTo());

Geolocation.GetLastKnownLocationAsync()
            .ToObservable()
            .Catch(Observable.Return(new Location()))
            .SubscribeOn(RxApp.MainThreadScheduler)
            .Subscribe(async location => { 
                var position = new Position(location.Latitude, location.Longitude);
                Position = position;
                await SetAddress(position);
            });
        }

        [Reactive] public string Street { get; set; }
        [Reactive] public string City { get; set; }
        [Reactive] public string Country { get; set; }
        [Reactive] public Position Position { get; set; }
        public ReactiveCommand<Unit, Unit> ExecuteSetPosition { get; set; }
        public ReactiveCommand<Position, Unit> ExecuteSetAddress { get; set; }
        public ReactiveCommand<Unit, Unit> ExecuteNavigateTo { get; set; }

        private async Task SetAddress(Position p)
        {
            var addrs = (await Geocoding.GetPlacemarksAsync(new Location(p.Latitude, p.Longitude))).FirstOrDefault();
            Street = $"{addrs.Thoroughfare} {addrs.SubThoroughfare}";
            City = $"{addrs.PostalCode} {addrs.Locality}";
            Country = addrs.CountryName;
            Position = p;
        }

        private async Task SetPosition(string street, string city, string country)
        {
            var location = (await Geocoding.GetLocationsAsync($"{street}, {city}, {country}")).FirstOrDefault();

            if (location == null) return;

            Position = new Position(location.Latitude, location.Longitude);
        }

        private Task NavigateTo()
        {
            // Check out: https://docs.microsoft.com/en-us/xamarin/essentials/maps
            var location = new Location(Position.Latitude, Position.Longitude);
            var options = new MapLaunchOptions { NavigationMode = NavigationMode.Driving };
            return Xamarin.Essentials.Map.OpenAsync(location, options);
        }
    }
}
