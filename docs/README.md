# Dokumentacja projektu RaceRL - Uczenie ze wzmocnieniem w wyścigach samochodowych

## Wprowadzenie

### Cel projektu

Projekt RaceRL ma na celu stworzenie inteligentnego systemu wyścigowego wykorzystującego uczenie ze wzmocnieniem (Reinforcement Learning), w którym autonomiczni agenci AI uczą się nie tylko podstaw jazdy, ale także zaawansowanych strategii kompetytywnego ścigania. Głównym założeniem jest progresywne nauczanie agentów - od podstawowych umiejętności sterowania pojazdem, przez optymalizację tras i minimalizację czasu okrążenia, aż po zaawansowane strategie wyścigowe i interakcje między konkurującymi agentami w czasie rzeczywistym.

### Zakres

Projekt obejmuje:
- **Środowisko symulacyjne 3D** - kompletne środowisko wyścigowe w Unity z realistyczną fizyką, wieloma torami o różnej trudności oraz systemem checkpointów
- **System wieloagentowy** - możliwość treningu wielu agentów jednocześnie, ze zderzeniami i interakcjami między nimi
- **Curriculum learning** - stopniowe zwiększanie trudności torów (od prostej linii, przez łatwe zakręty, po skomplikowane trasy mieszane)
- **Zaawansowana fizyka pojazdu** - własna implementacja obejmująca realistyczną dynamikę, przyczepność opon, zawieszenie, downforce aerodynamiczny i stabilizatory
- **Kompleksowy system nagród** - nagrody za przejazd checkpointów, ukończenie okrążeń, prędkość, pozycję w wyścigu oraz kary za kolizje

## Zastosowane metody

### Technologie

**Unity ML-Agents Toolkit (v1.1.0)** - główny framework do uczenia ze wzmocnieniem, zapewniający:
- Integrację środowiska Unity z algorytmami uczenia maszynowego
- Infrastrukturę do treningu agentów
- Narzędzia do zarządzania epizodami i zbierania danych treningowych

**PyTorch 2.8.0** - backend do uczenia głębokich sieci neuronowych, wykorzystywany przez ML-Agents

**Unity Engine (2022.3+)** - silnik gry służący jako środowisko symulacyjne 3D

**Algorytm PPO (Proximal Policy Optimization)** - algorytm uczenia ze wzmocnieniem typu policy gradient, charakteryzujący się:
- Stabilnym procesem uczenia dzięki mechanizmowi clipping
- Dobrą wydajnością w środowiskach ciągłych
- Efektywnym wykorzystaniem zebranych danych

### Architektura systemu

Projekt składa się z kilku kluczowych komponentów:

#### 1. Agent (RacistAgent.cs)

Agent stanowi centralny element systemu, dziedziczący po klasie `Agent` z ML-Agents. Zawiera:

**Przestrzeń obserwacji (3 wartości ciągłe):**
- Dot product kierunku agenta względem następnego checkpointa (czy jedzie we właściwym kierunku)
- Prędkość do przodu (forward speed) znormalizowana do [-1, 1]
- Prędkość boczna (lateral speed) - informacja o poślizgu

**Przestrzeń akcji (2 wartości ciągłe):**
- Throttle: przyspieszenie/hamowanie w zakresie [-1, 1]
- Steering: skręt kół w zakresie [-1, 1]

**System nagród:**
- **+0.5** za przejazd przez prawidłowy checkpoint
- **-0.3** za przejazd przez błędny checkpoint
- **+5.0** za ukończenie okrążenia
- **+2.0** za ukończenie wszystkich wymaganych okrążeń
- **+2.0** za wyprzedzenie przeciwnika
- **-0.5** za utratę pozycji
- Ciągła nagroda proporcjonalna do pozycji w wyścigu (1/rank * scale)
- Ciągła nagroda proporcjonalna do prędkości do przodu
- **-0.5** za kolizję ze ścianą (koniec epizodu)
- **-0.5/-0.02** za kolizje z innymi agentami

#### 2. Fizyka pojazdu (SimpleCar.cs, SimpleWheel.cs)

Własna implementacja fizyki powstała z konieczności - standardowe komponenty Unity (WheelColliders) okazały się niewystarczające dla potrzeb symulacji wyścigów i interakcji wieloagentowych.

