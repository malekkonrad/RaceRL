# RaceRL
Autonomous racing with reinforcement learning.

## Opis projektu

Projekt polega na wykonaniu symulacji/gry 3d wyścigów samochodowych. W grze będą występowały różne tory wyścigowe. Symulowane będą opory powietrza, drafting, downforce, przyczepność kół. W tym środowisku będą poruszały się samochody sterowane przez Agentów AI. Agent będzie agentem reinforcement learning. Agent będzie wiedział o wektorze prędkości samochodu, wektorze kierunku samochodu, nadchodzącym zakręcie (odległość oraz typ), czy koła się ślizgają oraz siatkę promieni mierzącą odległość do obiektu. Agent będzie miał kontrolę nad sterowaniem kół (od -1 do 1, wartość ciągła), pedałem gazu (wartość ciągła), hamulcem (wartość ciągła). Agenci mają za zadanie wygrać wyścig o długości kilku kółek wokół toru. Agenci mogą się zderzać oraz sobie przeszkadzać, mamy nadzieje zobaczenia typowego zachowania jak drafting czy pchanie siebie (tandem racing).

## Technologie

 - Unity
 - idk

## Literatura

 - idk

## Harmonogram:

#### Faza 1: Środowisko (2-3 tygodnie)

 - Implementacja fizyki/wybór narzędzia
 - Prosty tor testowy
 - Observation space + action space

#### Faza 2: Baseline (1-2 tygodnie)

 - Prosty heurystyczny agent (follow waypoints)
 - Pierwszy trening RL (PPO)
 - Metryki: completion rate, lap time

#### Faza 3: Optymalizacja (3-4 tygodnie)

 - Reward shaping
 - Curriculum learning (proste → trudne tory)
 - Hyperparameter tuning
 - Porównanie algorytmów

#### Faza 4: Multi-agent racing (2-3 tygodnie)

 - Trenowanie przeciwko sobie
 - Strategia wyprzedzania
 - Wyścigi turniejowe

#### Faza 5: Dokumentacja (1 tydzień)

 - Raport
 - Wykresy uczenia
 - Video demonstracje
