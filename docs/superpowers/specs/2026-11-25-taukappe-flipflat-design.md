# Taukappe mit FlipFlat-Deckel für Skywatcher 200/1000 PDS – Design

Stand: 25.11.2026 · Entwurf Rev J · Status: **Design abgestimmt, Messwerte ausstehend**

Visueller Entwurf mit Maßzeichnungen, Bewegungsstudie und 3D-Animation:
[`docs/hardware/taukappe-entwurf.html`](../../hardware/taukappe-entwurf.html)

## Ziel

Das bestehende FlipFlat Panel wird zu einer Taukappe mit integriertem Flat-Deckel umgebaut. Sie wird vorne auf den Tubus eines Skywatcher 200/1000 PDS gesetzt. Im Mittelpunkt steht die Gehäuseform, die Taukappe ist der Nebeneffekt. Die Elektronik wird neu und passend zum Gehäuse entworfen (ESP32, ASCOM Alpaca). Sie richtet sich nicht nach der bestehenden Nano-Platine.

## Ist-Stand und Probleme

| Ist | Problem |
|---|---|
| Gedruckte Box mit Kabelbindern am Tubus | nicht wiederholgenau, verrutscht |
| Servo in der Box bewegt den Deckel über Hebel und Schubstange | Spiel, Winkel begrenzt |
| Servo, Platine und Inverter in einer großen Box, Kabel außen im Geflechtschlauch | sperrig, unschöne Kabelführung |
| Deckel direkt an der Tubusöffnung | kein Tauschutz |

Funktioniert gut und wird übernommen: **EL-Folie auf Trägerplatte geklebt**. Das ist flach, leicht und mechanisch einfach.

## Entscheidungen

1. **Kappe voll gedruckt (Konzept A), ein Druckteil**, 225 mm lang, Ø außen ca. 238,5 mm. Passt stehend auf einen Drucker mit 256 mm Bauhöhe, gedruckt wird mit dem Klemmring nach unten. Material PETG oder ASA, schwarz matt, Wand 2,0 mm.
   - **Streulicht:** Statt Blendenringen hat die Innenwand feine umlaufende Rillen (Sägezahn, 0,8 mm hoch, Teilung 3 mm). Die 45°-Flanke zeigt nach unten und braucht deshalb keine Stützen, die steile Seite zeigt nach vorn zum einfallenden Licht. Optional kann innen Velours-Flockfolie eingeklebt werden.
   - Für Drucker unter 230 mm Bauhöhe lässt sich die Kappe im CAD-Modell in zwei Teile mit Steckfuge trennen.
2. **Befestigung: Randklemmung mit U-Profil** (Prinzip wie bei der Strato-Taukappe von 3D-Druck Krug, eigene Geometrie):
   - Der weiße Frontring des Tubus (innen 220 mm) wird **abgebaut**.
   - Die Innenlippe (1,6 mm, ca. 20 mm tief) greift in den Tubus und zentriert, die Außenschürze liegt außen an, die Schulter sitzt an der Vorderkante.
   - Geschraubt wird durch die **vorhandenen Bohrungen des weißen Rings** in Gewindeeinsätze, die in Verdickungen der Innenlippe sitzen. Es wird nichts gebohrt, die Lage am Umfang ist festgelegt.
   - Überlappung 25 mm, Überstand vor dem Tubus 200 mm.
3. **Antriebsblock** flach auf dem Frontring, Standard 12 Uhr, vorläufig ca. 130 × 66 × 24 mm (Höhe folgt aus Servo und Inverter). Er nimmt Servo, eine eigene Platine (ESP32, Alpaca) und den EL-Inverter auf, siehe „Elektronik im Antriebsblock“. Vorne steht eine Nase ca. 35 mm über die Kappenkante vor und trägt die Deckelachse. Die 12 V kommen über den Kabelkanal in der Kappenwand, USB-C hinten nur für Service. Der Block wird auf eine Schwalbenschwanzschiene am Frontring geschoben und mit einer Schraube gesichert.
4. **Direktantrieb, 270°**: Die Servowelle ist die Deckelachse (geklemmtes Alu-Servohorn 25T), gegenüber sitzt ein 608-Kugellager. Die Achse ist hohl, das EL-Kabel läuft hindurch. Der Deckelarm ist gekröpft, damit der Deckel offen flach auf dem Block liegt (Parkposition).
   - Servo: DS3218 (270°, 20 kg·cm). Statischer Bedarf ca. 4 kg·cm bei ca. 300 g Deckel.
   - Klassischer PWM-Servo, versorgt über einen Wandler auf der eigenen Platine (6 V, mindestens 3 A).