**SimpleCar - główny kontroler pojazdu:**
- Napęd na tylne koła (RWD)
- Skręcanie przednich kół z geometrią Ackermanna (wewnętrzne koło skręca mocniej)
- Krzywa momentu obrotowego silnika (AnimationCurve)
- System hamulców z podziałem siły (70% przód, 100% tył)
- Downforce aerodynamiczny proporcjonalny do prędkości
- Steer Helper - sztuczna siła stabilizująca, ułatwiająca ML agentom naukę skręcania
- Anti-roll bar (stabilizatory) - redukuje przechyły nadwozia w zakrętach
- Optymalizowany środek ciężkości (0, -0.6f, 0.2f) zapobiegający wywrotkom

**SimpleWheel - model koła z zawieszeniem:**
- Raycasting do detekcji kontaktu z podłożem
- Sprężyna ze wsparciem tłumienia (spring + damper)
- Kompresja zawieszenia uwzględniająca prędkość
- Symulacja przylegania bocznego (lateral grip) - zapobiega ślizgowi
- Filtr warstw (LayerMask) - sprawdza kontakt tylko z określonymi obiektami
- Debug visualization z Gizmos

#### 3. System checkpointów (TrackCheckpoints.cs, CheckpointSingle.cs)

**Funkcjonalności:**
- Rejestracja wielu pojazdów jednocześnie
- Śledzenie postępu każdego pojazdu (następny checkpoint, liczba okrążeń)
- Eventy dla poprawnych/błędnych checkpointów i ukończonych okrążeń
- System rankingu - oblicza pozycję każdego agenta na podstawie:
  - Obecnego checkpointa
  - Dystansu do następnego checkpointa
  - Opcjonalnie liczby okrążeń (dla długich wyścigów)

#### 4. Curriculum Learning (LevelManager.cs + race-ppo.yaml)

System progresywnego uczenia z 5 poziomami trudności:

**Level 0 (track_index=0):** Prosta linia - agent uczy się podstaw przyspieszania i hamowania (próg: reward ≥ 5)

**Level 1 (track_index=1):** Łatwa pętla w lewo - nauka prostych zakrętów (próg: reward ≥ 9)

**Level 2 (track_index=2):** Łatwa pętla w prawo - nauka zakrętów w drugą stronę (próg: reward ≥ 12)

**Level 3 (track_index=3):** Trasa mieszana średniej trudności - kombinacja zakrętów (próg: reward ≥ 40)

**Level 4 (track_index=4):** Trasa mieszana trudna - zaawansowane sekcje wymagające precyzji (próg: reward ≥ 60)

**LevelManager** dynamicznie przełącza aktywne tory w Unity w odpowiedzi na parametr `track_index` z ML-Agents Academy.

#### 5. System wieloagentowy (MultiAgentSpawner.cs)

- Instancjonowanie wielu agentów jednocześnie
- Przypisywanie unikalnych punktów startowych dla każdego agenta
- Automatyczna rejestracja w systemie checkpointów
- Zarządzanie cyklem życia agentów (spawn/despawn)
- Integracja z systemem kamer

### Konfiguracja uczenia (race-ppo.yaml)

```yaml
trainer_type: ppo
batch_size: 1024
buffer_size: 40960
learning_rate: 3.0e-4
hidden_units: 128
num_layers: 2
max_steps: 21,000,000
gamma: 0.99
```

Kluczowe parametry:
- **batch_size: 1024** - liczba przykładów w jednej aktualizacji gradientu
- **buffer_size: 40960** - rozmiar bufora experience replay
- **learning_rate: 3e-4** - standardowa wartość dla PPO
- **hidden_units: 128, num_layers: 2** - sieć neuronowa 128x128
- **max_steps: 21M** - maksymalna liczba kroków treningowych
- **gamma: 0.99** - współczynnik dyskontowania przyszłych nagród

## Implementacja

### Najciekawsze rozwiązania techniczne

#### 1. System nagród oparty na pozycji w wyścigu

Oprócz impulsów nagród za wyprzedzanie, zaimplementowano ciągłą presję motywującą do utrzymania dobrej pozycji:

```csharp
if (newRank > 0)
{
    float rankBonus = 1.0f / (float)newRank;
    AddReward(rankBonus * positionRewardScale);
}
```

