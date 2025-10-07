# RaceRL
Autonomous racing with reinforcement learning.


## Cel projektu:

Stworzenie inteligentnego systemu wyścigowego z wykorzystaniem reinforcement learning, w którym agenci AI najpierw uczą się podstaw jazdy, a następnie rozwijają umiejętności kompetytywnego ścigania się ze sobą w czasie rzeczywistym. Projekt zakłada progresywne nauczanie agentów - od podstawowych umiejętności sterowania pojazdem, przez optymalizację tras, aż po zaawansowane strategie wyścigowe i interakcje między konkurującymi agentami.



## Zakres projektu:

#### Środowisko symulacyjne:
- Budowa kompletnego środowiska wyścigowego 3D w Unity (tor, checkpointy, system detekcji kolizji)
- Integracja Unity ML-Agents Toolkit
- Implementacja fizyki pojazdów (realistyczna jazda, przyczepność, zarządzanie prędkością)
- System wizualizacji i monitorowania treningu

#### Modelowanie agentów:
- **Etap 1: Podstawowa jazda** - agent uczy się trzymać tor, nie wypadać, kontrolować prędkość
- **Etap 2: Optymalizacja** - minimalizacja czasu okrążenia, optymalne tory przejazdu zakrętów
- **Etap 3: Wyścigi kompetytywne** - strategie wyprzedzania, obrona pozycji, świadomość przeciwników

#### Systemy uczące

- Projektowanie przestrzeni obserwacji (raycasty, prędkość, pozycja na torze, pozycje przeciwników)
- Definiowanie przestrzeni akcji (sterowanie, przyspieszenie, hamowanie)
- Funkcje nagród dostosowane do każdego etapu uczenia
- Curriculum learning - stopniowe zwiększanie trudności

#### Wizualizacja i prezentacja

- System kamer do obserwacji wyścigu
- UI z informacjami o wyścigu (pozycje, czasy okrążeń)
- Narzędzia do analizy postępów treningu (wykresy, statystyki)
- Demo finałowe z wyścigiem na żywo


## Harmonogram:

###  Tydzień 1 (13-17.10): Inicjalizacja projektu
- [ ] Instalacja i konfiguracja środowiska Unity oraz ML-Agents Toolkit
- [ ] Zapoznanie się z środowiskiem Unity oraz przegląd dokumentacji ML-Agents
- [ ] Stworzenie podstawowego projektu Unity z prostym torem

### Tydzień 2 (20-24.10): Podstawowe środowisko treningowe
- [ ] Modelowanie prostego toru wyścigowego (prosta + kilka zakrętów)
- [ ] Implementacja systemu checkpointów
- [ ] Podstawowa kamera i oświetlenie
- [ ] Import lub stworzenie prostego modelu pojazdu 3D

### Tydzień 3 (27-31.10): Fizyka pojazdu
- [ ] Konfiguracja Rigidbody i colliderów
- [ ] Implementacja systemu sterowania (WheelColliders)
- [ ] Podstawowa fizyka: przyspieszanie, hamowanie, skręcanie
- [ ] Testowanie manualnego sterowania (klawiatura)

### Tydzień 4 (3-7.11): Integracja ML-Agents
- [ ] Integracja ML-Agents Toolkit z projektem
- [ ] Konfiguracja agentów i środowiska
- [ ] Implementacja podstawowych funkcji nagród
- [ ] Testowanie i debugowanie agentów

### Tydzień 5 (10-14.11): Pierwszy trening - podstawowa jazda
- [ ] Uruchomienie pierwszego treningu (algorytm PPO)
- [ ] Monitoring TensorBoard
- [ ] Iteracyjne dostrajanie nagród i obserwacji
- [ ] Agent przechodzący prosty tor bez wypadania

### Tydzień 6 (17-21.11): Rozbudowa środowiska treningowego
- [ ] Projektowanie bardziej skomplikowanego toru (więcej zakrętów, wzniesienia)
- [ ] Implementacja randomizacji środowiska (różne kolory, tekstury)
- [ ] Optymalizacja performance'u treningu

### Tydzień 7 (24-28.11): Optymalizacja funkcji nagrody
- [ ] Zaawansowane nagrody: bonus za prędkość, kara za zbyt ostre manewry
- [ ] Nagrody za optymalne linie przejazdu (apex points)
- [ ] Testowanie różnych wag nagród

### Tydzień 8 (24-28.11): Curriculum Learning
- [ ] Implementacja poziomów trudności toru
- [ ] Stopniowe wprowadzanie bardziej skomplikowanych sekcji
- [ ] Automatyczna zmiana trudności w zależności od postępów

### Tydzień 9 (1-5.12): Przygotowanie do wyścigów - Multi-Agent
- [ ] Rozszerzenie sceny o możliwość wielu agentów jednocześnie
- [ ] System detekcji innych pojazdów (raycasty, sensory)
- [ ] Rozszerzona przestrzeń obserwacji (pozycje przeciwników)
- [ ] Zarządzanie kolizjami między agentami

### Tydzień 10 (8-12.12): Self-Play - trening kompetytywny
- [ ] Konfiguracja ML-Agents Self-Play
- [ ] Nagrody za wyprzedzanie przeciwników
- [ ] Kary za zderzenia (ale nie za bliskość)
- [ ] Nagrody za obronę pozycji

### Tydzień 10 (15-19.12): Zaawansowane strategie wyścigowe
- [ ] Fine-tuning nagród dla strategii (blokowanie, zajmowanie idealnej linii)
- [ ] Implementacja "świadomości wyścigu" (pozycja w stawce)
- [ ] Różne style jazdy (agresywny vs defensywny)
- [ ] Long-term planning rewards

### Tydzień 11 (7-9.01): System wyścigowy i UI
- [ ] Implementacja systemu startu wyścigu (grid startowy, countdown)
- [ ] System zliczania okrążeń
- [ ] Klasyfikacja na żywo
- [ ] UI: pozycje, czasy, najszybsze okrążenie
- [ ] System kamer (śledząca lidera, swobodna, onboard)

### Tydzień 12 (12-16.01): Wizualizacja i analiza
- [ ] Dashboard z metrykami treningu
- [ ] Wizualizacja decyzji agenta (debug rays, heatmapy)
- [ ] Statystyki: średnie czasy, Win Rate, liczba wyprzedzeń

### Tydzień 13 (19-23.01): Prezentacja i dokumentacja
- [ ] Finalna dokumentacja projektu (architektura, wyniki eksperymentów)
- [ ] Analiza wyników (porównanie metod, wykres learning curves)
- [ ] Wnioski i możliwe rozszerzenia