5. **Deckel**: gedruckter Rahmen (Ø ≈ Kappe außen + 8 mm) und die vorhandene Trägerplatte mit EL-Folie. Die Platte sitzt herausnehmbar in Klemmnasen oder an M3-Schrauben. Eine Streuscheibe aus Opal-PMMA ist nur optional vorgesehen.
6. **Lichtdichtung durch Lippe und Magnete**:
   - Eine Lippe am Deckelrand übergreift die Kappenkante und bildet so ein Lichtlabyrinth.
   - Nur **3 Magnete** (Neodym 6 × 2 mm, N35) sitzen in der Kappenkante gegenüber dem Scharnier, bei 4, 6 und 8 Uhr (Scharnier bei 12 Uhr). Sie sind eingedruckt mit einer Druckpause unter 0,6 mm Deckschicht. Gegenstücke sind Stahlscheiben im Deckelrahmen.
   - Die Magnete sollen den Deckel nur sanft plan anziehen, die Lichtdichtheit macht die Lippe. Zusammen ziehen sie ca. 150–250 g, das sind beim Öffnen rund 4 kg·cm zusätzlich für den Servo.
   - 2 weitere Magnete halten den offenen Deckel in der Parkposition. Das ist nötig, weil der Servo nach der Fahrt stromlos geschaltet wird. Die Parkmagnete und ein Magnet im Deckelarm lösen zugleich die Hall-Endlagen aus.
   - Die Haltekraft wird über die Dicke der Deckschicht mit einem Testring eingestellt.
7. **Tau**: Beim Newton beschlägt zuerst der Fangspiegel. Ein Heizkanal der neuen Platine versorgt einen Heizstreifen am Fangspiegelhalter. Geregelt wird nach dem Taupunkt aus dem BME280.
8. **Ein Block, integrierte Kabelführung** (keine Aufteilung in vorne und hinten, keine losen Einzelkabel):
   - Die gesamte Elektronik (ESP32, Wandler, MOSFETs, Inverter, Servo, Sensoren) bleibt **in einem Block** an der Taukappe.
   - **12 V** ist das einzige Kabel von außen. Es kommt an eine Einbaubuchse am hinteren Ende des Klemmrings (12 Uhr). Von dort läuft ein in die Kappenwand eingeformter **Kabelkanal** (ca. 14 × 9 mm außen) bis unter den Block.
   - **Heizkabel zum Fangspiegel:** Es tritt unter dem Block durch die Kappenwand, läuft innen an der Wand bis zur Lippe und weiter an einer Spinnenstrebe zum Fangspiegel. An der Lippe sitzt ein kleiner Stecker, damit die Kappe abnehmbar bleibt.
   - **EL-Kabel:** durch die hohle Deckelachse.
   - Steuerung per Alpaca über WLAN. USB-C am Block nur für Service.
9. **Elektronik neu**: eigene Platine mit ESP32 und ASCOM Alpaca, zugeschnitten auf den Block. Die Nano-Platine MK3 wird nicht übernommen.

Verworfen: drei Ringsegmente mit Bajonett und Blendenringe (Rev I), Alu-Flachprofile längs, TPU-Dichtring, seitlich hängender Deckel, Filz mit Rändelschrauben-Klemmung, Lagenaufbau des Deckels mit Diffusor und Schaum.

## Elektronik im Antriebsblock

