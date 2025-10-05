# NeuroMate — HackYeah 2025

**Krótko:** NeuroMate to desktopowy/mobilny asystent biohackingu z awatarem — wykrywa spadki koncentracji i przeciążenie, łączy dane z wearables i analizę behawioralną, pozwala na uruchmianie krótkich gier kognitywnych i mikro-interwencyjnych (np. rozciąganie, oddech, przerwa), a wszystko prowadzi sympatyczny awatar.

---

## Spis treści
1. Opis projektu  
2. Najważniejsze funkcje  
3. Architektura i stos technologiczny  
4. Szybki start — uruchomienie lokalne  
5. Zespół
---

# 1. Opis projektu
NeuroMate to aplikacja desktopowa (Windows/macOS/Linux) zaprojektowana jako asystent poprawiający wydajność poznawczą i higienę pracy. Aplikacja:
- zbiera i agreguje codzienne metryki (sen, HR/HRV, kroki, aktywność) z konsumenckich wearables (lub importu CSV),  
- ocenia krótkoterminowy poziom „Focus” i wykrywa nagłe spadki koncentracji,  
- proponuje micro-interwencje (ćwiczenia oddechowe, krótkie gry kognitywne, zalecenia rozciągające),  
- prowadzi interakcję przez animowanego awatara (feedback, nagrody, motywacja),  
- zapisuje logi i możliwość eksportu wyników do pliku.  

Projekt powstał z myślą o zgłoszeniu do kategorii **Biohacking** na HackYeah 2025. Wymogi HackYeah (format zgłoszenia, 10 slajdów, kryteria oceny) zostały uwzględnione w planie.

---

# 2. Najważniejsze funkcje (MVP)
- 
- Moduł scoringu Focus (LOW / MED / HIGH) oparty na regułach: sen, HRV, aktywność.  
- Wykrywanie spadku koncentracji w czasie rzeczywistym (na bazie inputów behawioralnych i biometriki).  
- Awatar-coach (tekst + proste animacje/gif).  
- Kolekcja mikro-interwencji: 60s ćwiczenia oddechowe, 2–3 min gry kognitywne (Stroop, PVT-lite), przypomnienia o przerwie.  
- 

---

# 3. Architektura i stos technologiczny
**Główne technologie (zgodnie z repo):**
- C# / .NET MAUI — aplikacja desktopowa (UI multiplatform).  
- PowerShell — skrypty pomocnicze (build/deploy).  
- (opcjonalnie) Python — moduły analizy/ML (jeśli model będzie użyty lokalnie; tutaj miejsce na integrację).  

**Proponowana warstwa logiczna:**
- UI (MAUI) — ekran główny, status focus, awatar, szybkie akcje.  
- Service Layer — pobieranie/import danych, scoring, reguły interwencji.  
- Persistence — lokalna baza/pliki (SQLite lub lokalne JSON).  
- Integracja z wearables — adaptery CSV / API bridge (opcjonalnie).  
- Moduł analytics / eksperymentalny ML — offline / batch (poza MVP).

---

# 4. Szybki start — uruchomienie lokalne

> **Prerekwizyty**
- .NET 8 SDK (lub wersja wymagana przez projekt).  
- Visual Studio 2022/2023 z workload .NET MAUI (Windows) lub Visual Studio for Mac (macOS).  
- (opcjonalnie) Android/iOS SDK, jeśli chcesz budować mobilne artefakty.  

> **Instalacja (Windows / macOS / Linux — ogólnie)**

1. Sklonuj repo:
```bash
git clone https://github.com/xTaromarux/Hackyeah-NeuroMate-Politeanie.git
cd Hackyeah-NeuroMate-Politeanie
```

2. Przywróć zależności i zbuduj (dotnet CLI):
```bash
dotnet restore
dotnet build
```

3. Uruchom aplikację (dla Windows desktop — przykład):
```bash
dotnet run -f net8.0-windows
```

4. Alternatywnie: otwórz rozwiązanie w Visual Studio i uruchom z IDE (wybierz platformę: Windows / macOS).

---

# 5. Zespół
**Politeanie**
- Agata Sobczyk
- Eustachy Lisiński
- Kamil Gatlik
- Miłosz Wojtanek
- Paweł Kwolek
- Sławek Put 

---
