﻿using AddressEntry.ViewModels;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace AddressEntry
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = ViewModel;
        }

        public MainViewModel ViewModel { get; } = new MainViewModel();

        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.WhenAnyValue(v => v.ViewModel.Position)
                .Subscribe((Position p) => MainThread.BeginInvokeOnMainThread(() => SetPin(p)));

            Observable.FromEventPattern<MapClickedEventArgs>(
                mc => MapControl.MapClicked += mc, 
                mc => MapControl.MapClicked -= mc)
                .Subscribe(ev => ViewModel.ExecuteSetAddress.Execute(ev.EventArgs.Position));
        }

        private Unit SetPin(Position position)
        {
            Pin pin = new Pin
            {
                Label = "The Place",
                Address = $"{ViewModel.Street}, {ViewModel.PostalCode} {ViewModel.City}, {ViewModel.Country}",
                Type = PinType.Place,
                Position = position
            };

            var latDegrees = MapControl.VisibleRegion?.LatitudeDegrees ?? 0.01;
            var longDegrees = MapControl.VisibleRegion?.LongitudeDegrees ?? 0.01;
            MapControl.MoveToRegion(new MapSpan(position, latDegrees, longDegrees));
            MapControl?.Pins?.Clear();
            MapControl?.Pins?.Add(pin);
            return Unit.Default;
        }
    }
}
