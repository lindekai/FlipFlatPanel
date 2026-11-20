#!/usr/bin/env python3
"""Erzeugt den README-Hero-Banner (hero-dark.svg / hero-light.svg).

Aufruf aus dem Repo-Wurzelverzeichnis:  python3 tools/hero.py
"""
import math
import os
import random

W, H = 1280, 440
FONT = "ui-sans-serif, -apple-system, 'SF Pro Display', 'Segoe UI', Roboto, Helvetica, Arial, sans-serif"
MONO = "ui-monospace, 'SF Mono', Menlo, Consolas, monospace"

THEMES = {
    "dark": {
        "bg0": "#161D36", "bg1": "#0B1020",
        "star": "#E9ECF5", "stars": 70,
        "eyebrow": "#7FE7FF", "title": "#F1F3F8", "accent": "#7FE7FF", "sub": "#9AA4C0",
        "chip": "#151C33", "chipStroke": "#2A3352", "chipKey": "#7FE7FF", "chipVal": "#F1F3F8",
        "tube": "#2B2F38", "tubeStroke": "#454B59", "tubeHi": "#3A404C", "ring": "#1E222A",
        "cover": "#3A404C", "coverStroke": "#565D6D",
        "el": "#E8FCFF", "glow": "#7FE7FF", "glowOp": 0.55,
        "ghost": "#5B6580", "servo": "#1E222A", "servoStroke": "#565D6D",
        "barTrack": "#1E2540", "barStroke": "#2A3352", "label": "#9AA4C0",
        "shadow": 0.35,
    },
    "light": {
        "bg0": "#FFFFFF", "bg1": "#EEF1F6",
        "star": "#9AA3BC", "stars": 40,
        "eyebrow": "#0A7FA8", "title": "#111522", "accent": "#0A7FA8", "sub": "#4E586F",
        "chip": "#FFFFFF", "chipStroke": "#D5DAE4", "chipKey": "#0A7FA8", "chipVal": "#111522",
        "tube": "#FBFCFE", "tubeStroke": "#B9C0CE", "tubeHi": "#E6EAF1", "ring": "#2B2F38",
        "cover": "#E6EAF1", "coverStroke": "#9AA3BC",
        "el": "#FFFFFF", "glow": "#38C6EE", "glowOp": 0.45,
        "ghost": "#9AA3BC", "servo": "#2B2F38", "servoStroke": "#111522",
        "barTrack": "#E6EAF1", "barStroke": "#D5DAE4", "label": "#4E586F",
        "shadow": 0.12,
    },
}


def stars(c):
    rnd = random.Random(7)
    out = []
    for _ in range(c["stars"]):
        x, y = rnd.randint(0, W), rnd.randint(0, H)
        # Keine Sterne hinter dem Teleskop
        if 700 < x < 1260 and 90 < y < 420:
            continue
        r = rnd.choice([0.6, 0.8, 1.0, 1.2, 1.6])
        out.append(f'    <circle cx="{x}" cy="{y}" r="{r}" fill="{c["star"]}" opacity="{rnd.uniform(0.25, 0.85):.2f}"/>')
    return "\n".join(out)


def chip(x, y, key, val, c):
    w = 22 + 8.6 * len(key) + 10 + 7.4 * len(val) + 18
    return (
        f'    <rect x="{x}" y="{y}" width="{w:.0f}" height="34" rx="17" fill="{c["chip"]}" stroke="{c["chipStroke"]}"/>\n'
        f'    <text x="{x + 18}" y="{y + 22}" font-size="12" font-weight="700" letter-spacing="1.2" fill="{c["chipKey"]}">{key}</text>\n'
        f'    <text x="{x + 22 + 8.6 * len(key) + 6:.0f}" y="{y + 22}" font-size="14" font-weight="600" fill="{c["chipVal"]}">{val}</text>',
        w,
    )


