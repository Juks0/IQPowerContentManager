# Upload Assetofolder - Dokumentacja API

## Przegląd

API umożliwia kopiowanie całej zawartości folderu `assetofolder` z projektu do root folderu Assetto Corsa. Ta operacja kopiuje wszystkie pliki i foldery (w tym CSP, extension, content, itp.) do właściwego miejsca w instalacji Assetto Corsa.

**Kluczowe funkcje:**
- ✅ Automatyczne wykrywanie ścieżki do Assetto Corsa
- ✅ Kopiowanie całej zawartości folderu `assetofolder` do root folderu AC
- ✅ Rekurencyjne kopiowanie wszystkich plików i podfolderów
- ✅ Nadpisywanie istniejących plików
- ✅ Logowanie wszystkich operacji w konsoli
- ✅ Oznaczanie w stanie aplikacji, że assetofolder został wgrany

---

## Endpoint API

### POST /api/setup/upload-assetofolder

Kopiuje całą zawartość folderu `assetofolder` z projektu do root folderu Assetto Corsa.

**Request:** Brak (endpoint nie wymaga parametrów)

**Response (sukces):**
```json
{
  "success": true,
  "message": "Assetofolder został skopiowany do Assetto Corsa",
  "data": "Assetofolder został skopiowany do Assetto Corsa",
  "errorMessage": null
}
```

**Response (błąd - ścieżka nie skonfigurowana):**
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errorMessage": "Ścieżka do Assetto Corsa nie jest skonfigurowana"
}
```

**Response (błąd - nieprawidłowa ścieżka):**
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errorMessage": "Ścieżka do Assetto Corsa jest nieprawidłowa: C:\\Games\\AssettoCorsa"
}
```

**Response (błąd - folder nie istnieje):**
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errorMessage": "Folder assetofolder nie został znaleziony w projekcie.\nSzukano w: C:\\...\\assetofolder"
}
```

---

## Struktura folderu assetofolder

Folder `assetofolder` w projekcie zawiera następującą strukturę:

```
assetofolder/
├── content/
│   ├── cars/
│   │   ├── cky_porsche992_gt3rs_2023/
│   │   ├── ks_nissan_gtr/
│   │   └── ks_porsche_991_turbo_s/
│   └── tracks/
│       ├── ks_nordschleife/
│       └── ks_nurburgring/
├── extension/
│   ├── config/
│   ├── internal/
│   ├── lua/
│   ├── sfx/
│   ├── shaders.zip
│   ├── textures/
│   └── weather/
├── dwrite.dll
└── ...
```

**Po skopiowaniu do Assetto Corsa:**
- Pliki z `assetofolder/` → `{AC_ROOT}/`
- Foldery z `assetofolder/extension/` → `{AC_ROOT}/extension/`
- Foldery z `assetofolder/content/` → `{AC_ROOT}/content/`
- Plik `dwrite.dll` → `{AC_ROOT}/dwrite.dll`

---

## Przykłady użycia

### JavaScript (Fetch API)

```javascript
fetch('http://localhost:8080/api/setup/upload-assetofolder', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  }
})
  .then(response => response.json())
  .then(data => {
    if (data.success) {
      console.log('✓ Assetofolder skopiowany pomyślnie:', data.message);
    } else {
      console.error('✗ Błąd:', data.errorMessage);
    }
  })
  .catch(error => {
    console.error('Błąd sieci:', error);
  });
```

### cURL

```bash
curl -X POST http://localhost:8080/api/setup/upload-assetofolder
```

### Przykład z obsługą błędów

```javascript
async function uploadAssetofolder() {
  try {
    const response = await fetch('http://localhost:8080/api/setup/upload-assetofolder', {
      method: 'POST'
    });
    
    const data = await response.json();
    
    if (data.success) {
      console.log('✓ Assetofolder został skopiowany do Assetto Corsa');
      return true;
    } else {
      console.error('✗ Błąd kopiowania:', data.errorMessage);
      return false;
    }
  } catch (error) {
    console.error('Błąd sieci:', error);
    return false;
  }
}

