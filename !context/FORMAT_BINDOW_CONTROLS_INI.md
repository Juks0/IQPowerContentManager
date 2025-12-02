# Format bindów w pliku controls.ini

## Lokalizacja pliku

```
{Documents}\Assetto Corsa\cfg\controls.ini
```

Przykład: `C:\Users\Jan\Documents\Assetto Corsa\cfg\controls.ini`

---

## 1. Steering Wheel (Kierownica)

### Format w pliku:

```ini
[STEER]
JOY = 0
AXLE = 0
SCALE = 1.0
LOCK = 900
STEER_FILTER = 0.0
SPEED_SENSITIVITY = 0.0
```

### Opis wartości:

- **JOY** = indeks kontrolera (0 = pierwszy kontroler, 1 = drugi, itd.)
- **AXLE** = indeks osi kierownicy (zwykle 0)
- **SCALE** = skala kierownicy (1.0 = 100%, może być ujemna dla odwrócenia)
- **LOCK** = zakres obrotu w stopniach (np. 900 = 900 stopni)
- **STEER_FILTER** = filtr kierownicy (0.0 = brak filtra)
- **SPEED_SENSITIVITY** = czułość zależna od prędkości (0.0 = wyłączona)

### Przykład:

```ini
[STEER]
JOY = 0
AXLE = 0
SCALE = 1.0
LOCK = 900
STEER_FILTER = 0.0
SPEED_SENSITIVITY = 0.0
```

---

## 2. Pedals (Pedały)

### 2.1 Throttle (Gaz) - [THROTTLE]

```ini
[THROTTLE]
JOY = 0
AXLE = 1
MIN = -1.0
MAX = 1.0
```

### 2.2 Brakes (Hamulce) - [BRAKES]

```ini
[BRAKES]
JOY = 0
AXLE = 2
MIN = -1.0
MAX = 1.0
```

### 2.3 Clutch (Sprzęgło) - [CLUTCH]

```ini
[CLUTCH]
JOY = 0
AXLE = 3
MIN = -1.0
MAX = 1.0
```

### Opis wartości:

- **JOY** = indeks kontrolera (ten sam co dla kierownicy)
- **AXLE** = indeks osi pedału
  - 1 = gaz
  - 2 = hamulce
  - 3 = sprzęgło
- **MIN** = minimalna wartość osi (zwykle -1.0)
- **MAX** = maksymalna wartość osi (zwykle 1.0)

### Przykład kompletny:

```ini
[THROTTLE]
JOY = 0
AXLE = 1
MIN = -1.0
MAX = 1.0

[BRAKES]
JOY = 0
AXLE = 2
MIN = -1.0
MAX = 1.0

[CLUTCH]
JOY = 0
AXLE = 3
MIN = -1.0
MAX = 1.0
```

---

## 3. Gear Up / Gear Down (Zmiana biegów)

### 3.1 Gear Up (Bieg w górę) - [GEARUP]

```ini
[GEARUP]
JOY = 0
BUTTON = 2
KEY = 0x57
```

### 3.2 Gear Down (Bieg w dół) - [GEARDN]

```ini
[GEARDN]
JOY = 0
BUTTON = 3
KEY = 0x53
```

### Opis wartości:

- **JOY** = indeks kontrolera
  - Jeśli używasz przycisku na kole: indeks kontrolera (np. 0)
  - Jeśli używasz klawiatury: -1 lub brak
- **BUTTON** = indeks przycisku na kole
  - -1 = nie używany
  - 0, 1, 2, ... = numer przycisku
- **KEY** = kod klawisza w formacie hex
  - `0x57` = W
  - `0x53` = S
  - `-1` = nie używany

### Przykład - tylko klawiatura:

```ini
[GEARUP]
JOY = -1
BUTTON = -1
KEY = 0x57

[GEARDN]
JOY = -1
BUTTON = -1
KEY = 0x53
```

### Przykład - tylko przycisk na kole:

```ini
[GEARUP]
JOY = 0
BUTTON = 2
KEY = -1

[GEARDN]
JOY = 0
BUTTON = 3
KEY = -1
```

### Przykład - przycisk na kole + klawiatura (oba aktywne):

```ini
[GEARUP]
JOY = 0
BUTTON = 2
KEY = 0x57

[GEARDN]
JOY = 0
BUTTON = 3
KEY = 0x53
```

### Popularne kody klawiszy (hex):

