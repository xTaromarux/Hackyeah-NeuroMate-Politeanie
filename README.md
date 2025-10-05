🧠 NeuroMate — HackYeah 2025

Krótko:
NeuroMate to desktopowo-mobilny asystent biohackingu z awatarem 👾 — wykrywa spadki koncentracji i przeciążenie, łączy dane z wearables i analizę behawioralną, umożliwia krótkie gry kognitywne i mikro-interwencje (rozciąganie, oddech, przerwa), a wszystko prowadzi sympatyczny awatar-coach 💬

📑 Spis treści

Opis projektu

Najważniejsze funkcje (MVP)

Architektura i stos technologiczny

Szybki start — uruchomienie lokalne

Zespół

Struktura projektu

🧩 1. Opis projektu

NeuroMate to aplikacja desktopowa (Windows) oraz mobilna (Android), zaprojektowana jako osobisty asystent poprawiający wydajność poznawczą i higienę pracy 🧘‍♀️💡

Aplikacja:

📊 Zbiera i agreguje codzienne metryki (sen, HR/HRV, kroki, aktywność) z urządzeń typu wearables

🧠 Oceniia krótkoterminowy poziom „Focus” (LOW / MED / HIGH)

⚡ Wykrywa spadki koncentracji w czasie rzeczywistym

🕹️ Proponuje mikro-interwencje (ćwiczenia oddechowe, krótkie gry kognitywne, przypomnienia o przerwie)

🤖 Prowadzi interakcję z użytkownikiem przez animowanego awatara-coacha

📁 Zapisuje logi i pozwala eksportować wyniki

🚀 2. Najważniejsze funkcje (MVP)

🔍 Scoring „Focus” oparty na regułach (sen, HRV, aktywność)

🧩 Wykrywanie spadków koncentracji w czasie rzeczywistym

👾 Awatar-coach — komunikaty tekstowe + proste animacje

💨 Zestaw mikro-interwencji: oddechowe, kognitywne, stretching, przypomnienia

🏆 System punktowy i sklep z awatarami (customizacja postaci)

🧠 Trzy gry kognitywne: Task Switching, Stroop Test, PVT (Psychomotor Vigilance Test)

📥 Import i eksport danych aktywności

🤖 Inteligentny system rekomendacji interwencji w zależności od poziomu zmęczenia

🏗️ 3. Architektura i stos technologiczny

Główne technologie:

💻 C# / .NET MAUI — aplikacja desktopowa + mobilna

🧱 MVVM (CommunityToolkit.MVVM)

🗄️ SQLite — lokalna baza danych

📨 Messenger / MessageBus — komunikacja wewnętrzna

Kluczowe serwisy:

Serwis	Opis
🎭 AvatarService	Zarządzanie awatarami i personalizacją
💰 PointsService	System punktów i ekonomii
🧘 InterventionService	Logika interwencji i rekomendacji
🧠 PvtGameService	Gry kognitywne (Stroop, Task Switch, PVT)
🗃️ DatabaseService	Warstwa persystencji danych

Warstwy aplikacji:

🎨 UI (MAUI) — ekran główny, status Focus, awatar, akcje

⚙️ Service Layer — logika scoringu i interwencji

💾 Persistence — dane w SQLite / JSON

📊 Analytics (eksperymentalne) — moduły ML offline

⚡ 4. Szybki start — uruchomienie lokalne
🧠 NeuroMate – Instalacja
🔹 Dla użytkowników

Przejdź do zakładki Releases

Pobierz odpowiedni plik:

🪟 Windows: NeuroMate-win-x64.msix

🤖 Android: NeuroMate-Android.apk

Zainstaluj i uruchom aplikację

🔹 Dla deweloperów

Jeśli chcesz zbudować aplikację ze źródeł:

git clone https://github.com/xTaromarux/Hackyeah-NeuroMate-Politeanie.git
cd Hackyeah-NeuroMate-Politeanie
dotnet restore
dotnet build
dotnet run -f net8.0-windows


💡 Alternatywnie:
Otwórz projekt w Visual Studio 2022+ i wybierz platformę uruchomieniową:

🪟 Windows (Desktop)

🤖 Android (Emulator / Device)

⚙️ Konfiguracja pierwszego uruchomienia

📂 Aplikacja automatycznie utworzy lokalną bazę SQLite przy pierwszym starcie

🧍 Domyślny awatar zostanie przydzielony użytkownikowi

💎 Startowe punkty pozwolą na zakupy w sklepie z awatarami

🌐 Dostępne platformy

🪟 Windows (x64) — pełne wsparcie

🤖 Android — pełne wsparcie i synchronizacja funkcji

👥 5. Zespół — Politeanie
Imię i nazwisko	Rola
🎨 Agata Sobczyk	Research, prezentacja, neurostrona
🧠 Eustachy Lisiński	Backend / Architektura
💻 Kamil Gatlik	  Analiza danych / Integracje AI
🕹️ Miłosz Wojtanek	Gry kognitywne / Logika testów
⚙️ Paweł Kwolek	 Grafika, Animacje
🔊 Sławek Put 	UI/UX, Frontend
🗂️ 6. Struktura projektu
📁 /Views        → Strony XAML i code-behind  
📁 /ViewModels   → Modele widoku (MVVM)  
📁 /Services     → Serwisy aplikacji  
📁 /Database     → Warstwa danych (SQLite)  
📁 /Messages     → Komunikaty systemowe  
📁 /Models       → Modele danych

🏁 Podsumowanie

NeuroMate to projekt zrodzony z idei human-centered AI,
łączący dane biometryczne, psychologię poznawczą i gamifikację.
Z nami — Twoja koncentracja nie spadnie! 💪🧠