Lider otrzymuje 1.0 × scale punktu co klatkę, drugi miejsce 0.5 × scale, trzeci 0.33 × scale itd. To motywuje agenta nie tylko do wyprzedzania, ale także do utrzymania zdobytej pozycji.

#### 2. Własna implementacja fizyki z geometrią Ackermanna

Standardowe rozwiązania Unity nie sprawdzały się w kontekście ML-Agents. Własna implementacja pozwoliła na:
- Precyzyjną kontrolę nad każdym aspektem fizyki
- Deterministyczne zachowanie (kluczowe dla RL)
- Optymalizację wydajności dla wielu jednoczesnych symulacji
- Eliminację niestabilności WheelColliders

Geometria Ackermanna zapewnia realistyczne skręcanie:
```csharp
if(steerInput > 0) nearAngle *= 1.1f; // Wewnętrzne koło skręca mocniej
else farAngle *= 1.1f;
```

#### 3. Ignorowanie kolizji między agentami w fazie uczenia

Opcjonalny mechanizm pozwala wyłączyć kolizje Agent↔Agent poprzez warstwy Unity:

```csharp
Physics.IgnoreLayerCollision(s_AgentLayer, s_AgentLayer, ignoreAgentCollisions);
```

To przyspiesza wczesne etapy treningu, gdzie agenci jeszcze nie opanowali podstaw jazdy i częste kolizje mogłyby uniemożliwić naukę.

#### 4. Grace period po spawnie

Aby uniknąć fałszywych kar za "spawn collisions" (gdy agenci pojawiają się blisko siebie):

```csharp
if (Time.time - lastSpawnTime < spawnGracePeriod) return;
```

Ignoruje kolizje przez pierwsze 0.2s po respawnie.

#### 5. Detekcja warunków awaryjnych

System automatycznie kończy epizod gdy:
- Pojazd przekroczy prędkość 500 m/s (błęd fizyki)
- Pojazd spadnie pod poziom -10 jednostek Y (wypadł z mapy)
- Wszystkie 4 koła straciły kontakt z podłożem przez dłużej niż 0.75s (wywrócił się)

```csharp
if (speed > 500f || transform.position.y < -10f)
{
    SetReward(-1f); 
    EndEpisode();
}
```

#### 6. Steer Helper - pomoc w sterowaniu dla ML

Aby ułatwić agentom naukę skręcania bez nadmiernego driftowania, dodano sztuczną siłę rotacyjną:

```csharp
if (Mathf.Abs(currentSteerAngle) > 1f && rb.linearVelocity.magnitude > 5f)
{
    rb.AddRelativeTorque(Vector3.up * currentSteerAngle * steerHelper * rb.linearVelocity.magnitude);
}
```

To "fake physics" znacząco przyspiesza trening, zachowując przy tym przekonujące zachowanie pojazdu.

## Wyniki

### Status projektu

Projekt został pomyślnie zaimplementowany i działa prawidłowo. Agent jest w stanie nauczyć się podstawowej jazdy i ukończenia okrążeń na prostszych torach.

### Środowisko testowe

**Sprzęt:**
- CPU: [nie podano w dokumentacji]
- GPU: NVIDIA (wykorzystanie CUDA 12.8, cuDNN 9.10)
- RAM: [nie podano w dokumentacji]

**Oprogramowanie:**
- Unity 2022.3+
- ML-Agents Toolkit 1.1.0
- PyTorch 2.8.0
- Python 3.x

### Efektywność i działanie

**Warunki prawidłowego działania:**
- Agent skutecznie uczy się na poziomach 0-2 (prosta linia i łatwe pętle)
- System curriculum learning poprawnie przełącza między torami
- Fizyka pojazdu zachowuje się stabilnie przy normalnych prędkościach (< 500 m/s)
- System checkpointów prawidłowo wykrywa postęp i ukończenie okrążeń
- System wieloagentowy działa z wieloma agentami jednocześnie
- Kolizje są wykrywane i nagradzane zgodnie z założeniami

**Zidentyfikowane problemy:**
- **Curriculum na trudniejszych torach (3-4):** Agent ma trudności z nauką na bardziej skomplikowanych trasach - wymaga dłuższego treningu i potencjalnie dostrojenia nagród
- **Kolizje między agentami:** W fazie treningu wieloagentowego zderzenia mogą być zbyt częste, spowalniając naukę
- **Wywrotki:** Mimo obniżonego środka ciężkości i stabilizatorów, na szybkich zakrętach pojazd czasami może się wywrócić
- **Downforce:** Zbyt duży docisk aerodynamiczny może powodować przybicie pojazdu do podłoża - wymaga fine-tuningu