| Klawisz | Kod Hex | Klawisz | Kod Hex |
|---------|---------|---------|---------|
| W | 0x57 | S | 0x53 |
| A | 0x41 | D | 0x44 |
| Q | 0x51 | E | 0x45 |
| R | 0x52 | T | 0x54 |
| Space | 0x20 | Enter | 0x0D |
| Shift | 0x10 | Ctrl | 0x11 |
| Arrow Up | 0x26 | Arrow Down | 0x28 |
| Arrow Left | 0x25 | Arrow Right | 0x27 |

---

## 4. Handbrake (Hamulec ręczny) - [HANDBRAKE]

```ini
[HANDBRAKE]
JOY = 0
BUTTON = 4
KEY = 0x48
```

### Opis wartości:

- **JOY** = indeks kontrolera (lub -1 dla klawiatury)
- **BUTTON** = indeks przycisku (lub -1 dla klawiatury)
- **KEY** = kod klawisza w hex (lub -1 dla przycisku)

### Przykłady:

**Tylko przycisk na kole:**
```ini
[HANDBRAKE]
JOY = 0
BUTTON = 4
KEY = -1
```

**Tylko klawiatura (H):**
```ini
[HANDBRAKE]
JOY = -1
BUTTON = -1
KEY = 0x48
```

**Oba (przycisk + klawiatura):**
```ini
[HANDBRAKE]
JOY = 0
BUTTON = 4
KEY = 0x48
```

---

## 5. Shifter (H & SEQ) - [SHIFTER]

### 5.1 Aktywacja H-shifter'a

```ini
[SHIFTER]
ACTIVE = 1
JOY = 0
```

### 5.2 Bindy dla poszczególnych biegów

```ini
[SHIFTER]
ACTIVE = 1
JOY = 0
GEAR_1 = 0
GEAR_2 = 1
GEAR_3 = 2
GEAR_4 = 3
GEAR_5 = 4
GEAR_6 = 5
GEAR_7 = 6
GEAR_R = 7
```

### Opis wartości:

- **ACTIVE** = czy H-shifter jest aktywny
  - `1` = włączony
  - `0` = wyłączony (używa GEARUP/GEARDN)
- **JOY** = indeks kontrolera z H-shifter'em
- **GEAR_1** do **GEAR_7** = indeksy przycisków dla biegów 1-7
- **GEAR_R** = indeks przycisku dla wstecznego

### Przykład - H-shifter włączony:

```ini
[SHIFTER]
ACTIVE = 1
JOY = 0
GEAR_1 = 0
GEAR_2 = 1
GEAR_3 = 2
GEAR_4 = 3
GEAR_5 = 4
GEAR_6 = 5
GEAR_7 = 6
GEAR_R = 7
```

### Przykład - H-shifter wyłączony (używa GEARUP/GEARDN):

```ini
[SHIFTER]
ACTIVE = 0
JOY = -1
```

---

## 6. Change View (Zmiana widoku) - [ACTION_CHANGE_CAMERA]

```ini
[ACTION_CHANGE_CAMERA]
JOY = 0
BUTTON = 5
KEY = 0x43
```

### Opis wartości:

- **JOY** = indeks kontrolera (lub -1)
- **BUTTON** = indeks przycisku (lub -1)
- **KEY** = kod klawisza w hex (lub -1)

### Przykłady:

**Tylko klawiatura (C):**
```ini
[ACTION_CHANGE_CAMERA]
JOY = -1
BUTTON = -1
KEY = 0x43
```

**Tylko przycisk na kole:**
```ini
[ACTION_CHANGE_CAMERA]
JOY = 0
BUTTON = 5
KEY = -1
```

**Oba:**
```ini
[ACTION_CHANGE_CAMERA]
JOY = 0
BUTTON = 5
KEY = 0x43
```

---

## 7. Restart Race (Restart wyścigu) - [RESET_RACE]

```ini
[RESET_RACE]
KEY = 0x52
```

### Opis wartości:

- **KEY** = kod klawisza w hex (zwykle R = 0x52)

### Przykład:

```ini
[RESET_RACE]
KEY = 0x52
```

**Uwaga:** `RESET_RACE` zwykle używa tylko klawiatury (nie przycisku na kole).

---

## 8. Kompletny przykład pliku controls.ini

