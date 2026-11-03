"""Shared helpers: palette, SVG snippets, and a Playwright renderer (SVG -> PNG)."""
import random, math, os, base64, pathlib

OUT = pathlib.Path("/home/claude/fos/out")
SVGDIR = pathlib.Path("/home/claude/fos/svg")
FONTS = "/mnt/user-data/uploads/FieldsOfSuspicion/Assets/Fonts"

# Palette — taken from UIFactory.cs so UI art matches the code's colours
INK = "#3A2E25"; PAPER = "#F4ECD8"; LIGHT = "#EDE3D1"; MUTED = "#8C7B6B"
NIGHTBOX = "#2F2924"; GOLD = "#E0B04B"; TAN = "#E2D4B5"; FRAME = "#2A221C"
FRAMELINE = "#4A3B2F"; RED = "#8C3A2B"; EDGE = "#C9B994"
OLIVE = "#6E7446"; OLIVE_D = "#4C5232"; DUSTY = "#7C8FA0"; DUSTY_D = "#4E6070"
CARROT = "#E07A2E"; CARROT_D = "#B4561C"; LEAF = "#6F8F3A"; LEAF_D = "#4D6A25"
SOIL = "#5A3F2B"; SOIL_D = "#3E2B1D"; SOIL_L = "#7A5A3E"


def rng(seed):
    return random.Random(seed)


def svg(w, h, body, defs=""):
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" '
            f'viewBox="0 0 {w} {h}"><defs>{defs}</defs>{body}</svg>')


def lin(id_, stops, x1=0, y1=0, x2=0, y2=1):
    s = "".join(f'<stop offset="{o}" stop-color="{c}" stop-opacity="{a}"/>' for o, c, a in stops)
    return f'<linearGradient id="{id_}" x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}">{s}</linearGradient>'


def rad(id_, stops, cx=.5, cy=.5, r=.5, fx=None, fy=None):
    f = f' fx="{fx}" fy="{fy}"' if fx is not None else ""
    s = "".join(f'<stop offset="{o}" stop-color="{c}" stop-opacity="{a}"/>' for o, c, a in stops)
    return f'<radialGradient id="{id_}" cx="{cx}" cy="{cy}" r="{r}"{f}>{s}</radialGradient>'


def grain(id_="grain", freq=0.9, alpha=0.10, seed=3):
    """Paper / paint grain filter: multiply-ish speckle laid over the source."""
    return (f'<filter id="{id_}" x="0" y="0" width="100%" height="100%">'
            f'<feTurbulence type="fractalNoise" baseFrequency="{freq}" numOctaves="2" seed="{seed}" result="n"/>'
            f'<feColorMatrix in="n" type="matrix" values="0 0 0 0 0.2  0 0 0 0 0.15  0 0 0 0 0.1  0 0 0 {alpha*4} -{alpha*1.2}" result="g"/>'
            f'<feComposite in="g" in2="SourceGraphic" operator="in" result="gi"/>'
            f'<feMerge><feMergeNode in="SourceGraphic"/><feMergeNode in="gi"/></feMerge></filter>')


def blur(id_, sd):
    return f'<filter id="{id_}" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="{sd}"/></filter>'


def shadow(id_, dx=0, dy=6, sd=8, op=0.35):
    return (f'<filter id="{id_}" x="-20%" y="-20%" width="140%" height="150%">'
            f'<feDropShadow dx="{dx}" dy="{dy}" stdDeviation="{sd}" flood-color="#1a120b" flood-opacity="{op}"/></filter>')


def rough_edge(id_, scale=6, freq=0.04, seed=7):
    return (f'<filter id="{id_}" x="-5%" y="-5%" width="110%" height="110%">'
            f'<feTurbulence type="fractalNoise" baseFrequency="{freq}" numOctaves="3" seed="{seed}" result="t"/>'
            f'<feDisplacementMap in="SourceGraphic" in2="t" scale="{scale}" xChannelSelector="R" yChannelSelector="G"/></filter>')


