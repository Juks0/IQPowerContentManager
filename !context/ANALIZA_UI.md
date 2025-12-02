# Analiza generowania UI w projekcie AcManager

## Przegląd ogólny

Projekt AcManager to aplikacja **WPF (Windows Presentation Foundation)** używająca frameworka **ModernUI** do budowy interfejsu użytkownika. Aplikacja wykorzystuje wzorzec **MVVM (Model-View-ViewModel)** do separacji logiki biznesowej od prezentacji.

---

## 1. Architektura UI

### 1.1 Technologie

- **WPF (Windows Presentation Foundation)** - framework UI Microsoft
- **ModernUI** - biblioteka UI (`FirstFloor.ModernUI`)
- **XAML** - język znaczników do definiowania UI
- **MVVM Pattern** - wzorzec architektoniczny

### 1.2 Struktura projektu

```
AcManager\
  ├── App.xaml                    # Główny plik aplikacji
  ├── Pages\
  │   ├── Windows\
  │   │   └── MainWindow.xaml     # Główne okno aplikacji
  │   ├── Drive\
  │   │   └── QuickDrive.xaml     # Strona QuickDrive
  │   ├── Lists\
  │   │   └── CarsListPage.xaml   # Lista samochodów
  │   └── Selected\
  │       └── SelectedCarPage.xaml # Strona szczegółów samochodu
  └── Controls\                   # Własne kontrolki
```

---

## 2. Główne okno aplikacji (MainWindow)

### 2.1 Plik XAML

**Plik:** `AcManager\Pages\Windows\MainWindow.xaml`

```xml
<mui:ModernWindow x:Class="AcManager.Pages.Windows.MainWindow"
    Title="{x:Static g:AppStrings.Main_Title}"
    DefaultContentSource="/Pages/Drive/QuickDrive.xaml"
    d:DataContext="{d:DesignInstance w:MainWindow+ViewModel}">
    <!-- Zawartość okna -->
</mui:ModernWindow>
```

**Kluczowe elementy:**
- `ModernWindow` - okno z ModernUI
- `DefaultContentSource` - domyślna strona po uruchomieniu
- `DataContext` - kontekst danych (ViewModel)

### 2.2 Code-Behind

**Plik:** `AcManager\Pages\Windows\MainWindow.xaml.cs`

```csharp
public partial class MainWindow : IFancyBackgroundListener, INavigateUriHandler {
    public MainWindow() {
        DataContext = new ViewModel();  // Ustawienie ViewModel
        InitializeComponent();           // Inicjalizacja XAML
    }
    
    // Nawigacja
    public void NavigateTo(Uri uri) {
        // Przekierowanie do strony
    }
}
```

---

## 3. Ładowanie stron (Content Loading)

### 3.1 IContentLoader

**Interfejs:** `FirstFloor.ModernUI\Windows\IContentLoader.cs`

```csharp
public interface IContentLoader {
    Task<object> LoadContentAsync(Uri uri, CancellationToken cancellationToken);
    object LoadContent(Uri uri);
}
```

### 3.2 DefaultContentLoader

**Plik:** `FirstFloor.ModernUI\Windows\DefaultContentLoader.cs`

**Proces ładowania strony:**

1. **Ładowanie XAML:**
   ```csharp
   var loaded = Application.LoadComponent(uri);  // Ładuje plik XAML
   ```

2. **Obsługa parametrów:**
   ```csharp
   (loaded as IParametrizedUriContent)?.OnUri(uri);  // Przekazuje parametry z URI
   ```

3. **Asynchroniczne ładowanie danych:**
   ```csharp
   var loadable = loaded as ILoadableContent;
   if (loadable != null) {
       await loadable.LoadAsync(cancellationToken);  // Ładuje dane
       loadable.Initialize();                         // Inicjalizuje
   }
   ```

**Przykład użycia:**
```csharp
// Ładowanie strony QuickDrive
var content = ContentLoader.LoadContent(
    new Uri("/Pages/Drive/QuickDrive.xaml", UriKind.Relative)
);
```

### 3.3 Nawigacja przez URI

