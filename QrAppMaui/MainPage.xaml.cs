using ZXing;
using ZXing.Net.Maui;

namespace QrAppMaui
{
    public partial class MainPage: ContentPage
    {
        bool _isCameraRunning;

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
                    "Aby skanować kody uczniów, zezwól aplikacji na dostęp do aparatu w ustawieniach systemowych.",
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
            HelperLabel.Text = "Skieruj aparat na kod ucznia.";
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

            var code = result.Value?.Trim() ?? string.Empty;

   
            var words = code.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            Dispatcher.Dispatch(() =>
            {
                if (words.Length != 2)
                {
                    ShowScanError();
                    return;
                }

                ScannedCodeLabel.Text = code;
                FirstNameEntry.Text = words[0];
                LastNameEntry.Text = words[1];
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

            FirstNameEntry.Text = string.Empty;
            LastNameEntry.Text = string.Empty;
            ScannedCodeLabel.Text = "—";

           
            if (_isCameraRunning)
                BarcodeReader.IsDetecting = true;
        }

        void OnBackdropTapped(object sender, TappedEventArgs e) => HideResultSheet();

        void OnCancelClicked(object sender, EventArgs e) => HideResultSheet();

        async void OnSaveClicked(object sender, EventArgs e)
        {
            var firstName = FirstNameEntry.Text?.Trim();
            var lastName = LastNameEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                await DisplayAlert("Brak danych", "Uzupełnij imię i nazwisko ucznia.", "OK");
                return;
            }

            Checkinstore.Entries.Insert(0, new Checkinentry
            {
                FirstName = firstName,
                LastName = lastName,
                Timestamp = DateTime.Now
            });

            HideResultSheet();
        }

        void OnScannerTabTapped(object sender, TappedEventArgs e)
        {
         
        }

        void OnListTabTapped(object sender, TappedEventArgs e)
        {
           
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

     
            if (_isCameraRunning)
                StopCamera();
        }
    }
}


