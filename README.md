# Ehternum – Eine neue Heimat

Ein spielbarer 2D-Survival-Prototyp in **Godot 4.4.1 .NET und C# (.NET 8)**.
Originale, im Spiel gezeichnete Vektorgrafik, Draufsicht, friedliche Tierwelt.
Für PC und Controllerbedienung / Steam Deck mit 1280 × 800 ausgelegt.
KI-generierter Quellcode und Grafikcode, erstellt für Steph.

## Starten

1. Godot **4.4.1 .NET** installieren (die Standardversion ohne .NET reicht nicht).
2. Das **.NET 8 SDK** installieren, nicht nur die Runtime.
3. ZIP vollständig entpacken. In Godot `project.godot` importieren.
4. Oben rechts **Build** ausführen, danach **F6/F5** beziehungsweise Projekt starten.
5. „Neue Expedition“ auswählen.

Beim ersten Build benötigt Godot Zugriff auf NuGet. Die mitgelieferte
`.csproj` legt die Engine-SDK-Version fest. Andere Godot-Versionen erst nach
Anpassung und erneutem Test verwenden.

## Spielablauf

Die Kapsel enthält Replikator, Bio-Bett, Basisspeicher und Solar-Notversorgung.
Ein Laser und Vorräte sind vorhanden. Suche die drei Kristalltypen:

| Rohstoff | Darstellung | Nutzen |
|---|---|---|
| Ehternum | Lavendel | Speichererweiterungen, Archiventschlüsselung, Spezialbauteile |
| Elekrerium | Ocker | Basisenergie; eine Einheit liefert 25 Energie |
| Universerium | Türkis | Grundstoff für den Replikator |

An der Kapsel E / A drücken. Ehternum zunächst in Speicherstufe 1 investieren,
danach Werkzeug- oder Medizinarchiv entpacken. Weitere Speicherstufen erschließen
Energie-, Habitat- und Kommunikationsarchiv. Freischaltungen bleiben erhalten;
Stufen sind Kapazitätsschwellen, keine pro Rezept verbrauchten Speicherplätze.

Replizierte Nahrung, Wasser, Medikamente und Ausrüstung im Inventar benutzen.
Kisten ebenfalls über das Inventar platzieren. Aufgestellte Kisten mit E / A
öffnen; Ein- und Auslagerung erfolgt in Portionen bis zehn Einheiten.
Kisten innerhalb von 360 Welteinheiten um die Kapsel versorgen den Replikator.

Im Osten liegen Giftsporen, im Westen krank machende kalte Gewässer. Im Süden
der Kapsel liegt scharfkantiger Schrott, der die Bewegung durch Verletzungen
erschweren kann. Das Bio-Bett heilt alle drei Zustände für 18 Energie sofort.
Freundliche Tiere können mit E / A gestreichelt werden. Es gibt keine Gegner.

Habitatgarten und Sender bei der Kapsel aufstellen. Der Garten versorgt dich im
Basisbereich mit Nahrung und Wasser. Der Sender braucht für 60 Sekunden Energie;
danach wird die Rettung bestätigt. Epilog oder freies Weiterspielen sind möglich.
Beide Fortschrittsziele bleiben im Spielstand erhalten.

## Steuerung

| Funktion | Tastatur / Maus | Controller / Steam Deck |
|---|---|---|
| Bewegung | WASD / Pfeile | Linker Stick |
| Zielrichtung | Maus beim Lasern / Bewegungsrichtung | Rechter Stick |
| Laser | Linke Maustaste / Leertaste halten | RT oder X halten |
| Interaktion | E | A |
| Inventar | I / Tab | Y |
| Menünavigation | Pfeile, Enter | Steuerkreuz, A |
| Zurück | Escape | B |
| Pause | P | Start |
| Essen / Trinken | 1 / 2 | Über Inventarmenü |
| Speichern / Laden | F5 / F9 | Über Pausemenü |