**Przykłady URI:**
- `/Pages/Drive/QuickDrive.xaml` - QuickDrive
- `/Pages/Lists/CarsListPage.xaml` - Lista samochodów
- `/Pages/Selected/SelectedCarPage.xaml?Id=ferrari_f40` - Szczegóły samochodu

**Obsługa parametrów:**
```csharp
public void OnUri(Uri uri) {
    var carId = uri.GetQueryParam("Id");  // Pobiera parametr z URI
    // ...
}
```

---

## 4. Wzorzec MVVM (Model-View-ViewModel)

### 4.1 Struktura MVVM

```
┌─────────────┐
│    View     │  (XAML) - Prezentacja
│  (QuickDrive│
│   .xaml)    │
└──────┬──────┘
       │ DataBinding
       │
┌──────▼──────┐
│  ViewModel  │  (C#) - Logika prezentacji
│ (QuickDrive │
│ +ViewModel) │
└──────┬──────┘
       │
┌──────▼──────┐
│    Model    │  (C#) - Logika biznesowa
│ (CarObject, │
│ CarsManager)│
└─────────────┘
```

### 4.2 Przykład: QuickDrive

#### View (XAML)

**Plik:** `AcManager\Pages\Drive\QuickDrive.xaml`

```xml
<UserControl x:Class="AcManager.Pages.Drive.QuickDrive"
    d:DataContext="{d:DesignInstance dr:QuickDrive+ViewModel}">
    
    <!-- Binding do właściwości ViewModel -->
    <TextBlock Text="{Binding SelectedCar.DisplayName}" />
    <Button Command="{Binding GoCommand}" Content="Play" />
    
</UserControl>
```

#### ViewModel (C#)

**Plik:** `AcManager\Pages\Drive\QuickDrive.xaml.cs`

```csharp
public partial class QuickDrive {
    private ViewModel Model => (ViewModel)DataContext;
    
    public void Initialize() {
        // Tworzenie ViewModel i ustawienie jako DataContext
        DataContext = new ViewModel(...);
    }
    
    public partial class ViewModel : NotifyPropertyChanged {
        private CarObject _selectedCar;
        
        public CarObject SelectedCar {
            get => _selectedCar;
            set {
                if (Equals(value, _selectedCar)) return;
                _selectedCar = value;
                OnPropertyChanged();  // Powiadamia UI o zmianie
            }
        }
        
        private ICommand _goCommand;
        public ICommand GoCommand => _goCommand ?? 
            (_goCommand = new AsyncCommand(Go, () => SelectedCar != null));
        
        private async Task Go() {
            // Logika uruchamiania gry
        }
    }
}
```

#### Model (C#)

**Plik:** `AcManager.Tools\Objects\CarObject.cs`

```csharp
public class CarObject : AcJsonObjectNew {
    public string DisplayName { get; }
    public string Id { get; }
    // ...
}
```

### 4.3 Data Binding

**Binding jednokierunkowy (OneWay):**
```xml
<TextBlock Text="{Binding SelectedCar.DisplayName}" />
```

**Binding dwukierunkowy (TwoWay):**
```xml
<TextBox Text="{Binding DriverName, Mode=TwoWay}" />
```

**Binding z konwerterem:**
```xml
<TextBlock Visibility="{Binding IsVisible, Converter={StaticResource BooleanToVisibilityConverter}}" />
```

**Binding z komendą:**
```xml
<Button Command="{Binding GoCommand}" Content="Play" />
```

---

## 5. Cykl życia strony

### 5.1 Interfejsy

**ILoadableContent** - dla stron wymagających asynchronicznego ładowania:
```csharp
public interface ILoadableContent {
    Task LoadAsync(CancellationToken cancellationToken);
    void Load();
    void Initialize();
}
```

**IParametrizedUriContent** - dla stron z parametrami:
```csharp
public interface IParametrizedUriContent {
    void OnUri(Uri uri);
}
```

### 5.2 Przykład implementacji

**Plik:** `AcManager\Pages\Drive\QuickDrive.xaml.cs`

