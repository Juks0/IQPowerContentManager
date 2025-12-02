# Ustawienia skrzyni biegów (automatyczna/manualna)

## Przegląd

Możesz ustawić typ skrzyni biegów (automatyczna lub manualna) przed wejściem do gry, edytując plik konfiguracyjny `assists.ini`.

---

## Lokalizacja pliku

**Plik:** `assists.ini`

**Pełna ścieżka:**
```
{Documents}\Assetto Corsa\cfg\assists.ini
```

**Przykład na Windows:**
```
C:\Users\{TwojaNazwaUżytkownika}\Documents\Assetto Corsa\cfg\assists.ini
```

---

## Jak ustawić skrzynię biegów

### Krok 1: Otwórz plik assists.ini

1. Przejdź do katalogu:
   ```
   {Documents}\Assetto Corsa\cfg\
   ```
2. Otwórz plik `assists.ini` w Notatniku lub innym edytorze tekstu

### Krok 2: Znajdź sekcję [ASSISTS]

W pliku powinna być sekcja `[ASSISTS]`, która wygląda mniej więcej tak:

```ini
[ASSISTS]
IDEAL_LINE = 0
AUTO_BLIP = 0
STABILITY_CONTROL = 0
AUTO_BRAKE = 0
AUTO_SHIFTER = 0
ABS = 1
TRACTION_CONTROL = 1
AUTO_CLUTCH = 0
VISUALDAMAGE = 0
DAMAGE = 100
FUEL_RATE = 100
TYRE_WEAR = 100
TYRE_BLANKETS = 0
SLIPSTREAM = 100
```

### Krok 3: Zmień wartość AUTO_SHIFTER

Znajdź linię z `AUTO_SHIFTER` i ustaw odpowiednią wartość:

**Skrzynia manualna (ręczna):**
```ini
AUTO_SHIFTER = 0
```

**Skrzynia automatyczna:**
```ini
AUTO_SHIFTER = 1
```

### Krok 4: Zapisz plik

Zapisz plik i zamknij edytor.

---

## Przykładowa konfiguracja

### Przykład 1: Skrzynia manualna

```ini
[ASSISTS]
IDEAL_LINE = 0
AUTO_BLIP = 0
STABILITY_CONTROL = 0
AUTO_BRAKE = 0
AUTO_SHIFTER = 0          # 0 = manualna (ręczna)
ABS = 1
TRACTION_CONTROL = 1
AUTO_CLUTCH = 0
VISUALDAMAGE = 0
DAMAGE = 100
FUEL_RATE = 100
TYRE_WEAR = 100
TYRE_BLANKETS = 0
SLIPSTREAM = 100
```

### Przykład 2: Skrzynia automatyczna

```ini
[ASSISTS]
IDEAL_LINE = 0
AUTO_BLIP = 0
STABILITY_CONTROL = 0
AUTO_BRAKE = 0
AUTO_SHIFTER = 1          # 1 = automatyczna
ABS = 1
TRACTION_CONTROL = 1
AUTO_CLUTCH = 0
VISUALDAMAGE = 0
DAMAGE = 100
FUEL_RATE = 100
TYRE_WEAR = 100
TYRE_BLANKETS = 0
SLIPSTREAM = 100
```

---

## Wartości AUTO_SHIFTER

| Wartość | Typ skrzyni | Opis |
|---------|-------------|------|
| `0` | Manualna (ręczna) | Musisz ręcznie zmieniać biegi |
| `1` | Automatyczna | Gra automatycznie zmienia biegi za Ciebie |

---

## Ważne uwagi

### ⚠️ Nadpisywanie pliku

**Uwaga:** Plik `assists.ini` może być **nadpisywany przez Content Manager** podczas uruchamiania gry, jeśli ustawienia asystentów są zmieniane w interfejsie Content Managera.

### ✅ Kiedy ustawienia są zachowywane

Ustawienia w `assists.ini` są zachowywane, jeśli:
- Nie zmieniasz ustawień asystentów w interfejsie Content Managera
- Edytujesz plik **po** uruchomieniu gry przez Content Manager
- Używasz bezpośredniego uruchamiania gry (bez Content Managera)

### 📝 Kiedy edytować plik

**Najlepszy moment na edycję:**
1. **Przed uruchomieniem gry** - ustaw wartość `AUTO_SHIFTER` w pliku
2. **Uruchom grę** - ustawienie zostanie zastosowane
3. **Po zakończeniu gry** - jeśli chcesz zmienić na stałe, edytuj plik ponownie

---