Menüs pausieren Überleben, Tiere und Weltzeit. Ein begonnener Replikationsauftrag
läuft weiter. Seine Restzeit und bezahlten Zutaten werden gespeichert. Ist der
Rucksack bei Fertigstellung voll, wartet das Produkt, bis Platz frei wird.

## Speichern

Automatisch alle 45 Sekunden aktiver Weltzeit und beim normalen Schließen des
Spielfensters. Ein Spielstand, JSON-Version 1, im Godot-`user://`-Verzeichnis:

- Linux / Steam Deck: `~/.local/share/godot/app_userdata/Ehternum – Eine neue Heimat/`
- Windows: `%APPDATA%/Godot/app_userdata/Ehternum – Eine neue Heimat/`

Eine neue Expedition ersetzt den bisherigen Spielstand erst beim nächsten
Speichern; zuvor erscheint eine Bestätigung. Ressourcen regenerieren nach 75
Sekunden Weltzeit. Solar-Notladung und Bergung bei Bewusstlosigkeit verhindern
einen dauerhaft verlorenen Spielstand.

## Projektstruktur und Erweiterung

- `Scripts/Model.cs`: Definitionen, Inventar, Transaktionen, Forschung, Spielstand.
- `Scripts/Game.cs`: Spielablauf, Interaktion, UI, Eingaben, Herstellung.
- `Scripts/World.cs`: Originale Vektorillustration, Ressourcenvorkommen, Tiere.
- `Scripts/Smoke.cs`: Integrierter automatisierter Funktionstest.
- `Data/*.json`: Gegenstände, Rezepte und Technologiebaum.
- `Docs/INHALTE_ERWEITERN.md`: Neue Inhalte mit Beispielen.
- `Docs/SPIELKONZEPT.md`: Vollständiges Konzept und Umfang dieser Version.
- `Docs/TESTBERICHT.md`: Tatsächlich ausgeführte Prüfungen und Grenzen.

## Export und Steam Deck

Die Exportvorlagen für genau Godot 4.4.1 installieren. Linux- und Windows-Presets
sind vorbereitet. `Data/*.json` ist ausdrücklich im Export enthalten. Im Editor
unter Projekt → Exportieren exportieren. **Exportierte Builds wurden in dieser
Umgebung nicht erzeugt oder auf Zielhardware getestet.**

Für Steam Deck einen Linux-x86_64-Build exportieren, den gesamten Exportordner
übertragen, die ausführbare Datei mit `chmod +x` ausführbar machen und in Steam
als „Steam-fremdes Spiel“ hinzufügen. Steam-Input-Layout „Gamepad“ auswählen.
Die Spielauflösung entspricht 1280 × 800. Controller-Menüs verwenden Godots
GUI-Fokus. Es gibt keine offizielle Steam-Deck-Verifizierung.

## Test erneut ausführen

Nach einem normalen Godot-/C#-Build:

```bash
godot --headless --path . -- --smoke-test
```

Den tatsächlichen Namen deiner Godot-.NET-Datei verwenden. Der Test verwendet
`ehternum-smoke-save.json`, nicht deinen normalen Spielstand. Erfolg:
`SMOKE SUCCESS: 20 checks`. Danach beendet er sich.

## Bewusster Prototypumfang

Eine feste, deterministisch erzeugte Landschaft von 3600 × 2800 Welteinheiten,
sechs Archive inklusive Notversorgung, neun Rezepte, ein Spielstand und einfache
Kollisionsformen. Noch kein frei editierbarer Basisbau, kein Audio, keine
Lokalisierung, keine Steam-API, keine animierten Sprite-Assets. Das Bio-Bett
behandelt sofort. Kein fertiges kommerzielles Spiel und kein installierbares
Windows-/Steam-Deck-Paket. Die Architektur lässt Inhalte über JSON wachsen;
neue Verhaltensarten brauchen C#-Logik.
