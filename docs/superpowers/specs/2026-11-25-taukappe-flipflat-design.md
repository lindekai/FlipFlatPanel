# Taukappe mit FlipFlat-Deckel für Skywatcher 200/1000 PDS – Design

Stand: 25.11.2026 · Entwurf Rev G · Status: **Design abgestimmt, Messwerte ausstehend**

Visueller Entwurf mit Maßzeichnungen, Bewegungsstudie und 3D-Animation:
[`docs/hardware/taukappe-entwurf.html`](../../hardware/taukappe-entwurf.html)

## Ziel

Das bestehende FlipFlat Panel wird zu einer Taukappe mit integriertem Flat-Deckel umgebaut. Sie wird vorne auf den Tubus eines Skywatcher 200/1000 PDS gesetzt. Heute nur Hardware (Gehäuse, Mechanik). Firmware, Treiber und App bleiben unverändert.

## Ist-Stand und Probleme

| Ist | Problem |
|---|---|
| Gedruckte Box mit Kabelbindern am Tubus | nicht wiederholgenau, verrutscht |
| Servo in der Box bewegt den Deckel über Hebel und Schubstange | Spiel, Winkel begrenzt |
| Servo, Platine und Inverter in einer großen Box, Kabel außen im Geflechtschlauch | sperrig, unschöne Kabelführung |
| Deckel direkt an der Tubusöffnung | kein Tauschutz |

Funktioniert gut und wird übernommen: **EL-Folie auf Trägerplatte geklebt**. Das ist flach, leicht und mechanisch einfach.

## Entscheidungen

1. **Kappe voll gedruckt (Konzept A)**, 3 Ringsegmente, gesamt 225 mm (Klemmring 35 / Mittelring 80 / Frontring 110). Verbunden über eine Steckfuge mit Bajonett-Nocken. Material PETG oder ASA, schwarz matt, Wand 2,0 mm, innen 3 Blendenringe.
2. **Befestigung: Randklemmung mit U-Profil** (Prinzip wie bei der Strato-Taukappe von 3D-Druck Krug, eigene Geometrie):
   - Der weiße Frontring des Tubus (innen 220 mm) wird **abgebaut**.
   - Die Innenlippe (1,6 mm, ca. 20 mm tief) greift in den Tubus und zentriert, die Außenschürze liegt außen an, die Schulter sitzt an der Vorderkante.
   - Geschraubt wird durch die **vorhandenen Bohrungen des weißen Rings** in Gewindeeinsätze, die in Verdickungen der Innenlippe sitzen. Es wird nichts gebohrt, die Lage am Umfang ist festgelegt.
   - Überlappung 25 mm, Überstand vor dem Tubus 200 mm.
3. **Antriebsblock** flach auf dem Frontring, Standard 12 Uhr, vorläufig ca. 165 × 70 × 32 mm (die Höhe folgt aus der Platine, siehe unten). Er nimmt Servo, Platine und EL-Inverter hintereinander auf: Platine hinten, Inverter in der Mitte, Servo vorne in der Nase. Vorne steht eine Nase ca. 35 mm über die Kappenkante vor und trägt die Deckelachse. Die Anschlüsse (12 V, USB-C) liegen hinten, eine Kabelverschraubung führt Heizung und Fühler hinaus. Der Block wird auf eine Schwalbenschwanzschiene am Frontring geschoben und mit einer Schraube gesichert.
4. **Direktantrieb, 270°**: Die Servowelle ist die Deckelachse (geklemmtes Alu-Servohorn 25T), gegenüber sitzt ein 608-Kugellager. Die Achse ist hohl, das EL-Kabel läuft hindurch. Der Deckelarm ist gekröpft, damit der Deckel offen flach auf dem Block liegt (Parkposition).
   - Servo: DS3218 (270°, 20 kg·cm). Statischer Bedarf ca. 4 kg·cm bei ca. 300 g Deckel.
   - Die vorhandene Firmware mit Pulsbreiten 540–2500 µs deckt den 270°-Bereich ab.
5. **Deckel**: gedruckter Rahmen (Ø ≈ Kappe außen + 8 mm) und die vorhandene Trägerplatte mit EL-Folie. Die Platte sitzt herausnehmbar in Klemmnasen oder an M3-Schrauben. Eine Streuscheibe aus Opal-PMMA ist nur optional vorgesehen.
6. **Lichtdichtung durch Lippe und Magnete**:
   - Eine Lippe am Deckelrand übergreift die Kappenkante und bildet so ein Lichtlabyrinth.
   - Nur **3 Magnete** (Neodym 6 × 2 mm, N35) sitzen in der Kappenkante gegenüber dem Scharnier, bei 4, 6 und 8 Uhr (Scharnier bei 12 Uhr). Sie sind eingedruckt mit einer Druckpause unter 0,6 mm Deckschicht. Gegenstücke sind Stahlscheiben im Deckelrahmen.
   - Die Magnete sollen den Deckel nur sanft plan anziehen, die Lichtdichtheit macht die Lippe. Zusammen ziehen sie ca. 150–250 g, das sind beim Öffnen rund 4 kg·cm zusätzlich für den Servo.
   - 2 weitere Magnete halten den offenen Deckel in der Parkposition. Das ist nötig, weil die Firmware den Servo nach 700 ms stromlos schaltet.
   - Die Haltekraft wird über die Dicke der Deckschicht mit einem Testring eingestellt.
7. **Tau**: Beim Newton beschlägt zuerst der Fangspiegel. Der Heizkanal Q2/J5 der Platine soll einen Heizstreifen am Fangspiegelhalter versorgen, A0 einen Temperaturfühler. Das ist für später, die Firmware bleibt heute unverändert.

