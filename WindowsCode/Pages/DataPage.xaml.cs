using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using WindowsCode.Classes;
using System.Text;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Devices.Geolocation;
using System.Diagnostics;
using System.Threading;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Storage.Streams;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsCode.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DataPage : Page
    {
        StringBuilder dataBuilder = new StringBuilder();
        MapIcon groundStationPosition;
        MapIcon probePosition;
        Geolocator geolocator;

        CancellationTokenSource cancelTokenSource;

        public DataPage()
        {
            this.InitializeComponent();
            cancelTokenSource = new CancellationTokenSource();

            geolocator = new Geolocator();
            groundStationPosition = new MapIcon();
            groundStationPosition.Title = "Ground Station";
            groundStationPosition.ZIndex = 1;
            ProbeLocation.MapElements.Add(groundStationPosition);
            probePosition = new MapIcon();
            probePosition.Title = "Probe";
            probePosition.ZIndex = 2;

            probePosition.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx://Assets/icon2test.png"));
            ProbeLocation.MapElements.Add(probePosition);

            SetCenter(cancelTokenSource.Token);
            UpdateMap(cancelTokenSource.Token);

            if (MesurementState.CurrentItem.Data.Count > 0)
            {
                MesurementState.CurrentItem.Data.ForEach(data => {
                    TempChart.Push(data.Temperature);
                    PresChart.Push(data.Pressure);
                    dataBuilder.Append(data.RawData + '\n');
                });
                TemperatureValue.Text = MesurementState.CurrentItem.Data.Last().Temperature.ToString() + " °C";
                PressureValue.Text = MesurementState.CurrentItem.Data.Last().Pressure.ToString() + " mB";
                XAxisValue.Text = MesurementState.CurrentItem.Data.Last().X_Acceleration.ToString();
                YAxisValue.Text = MesurementState.CurrentItem.Data.Last().Y_Acceleration.ToString();
                ZAxisValue.Text = MesurementState.CurrentItem.Data.Last().Z_Acceleration.ToString();
            }

            MesurementState.CurrentItem.Data.CollectionChanged -= Data_CollectionChanged;
            MesurementState.CurrentItem.Data.CollectionChanged += Data_CollectionChanged;
        }

        private async void Data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            dataBuilder = new StringBuilder();
            e.NewItems.Cast<CSVData>().ForEach(data => {
                dataBuilder.Append(data.RawData + '\n');
                TempChart.Push(data.Temperature);
                PresChart.Push(data.Pressure);
                probePosition.Location = new Geopoint(new BasicGeoposition
                {
                    Latitude = data.LatitudeInDegrees,
                    Longitude = data.LongitudeInDegrees,
                    Altitude = data.Altitude
                });
                //XAccChart.Push(data.X_Acceleration);
                //YAccChart.Push(data.Y_Acceleration);
                //ZAccChart.Push(data.Z_Acceleration);
            });

            //TemperatureValue.Text = e.NewItems.Cast<CSVData>().Last().Temperature.ToString() + " °C";
            //PressureValue.Text = e.NewItems.Cast<CSVData>().Last().Pressure.ToString() + " mB";
            //XAxisValue.Text = e.NewItems.Cast<CSVData>().Last().X_Acceleration.ToString();
            //YAxisValue.Text = e.NewItems.Cast<CSVData>().Last().Y_Acceleration.ToString();
            //ZAxisValue.Text = e.NewItems.Cast<CSVData>().Last().Z_Acceleration.ToString();

            FileIO.AppendTextAsync((await StorageApplicationPermissions.FutureAccessList.GetFileAsync(DataState.OutputFileToken)), dataBuilder.ToString()).AsTask().Wait();
            DataBlock.Text += dataBuilder.ToString();
        }

        private async void SetCenter(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                var position = await geolocator.GetGeopositionAsync().AsTask(token);
                ProbeLocation.Center = position.Coordinate.Point;
            }
            catch (Exception) { }
        }

        private async void UpdateMap(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    var position = await geolocator.GetGeopositionAsync().AsTask(token);
                    groundStationPosition.Location = position.Coordinate.Point;
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception) { }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (!cancelTokenSource?.IsCancellationRequested ?? false)
                cancelTokenSource?.Cancel();
        }
    }
}