def telescope(c):
    # Lokales Koordinatensystem: Öffnung bei x=0, Tubus nach rechts, Scharnier oben an der Öffnung
    hinge = (-10, -78)
    open_angle = 112
    # Bogen-Pfeil für die Klappbewegung: Unterkante der Klappe (Länge 156) vom geschlossenen zum offenen Zustand
    L = 156
    a0, a1 = math.radians(90), math.radians(90 + open_angle - 8)
    p0 = (hinge[0] + L * math.cos(a0) + 30, hinge[1] + L * math.sin(a0) - 6)
    p1 = (hinge[0] + (L + 30) * math.cos(a1), hinge[1] + (L + 30) * math.sin(a1))
    arc = f'M{p0[0]:.1f} {p0[1]:.1f} A{L + 30} {L + 30} 0 0 1 {p1[0]:.1f} {p1[1]:.1f}'
    return f'''  <g transform="translate(872 178) rotate(18) scale(0.86)">
    <!-- Tubus -->
    <g filter="url(#sh)">
      <rect x="110" y="-64" width="270" height="128" rx="10" fill="{c["tube"]}" stroke="{c["tubeStroke"]}" stroke-width="2"/>
      <rect x="0" y="-74" width="122" height="148" rx="8" fill="{c["tube"]}" stroke="{c["tubeStroke"]}" stroke-width="2"/>
      <rect x="380" y="-54" width="26" height="108" rx="4" fill="{c["tubeHi"]}" stroke="{c["tubeStroke"]}" stroke-width="2"/>
      <rect x="406" y="-20" width="34" height="40" rx="3" fill="{c["ring"]}"/>
    </g>
    <rect x="14" y="-62" width="96" height="10" rx="5" fill="{c["tubeHi"]}"/>
    <rect x="124" y="-52" width="240" height="10" rx="5" fill="{c["tubeHi"]}"/>
    <rect x="196" y="-70" width="18" height="140" rx="3" fill="{c["ring"]}" opacity="0.9"/>
    <rect x="300" y="-70" width="18" height="140" rx="3" fill="{c["ring"]}" opacity="0.9"/>
    <!-- Servo am Scharnier -->
    <rect x="6" y="-100" width="46" height="24" rx="4" fill="{c["servo"]}" stroke="{c["servoStroke"]}" stroke-width="1.5"/>
    <circle cx="16" cy="-88" r="4" fill="{c["glow"]}"/>
    <!-- Geschlossene Position (Geist) -->
    <rect x="-18" y="-78" width="16" height="{L}" rx="4" fill="none" stroke="{c["ghost"]}" stroke-width="1.6" stroke-dasharray="5 5"/>
    <path d="{arc}" fill="none" stroke="{c["ghost"]}" stroke-width="2" stroke-dasharray="2 7" stroke-linecap="round" marker-end="url(#arrow)"/>
    <!-- Klappe mit leuchtender EL-Folie -->
    <g transform="translate({hinge[0]} {hinge[1]}) rotate({open_angle})">
      <ellipse cx="34" cy="{L / 2}" rx="70" ry="{L / 2 + 34}" fill="url(#elglow)"/>
      <rect x="-8" y="0" width="16" height="{L}" rx="4" fill="{c["cover"]}" stroke="{c["coverStroke"]}" stroke-width="1.5"/>
      <rect x="6" y="8" width="7" height="{L - 16}" rx="3.5" fill="{c["el"]}" filter="url(#bloom)"/>
      <rect x="6" y="8" width="7" height="{L - 16}" rx="3.5" fill="{c["el"]}"/>
    </g>
    <circle cx="{hinge[0]}" cy="{hinge[1]}" r="6" fill="{c["servo"]}" stroke="{c["servoStroke"]}" stroke-width="1.5"/>
  </g>'''