### Metryki (planowane, nie w pełni zaimplementowane)

W projekcie przewidziano śledzenie następujących metryk:
- Średni czas okrążenia
- Procent ukończonych okrążeń bez kolizji
- Liczba kolizji na wyścig
- Liczba wyprzedzeń
- Analiza linii przejazdu
- Stabilność prędkości i sterowania

## Wnioski

### Osiągnięcia

1. **Skuteczność ML-Agents dla autonomicznej jazdy:** Unity ML-Agents Toolkit w połączeniu z algorytmem PPO okazał się bardzo dobrym wyborem dla tego typu zadania. Agent jest w stanie nauczyć się podstawowych umiejętności jazdy i nawigacji po torze.

2. **Własna fizyka kluczem do sukcesu:** Decyzja o implementacji własnego systemu fizyki pojazdu zamiast używania standardowych WheelColliders Unity była słuszna. Pozwoliło to na pełną kontrolę nad zachowaniem pojazdu i eliminację niestabilności.

3. **Curriculum learning działa:** Stopniowe wprowadzanie trudniejszych torów pozwala agentowi na progresywną naukę - od prostych manewrów do skomplikowanych tras.

4. **System nagród wymaga balansowania:** Złożony system nagród (checkpointy, prędkość, pozycja, kolizje) wymaga starannego dostrajania wag, aby agent uczył się pożądanych zachowań.

### Wyzwania techniczne

1. **Fizyka w uczeniu maszynowym:** Stworzenie deterministycznej i stabilnej fizyki, która jednocześnie jest realistyczna i umożliwia szybką naukę, to trudne zadanie wymagające wielu iteracji.

2. **Balans nagród:** Znalezienie odpowiednich wag dla różnych składowych funkcji nagrody (prędkość vs bezpieczeństwo, agresja vs ostrożność) wymaga eksperymentowania.

3. **Długi czas treningu:** Uczenie agenta do zaawansowanego ścigania wymaga milionów kroków treningowych i znacznych zasobów obliczeniowych.

### Perspektywy rozwoju

Projekt ma duży potencjał rozwoju w następujących kierunkach:

#### Krótkoterminowe rozszerzenia:
- **Optymalizacja curriculum learning:** Dostrojenie progów przejścia między poziomami i wag nagród dla trudniejszych torów
- **Rozbudowa obserwacji:** Dodanie raycastów do wykrywania przeciwników i ścian, informacji o przyczepności opon
- **Lepsza wizualizacja treningu:** Implementacja UI z wykresami uczenia, statystykami wyścigów, heatmapami

#### Średnioterminowe cele:
- **Zaawansowane strategie wyścigowe:** Nauka draftingu, blokowania, optymalnych linii przejazdu
- **Różnorodność agentów:** Różne style jazdy (agresywny, defensywny, balanced)
- **Trening przeciwko samemu sobie (self-play):** Mechanizm, gdzie agent trenuje przeciwko swoim poprzednim wersjom

#### Długoterminowa wizja:
- **Model Pacejka dla opon:** Realistyczna symulacja przyczepności z uwzględnieniem temperatury, zużycia
- **Strategie pit-stopów:** Zarządzanie paliwem, oponami, naprawami
- **Warunki pogodowe:** Deszcz, zmienne warunki przyczepności
- **Transfer learning:** Wykorzystanie wytrenowanego modelu jako punkt startowy dla innych torów/warunków
- **Multiplayer z ludźmi:** Możliwość rywalizacji człowiek vs AI

### Podsumowanie

Projekt RaceRL demonstruje potencjał uczenia ze wzmocnieniem w zastosowaniach symulacyjnych, szczególnie w kontekście autonomicznej jazdy i strategii wyścigowych. Połączenie Unity ML-Agents, własnej fizyki pojazdu i curriculum learning stworzyło solidną podstawę do dalszego rozwoju w kierunku w pełni autonomicznych, kompetytywnych agentów wyścigowych. Mimo napotkanych wyzwań technicznych, system działa prawidłowo i stanowi dobry punkt wyjścia do bardziej zaawansowanych eksperymentów z deep reinforcement learning w środowisku 3D.