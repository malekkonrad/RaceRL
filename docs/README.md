# Dokumentacja projektu RaceRL - Uczenie ze wzmocnieniem w wyścigach samochodowych

# Wstepna wersja


## Rozwiązania techniczne
- Raycasting do wykrywania przeszkód i ścian toru
- Własna implementacja fizyki pojazdu (Rigidbody + siły)


## Obecny stan prac



## Napotkane problemy i wyzwania
- fizyka pojazdu wymaga własnej implementacji ze względu na specyficzne potrzeby symulacji wyścigów i interakcji między agentami - standardowe komponenty Unity (WheelColliders) okazały się niewystarczające

- podskakiwanie pojazdu przy przechodzeniu przez checkpointy (czasami odlatywał w górę) 
- nieoczywiste przejście między sterowaniem manualnym a sterowaniem przez agenta ML-Agents (w trakcie)


## Plany na przyszłość