def brightness_bar(c):
    x, y, w = 760, 382, 440
    knob = x + w * 128 / 255
    return f'''  <g font-family="{MONO}">
    <rect x="{x}" y="{y}" width="{w}" height="10" rx="5" fill="{c["barTrack"]}" stroke="{c["barStroke"]}"/>
    <rect x="{x}" y="{y}" width="{w}" height="10" rx="5" fill="url(#ramp)"/>
    <circle cx="{knob:.1f}" cy="{y + 5}" r="9" fill="{c["el"]}" stroke="{c["glow"]}" stroke-width="3"/>
    <text x="{x}" y="{y + 32}" font-size="12" fill="{c["label"]}">0</text>
    <text x="{knob:.1f}" y="{y + 32}" text-anchor="middle" font-size="12" font-weight="700" fill="{c["accent"]}">SETBRIGHTNESS:128</text>
    <text x="{x + w}" y="{y + 32}" text-anchor="end" font-size="12" fill="{c["label"]}">255</text>
  </g>'''


def render(name, c):
    chips, cx = [], 72
    for key, val in [("COVER", "Auf / Zu"), ("FLAT", "0–255"), ("ASCOM", "Platform 7")]:
        svg, w = chip(cx, 316, key, val, c)
        chips.append(svg)
        cx += w + 12
    chips = "\n".join(chips)

    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}" role="img" aria-labelledby="t d">
  <title id="t">FlipFlat Panel</title>
  <desc id="d">DIY Flip-Flat-Panel für die Astrofotografie: ein Servo klappt den Teleskopdeckel auf und zu, eine dimmbare EL-Folie im Deckel liefert das Licht für Flat-Frames. Gesteuert per ASCOM aus N.I.N.A.</desc>
  <defs>
    <radialGradient id="bg" cx="0.78" cy="0.45" r="0.7"><stop offset="0" stop-color="{c["bg0"]}"/><stop offset="1" stop-color="{c["bg1"]}"/></radialGradient>
    <radialGradient id="elglow"><stop offset="0" stop-color="{c["glow"]}" stop-opacity="{c["glowOp"]}"/><stop offset="0.55" stop-color="{c["glow"]}" stop-opacity="{c["glowOp"] * 0.25:.3f}"/><stop offset="1" stop-color="{c["glow"]}" stop-opacity="0"/></radialGradient>
    <linearGradient id="ramp" x1="0" x2="1"><stop offset="0" stop-color="{c["glow"]}" stop-opacity="0"/><stop offset="1" stop-color="{c["glow"]}" stop-opacity="0.9"/></linearGradient>
    <filter id="sh" x="-10%" y="-20%" width="120%" height="150%"><feDropShadow dx="0" dy="10" stdDeviation="14" flood-color="#000000" flood-opacity="{c["shadow"]}"/></filter>
    <filter id="bloom" x="-200%" y="-20%" width="500%" height="140%"><feGaussianBlur stdDeviation="6"/></filter>
    <marker id="arrow" viewBox="0 0 10 10" refX="5" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse"><path d="M0 0 L10 5 L0 10 z" fill="{c["ghost"]}"/></marker>
  </defs>
  <rect width="{W}" height="{H}" rx="24" fill="url(#bg)"/>
  <g>
{stars(c)}
  </g>
{telescope(c)}
{brightness_bar(c)}
  <g font-family="{FONT}">
    <text x="72" y="118" font-size="15" font-weight="700" letter-spacing="2.4" fill="{c["eyebrow"]}">ARDUINO NANO · SERVO · EL-FOLIE · ASCOM</text>
    <text x="70" y="190" font-size="68" font-weight="800" letter-spacing="-1.8" fill="{c["title"]}">FlipFlat <tspan fill="{c["accent"]}">Panel</tspan></text>
    <text x="72" y="238" font-size="22" fill="{c["sub"]}">Motorisierter Deckel und dimmbares Flat-Panel</text>
    <text x="72" y="268" font-size="22" fill="{c["sub"]}">in einem Gerät – automatisch gesteuert aus N.I.N.A.</text>
{chips}
  </g>
</svg>
'''


if __name__ == "__main__":
    out = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "docs", "images")
    os.makedirs(out, exist_ok=True)
    for name, c in THEMES.items():
        with open(os.path.join(out, f"hero-{name}.svg"), "w", encoding="utf-8") as f:
            f.write(render(name, c))
        print(f"docs/images/hero-{name}.svg")
