# Ehternum – Eine neue Heimat

## Vereinbarte Ausrichtung

2D-Draufsicht, gezeichnete Gestaltung, Godot mit C#, PC und Steam Deck.
Eine einzelne überlebende Person, eine abgestürzte Raumkapsel und freundliche
Lebewesen. Keine feindlichen Tiere und kein Kampfzwang. Rettung ermöglichen und
eine Heimat errichten sind parallele Ziele; anschließend bleibt die Welt spielbar.

## Ausgangslage

Die Kapsel landet schwer beschädigt in einem fremden Tal. Der Replikator und das
Bio-Bett sind funktionsfähig. Der Speicher des Replikators ist beschädigt, aber
komprimierte Bauplansicherungen sind erhalten. Das Wissen ist nicht verloren,
sondern zunächst technisch unzugänglich. Der Spieler stellt es schrittweise wieder
her und verwandelt den Absturzort in eine bewohnbare Basis.

Die Anfangsausrüstung besteht aus einem Laser, kleinem Rucksack und Vorräten.
Nahrung, Wasser und eine einfache Kiste sind bereits herstellbar. Kein Rezept,
das für den ersten Abbau benötigt wird, liegt hinter einer Forschungssperre.

## Zentrale Entscheidungen

Ehternum erweitert Speicher und dient als Kostenbestandteil für Archive und einige
Spezialbauteile. Elekrerium liefert Energie. Universerium ist der universelle
Herstellungsgrundstoff. Die Rohstoffe sind bewusst funktional unterscheidbar.

Die Spielschleife: erkunden → abbauen → zur Basis bringen → lagern → replizieren →
Speicher erweitern → Sicherungen entpacken → Ausrüstung nutzen → weiter erkunden.
Es gibt keine Charakterlevel. Fortschritt entsteht durch zurückgewonnenes Wissen,
konkrete Gegenstände und besser vorbereitete Expeditionen.

Eine Speicherstufe ist eine Kapazitätsschwelle. Entpacken verbraucht die einmaligen
Rohstoffkosten, aber keine dauernd neu zu belegenden Speicherplätze. Freigeschaltete
Rezepte bleiben zugänglich. Technologien können mehrere Voraussetzungen haben,
wodurch echte Verzweigungen und spätere Zusammenführungen möglich sind.

## Technologiebaum der ersten Version

| Archiv | Stufe | Voraussetzungen | Baupläne |
|---|---:|---|---|
| Notversorgung | 0 | zu Beginn | Nahrung, Wasser, Lagerkiste |
| Werkzeugarchiv | 1 | Notversorgung | Fokuslinse / Laser II |
| Medizinarchiv | 1 | Notversorgung | Medizin |
| Energiearchiv | 2 | Werkzeugarchiv | Energiekern |
| Habitatarchiv | 2 | Medizinarchiv | Schutzanzug, Habitatgarten |
| Kommunikationsarchiv | 3 | Energie- und Habitatarchiv | Notrufsender |

Die Erweiterung auf Stufe n+1 kostet 8+8n Ehternum. Archive haben zusätzlich eigene
Ehternumkosten. Die erste Version erlaubt sechs Speicherstufen; spätere Stufen
sind für zusätzliche Datendefinitionen reserviert.

## Überleben und Behandlung

Gesundheit, Hunger und Durst liegen zwischen 0 und 100. Hunger und Durst sinken
langsam. Bei leeren Versorgungswerten sinkt die Gesundheit. Umweltgefahren können
Vergiftung, Krankheit oder Verletzung verursachen.

- Im Sporenhain steigt die Vergiftung, außerhalb klingt sie langsam ab.
- Die kalten Gewässer erhöhen den Krankheitswert.
- Schrott nahe dem Absturzort verursacht beim Darüberlaufen Verletzungen.
- Verletzungen verlangsamen die Bewegung.
- Der Schutzanzug reduziert die Zunahme dieser Zustände deutlich.
- Medikamente lindern Krankheit und Vergiftung und heilen etwas Gesundheit.
- Das Bio-Bett verbraucht 18 Energie und heilt sofort vollständig.

Die Sofortbehandlung ist eine Vereinfachung dieses Prototyps. Eine sichtbare
Behandlungsdauer kann später ergänzt werden. Bewusstlosigkeit führt zur Bergung
an der Kapsel mit erhaltenem Inventar. Das Spiel setzt auf Vorbereitung und
Entdeckung, nicht auf einen endgültig verlorenen Durchlauf.