```ini
[HEADER]
INPUT_METHOD = 1

[CONTROLLERS]
CON0 = Logitech G29
PGUID0 = {C24F046D-0000-0000-0000-504944564944}

[STEER]
JOY = 0
AXLE = 0
SCALE = 1.0
LOCK = 900
DEBOUNCING_MS = 0
FF_GAIN = 1.0
FILTER_FF = 0.0
STEER_FILTER = 0.0
SPEED_SENSITIVITY = 0.0

[THROTTLE]
JOY = 0
AXLE = 1
MIN = -1.0
MAX = 1.0

[BRAKES]
JOY = 0
AXLE = 2
MIN = -1.0
MAX = 1.0

[CLUTCH]
JOY = 0
AXLE = 3
MIN = -1.0
MAX = 1.0

[GEARUP]
JOY = 0
BUTTON = 2
KEY = 0x57

[GEARDN]
JOY = 0
BUTTON = 3
KEY = 0x53

[HANDBRAKE]
JOY = 0
BUTTON = 4
KEY = 0x48

[SHIFTER]
ACTIVE = 0
JOY = -1

[ACTION_CHANGE_CAMERA]
JOY = 0
BUTTON = 5
KEY = 0x43

[RESET_RACE]
KEY = 0x52
```

---

## 9. Ważne sekcje pomocnicze

### 9.1 [CONTROLLERS] - lista kontrolerów

```ini
[CONTROLLERS]
CON0 = Logitech G29
PGUID0 = {C24F046D-0000-0000-0000-504944564944}
__IGUID0 = {C24F046D-0000-0000-0000-504944564944}
```

- **CON0**, **CON1**, ... = nazwy kontrolerów
- **PGUID0**, **PGUID1**, ... = Product GUID (identyfikator produktu)
- **__IGUID0**, **__IGUID1**, ... = Instance GUID (identyfikator instancji)

### 9.2 [HEADER] - metoda wejścia

```ini
[HEADER]
INPUT_METHOD = 1
```

- **INPUT_METHOD**:
  - `0` = klawiatura
  - `1` = koło kierownicy
  - `2` = kontroler (gamepad)

---

## 10. Jak znaleźć indeksy przycisków i osi?

### 10.1 Indeksy osi (AXLE)

- **0** = kierownica (X-axis)
- **1** = gaz (Y-axis lub inna oś)
- **2** = hamulce
- **3** = sprzęgło
- **4+** = dodatkowe osie (jeśli dostępne)

### 10.2 Indeksy przycisków (BUTTON)

- **0, 1, 2, 3, ...** = kolejne przyciski na kole
- Zwykle:
  - 0 = pierwszy przycisk
  - 1 = drugi przycisk
  - 2 = trzeci przycisk
  - itd.

**Uwaga:** Indeksy mogą się różnić w zależności od modelu kontrolera. Najlepiej sprawdzić w ustawieniach gry lub użyć Content Manager, który automatycznie wykrywa przyciski.

---

## 11. Konwersja kodów klawiszy

### 11.1 Jak znaleźć kod hex dla klawisza?

**Metoda 1 - Użyj Content Manager:**
- Otwórz ustawienia sterowania
- Kliknij na bind i naciśnij klawisz
- CM automatycznie zapisze kod

**Metoda 2 - Tabela kodów:**

| Klawisz | Hex | Klawisz | Hex | Klawisz | Hex |
|---------|-----|---------|-----|---------|-----|
| A | 0x41 | B | 0x42 | C | 0x43 |
| D | 0x44 | E | 0x45 | F | 0x46 |
| G | 0x47 | H | 0x48 | I | 0x49 |
| J | 0x4A | K | 0x4B | L | 0x4C |
| M | 0x4D | N | 0x4E | O | 0x4F |
| P | 0x50 | Q | 0x51 | R | 0x52 |
| S | 0x53 | T | 0x54 | U | 0x55 |
| V | 0x56 | W | 0x57 | X | 0x58 |
| Y | 0x59 | Z | 0x5A | | |
| Space | 0x20 | Enter | 0x0D | Esc | 0x1B |
| Shift | 0x10 | Ctrl | 0x11 | Alt | 0x12 |
| Tab | 0x09 | Backspace | 0x08 | Delete | 0x2E |

### 11.2 Format zapisu

- Zawsze używaj formatu hex z prefiksem `0x`
- Przykład: `KEY = 0x57` (nie `KEY = 87`)

---

## 12. Przykłady różnych konfiguracji

### 12.1 Tylko klawiatura (bez koła)