// Użycie
uploadAssetofolder().then(success => {
  if (success) {
    console.log('Można teraz uruchomić grę');
  }
});
```

---

## Wykrywanie ścieżki Assetto Corsa

Endpoint automatycznie wykrywa ścieżkę do Assetto Corsa, sprawdzając następujące lokalizacje:

1. **Steam (32-bit):**
   ```
   C:\Program Files (x86)\Steam\steamapps\common\assettocorsa
   ```

2. **Steam (64-bit):**
   ```
   C:\Program Files\Steam\steamapps\common\assettocorsa
   ```

3. **Folder assetofolder w projekcie** (dla środowiska deweloperskiego)

4. **Inne typowe lokalizacje**

**Walidacja:**
- Sprawdza, czy folder zawiera `content/cars` i `apps`
- Sprawdza, czy istnieje `acs.exe` lub `acs_pro.exe` lub `AssettoCorsa.exe`

---

## Proces kopiowania

### 1. Weryfikacja
- Sprawdza, czy ścieżka do AC jest prawidłowa
- Sprawdza, czy folder `assetofolder` istnieje w projekcie

### 2. Kopiowanie
- Kopiuje wszystkie pliki z głównego folderu `assetofolder`
- Rekurencyjnie kopiuje wszystkie podfoldery:
  - `extension/` → `{AC_ROOT}/extension/`
  - `content/` → `{AC_ROOT}/content/`
  - `dwrite.dll` → `{AC_ROOT}/dwrite.dll`
  - Wszystkie inne pliki i foldery

### 3. Nadpisywanie
- Istniejące pliki są automatycznie nadpisywane
- Istniejące foldery są mergowane (zawartość jest dodawana, nie zastępowana)

### 4. Oznaczenie w stanie
- Po pomyślnym skopiowaniu, `AssetofolderUploaded = true` jest zapisywane w stanie aplikacji

---

## Logowanie

Wszystkie operacje są logowane w konsoli z timestampem:

```
[2024-01-15 10:30:00.123] [SETUP] Kopiowanie assetofolder do AC...
[2024-01-15 10:30:00.124] [SETUP] AC Root: C:\Program Files\Steam\steamapps\common\assettocorsa
[2024-01-15 10:30:00.125] [CONTENT] Kopiowanie zawartości assetofolder do AC...
[2024-01-15 10:30:00.126] [CONTENT] Źródło: C:\...\assetofolder
[2024-01-15 10:30:00.127] [CONTENT] Cel: C:\Program Files\Steam\steamapps\common\assettocorsa
[2024-01-15 10:30:05.456] [CONTENT] ✓ Cała zawartość assetofolder została skopiowana do: ...
[2024-01-15 10:30:05.457] [SETUP] ✓ Assetofolder skopiowany pomyślnie
```

---

## Błędy i obsługa

### Błąd: Ścieżka do Assetto Corsa nie jest skonfigurowana
```json
{
  "success": false,
  "errorMessage": "Ścieżka do Assetto Corsa nie jest skonfigurowana"
}
```
**Rozwiązanie:** Upewnij się, że Assetto Corsa jest zainstalowane w standardowej lokalizacji Steam lub skonfiguruj ścieżkę ręcznie.

### Błąd: Nieprawidłowa ścieżka
```json
{
  "success": false,
  "errorMessage": "Ścieżka do Assetto Corsa jest nieprawidłowa: C:\\Games\\AssettoCorsa"
}
```
**Rozwiązanie:** Sprawdź, czy ścieżka wskazuje na prawidłowy folder Assetto Corsa (zawiera `content/cars`, `apps`, `acs.exe`).

### Błąd: Folder assetofolder nie istnieje
```json
{
  "success": false,
  "errorMessage": "Folder assetofolder nie został znaleziony w projekcie.\nSzukano w: C:\\...\\assetofolder"
}
```
**Rozwiązanie:** Upewnij się, że folder `assetofolder` istnieje w projekcie w jednej z następujących lokalizacji:
- `{project_root}/assetofolder`
- `{bin_dir}/../assetofolder` (dla Debug)
- `{bin_dir}/../../assetofolder` (dla Release)

### Błąd: Błąd podczas kopiowania
```json
{
  "success": false,
  "errorMessage": "Błąd podczas kopiowania zawartości assetofolder: Access denied"
}
```
**Rozwiązanie:** 
- Upewnij się, że masz uprawnienia do zapisu w folderze Assetto Corsa
- Sprawdź, czy żadne pliki nie są używane przez inne procesy
- Uruchom aplikację z uprawnieniami administratora, jeśli to konieczne

---

## Uwagi techniczne

1. **Rekurencyjne kopiowanie:** Metoda `CopyDirectoryContents` rekurencyjnie kopiuje wszystkie pliki i foldery, zachowując strukturę katalogów.

2. **Merge zamiast zastępowania:** Jeśli folder już istnieje w AC, jego zawartość jest mergowana (dodawana), a nie zastępowana. Istniejące pliki są nadpisywane.

3. **Automatyczne wykrywanie ścieżki:** Endpoint automatycznie wykrywa ścieżkę do AC, sprawdzając typowe lokalizacje Steam.

4. **Walidacja AC root:** Przed kopiowaniem sprawdzane jest, czy folder jest prawidłowym root folderem AC (zawiera wymagane foldery i pliki).

5. **Logowanie:** Wszystkie operacje są szczegółowo logowane w konsoli z timestampem.

6. **Stan aplikacji:** Po pomyślnym skopiowaniu, `AssetofolderUploaded = true` jest zapisywane w `ApplicationState`.

7. **Bez interakcji:** Metoda `CopyAssetofolderToAcSilent()` nie wymaga interakcji z konsolą (w przeciwieństwie do `CopyAssetofolderToAc()`, która pyta użytkownika o potwierdzenie).

---

## Przykładowe scenariusze użycia

### Scenariusz 1: Pierwsze wgranie assetofolder
1. Użytkownik wywołuje `POST /api/setup/upload-assetofolder`
2. API wykrywa ścieżkę do AC
3. Kopiuje całą zawartość z `assetofolder` do root folderu AC
4. Oznacza w stanie, że assetofolder został wgrany
5. Użytkownik może teraz uruchomić grę z nową zawartością

### Scenariusz 2: Aktualizacja zawartości
1. Użytkownik zaktualizował pliki w folderze `assetofolder`
2. Wywołuje `POST /api/setup/upload-assetofolder`
3. API nadpisuje istniejące pliki nowymi wersjami
4. Nowa zawartość jest dostępna w grze

### Scenariusz 3: Dodanie nowych plików
1. Użytkownik dodał nowe pliki do `assetofolder`
2. Wywołuje `POST /api/setup/upload-assetofolder`
3. API kopiuje nowe pliki, zachowując istniejące
4. Wszystkie pliki są dostępne w grze

---

## Integracja z innymi endpointami

### Przed uruchomieniem gry
Zalecane jest wywołanie `POST /api/setup/upload-assetofolder` przed uruchomieniem gry, aby upewnić się, że wszystkie pliki są aktualne:

```javascript
// 1. Wgraj assetofolder
await fetch('http://localhost:8080/api/setup/upload-assetofolder', {
  method: 'POST'
});