Verworfen: Alu-Flachprofile längs, TPU-Dichtring, seitlich hängender Deckel, Filz mit Rändelschrauben-Klemmung, Lagenaufbau des Deckels mit Diffusor und Schaum.

## Elektronik im Antriebsblock

### Platine MK3 rev3 (Werte aus `PCB/gerberfile`, EAGLE-Export v19)

| Merkmal | Wert |
|---|---|
| Außenmaß | **60,8 × 52,9 mm**, Ecken gerundet |
| Befestigung | 4 Löcher Ø 3,5 (für M3), Raster **56 × 48 mm**, je 2,5 mm vom Rand |
| Linke Kante (52,9) | DC-Buchse J1 (Oberseite), USB-C des Nano |
| Rechte Kante (52,9) | Servo JP1 (3-polig), A0 JP2, EL-Folie J2 (JST-XH) |
| Obere Kante, **Unterseite** | Buchse für die Heizfolie J5 (gleicher Typ wie J1) |
| Höchste Bauteile Oberseite | Elkos C1/C6 10 × 10,5 mm, DC-Buchse J1 |
| Bauteile Unterseite | 2× D2PAK-MOSFET (ca. 4,5 mm), DC-Buchse J5 (ca. 11 mm) |
| Spannungswandler | MP2338, 12 V → 5 V (Rückkopplung 90,9 k / 10 k) für den Servo |

**Einbau:** Die Platine liegt flach im hinteren Teil des Blocks. Die 60,8-mm-Seite zeigt in Tubusrichtung, die linke Kante mit DC-Buchse und USB-C zur Rückwand. So sind beide Anschlüsse von hinten erreichbar. J5 auf der Unterseite braucht einen Durchbruch in der Seitenwand unterhalb der Platine.

**Bauhöhe:** Weil J5 auf der Unterseite sitzt, braucht die Platine ca. 12 mm hohe Abstandshalter. Darüber kommen 1,6 mm Platine und oben ca. 11 mm (Elkos, J1). Das ergibt **ca. 25 mm innen**. Steckt der Nano in Buchsenleisten, sind es ca. 28 mm. Der Block wird damit **ca. 30–32 mm hoch statt 28 mm**. Die Drehachse rückt um denselben Betrag nach oben, damit der Deckel in der Parkposition weiter flach auf dem Block liegt (parametrisch).

### Servo

- Annahme: DS3218 mit 270°, 20 kg·cm, Metallgetriebe, 40 × 20 × 40,5 mm. Er liegt in der Nase, die Welle zeigt quer zum Tubus und ist zugleich die Deckelachse.
- Bedarf: ca. 4 kg·cm für das Deckelgewicht, plus ca. 4 kg·cm für die Magnete beim Öffnen. Mit 20 kg·cm bleibt reichlich Reserve.
- Das Servokabel ist kurz, es geht direkt zu JP1 an der rechten Platinenkante.
- Offen: Welcher Servo ist heute verbaut? Mit einem 180°-Servo gibt es keine Parkposition auf dem Block. Der Deckel stünde dann offen senkrecht.
- Risiko: Der DS3218 zieht beim Blockieren ca. 2 A bei 5 V. Ob der MP2338 das liefert, muss im Datenblatt und am Prüfstand geprüft werden. C6 (1000 µF) puffert Spitzen.

### EL-Inverter

- Maße noch offen, typische 12-V-Inverter für A3/A4-Folien sind ca. 50 × 30 × 18 mm.
- Er sitzt zwischen Platine und Servo. Seine Ausgangsleitung (ca. 100 V Wechselspannung) läuft kurz zur hohlen Achse und dort hindurch.
- Inverter werden warm und pfeifen leise. Deshalb wird er mit Schaumstoff oder Silikon gelagert, nicht hart verschraubt, und bekommt ein paar Lüftungsschlitze nach unten.

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
9. Verbauter Servo (Typ, Drehwinkel, Maße), verbauter Temperaturfühler (Typ, Kabel)
10. Platine bestückt: Steckt der Nano gelötet oder in Buchsenleisten? Größte Höhe über der Oberseite und unter der Unterseite?
11. Lage des Antriebsblocks am Umfang (Sucher, Prismenschiene, Balance)

## Risiken

- Kann der MP2338-Stepdown auf der noch ungetesteten Platine den Blockierstrom des DS3218 (bis ca. 2,5 A) liefern?
- Bauhöhe der Platine: Mit J5 auf der Unterseite werden ca. 25–28 mm innen gebraucht, der Block wird ca. 30–32 mm hoch. Ändern ließe sich das nur mit einer Platinenrevision, etwa J5 auf die Oberseite.
- Vorne kommt ca. 1 kg dazu, abzüglich des weißen Rings. Der Tubus muss neu ausbalanciert werden.
- Steifigkeit nach Abbau des weißen Rings: unkritisch. Eine gefräste, steife Fangspiegelspinne ist verbaut, und die Kappe versteift die Vorderkante zusätzlich über Schürze und Innenlippe.
- Hinter dem Inverter liegt Wechselspannung von ca. 100 V. Die Leitung durch die Achse muss isoliert und gegen Knicken gesichert sein.

## Nächste Schritte

1. Messwerte eintragen, Parameter und Entwurf aktualisieren (Rev H)
2. Implementierungsplan schreiben (writing-plans)
3. Parametrisches CAD-Modell, Werkzeug noch offen: OpenSCAD (Empfehlung, versionierbar) oder Fusion
4. Zuerst Testdrucke: ein Segment des Klemmrings und ein Magnet-Testring
