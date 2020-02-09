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

            ExecuteSetPosition = ReactiveCommand.CreateFromTask(() => SetPosition(Street, PostalCode, City, Country));

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
        [Reactive] public string PostalCode { get; set; }
        [Reactive] public string City { get; set; }
        [Reactive] public string Country { get; set; }
        [Reactive] public Position Position { get; set; }
        public ReactiveCommand<Unit, Unit> ExecuteSetPosition { get; set; }
        public ReactiveCommand<Position, Unit> ExecuteSetAddress { get; set; }
        public ReactiveCommand<Unit, Unit> ExecuteNavigateTo { get; set; }

        private async Task SetAddress(Position p)
        {
            (string Street, string City, string Country) SplitAddress(string address)
            {
                if(string.IsNullOrEmpty(address)) return (string.Empty, string.Empty, string.Empty);
                string[] addressParts; 
                switch (Device.RuntimePlatform) 
                { 
                    case Device.Android: 
                        addressParts = address.Split(','); 
                        break; 
                    case Device.UWP: 
                        addressParts = address.Split(new[] { '\r','\n' }, StringSplitOptions.RemoveEmptyEntries); 
                        break; 
                    case Device.iOS: 
                        addressParts = address.Split('\n'); 
                        break;
                    default:
                        addressParts = new string[0];
                        break;
                }
                if(addressParts.Length < 3) return (string.Empty, string.Empty, string.Empty);
                var street = addressParts[0];
                var city = addressParts[1];
                var country = addressParts[2];

                return (street, city, country);
            }

            var addrs = (await Geocoding.GetPlacemarksAsync(new Location(p.Latitude, p.Longitude))).FirstOrDefault();
            Street = $"{addrs.Thoroughfare} {addrs.SubThoroughfare}";
            City = $"{addrs.PostalCode} {addrs.Locality}";
            Country = addrs.CountryName;
            Position = p;
        }

        private async Task SetPosition(string street, string postalCode, string city, string country)
        {
            var location = (await Geocoding.GetLocationsAsync($"{street}, {postalCode} {city}, {country}")).FirstOrDefault();

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