## Alternatywne metody

### Metoda 1: Edycja bezpośrednia pliku

1. Otwórz `assists.ini` w edytorze tekstu
2. Zmień `AUTO_SHIFTER = 0` na `AUTO_SHIFTER = 1` (lub odwrotnie)
3. Zapisz plik

### Metoda 2: Przez interfejs Content Managera

Jeśli Content Manager ma opcję ustawiania asystentów:
1. Otwórz Content Manager
2. Przejdź do ustawień asystentów
3. Włącz/wyłącz "Auto Shifter" (automatyczna skrzynia biegów)
4. Zapisz ustawienia

**Uwaga:** Ta metoda może nadpisać ręczne ustawienia w pliku.

---

## Sprawdzenie ustawień

### Jak sprawdzić, czy ustawienie zostało zastosowane:

1. **Przed wejściem do gry:**
   - Otwórz `assists.ini`
   - Sprawdź wartość `AUTO_SHIFTER`

2. **W grze:**
   - Wejdź do menu ustawień asystentów
   - Sprawdź, czy "Auto Shifter" jest włączone/wyłączone zgodnie z ustawieniem

---

## Rozwiązywanie problemów

### Problem: Ustawienie nie działa

**Możliwe przyczyny:**
1. Plik został nadpisany przez Content Manager
2. Nieprawidłowa składnia w pliku (np. spacje, błędne wartości)
3. Plik jest tylko do odczytu

**Rozwiązanie:**
1. Sprawdź, czy plik nie jest tylko do odczytu (kliknij prawym przyciskiem → Właściwości → odznacz "Tylko do odczytu")
2. Upewnij się, że wartość to dokładnie `0` lub `1` (bez dodatkowych znaków)
3. Edytuj plik **po** uruchomieniu gry przez Content Manager

### Problem: Plik nie istnieje

**Rozwiązanie:**
1. Uruchom grę przynajmniej raz przez Content Manager
2. Plik `assists.ini` zostanie utworzony automatycznie
3. Następnie możesz go edytować

---

## Podsumowanie - szybka referencja

| Co | Gdzie | Jak |
|----|-------|-----|
| **Plik konfiguracyjny** | `{Documents}\Assetto Corsa\cfg\assists.ini` | Otwórz w Notatniku |
| **Sekcja** | `[ASSISTS]` | Znajdź w pliku |
| **Parametr** | `AUTO_SHIFTER` | Zmień wartość |
| **Manualna** | `AUTO_SHIFTER = 0` | Ustaw na 0 |
| **Automatyczna** | `AUTO_SHIFTER = 1` | Ustaw na 1 |

---

## Przykładowe ścieżki dla różnych systemów

### Windows 10/11
```
C:\Users\{NazwaUżytkownika}\Documents\Assetto Corsa\cfg\assists.ini
```

### Przykład z konkretną nazwą użytkownika
```
C:\Users\Jan\Documents\Assetto Corsa\cfg\assists.ini
```

---

## Dodatkowe informacje

### Inne ustawienia w assists.ini

Plik `assists.ini` zawiera również inne ustawienia asystentów:

- `IDEAL_LINE` - Linia idealna (0 = wyłączona, 1 = włączona)
- `AUTO_BLIP` - Automatyczne blipowanie (0 = wyłączone, 1 = włączone)
- `STABILITY_CONTROL` - Kontrola stabilności (0-100)
- `AUTO_BRAKE` - Automatyczne hamowanie (0 = wyłączone, 1 = włączone)
- `ABS` - ABS (0 = wyłączony, 1 = factory, 2 = włączony)
- `TRACTION_CONTROL` - Kontrola trakcji (0 = wyłączony, 1 = factory, 2 = włączony)
- `AUTO_CLUTCH` - Automatyczne sprzęgło (0 = wyłączone, 1 = włączone)

### Powiązane pliki

- `race.ini` - Konfiguracja sesji wyścigowej (samochód, tor, itp.)
- `controls.ini` - Ustawienia sterowania
- `video.ini` - Ustawienia grafiki

---

## Wsparcie

Jeśli masz problemy z ustawieniem skrzyni biegów:

1. Sprawdź, czy plik `assists.ini` istnieje
2. Sprawdź składnię pliku (czy nie ma błędów)
3. Upewnij się, że wartość `AUTO_SHIFTER` to `0` lub `1`
4. Sprawdź, czy plik nie jest tylko do odczytu

---

**Ostatnia aktualizacja:** Dokumentacja dla Content Manager - ustawienia skrzyni biegów

