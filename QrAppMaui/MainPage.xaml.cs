using ZXing;
using ZXing.Net.Maui;

namespace QrAppMaui
{
    public partial class MainPage : ContentPage
    {
        bool _isCameraRunning;
        bool _resumeCameraOnAppear;
        string _scannedSerial = string.Empty;

        public MainPage()
        {
            InitializeComponent();

            BarcodeReader.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.All,
                AutoRotate = true,
                Multiple = false
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await Checkinstore.InitAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd bazy danych", ex.Message, "OK");
            }

       
            if (_resumeCameraOnAppear && !ResultSheet.IsVisible)
            {
                _resumeCameraOnAppear = false;

                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status == PermissionStatus.Granted)
                    StartCamera();
            }
        }

        void OnCameraBoxTapped(object sender, TappedEventArgs e)
        {
            if (!_isCameraRunning)
                OnStartCameraClicked(sender, e);
        }

        async void OnStartCameraClicked(object sender, EventArgs e)
        {
            if (_isCameraRunning)
            {
                StopCamera();
                return;
            }

            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert(
                    "Brak uprawnień",
                    "Aby skanować kody QR, zezwól aplikacji na dostęp do aparatu w ustawieniach systemowych.",
                    "OK");
                return;
            }

            StartCamera();
        }

        void StartCamera()
        {
            _isCameraRunning = true;

            CameraPlaceholder.IsVisible = false;
            BarcodeReader.IsVisible = true;
            BarcodeReader.IsDetecting = true;

            StartCameraButton.Text = "Zatrzymaj skanowanie";
            HelperLabel.Text = "Skieruj aparat na kod QR urządzenia.";
        }

        void StopCamera()
        {
            _isCameraRunning = false;

            BarcodeReader.IsDetecting = false;
            BarcodeReader.IsVisible = false;
            CameraPlaceholder.IsVisible = true;

            StartCameraButton.Text = "Uruchom aparat";
            HelperLabel.Text = "Naciśnij, by włączyć podgląd z kamery.";
        }

        void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var result = e.Results?.FirstOrDefault();
            if (result is null)
                return;

            BarcodeReader.IsDetecting = false;

            var serial = result.Value?.Trim() ?? string.Empty;

            Dispatcher.Dispatch(() =>
            {
                if (string.IsNullOrEmpty(serial))
                {
                    ShowScanError();
                    return;
                }

                _scannedSerial = serial;
                ScannedCodeLabel.Text = serial;


                FirstNameEntry.Text = string.Empty;
                LastNameEntry.Text = string.Empty;
                ClassEntry.Text = string.Empty;

                ShowResultSheet();
            });
        }

        async void ShowScanError()
        {
            ScanErrorBadge.IsVisible = true;
            await Task.Delay(1500);
            ScanErrorBadge.IsVisible = false;

            if (_isCameraRunning)
                BarcodeReader.IsDetecting = true;
        }

        void ShowResultSheet()
        {
            ResultBackdrop.IsVisible = true;
            ResultSheet.IsVisible = true;
        }

        void HideResultSheet()
        {
            ResultBackdrop.IsVisible = false;
            ResultSheet.IsVisible = false;

            _scannedSerial = string.Empty;
            FirstNameEntry.Text = string.Empty;
            LastNameEntry.Text = string.Empty;
            ClassEntry.Text = string.Empty;
            ScannedCodeLabel.Text = "—";

            if (_isCameraRunning)
                BarcodeReader.IsDetecting = true;
        }
        void OnCancelClicked(object sender, EventArgs e) => HideResultSheet();

        async void OnSaveClicked(object sender, EventArgs e)
        {
            var firstName = FirstNameEntry.Text?.Trim();
            var lastName = LastNameEntry.Text?.Trim();
            var className = ClassEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(className))
            {
                await DisplayAlert("Brak danych", "Uzupełnij imię, nazwisko i klasę.", "OK");
                return;
            }

            try
            {
                await Checkinstore.AddAsync(new Checkinentry
                {
                    SerialNumber = _scannedSerial,
                    FirstName = firstName,
                    LastName = lastName,
                    ClassName = className,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd zapisu", ex.Message, "OK");
                return;
            }

            HideResultSheet();
        }

        void OnScannerTabTapped(object sender, TappedEventArgs e)
        {
        }

        async void OnListTabTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new EntriesPage(), false);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
          
            _resumeCameraOnAppear = _isCameraRunning;

            if (_isCameraRunning)
                StopCamera();
        }
    }
}