**Grundsatz:** Die Elektronik richtet sich nach dem Gehäuse. Die bestehende Nano-Platine MK3 rev3 wird **nicht** übernommen. Für den Block wird eine eigene Platine gezeichnet. Firmware-Basis ist der Alpaca-Entwurf im Nachbar-Repo `FlipFlatPanel-ESP32` (ESP32, Alpaca über WLAN, BME280, Heizungsregelung, Hall-Endlagen, SoftClose).

### Eigene Platine (Entwurf)

| Baustein | Auslegung |
|---|---|
| Mikrocontroller | ESP32-S3-MINI-1 (klein, WLAN, USB-C nativ für Service und Flashen) |
| Schnittstelle | ASCOM Alpaca über WLAN (Port 11111, Discovery), Serial über USB als Ersatz |
| Versorgung | nur 12 V über ein Kabel zur Kappe. 3,3 V für ESP32 und Sensoren |
| Servo-Versorgung | Spannungswandler 12 → 6 V, **mindestens 3 A** Dauerstrom, großer Pufferkondensator |
| Leistungsausgänge | 2 MOSFET-Kanäle (Logic Level): EL-Inverter (PWM-Dimmung) und Heizstreifen am Fangspiegel |
| Sensoren | BME280 (Temperatur, Feuchte, Druck) an einer Lüftungsöffnung hinten im Block. 2 Hall-Schalter (Endlagen ZU und AUF) |
| Bestückung | nur SMD, flach. Steckverbinder liegen so, dass die Bauhöhe niedrig bleibt |
| Format | schmal, ca. 72 × 28 mm, liegt neben dem Inverter (vorläufig) |

**Hall-Endlagen ohne zusätzliche Magnete:** Der Sensor „ZU“ in der Nase spricht auf einen Magneten im Deckelarm an. Der Sensor „AUF“ unter der Oberseite des Blocks spricht auf den Parkmagneten im Deckel an. Damit meldet der Treiber den echten `CoverState` statt eines angenommenen.

### Servo (Entscheidung: DS3218MG, 270°-Version, PWM)

| | Moment |
|---|---|
| Deckelgewicht (ca. 300 g, Hebel ca. 14 cm) | ca. 4 kg·cm |
| Magnete lösen | ca. 4 kg·cm |
| Bedarf | ca. 8 kg·cm |
| mit doppelter Sicherheit | ca. 16 kg·cm |
| MG90S (ESP32-Entwurf) | ca. 2 kg·cm, zu schwach |
| **DS3218MG** | **ca. 19–21 kg·cm** (Herstellerangabe, beim Kauf prüfen) |

- **Mechanisch:** 40 × 20 × 40,5 mm, Metallgetriebe, Befestigungslaschen, 25-Zahn-Welle mit geklemmtem Alu-Servohorn. Er liegt quer in der Nase, die Welle ist die Deckelachse.
- **Elektrisch:** 6 V vom Wandler auf der eigenen Platine (mindestens 3 A, Puffer-Elko ca. 1000 µF). Blockierstrom bis ca. 2,5 A. Das PWM-Signal kommt vom ESP32 über einen Pegelwandler 3,3 → 5 V. 500–2500 µs entsprechen dem vollen 270°-Weg.
- **Beim Kauf ausdrücklich die 270°-Version wählen.** Mit 180° gibt es keine Parkposition auf dem Block.
- **Mehr Reserve:** Der DS3225 (ca. 25 kg·cm) hat dieselbe Größe und dieselbe Welle und passt ohne Gehäuseänderung.
- Der Servo wird nach der Fahrt stromlos geschaltet. Die Magnete halten den Deckel in beiden Endlagen, die Hall-Sensoren bestätigen die Lage.

### EL-Inverter

- Maße noch offen, typische 12-V-Inverter für A3/A4-Folien sind ca. 50 × 30 × 18 mm. **Er bestimmt zusammen mit dem Servo die Blockhöhe.**
- Er liegt neben der Platine. Seine Ausgangsleitung (ca. 100 V Wechselspannung) läuft kurz zur hohlen Achse und dort hindurch.
- Inverter werden warm und pfeifen leise. Deshalb wird er in Schaumstoff gelagert und bekommt Lüftungsschlitze. Der BME280 sitzt weit weg vom Inverter.