## Energie und Herstellung

Die Basis startet mit 65 von 100 Energie. Eine Einheit Elekrerium liefert 25
Energie, der Energiekern vergrößert die Kapazität auf 200. Eine langsame solare
Notladung verhindert, dass ein Energiemangel die gesamte Partie blockiert.

Rezepte definieren Ausgangsmaterial, Energiekosten, Herstellungsdauer und
Ausgabemenge. Nur ein Auftrag läuft gleichzeitig. Materialien und Energie werden
beim Start reserviert beziehungsweise bezahlt. Ein fertiges Produkt wartet bei
vollem Inventar; es verschwindet nicht. Produktionsauftrag und Restzeit sind
Bestandteil des Spielstands.

Der Handlaser arbeitet in dieser Version unabhängig von der Basisenergie. Das
verhindert eine Abhängigkeit, bei der ohne Energie kein neuer Energierohstoff
abgebaut werden könnte. Laser II verdoppelt die Abbaugeschwindigkeit.

## Inventar und Basis

Der Rucksack hat 16 Stapelplätze. Kisten bieten 24 Plätze. Die Stapelgröße steht
je Gegenstand in der Datendefinition. Platzierung erfolgt vor der Spielfigur;
Überlappung mit Kapsel und bestehenden Kisten wird geprüft. Kisten kollidieren
mit der Figur. Sie sind in dieser Version nicht wieder aufnehmbar.

Kisten innerhalb von 360 Welteinheiten um den Kapselmittelpunkt sind an die
Herstellung angeschlossen. Außenlager müssen manuell besucht werden. Der
Replikator bezahlt zuerst aus dem Rucksack und anschließend aus Basiskisten.

Der Habitatgarten wird bei der Kapsel errichtet. Er versorgt die Person im
Basisbereich stetig mit Nahrung und Wasser. Der Sender wird ebenfalls bei der
Basis aufgestellt. Er baut mit Energie über 60 Sekunden ein Signal auf.

## Welt und Präsentation

Eine feste Tal-Landschaft verbindet eine offene, ockerfarbene Absturzstelle mit
ruhigen Gewässern, fremdartiger Vegetation, lavendelfarbenen Kristallen und einem
Sporenhain. Die Grafik besteht aus originalen Vektorformen mit dunklen Konturen,
gedämpften Farben, transparenten Wasserschichten und geschichteten Pflanzensilhouetten.
Es werden keine fremden Spiele-Assets oder KI-Bitmapbilder benötigt.

Freundliche Tiere bewegen sich zwischen ihren Ruheplätzen und reagieren auf
Interaktion. Es gibt keine Jagd, Kampfwerte oder Schaden durch Tiere. Die erste
Version verwendet Textmeldungen statt gesprochener Dialoge und enthält kein Audio.

## Schluss und freies Spiel

Nach erfolgreichem Notruf bestätigt ein Rettungsschiff die Position. Der Spieler
kann den Epilog aufrufen oder unmittelbar bleiben. Auch nach dem Epilog kann er
in die Welt zurückkehren. Ein errichteter Garten macht den Absturzort dauerhaft
bewohnbar. Damit werden Rettung und neue Heimat beide im Spiel erreichbar.

## Erweiterbarkeit

Gegenstände, Rezepte und Technologien liegen als JSON vor. Ein neues Rezept oder
ein neuer Technologiezweig mit bekannten Gegenstandsarten benötigt keinen neuen
C#-Code. Neue Mechaniken benötigen Implementierung und Zustandsdefinitionen.
Die Startvalidierung erkennt fehlende Referenzen, unzulässige Kosten und Zyklen.
IDs bleiben für Spielstandkompatibilität stabil. Alte Spielstände besitzen neue
Technologien zunächst nicht, können sie aber über vorhandene Voraussetzungen
erschließen. Eine Anleitung mit vollständigen Beispielen liegt bei.

## Nächste sinnvolle Ausbauschritte

Nach Spieltests: Balance und UI-Lesbarkeit auf Steam Deck, Sounddesign, neue
Entdeckungsorte mit Logbüchern, mehr Baupläne, umplatzierbare Gebäude und eine
getrennte Szenenstruktur für Werkzeuge und Gebäude. Ein Ausbau auf Mehrspieler
oder prozedurale Welten ist kein Bestandteil dieser ersten Version.