```csharp
public partial class QuickDrive : ILoadableContent {
    public Task LoadAsync(CancellationToken cancellationToken) {
        // Asynchroniczne ładowanie danych
        return WeatherManager.Instance.EnsureLoadedAsync();
    }
    
    public void Load() {
        // Synchroniczne ładowanie danych
        WeatherManager.Instance.EnsureLoaded();
    }
    
    public void Initialize() {
        // Inicjalizacja ViewModel
        DataContext = new ViewModel(...);
        InitializeComponent();
    }
}
```

**Kolejność wywołań:**
1. `LoadAsync()` / `Load()` - ładowanie danych
2. `Initialize()` - inicjalizacja ViewModel
3. `InitializeComponent()` - inicjalizacja XAML
4. `OnLoaded` event - strona załadowana

---

## 6. Nawigacja

### 6.1 ModernFrame

**ModernFrame** - kontrolka do wyświetlania stron:

```xml
<mui:ModernFrame Source="{Binding CurrentSource}" />
```

**Właściwości:**
- `Source` - URI strony do wyświetlenia
- `ContentLoader` - loader do ładowania stron

### 6.2 Nawigacja programatyczna

**Przykłady:**

```csharp
// Nawigacja w MainWindow
public void NavigateTo(Uri uri) {
    // Ustawia Source w ModernFrame
    CurrentSource = uri;
}

// Nawigacja z ViewModel
new NavigateCommand(this, new Uri("/Pages/Drive/QuickDrive.xaml", UriKind.Relative)).Execute();

// Nawigacja z parametrami
var uri = new Uri("/Pages/Selected/SelectedCarPage.xaml?Id=ferrari_f40", UriKind.Relative);
NavigateTo(uri);
```

### 6.3 Historia nawigacji

**ModernFrame** automatycznie zarządza historią:
- `_history.Push()` - dodaje do historii
- `_future.Push()` - dodaje do przyszłości (przy cofaniu)
- Przyciski Wstecz/Dalej działają automatycznie

---

## 7. Resource Dictionaries (Zasoby)

### 7.1 App.xaml - Globalne zasoby

**Plik:** `AcManager\App.xaml`

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- ModernUI style -->
            <mui:SharedResourceDictionary Source="/FirstFloor.ModernUI;component/Assets/ModernUI.xaml" />
            
            <!-- Własne style -->
            <mui:SharedResourceDictionary Source="/AcManager.Controls;component/Assets/ModernUI.AcTheme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 7.2 Lokalne zasoby strony

**Przykład z QuickDrive.xaml:**

```xml
<UserControl.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <mui:SharedResourceDictionary Source="/AcManager.Controls;component/Assets/AcItemWrapperSpecific.xaml" />
        </ResourceDictionary.MergedDictionaries>
        
        <!-- BindingProxy do przekazywania DataContext -->
        <mui:BindingProxy x:Key="Model" Data="{Binding}" />
        
        <!-- Konwertery -->
        <BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter" />
    </ResourceDictionary>
</UserControl.Resources>
```

---

## 8. Własne kontrolki

### 8.1 Kontrolki w AcManager.Controls

**Przykłady:**
- `AcListPage` - strona z listą obiektów
- `CarBlock` - blok wyświetlający samochód
- `TrackBlock` - blok wyświetlający tor
- `AcObjectBase` - bazowa kontrolka dla obiektów AC

### 8.2 Przykład użycia

```xml
<c:CarBlock Car="{Binding SelectedCar}" 
            Skin="{Binding SelectedCar.SelectedSkin}"
            Click="OnCarBlockClick" />
```

---

## 9. Commands (Komendy)

### 9.1 Typy komend

**AsyncCommand** - dla operacji asynchronicznych:
```csharp
public ICommand GoCommand => _goCommand ?? 
    (_goCommand = new AsyncCommand(Go, () => SelectedCar != null));
```

**DelegateCommand** - dla prostych akcji:
```csharp
public ICommand ChangeCarCommand => _changeCarCommand ?? 
    (_changeCarCommand = new DelegateCommand(() => {
        // Akcja
    }));
```

**NavigateCommand** - dla nawigacji:
```csharp
new NavigateCommand(this, new Uri("/Pages/Drive/QuickDrive.xaml", UriKind.Relative))
```