def carrot(x, y, s=1.0, rot=0, top=True):
    """A carrot lying/standing, tip down at (x, y+len)."""
    L = 70 * s; W = 16 * s
    leaves = ""
    if top:
        leaves = (f'<path d="M0,0 C-10,-28 -22,-40 -26,-52 C-14,-44 -6,-30 0,-8 Z" fill="{LEAF}"/>'
                  f'<path d="M0,0 C2,-30 0,-48 6,-62 C10,-46 6,-26 2,-6 Z" fill="{LEAF_D}"/>'
                  f'<path d="M0,0 C10,-24 22,-34 28,-46 C24,-30 12,-16 2,-4 Z" fill="{LEAF}"/>')
        leaves = f'<g transform="scale({s})">{leaves}</g>'
    body = (f'<path d="M{-W/2},{2*s} C{-W/2},{L*0.5} {-W*0.12},{L*0.92} 0,{L} '
            f'C{W*0.12},{L*0.92} {W/2},{L*0.5} {W/2},{2*s} Q0,{-4*s} {-W/2},{2*s} Z" fill="{CARROT}"/>'
            f'<path d="M{W*0.1},{4*s} C{W*0.35},{L*0.45} {W*0.1},{L*0.85} 0,{L} '
            f'C{W*0.12},{L*0.92} {W/2},{L*0.5} {W/2},{2*s} Z" fill="{CARROT_D}" opacity=".55"/>')
    rings = "".join(f'<path d="M{-W*0.4+i*0.6},{L*f} q{W*0.22},{3*s} {W*0.42},0" stroke="{CARROT_D}" stroke-width="{1.4*s}" fill="none" opacity=".7"/>'
                    for i, f in enumerate([0.22, 0.4, 0.58]))
    return f'<g transform="translate({x},{y}) rotate({rot})">{leaves}{body}{rings}</g>'


def onion(x, y, r, c="#9A5B4A"):
    return (f'<g transform="translate({x},{y})"><path d="M0,{-r*1.25} C{r*0.35},{-r*0.9} {r},{-r*0.6} {r},0 '
            f'C{r},{r*0.8} {r*0.5},{r} 0,{r} C{-r*0.5},{r} {-r},{r*0.8} {-r},0 C{-r},{-r*0.6} {-r*0.35},{-r*0.9} 0,{-r*1.25} Z" fill="{c}"/>'
            f'<path d="M0,{-r*1.2} C{r*0.2},{-r*0.4} {r*0.5},{r*0.4} {r*0.2},{r*0.95}" stroke="#5e3328" stroke-width="1.5" fill="none" opacity=".5"/>'
            f'<ellipse cx="{-r*0.35}" cy="{-r*0.2}" rx="{r*0.22}" ry="{r*0.45}" fill="#fff" opacity=".18"/></g>')


def basket(x, y, w, h, c="#9C7244"):
    weave = "".join(f'<line x1="{x+i}" y1="{y+4}" x2="{x+i}" y2="{y+h-2}" stroke="#6e4c2a" stroke-width="2" opacity=".5"/>'
                    for i in range(8, int(w), 14))
    rows = "".join(f'<line x1="{x+2}" y1="{y+j}" x2="{x+w-2}" y2="{y+j}" stroke="#c39a62" stroke-width="2" opacity=".45"/>'
                   for j in range(10, int(h), 10))
    return (f'<path d="M{x},{y} L{x+w},{y} L{x+w-8},{y+h} Q{x+w/2},{y+h+6} {x+8},{y+h} Z" fill="{c}"/>{weave}{rows}'
            f'<rect x="{x-4}" y="{y-6}" width="{w+8}" height="10" rx="5" fill="#b8884f"/>')


def render(items, scale=1):
    """items: list of (name, w, h, svg_string, subdir). Writes svg + png."""
    from playwright.sync_api import sync_playwright
    lora = base64.b64encode(open(f"{FONTS}/Lora-Bold.ttf", "rb").read()).decode()
    bvp = base64.b64encode(open(f"{FONTS}/BeVietnamPro-Regular.ttf", "rb").read()).decode()
    css = (f"@font-face{{font-family:Lora;font-weight:700;src:url(data:font/ttf;base64,{lora})}}"
           f"@font-face{{font-family:BVP;src:url(data:font/ttf;base64,{bvp})}}"
           "html,body{margin:0;background:transparent}svg{display:block}")
    with sync_playwright() as p:
        b = p.chromium.launch()
        for name, w, h, s, sub in items:
            (SVGDIR / sub).mkdir(parents=True, exist_ok=True)
            (OUT / sub).mkdir(parents=True, exist_ok=True)
            (SVGDIR / sub / f"{name}.svg").write_text(s)
            pg = b.new_page(viewport={"width": w, "height": h}, device_scale_factor=scale)
            pg.set_content(f"<html><head><style>{css}</style></head><body>{s}</body></html>")
            pg.wait_for_timeout(60)
            pg.screenshot(path=str(OUT / sub / f"{name}.png"), omit_background=True,
                          clip={"x": 0, "y": 0, "width": w, "height": h})
            pg.close()
            print("rendered", sub, name, w, h)
        b.close()
