# Testbericht – 15. Oktober 2026

## Geprüfte Umgebung

Godot 4.4.1 stable .NET, Linux x86_64, .NET 8.0.32 Runtime / SDK 8.0.426.
Die C#-Quellen wurden mit dem Roslyn-Compiler des SDK, den .NET-8-Referenzassemblies,
GodotSharp 4.4.1 und Godots Source-Generatoren kompiliert. Ergebnis: keine
Compilerfehler oder Warnungen. Das Standard-MSBuild-Kommando war in dieser
Laufzeit wegen fehlender Prozessinformationen nicht ausführbar; deshalb erfolgte
die Kompilierung direkt. Das ist kein Test eines normalen Editor-Builds auf dem
PC des Benutzers.

Danach wurde die erzeugte Assembly in der echten Godot-.NET-Engine geladen und
der integrierte Funktionstest ausgeführt. Abschließender Lauf:

```
PASS · Startarchiv und Startinventar
PASS · Kommunikation ist an Voraussetzungen gebunden
PASS · Fehlgeschlagene Transaktion verbraucht nichts
PASS · Ehternum erweitert den Speicher
PASS · Erste Technologieverzweigung
PASS · Energie- und Habitatarchiv
PASS · Kommunikationsarchiv erreichbar
PASS · Herstellung startet
PASS · Werkzeug wird produziert und ausgerüstet
PASS · Rohstoffe einlagern
PASS · Basiskiste wird für Herstellung berücksichtigt
PASS · Rohstoffe entnehmen
PASS · Bio-Bett heilt alle Zustände mit Energieverbrauch
PASS · Habitat und Sender platzierbar
PASS · Rettung wird freigeschaltet
PASS · Spielstand wird geschrieben
PASS · Spielstand inklusive Baufortschritt und Produktionsauftrag geladen
PASS · JSON-Roundtrip erhält den Technologiebaum
PASS · Unbekannte Spielstandversion wird abgelehnt
PASS · Alle Menüs ohne Ausnahme aufgebaut
SMOKE SUCCESS: 20 checks
```

Der Test läuft ohne Fenster und verwendet einen separaten Testspielstand.
Zusätzlich wurde ein regulärer Start mit zehn Frames ohne Laufzeitfehler geprüft.
Ein im Test gefundener verzögerter Fokusaufruf auf entfernten UI-Elementen wurde
korrigiert; der abschließende Lauf enthält keine Engine-Fehler.

## Nicht geprüft

- Grafisches Renderbild und Lesbarkeit: Der verfügbare Displayserver konnte hier
  nicht von Godot genutzt werden. Es gibt deshalb keine visuelle Abnahme.
- Echte Maus-/Tastatur- und Gamepad-Sitzung, Analogstickgefühl, Steam Input.
- Exportierte Windows-/Linux-Binaries, Exportvorlagen und Installation.
- Verhalten auf echter Steam-Deck-Hardware, Akkuverbrauch und Bildrate.
- Langzeitbalance und ein kompletter Durchlauf ohne Testressourcen.
- Audio: in dieser Version bewusst nicht enthalten.

Die zwanzig Prüfungen sind automatisierte Funktionsprüfungen und ersetzen keine
manuelle Spielrunde. Der komplette Fortschrittsweg wurde mit bereitgestellten
Testressourcen überprüft, nicht durch minutenlangen natürlichen Ressourcenabbau.

## Empfohlene erste lokale Prüfung

1. Mit Godot 4.4.1 .NET importieren, normal bauen und starten.
2. Mit Maus und Leertaste alle drei Startvorkommen abbauen.
3. Erste Speichererweiterung und Nahrung herstellen.
4. Eine Kiste platzieren und Ressourcen umlagern.
5. Speichern, neu starten und laden.
6. Mit Controller durch alle Menüs navigieren.
7. Erst danach Linux/Windows exportieren und auf Zielgeräten testen.