### 9.2 Binding komend w XAML

```xml
<Button Command="{Binding GoCommand}" Content="Play" />
<Button Command="{Binding ChangeCarCommand}" Content="Change Car" />
```

---

## 10. NotifyPropertyChanged

### 10.1 Implementacja

**Bazowa klasa:** `FirstFloor.ModernUI\Presentation\NotifyPropertyChanged.cs`

```csharp
public class NotifyPropertyChanged : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool Apply<T>(T value, ref T field, Action onChanged = null) {
        if (Equals(value, field)) return false;
        field = value;
        OnPropertyChanged();
        onChanged?.Invoke();
        return true;
    }
}
```

### 10.2 Użycie w ViewModel

```csharp
public partial class ViewModel : NotifyPropertyChanged {
    private CarObject _selectedCar;
    
    public CarObject SelectedCar {
        get => _selectedCar;
        set {
            if (Apply(value, ref _selectedCar)) {
                // Dodatkowa logika przy zmianie
                OnPropertyChanged(nameof(GoCommand));  // Powiadamia o zmianie komendy
            }
        }
    }
}
```

---

## 11. Przykład kompletnej strony

### 11.1 XAML (View)

**Plik:** `AcManager\Pages\Drive\QuickDrive.xaml`

```xml
<UserControl x:Class="AcManager.Pages.Drive.QuickDrive"
    d:DataContext="{d:DesignInstance dr:QuickDrive+ViewModel}">
    
    <Grid>
        <!-- Blok samochodu -->
        <c:CarBlock Car="{Binding SelectedCar}" 
                    Click="OnCarBlockClick" />
        
        <!-- Blok toru -->
        <c:TrackBlock Track="{Binding SelectedTrack}" 
                     Click="OnTrackBlockClick" />
        
        <!-- Przycisk Play -->
        <Button Command="{Binding GoCommand}" 
                Content="Play" />
    </Grid>
</UserControl>
```

### 11.2 Code-Behind (ViewModel)

**Plik:** `AcManager\Pages\Drive\QuickDrive.xaml.cs`

```csharp
public partial class QuickDrive : ILoadableContent {
    private ViewModel Model => (ViewModel)DataContext;
    
    public Task LoadAsync(CancellationToken cancellationToken) {
        return WeatherManager.Instance.EnsureLoadedAsync();
    }
    
    public void Initialize() {
        DataContext = new ViewModel(...);
        InitializeComponent();
    }
    
    public partial class ViewModel : NotifyPropertyChanged {
        private CarObject _selectedCar;
        
        public CarObject SelectedCar {
            get => _selectedCar;
            set => Apply(value, ref _selectedCar);
        }
        
        public ICommand GoCommand => new AsyncCommand(Go, () => SelectedCar != null);
        
        private async Task Go() {
            // Logika uruchamiania gry
        }
    }
}
```

---

## 12. Przepływ danych

### 12.1 Od Modelu do View

```
CarsManager.Instance (Model)
    ↓
    GetById("ferrari_f40")
    ↓
ViewModel.SelectedCar (ViewModel)
    ↓
    {Binding SelectedCar} (XAML)
    ↓
UI wyświetla samochód
```

### 12.2 Od View do Modelu

```
Użytkownik klika przycisk
    ↓
Command.Execute() (ViewModel)
    ↓
ViewModel.Go() (ViewModel)
    ↓
GameWrapper.StartAsync() (Model)
    ↓
Gra uruchomiona
```

---

## 13. Design-Time Support

### 13.1 DesignInstance

**Umożliwia podgląd strony w Visual Studio Designer:**

```xml
d:DataContext="{d:DesignInstance dr:QuickDrive+ViewModel}"
```

**Efekt:**
- Designer pokazuje przykładowe dane
- IntelliSense działa w XAML
- Można zobaczyć layout bez uruchamiania aplikacji

---

## 14. Podsumowanie

### 14.1 Kluczowe komponenty