// 2. Uruchom grę
await fetch('http://localhost:8080/api/setup/launch', {
  method: 'POST'
});
```

### Sprawdzanie statusu
Możesz sprawdzić, czy assetofolder został wgrany, używając `GET /api/setup/summary`:

```javascript
const response = await fetch('http://localhost:8080/api/setup/summary');
const data = await response.json();
// data.data zawiera informacje o stanie, w tym czy assetofolder został wgrany
```

---

## Bezpieczeństwo

1. **Walidacja ścieżki:** Endpoint waliduje, czy ścieżka do AC jest prawidłowa przed kopiowaniem.

2. **Obsługa błędów:** Wszystkie błędy są przechwytywane i zwracane w odpowiedzi API.

3. **Logowanie:** Wszystkie operacje są logowane dla celów diagnostycznych.

4. **Nadpisywanie:** Istniejące pliki są nadpisywane - upewnij się, że masz backup, jeśli to konieczne.

---

## Wydajność

- **Duże pliki:** Kopiowanie dużych plików (np. `dwrite.dll` ~124MB) może zająć kilka sekund.
- **Wiele plików:** Kopiowanie wielu małych plików może zająć więcej czasu niż jeden duży plik.
- **Sieć:** Jeśli AC jest na dysku sieciowym, kopiowanie może być wolniejsze.

**Zalecenia:**
- Wywołuj endpoint tylko gdy jest to konieczne (np. po aktualizacji zawartości)
- Nie wywołuj endpointu przed każdym uruchomieniem gry, jeśli zawartość się nie zmieniła

---

## Data ostatniej aktualizacji

**Wersja:** Aktualna  
**Data:** 2024-01-15  
**Ostatnia zmiana:** Dodano endpoint do kopiowania zawartości folderu assetofolder do Assetto Corsa


