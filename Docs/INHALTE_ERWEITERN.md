# Inhalte erweitern

Alle IDs sind dauerhaft, kleingeschrieben und unabhängig vom Anzeigenamen.
IDs nach Veröffentlichung nicht umbenennen: Spielstände verweisen darauf.
Definitionen liegen in `Data/`, JSON ohne Kommentare oder nachgestellte Kommas.

## 1. Gegenstand

In `items.json` ein Objekt an die Liste anhängen:

```json
{"id":"ration_plus","name":"Expeditionsration","kind":"food","stack":50}
```

Vorhandene `kind`-Werte:

| kind | Verhalten |
|---|---|
| memory / material / fuel | Rohstoff; `elekrerium` wird im Basismenü eingespeist |
| food | Hunger +38 |
| water | Durst +45 |
| medicine | Vergiftung -45, Krankheit -40, Gesundheit +12 |
| laser | Dauerhafte Laser-II-Verbesserung |
| suit | Schutzanzug gegen Umweltgefahren |
| cell | Energieobergrenze von 100 auf 200 |
| chest | Platzierbare Lagerkiste mit 24 Stapelplätzen |
| garden | Ein Habitatgarten an der Basis |
| beacon | Ein Notrufsender an der Basis |

Ein Gegenstand mit vorhandener Art funktioniert ohne zusätzliche C#-Änderung.
Individuelle Wirkstärken, weitere Laserstufen oder eine neue Gebäudeart benötigen
eine Erweiterung von `ItemDef`, `Model.Consume` beziehungsweise `Game.Place`.

## 2. Rezept

In `recipes.json` ergänzen:

```json
{
  "id":"ration_plus_recipe",
  "name":"Expeditionsration",
  "output":"ration_plus",
  "amount":3,
  "cost":{"universerium":5},
  "energy":7,
  "seconds":4
}
```

`output` verweist auf einen Gegenstand. Kosten werden vollständig geprüft und
dann gemeinsam bezahlt. Bei voller Tasche startet kein Auftrag. Der Auftrag
läuft nacheinander, es gibt derzeit keine Warteschlange. Negative Kosten und
unbekannte Referenzen werden beim Start abgelehnt.

## 3. Technologie

In `technologies.json` ergänzen:

```json
{
  "id":"expedition",
  "name":"Expeditionsarchiv",
  "memory":4,
  "requires":["habitat","power"],
  "cost":{"ehternum":20},
  "recipes":["ration_plus_recipe"]
}
```

Alle Voraussetzungen müssen freigeschaltet sein. Die Speicherstufe muss
mindestens 4 betragen. Ehternumkosten werden beim Entpacken einmal bezahlt.
Das Menü erzeugt Einträge automatisch, auch für neue Zweige. Die aktuelle
Speichererweiterungsfunktion erlaubt sechs Stufen. Für mehr Stufen die Obergrenze
in `ExpandMemory`, beim Laden und im UI gemeinsam anpassen.

Der Start validiert doppelte IDs, fehlende Referenzen und zyklische Abhängigkeiten.
Die Reihenfolge in der JSON-Datei bestimmt die Darstellung im Archivmenü.
Neue, nicht initial freigeschaltete Technologien sind auch mit älteren
Spielständen erreichbar, sofern sie an vorhandene Archive anschließen.

## 4. Neues Rohstoffvorkommen

Ein neuer Rohstoff benötigt zusätzlich Einträge in `World._Ready()` und eine
Farbzuordnung in `DrawDeposit()`. Automatische Vorkommen kennen zunächst nur die
drei Hauptressourcen. Neue Definitionen allein platzieren keine Vorkommen.

## 5. Neue Mechanik

- Verhalten in `Model` implementieren, wenn es ohne Godot testbar ist.
- Darstellung und Interaktion in `Game` / `World` ergänzen.
- Persistenten Zustand in `SaveState` speichern und beim Laden validieren.
- Für inkompatible Änderungen die Spielstandversion erhöhen und Migration ergänzen.
- Exportieren, laden, mit Controller navigieren und Smoke-Test erneut ausführen.

Die Model-Schicht hat keine Godot-Abhängigkeit und kann später in eine eigene
Bibliothek oder ein Testprojekt verschoben werden. Der Prototyp hält die Anzahl
der Projekte bewusst klein.
