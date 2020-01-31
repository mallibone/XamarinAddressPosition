using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xamarin.Essentials;
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
        }

        private async Task SetAddress(Position p)
        {
            (string Street, string City, string Country) SplitAddress(string address)
            {
                var addressParts = address.Split(',');
                if(address.Length < 4) return (string.Empty, string.Empty, string.Empty);
                var street = addressParts[0];
                var city = addressParts[1];
                var country = addressParts[2];

                return (street, city, country);
            }

            var addrs =  await _geocoder.GetAddressesForPositionAsync(p).ToObservable().Select(a => SplitAddress(a.First())).FirstAsync();
            Street = addrs.Street;
            City = addrs.City;
            Country = addrs.Country;
            Position = p;
        }

        private async Task SetPosition(string street, string postalCode, string city, string country)
        {
            var geocoder = new Geocoder();
            Position = await geocoder.GetPositionsForAddressAsync($"{street}, {postalCode} {city}, {country}")
                            .ToObservable()
                            .Select(p => p.First())
                            .FirstAsync();
        }

        [Reactive] public string Street { get; set; }
        [Reactive] public string PostalCode { get; set; }
        [Reactive] public string City { get; set; }
        [Reactive] public string Country { get; set; }
        [Reactive] public Position Position { get; set; }
        public ReactiveCommand<Unit, Unit> ExecuteSetPosition { get; set; }
        public ReactiveCommand<Position, Unit> ExecuteSetAddress { get; set; }

        private async Task<Location> GetLocation()
        {
            try
            {
                Location location = await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }
                return location;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception

                return null;
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception

                return null;
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception

                return null;
            }
            catch (Exception ex)
            {
                // Unable to get location

                return null;
            }
        }
    }
}
