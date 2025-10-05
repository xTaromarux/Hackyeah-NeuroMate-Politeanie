# NeuroMate — HackYeah 2025 (NeuroMate / NeuroPilot)

**Krótko:** NeuroMate to desktopowy asystent biohackingu z awatarem — wykrywa spadki koncentracji i przeciążenie, łączy dane z wearables i prostą analizę behawioralną, uruchamia krótkie gry kognitywne i mikro-interwencje (np. rozciąganie, oddech, przerwa), a wszystko prowadzi sympatyczny awatar. Repozytorium: https://github.com/xTaromarux/Hackyeah-NeuroMate-Politeanie.

> Uwaga: Bazuję na strukturze repo widocznej na GitHub (C#, .NET/MAUI projekt) oraz materiałach konkursowych HackYeah. Jeśli chcesz, mogę dopracować README dopiero po podaniu konkretnych plików / fragmentów kodu, ale poniższa wersja jest kompletna i gotowa do wstawienia jako pierwszy README w repo.

---

## Spis treści
1. Opis projektu  
2. Najważniejsze funkcje  
3. Architektura i stos technologiczny  
4. Szybki start — uruchomienie lokalne  
5. Konfiguracja i integracje (wearables, pliki EEG itp.)  
6. Jak przygotować demo / zgłoszenie na HackYeah  
7. Struktura repozytorium (proponowana)  
8. Testy / CI / packaging  
9. Wkład (Contribution), licencja, kontakt  
10. Znane ograniczenia i TODO

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
- Import/wczytanie dziennych danych z wearables (CSV / prosty JSON).  
- Prosty moduł scoringu Focus (LOW / MED / HIGH) oparty na regułach: sen, HRV, aktywność.  
- Wykrywanie spadku koncentracji w czasie rzeczywistym (na bazie inputów behawioralnych i biometriki).  
- Awatar-coach (tekst + proste animacje/gif).  
- Kolekcja mikro-interwencji: 60s ćwiczenia oddechowe, 2–3 min gry kognitywne (Stroop, PVT-lite), przypomnienia o przerwie.  
- Eksport raportów (CSV/PDF) + screeny do prezentacji.  

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

# 5. Konfiguracja i integracje
**Import danych (MVP):**
- Format CSV: kolumny rekomendowane: `date, total_sleep_minutes, sleep_efficiency, hrv_rmssd, hr_mean, steps_total, active_minutes`.
- W folderze `data/` umieść plik `sample_input.csv` i użyj funkcji `Import` w aplikacji.

**Instrukcja integracji z urządzeniami:**
- Konsumenckie wearables: eksportuj dane z aplikacji producenta (Fitbit / Garmin / Oura) do CSV i importuj.  
- Dla integracji API (opcjonalnie): utwórz adapter, który mapuje dostarczone pola na nasz model danych.

**Konfiguracja scoringu:**
- Parametry progów (np. min sleep, HRV thresholds) trzymane w `config.json`. Możesz je edytować, by dostosować reguły.

---

# 6. Jak przygotować demo / zgłoszenie na HackYeah
HackYeah wymaga przesłania opisu projektu + max 10-slajdowej prezentacji oraz (opcjonalnie) repo/dema. W README warto dołączyć sekcję „HackYeah submission checklist” i przygotować materiały:

**Checklist (wyjściowa):**
- [ ] Tytuł projektu (NeuroMate / NeuroPilot).  
- [ ] Krótki opis (1-2 zdania).  
- [ ] Zespół (lista osób + role).  
- [ ] PDF — max 10 slajdów (pitch + demo).  
- [ ] Link do repo + instrukcja uruchomienia (this README).  
- [ ] Krótkie nagranie demo (30–90s) / link do aplikacji.  
- [ ] Screenshots: UI, awatar, wykres FocusScore.  

Przypomnienie: zasady i kryteria HackYeah — Idea & Innovation (30%), Relation to Category (20%), Practical Applicability (20%), Design (20%), Completeness (10%).

---

# 7. Struktura repozytorium (proponowana / spodziewana)
```
/ (root)
├─ NeuroMate.sln
├─ NeuroMate/                  # projekt MAUI
│  ├─ App.xaml
│  ├─ MainPage.xaml
│  ├─ Views/
│  ├─ ViewModels/
│  ├─ Services/                # import danych, scoring
│  └─ Resources/Images         # avatar, assets
├─ scripts/
│  ├─ build.ps1
│  └─ pack.sh
├─ data/
│  └─ sample_input.csv
├─ docs/
│  └─ pitch_slides.pdf
└─ README.md
```

---

# 8. Testy, CI i packaging
**Testy:** Dodaj projekt testowy (xUnit / NUnit) z kilkoma testami jednostkowymi: scoring, import CSV, logika interwencji.

**CI (GitHub Actions) — przykładowy flow:**
- on: push na `main`
- steps:
  - checkout
  - setup-dotnet (dotnet-version: 8.x)
  - dotnet restore, dotnet build, dotnet test
  - opcjonalnie: build artefaktu (zip) i upload do Release

**Packaging:** Dla Windows — self-contained EXE/MSIX. Dla macOS — .app. Dla szybkości demo możesz udostępnić `dotnet publish -c Release -r win-x64 --self-contained`.

---

# 9. Contribution, licencja, kontakt
**Contribution**
- Wszelkie PRy mile widziane — otwórz issue z opisem.
- Styl kodu: C# idiomatyczny, bez zbędnych komentarzy (preferencje użytkownika).  
- Branch model: feature/*, fix/*, hotfix/*; PR do `main`.

**Licencja**
- Dodaj plik `LICENSE` (np. MIT) — jeśli chcesz, mogę wygenerować.

**Kontakt**
- Autor repo: xTaromarux (GitHub)  
- E-mail / Slack / Discord — dopisz kontakt zespołu tutaj.

---

# 10. Znane ograniczenia i TODO
**Znalezione / przewidywane:**
- UI i animacje awatara — MVP: GIF / proste animacje; docelowo: Lottie lub sprite animation.  
- Integracje z API producentów wearables wymagają autoryzacji OAuth i osobnych adapterów.  
- Brak testów automatycznych (zalecane dodać).  
- Model ML/zaawansowana fuzja danych (EEG + behawioralne) — poza MVP, roadmap.

**Proponowane priorytety (na HackYeah):**
1. Solidny, działający demo-flow: import CSV → FocusScore → interwencja → eksport raportu.  
2. Atrakcyjna prezentacja i kilka screenshotów / krótki filmik demo.  
3. Stabilne buildy dla Windows (łatwe uruchomienie przez jurorów).

---

## Źródła
- Strona repo GitHub: https://github.com/xTaromarux/Hackyeah-NeuroMate-Politeanie  
- Reguły i wymagania konkursu HackYeah — Open Task: Biohacking