1. **MainWindow** - główne okno aplikacji
2. **ModernFrame** - wyświetlanie stron
3. **IContentLoader** - ładowanie stron XAML
4. **ViewModel** - logika prezentacji
5. **DataBinding** - łączenie View z ViewModel
6. **Commands** - akcje użytkownika
7. **NotifyPropertyChanged** - powiadamianie o zmianach

### 14.2 Przepływ tworzenia strony

1. **Utworzenie pliku XAML** - definicja UI
2. **Utworzenie pliku .xaml.cs** - code-behind
3. **Utworzenie ViewModel** - logika prezentacji
4. **Ustawienie DataContext** - połączenie View z ViewModel
5. **Binding właściwości** - łączenie danych z UI
6. **Dodanie komend** - obsługa akcji użytkownika
7. **Rejestracja w nawigacji** - dodanie do menu/nawigacji

### 14.3 Zalety tego podejścia

- **Separacja odpowiedzialności** - View, ViewModel, Model są oddzielone
- **Testowalność** - ViewModel można testować bez UI
- **Reużywalność** - ViewModel może być używany w różnych View
- **Maintainability** - łatwiejsze utrzymanie kodu
- **Design-Time Support** - podgląd w Visual Studio

---

## 15. Przykłady użycia

### 15.1 Tworzenie nowej strony

**Krok 1: Utworzenie XAML**
```xml
<UserControl x:Class="AcManager.Pages.Example.ExamplePage"
    d:DataContext="{d:DesignInstance ex:ExamplePage+ViewModel}">
    <TextBlock Text="{Binding Title}" />
</UserControl>
```

**Krok 2: Utworzenie Code-Behind**
```csharp
public partial class ExamplePage {
    private ViewModel Model => (ViewModel)DataContext;
    
    public void Initialize() {
        DataContext = new ViewModel();
        InitializeComponent();
    }
    
    public partial class ViewModel : NotifyPropertyChanged {
        public string Title { get; } = "Example Page";
    }
}
```

**Krok 3: Dodanie do nawigacji**
```csharp
// W MainWindow.xaml.cs
new TitleLinkEnabledEntry("example", "Example")
```

### 15.2 Binding z konwerterem

```xml
<!-- Konwerter w Resources -->
<BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter" />

<!-- Użycie -->
<TextBlock Visibility="{Binding IsVisible, Converter={StaticResource BooleanToVisibilityConverter}}" />
```

### 15.3 Komenda z parametrem

```csharp
public ICommand SelectCarCommand => new DelegateCommand<CarObject>(car => {
    SelectedCar = car;
});
```

```xml
<Button Command="{Binding SelectCarCommand}" 
        CommandParameter="{Binding}" 
        Content="Select" />
```

---

## 16. Debugowanie UI

### 16.1 Output Binding Errors

W Visual Studio można włączyć wyświetlanie błędów binding:
- Output Window → Show output from: "Debug"
- Błędy binding są wyświetlane w konsoli

### 16.2 Snoop / WPF Inspector

Narzędzia do inspekcji drzewa wizualnego:
- **Snoop** - open-source tool
- **WPF Inspector** - Visual Studio extension

### 16.3 Breakpoints w ViewModel

Można ustawić breakpointy w ViewModel, aby debugować logikę:
```csharp
public CarObject SelectedCar {
    get => _selectedCar;
    set {
        // Breakpoint tutaj
        if (Apply(value, ref _selectedCar)) {
            // Logika
        }
    }
}
```

---

## Podsumowanie końcowe

Projekt AcManager używa nowoczesnego podejścia do budowy UI w WPF:

1. **MVVM Pattern** - separacja logiki od prezentacji
2. **ModernUI Framework** - gotowe komponenty UI
3. **Data Binding** - automatyczna synchronizacja danych
4. **Commands** - obsługa akcji użytkownika
5. **Async Loading** - asynchroniczne ładowanie danych
6. **URI Navigation** - nawigacja przez URI
7. **Resource Dictionaries** - centralne zarządzanie stylami

To podejście zapewnia:
- ✅ Czytelny i utrzymywalny kod
- ✅ Łatwe testowanie
- ✅ Dobrą wydajność
- ✅ Wsparcie dla design-time

