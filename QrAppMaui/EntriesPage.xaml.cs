namespace QrAppMaui
{
    public partial class EntriesPage : ContentPage
    {
        public EntriesPage()
        {
            InitializeComponent();
            DateFilterPicker.Date = DateTime.Today;
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

            ApplyFilter();
        }

        // Podpięte pod: zmiana tekstu, zmiana daty, przełączenie filtra daty
        void OnFilterChanged(object sender, EventArgs e) => ApplyFilter();

        void ApplyFilter()
        {
            // Szukanie po fragmentach: "jan kow" znajdzie Jan Kowalski (kolejność dowolna)
            var tokens = (SearchEntry.Text ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            DateTime? date = DateFilterCheck.IsChecked ? DateFilterPicker.Date : null;

            var result = Checkinstore.Entries.Where(entry =>
            {
                if (date is not null && entry.Timestamp.Date != date.Value.Date)
                    return false;

                var haystack = $"{entry.FirstName} {entry.LastName} {entry.ClassName} {entry.SerialNumber}";
                return tokens.All(t => haystack.Contains(t, StringComparison.CurrentCultureIgnoreCase));
            }).ToList();

            EntriesList.ItemsSource = result;
            CountLabel.Text = $"Wyniki: {result.Count}";
        }

        async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is not Button { BindingContext: Checkinentry entry })
                return;

            var confirmed = await DisplayAlert(
                "Usuń wpis",
                $"Usunąć wpis: {entry.FullName} ({entry.SerialNumber})?",
                "Usuń",
                "Anuluj");

            if (!confirmed)
                return;

            try
            {
                await Checkinstore.DeleteAsync(entry);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd usuwania", ex.Message, "OK");
            }
        }

        async void OnScannerTabTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PopAsync(false);
        }

        void OnListTabTapped(object sender, TappedEventArgs e)
        {
            // już jesteśmy na liście
        }
    }
}
