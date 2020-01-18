using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
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
            this.WhenAnyValue(x => x.Street
                                , x => x.PostalCode
                                , x => x.City
                                , x => x.Country
                                , (street, postalCode, city, country) => GetCoordinates(street, postalCode, city, country))
                                .SelectMany(x => x)
                                .Subscribe(x => Position = x);

            this.WhenAnyValue(x => x.Position
                                    , (p) => GetAddress(p))
                                    .SelectMany(x => x)
                                    .Subscribe((address) => {
                                        Street = address.Street;
                                        PostalCode = address.PostalCode;
                                        City = address.City;
                                        Country = address.Country;
                                    } );
                                //.ToPropertyEx(this, x => x.Position);
        }

        private IObservable<(string Street, int PostalCode, string City, string Country)> GetAddress(Position p)
        {
            (string Street, int PostalCode, string City, string Country) SplitAddress(string address)
            {
                var addressParts = address.Split('\n');

                return ("gna", 123, "gna", "gna");
            }

            return _geocoder.GetAddressesForPositionAsync(p).ToObservable().Select(a => SplitAddress(a.First())).FirstAsync();
        }

        private IObservable<Position> GetCoordinates(string street, int postalCode, string city, string country)
        {
            var geocoder = new Geocoder();
            return geocoder.GetPositionsForAddressAsync($"{street}, {postalCode} {city}, {country}").ToObservable().Select(p => p.First()).FirstAsync();
        }

        [Reactive] public string Street { get; set; }
        [Reactive] public int PostalCode { get; set; }
        [Reactive] public string City { get; set; }
        [Reactive] public string Country { get; set; }
        //public Position Position { [ObservableAsProperty] get; }
        [Reactive] public Position Position { get; set; }

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
