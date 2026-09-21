# QrAppMaui

Aplikacja .NET MAUI do rejestrowania obecności uczniów przez skanowanie kodów QR. Nauczyciel skanuje kod, uzupełnia dane w krótkim formularzu, a wpis trafia do lokalnej bazy danych i listy wejść z możliwością wyszukiwania i filtrowania.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)
![MAUI](https://img.shields.io/badge/.NET%20MAUI-Android%20%7C%20iOS%20%7C%20MacCatalyst%20%7C%20Windows-blueviolet)
![License](https://img.shields.io/badge/license-unspecified-lightgrey)

## Spis treści

- [Funkcje](#funkcje)
- [Stos technologiczny](#stos-technologiczny)
- [Struktura projektu](#struktura-projektu)
- [Wymagania](#wymagania)
- [Uruchomienie projektu](#uruchomienie-projektu)
- [Uprawnienia kamery](#uprawnienia-kamery)
- [Jak działa skanowanie](#jak-działa-skanowanie)
- [Przechowywanie danych](#przechowywanie-danych)
- [Znane ograniczenia](#znane-ograniczenia)
- [Plany rozwoju](#plany-rozwoju)

## Funkcje

- Podgląd kamery na żywo i skanowanie kodów QR/kreskowych (`ZXing.Net.MAUI`).
- Panel wysuwany z dołu ekranu do uzupełnienia imienia, nazwiska i klasy po odczytaniu kodu.
- Zapis wpisów do lokalnej bazy SQLite — historia przetrwa restart aplikacji.
- Lista wejść z wyszukiwaniem tekstowym (dowolna kolejność słów) oraz filtrem po dacie.
- Usuwanie pojedynczych wpisów z potwierdzeniem.
- Automatyczne wznawianie skanowania po powrocie na ekran skanera (np. z listy wejść), bez konieczności ponownego uruchamiania podglądu kamery.
- Panel z wynikiem odczytu zamyka się wyłącznie przyciskiem „Anuluj” lub „Zapisz wpis” — przypadkowe dotknięcie ekranu nie kasuje wpisywanych danych.

## Technologie

| Warstwa | Technologia |
|---|---|
| Framework | .NET MAUI (net9.0) |
| Skanowanie kodów | `ZXing.Net.Maui.Controls` |
| Baza danych | SQLite (`sqlite-net-pcl`) |
| Platformy docelowe | Android, iOS, MacCatalyst, Windows |

## Struktura projektu

```text
QrAppMaui/
├── App.xaml(.cs)              # Punkt wejścia aplikacji
├── AppShell.xaml(.cs)         # Nawigacja Shell, rejestracja MainPage jako strony startowej
├── MainPage.xaml(.cs)         # Ekran skanera + panel wyniku odczytu
├── EntriesPage.xaml(.cs)      # Lista wejść z wyszukiwaniem i filtrem daty
├── Checkinentry.cs            # Model pojedynczego wpisu obecności
├── Checkinstore.cs            # Warstwa dostępu do danych (SQLite + kolekcja w pamięci)
├── MauiProgram.cs             # Konfiguracja hosta MAUI, rejestracja ZXing
├── GlobalXmlns.cs             # Globalne aliasy XML dla XAML
├── Platforms/                 # Kod i manifesty specyficzne dla platform
└── Resources/                 # Style, kolory, fonty, ikony, splash
```

## Wymagania

- .NET SDK 9.0 lub nowszy.
- Zainstalowany workload MAUI: `dotnet workload install maui`.
- Do budowania pod iOS/MacCatalyst — macOS z Xcode.
- Do budowania pod Android — Android SDK (instalowany automatycznie razem z workloadem lub przez Visual Studio).

## Uruchomienie projektu

```bash
git clone <adres-repozytorium>
cd QrAppMaui
dotnet restore
```

Uruchomienie na konkretnej platformie:

```bash
dotnet build -t:Run -f net9.0-android
dotnet build -t:Run -f net9.0-ios
dotnet build -t:Run -f net9.0-maccatalyst
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

> [!TIP]
> Do realnego testowania skanowania kodów najwygodniej użyć fizycznego telefonu (Android) — emulatory zwykle nie mają dostępu do prawdziwej kamery, więc podgląd na żywo nie zadziała poprawnie.

## Uprawnienia kamery

| Platforma | Status |
|---|---|
| Android | Zadeklarowane w `Platforms/Android/AndroidManifest.xml` (`android.permission.CAMERA`) |
| iOS / MacCatalyst | **Brak** wpisu `NSCameraUsageDescription` w `Info.plist` |

> [!WARNING]
> Na iOS i MacCatalyst system wymaga klucza `NSCameraUsageDescription` w `Info.plist` z opisem, po co aplikacji dostęp do kamery. Bez niego próba użycia kamery zakończy się awarią aplikacji (crash), a nie zwykłym odrzuceniem uprawnienia. Trzeba dodać go w `Platforms/iOS/Info.plist` oraz `Platforms/MacCatalyst/Info.plist`, np.:
> ```xml
> <key>NSCameraUsageDescription</key>
> <string>Aplikacja używa aparatu do skanowania kodów QR uczniów.</string>
> ```

Aplikacja i tak pyta o zgodę w czasie działania (`Permissions.RequestAsync<Permissions.Camera>()`), ale deklaracja w manifeście/plist jest wymagana niezależnie od tego.

## Jak działa skanowanie

1. Na ekranie „Skaner” użytkownik naciska „Uruchom aparat” (lub dotyka ramki podglądu) — uruchamia się `CameraBarcodeReaderView`.
2. Po wykryciu kodu detekcja jest natychmiast zatrzymywana, żeby ten sam kod nie wywołał zdarzenia wielokrotnie.
3. Odczytana wartość trafia do `ScannedCodeLabel`, a od dołu wysuwa się panel z polami **Imię**, **Nazwisko** i **Klasa** do ręcznego uzupełnienia.
4. „Zapisz wpis” waliduje, że żadne pole nie jest puste, i zapisuje wpis przez `Checkinstore.AddAsync(...)`.
5. „Anuluj” zamyka panel bez zapisu i wznawia skanowanie kolejnego kodu.

> [!NOTE]
> Kod QR jest obecnie traktowany wyłącznie jako identyfikator/numer seryjny (`SerialNumber`) — imię, nazwisko i klasa są zawsze wpisywane ręcznie przy każdym skanie, aplikacja nie odczytuje ich automatycznie z treści kodu ani z żadnej bazy uczniów.

## Przechowywanie danych

- Baza SQLite: `checkins.db3` w `FileSystem.AppDataDirectory` (katalog danych aplikacji, osobny dla każdej platformy).
- `Checkinstore.Entries` to `ObservableCollection<Checkinentry>` wczytywana z bazy przy pierwszym `InitAsync()` i trzymana w pamięci przez cały czas działania aplikacji — UI (lista wejść) subskrybuje ją bezpośrednio, więc nowe/usunięte wpisy pojawiają się automatycznie.
- Każdy zapis (`AddAsync`) i usunięcie (`DeleteAsync`) aktualizuje jednocześnie bazę i kolekcję w pamięci, więc dane przetrwają restart aplikacji.

> [!IMPORTANT]
> Usunięcie aplikacji lub wyczyszczenie jej danych (odinstalowanie, „Wyczyść dane” w ustawieniach systemu) skasuje plik `checkins.db3`, a wraz z nim całą historię wejść. Warto rozważyć okresowy eksport/backup, jeśli historia ma być długoterminowo przechowywana.

## Znane ograniczenia

- Brak `NSCameraUsageDescription` na iOS/MacCatalyst (patrz sekcja wyżej).
- Brak eksportu listy wejść (np. do CSV/Excela) — dane można obecnie tylko przeglądać i usuwać w aplikacji.
- Brak uwierzytelniania/kont użytkowników — aplikacja zakłada jednego, zaufanego operatora (np. nauczyciela) na urządzenie.
- Formaty odczytywanych kodów nie są ograniczone (`BarcodeFormats.All`) — przy skanowaniu w zatłoczonym otoczeniu może to zwiększyć ryzyko przypadkowego odczytania niewłaściwego kodu.

## Plany rozwoju

- Strona listy wejść — zrealizowana (`EntriesPage`).
- Panel formularza po skanie z polami Imię/Nazwisko/Klasa — zrealizowany.
- Trwałe wznawianie skanowania po powrocie na ekran skanera — zrealizowane.

> [!TIP]
> Jeśli docelowo kody QR mają jednoznacznie identyfikować konkretnego ucznia, warto rozważyć dodanie tabeli uczniów w tej samej bazie SQLite i automatyczne uzupełnianie imienia/nazwiska/klasy po odczytaniu kodu, zamiast wpisywania ich ręcznie za każdym razem.
