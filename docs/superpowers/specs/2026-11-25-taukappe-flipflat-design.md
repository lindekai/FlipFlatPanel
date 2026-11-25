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
3. **Antriebsblock** flach auf dem Frontring, Standard 12 Uhr, ca. 155 × 70 × 28 mm. Er nimmt Servo, Platine (55 × 48) und EL-Inverter auf. Vorne steht eine Nase ca. 35 mm über die Kappenkante vor und trägt die Deckelachse. Die Anschlüsse (12 V, USB-C) liegen hinten, eine Kabelverschraubung führt Heizung und Fühler hinaus. Der Block wird auf eine Schwalbenschwanzschiene am Frontring geschoben und mit einer Schraube gesichert.
4. **Direktantrieb, 270°**: Die Servowelle ist die Deckelachse (geklemmtes Alu-Servohorn 25T), gegenüber sitzt ein 608-Kugellager. Die Achse ist hohl, das EL-Kabel läuft hindurch. Der Deckelarm ist gekröpft, damit der Deckel offen flach auf dem Block liegt (Parkposition).
   - Servo: DS3218 (270°, 20 kg·cm). Statischer Bedarf ca. 4 kg·cm bei ca. 300 g Deckel.
   - Die vorhandene Firmware mit Pulsbreiten 540–2500 µs deckt den 270°-Bereich ab.
5. **Deckel**: gedruckter Rahmen (Ø ≈ Kappe außen + 8 mm) und die vorhandene Trägerplatte mit EL-Folie. Die Platte sitzt herausnehmbar in Klemmnasen oder an M3-Schrauben. Eine Streuscheibe aus Opal-PMMA ist nur optional vorgesehen.
6. **Lichtdichtung durch Lippe und Magnete**:
   - Eine Lippe am Deckelrand übergreift die Kappenkante und bildet so ein Lichtlabyrinth.
   - 8 Magnete (Neodym 6 × 2 mm, N35) sitzen in der Kappenkante, eingedruckt mit einer Druckpause unter 0,6 mm Deckschicht. Gegenstücke sind Stahlscheiben im Deckelrahmen.
   - 2 weitere Magnete halten den offenen Deckel in der Parkposition. Das ist nötig, weil die Firmware den Servo nach 700 ms stromlos schaltet.
   - Die Haltekraft aller Magnete zusammen soll ca. 400–500 g nicht überschreiten. Sie wird über die Dicke der Deckschicht mit einem Testring eingestellt.
7. **Tau**: Beim Newton beschlägt zuerst der Fangspiegel. Der Heizkanal Q2/J5 der Platine soll einen Heizstreifen am Fangspiegelhalter versorgen, A0 einen Temperaturfühler. Das ist für später, die Firmware bleibt heute unverändert.

Verworfen: Alu-Flachprofile längs, TPU-Dichtring, seitlich hängender Deckel, Filz mit Rändelschrauben-Klemmung, Lagenaufbau des Deckels mit Diffusor und Schaum.

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
5. Abstand der Spinnenschrauben von der Vorderkante (Grenze für die Tiefe der Innenlippe)
6. Abstand von der Vorderkante zum Fangspiegel
7. Trägerplatte: Ø, Dicke, Material, Gewicht. Ø der Leuchtfläche, Lage und Höhe des Anschlusses
8. EL-Inverter: L × B × H (entscheidet, ob 28 mm Blockhöhe reichen)
9. Verbauter Servo (Typ, Drehwinkel), verbauter Temperaturfühler
10. Lage des Antriebsblocks am Umfang (Sucher, Prismenschiene, Balance)

## Risiken

- Kann der MP2338-Stepdown auf der noch ungetesteten Platine den Blockierstrom des DS3218 (bis ca. 2,5 A) liefern?
- Passen Nano mit Steckleisten, Elkos und Inverter in 28 mm Blockhöhe (ca. 20 mm innen)?
- Vorne kommt ca. 1 kg dazu, abzüglich des weißen Rings. Der Tubus muss neu ausbalanciert werden.
- Hinter dem Inverter liegt Wechselspannung von ca. 100 V. Die Leitung durch die Achse muss isoliert und gegen Knicken gesichert sein.

## Nächste Schritte

1. Messwerte eintragen, Parameter und Entwurf aktualisieren (Rev H)
2. Implementierungsplan schreiben (writing-plans)
3. Parametrisches CAD-Modell, Werkzeug noch offen: OpenSCAD (Empfehlung, versionierbar) oder Fusion
4. Zuerst Testdrucke: ein Segment des Klemmrings und ein Magnet-Testring