```ini
[HEADER]
INPUT_METHOD = 0

[STEER]
JOY = -1
AXLE = -1

[THROTTLE]
JOY = -1
AXLE = -1
KEY = 0x57

[BRAKES]
JOY = -1
AXLE = -1
KEY = 0x53

[GEARUP]
JOY = -1
BUTTON = -1
KEY = 0x45

[GEARDN]
JOY = -1
BUTTON = -1
KEY = 0x51

[HANDBRAKE]
JOY = -1
BUTTON = -1
KEY = 0x48

[SHIFTER]
ACTIVE = 0
JOY = -1

[ACTION_CHANGE_CAMERA]
JOY = -1
BUTTON = -1
KEY = 0x43

[RESET_RACE]
KEY = 0x52
```

### 12.2 Koło + klawiatura (mieszane)

```ini
[HEADER]
INPUT_METHOD = 1

[STEER]
JOY = 0
AXLE = 0
SCALE = 1.0

[THROTTLE]
JOY = 0
AXLE = 1

[BRAKES]
JOY = 0
AXLE = 2

[CLUTCH]
JOY = 0
AXLE = 3

[GEARUP]
JOY = -1
BUTTON = -1
KEY = 0x57

[GEARDN]
JOY = -1
BUTTON = -1
KEY = 0x53

[HANDBRAKE]
JOY = 0
BUTTON = 4
KEY = 0x48

[SHIFTER]
ACTIVE = 0
JOY = -1

[ACTION_CHANGE_CAMERA]
JOY = 0
BUTTON = 5
KEY = 0x43

[RESET_RACE]
KEY = 0x52
```

### 12.3 H-shifter włączony

```ini
[HEADER]
INPUT_METHOD = 1

[STEER]
JOY = 0
AXLE = 0

[THROTTLE]
JOY = 0
AXLE = 1

[BRAKES]
JOY = 0
AXLE = 2

[CLUTCH]
JOY = 0
AXLE = 3

[GEARUP]
JOY = -1
BUTTON = -1
KEY = -1

[GEARDN]
JOY = -1
BUTTON = -1
KEY = -1

[HANDBRAKE]
JOY = 0
BUTTON = 4

[SHIFTER]
ACTIVE = 1
JOY = 0
GEAR_1 = 0
GEAR_2 = 1
GEAR_3 = 2
GEAR_4 = 3
GEAR_5 = 4
GEAR_6 = 5
GEAR_7 = 6
GEAR_R = 7

[ACTION_CHANGE_CAMERA]
JOY = 0
BUTTON = 5
KEY = 0x43

[RESET_RACE]
KEY = 0x52
```

---

## 13. Uwagi końcowe

### 13.1 Wartości -1

- **JOY = -1** = nie używany
- **BUTTON = -1** = nie używany
- **KEY = -1** = nie używany
- **AXLE = -1** = nie używany

### 13.2 Priorytety

- Jeśli ustawisz zarówno **BUTTON** jak i **KEY**, oba będą działać
- Gra akceptuje input z obu źródeł jednocześnie

### 13.3 Backup

⚠️ **Zawsze rób backup pliku `controls.ini` przed ręczną edycją!**

### 13.4 Sprawdzanie zmian

- Po zapisaniu pliku, uruchom grę
- Sprawdź czy bindy działają poprawnie
- Jeśli coś nie działa, przywróć backup

---

## 14. Podsumowanie - szybka referencja

| Bind | Sekcja | JOY | BUTTON/AXLE | KEY |
|------|--------|-----|-------------|-----|
| Steering Wheel | `[STEER]` | 0 | AXLE = 0 | - |
| Throttle | `[THROTTLE]` | 0 | AXLE = 1 | - |
| Brakes | `[BRAKES]` | 0 | AXLE = 2 | - |
| Clutch | `[CLUTCH]` | 0 | AXLE = 3 | - |
| Gear Up | `[GEARUP]` | 0/-1 | BUTTON = 2 | 0x57 |
| Gear Down | `[GEARDN]` | 0/-1 | BUTTON = 3 | 0x53 |
| Handbrake | `[HANDBRAKE]` | 0/-1 | BUTTON = 4 | 0x48 |
| H-Shifter | `[SHIFTER]` | 0 | ACTIVE = 1 | - |
| Change View | `[ACTION_CHANGE_CAMERA]` | 0/-1 | BUTTON = 5 | 0x43 |
| Restart Race | `[RESET_RACE]` | - | - | 0x52 |

---

**Gotowe!** Teraz wiesz jak zapisać wszystkie bindy w pliku `controls.ini`.