### Block

Vorläufig **ca. 130 × 66 × 24 mm**: Platine und Inverter hinten nebeneinander, Servo vorne quer, die Nase trägt die Achse. Die 12 V kommen über den Kabelkanal von unten in den Block. Hinten liegt USB-C (Service). Das Heizkabel geht nach unten durch die Kappenwand ins Innere.

## Parameter

| Maß | Formel | mit D = 232, t = 1 |
|---|---|---|
| D | Tubus-Ø außen ohne weißen Ring | **messen** |
| t | Wandstärke des Tubus | **messen** |
| Innenlippe außen | D − 2t − 0,5 | 229,5 |
| Nutbreite radial | t + 0,75 | 1,75 |
| Kappe innen | D + 1,0 | 233 |
| Kappe außen | Kappe innen + 4,0 | 237 |
| Deckel außen | Kappe außen + 8 | 245 (Druckbett 256: passt bis 250) |
| Trägerplatte max. | Kappe innen − 6 | 227 |

## Offene Messwerte (vom Nutzer)

1. Tubus-Ø außen vorne, ohne weißen Ring
2. Bohrungen des weißen Rings: Anzahl, Winkellage, Abstand von der Vorderkante, Ø bzw. Gewinde, Befestigungsart (Blechschraube, Mutter, Niet)
3. Gewicht des weißen Rings
4. Wandstärke des Tubus. Form der Vorderkante: gerade oder gebördelt?
5. Abstand der Spinnenschrauben von der Vorderkante (Grenze für die Tiefe der Innenlippe). Die Spinne ist eine gefräste Nachrüstspinne, ihre Befestigung kann von der Serie abweichen, deshalb direkt messen.
6. Abstand von der Vorderkante zum Fangspiegel
7. Trägerplatte: Ø, Dicke, Material, Gewicht. Ø der Leuchtfläche, Lage und Höhe des Anschlusses
8. EL-Inverter: L × B × H, wo die Kabel herauskommen, Befestigung (Laschen, Löcher, nur Gehäuse?)
9. Verbauter Temperaturfühler (Typ, Kabel). Der Servo ist entschieden: DS3218MG 270°
10. Lage des Antriebsblocks am Umfang (Sucher, Prismenschiene, Balance) und Winkel der Spinnenstreben. Steht eine Strebe bei 12 Uhr, läuft das Heizkabel direkt an ihr entlang.

## Risiken

- Der Blockierstrom des DS3218 (bis ca. 2,5 A) muss vom 6-V-Wandler der neuen Platine sicher geliefert werden. Bei der Wahl des Bauteils beachten.
- Die Blockhöhe hängt am Inverter. Ist er höher als ca. 18 mm, wird der Block höher oder der Inverter muss anders liegen.
- Vorne kommt ca. 1 kg dazu, abzüglich des weißen Rings. Der Tubus muss neu ausbalanciert werden.
- Steifigkeit nach Abbau des weißen Rings: unkritisch. Eine gefräste, steife Fangspiegelspinne ist verbaut, und die Kappe versteift die Vorderkante zusätzlich über Schürze und Innenlippe.
- Hinter dem Inverter liegt Wechselspannung von ca. 100 V. Die Leitung durch die Achse muss isoliert und gegen Knicken gesichert sein.

## Nächste Schritte

1. Messwerte eintragen, Parameter und Entwurf aktualisieren (Rev H)
2. Implementierungsplan schreiben (writing-plans)
3. Platine für den Block entwerfen (Schaltung aus dem ESP32-Entwurf, Format aus dem CAD-Modell)
4. Parametrisches CAD-Modell, Werkzeug noch offen: OpenSCAD (Empfehlung, versionierbar) oder Fusion
5. Zuerst Testdrucke: ein kurzer Abschnitt des Klemmrings (U-Profil, ca. 40 mm hoch) und ein Magnet-Testring
