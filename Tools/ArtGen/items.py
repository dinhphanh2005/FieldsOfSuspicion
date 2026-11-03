"""Paper, board, farm, night map, UI pieces and icons."""
from common import *
from bgs import planks_v, tree

PAD = 28  # transparent margin around paper items so their baked shadow fits


def paper_defs(seed=3):
    return (grain("g", .9, .10, seed) + rough_edge("rough", 5, .05, seed) + shadow("sh", 0, 10, 12, .38)
            + rad("age", [(0, "#000", 0), (.7, "#6b4a2a", .0), (1, "#6b4a2a", .32)], r=.72)
            + lin("brass", [(0, "#f2cf7a", 1), (.5, "#c49a3e", 1), (1, "#8a6526", 1)], 0, 0, 1, 1))


def sheet(w, h, fill, extra="", seed=3, torn_top=False):
    x0, y0, x1, y1 = PAD, PAD, w - PAD, h - PAD
    r = rng(seed)
    if torn_top:
        pts = [f"{x0},{y0+8}"]
        x = x0
        while x < x1:
            x += r.uniform(10, 22); pts.append(f"{min(x,x1):.0f},{y0+r.uniform(0,16):.0f}")
        d = "M" + " L".join(pts) + f" L{x1},{y1} L{x0},{y1} Z"
    else:
        d = f"M{x0},{y0} L{x1},{y0} L{x1},{y1} L{x0},{y1} Z"
    clip = f'<clipPath id="pc{seed}"><path d="{d}"/></clipPath>'
    body = (f'<g filter="url(#sh)"><g filter="url(#rough)"><path d="{d}" fill="{fill}"/></g></g>'
            f'<g clip-path="url(#pc{seed})" filter="url(#g)"><rect x="{x0}" y="{y0}" width="{x1-x0}" height="{y1-y0}" fill="{fill}"/>{extra}'
            f'<rect x="{x0}" y="{y0}" width="{x1-x0}" height="{y1-y0}" fill="url(#age)"/></g>')
    return clip, body


def paper_letter():
    w, h = 900, 1200
    extra = (f'<rect x="{PAD}" y="{PAD+380}" width="{w}" height="3" fill="#7a5a3a" opacity=".18"/><rect x="{PAD}" y="{PAD+383}" width="{w}" height="2" fill="#fff" opacity=".35"/>'
             f'<rect x="{PAD}" y="{PAD+760}" width="{w}" height="3" fill="#7a5a3a" opacity=".18"/><rect x="{PAD}" y="{PAD+763}" width="{w}" height="2" fill="#fff" opacity=".35"/>'
             f'<circle cx="{w-170}" cy="{h-190}" r="70" fill="none" stroke="#9a6a3a" stroke-width="10" opacity=".12"/>'
             f'<circle cx="{w-170}" cy="{h-190}" r="62" fill="#9a6a3a" opacity=".05"/>'
             f'<path d="M{PAD},{PAD+1} h{w}" stroke="#a88a62" stroke-width="2" opacity=".4"/>')
    clip, body = sheet(w, h, "#F1E6CC", extra, 3)
    return svg(w, h, body, paper_defs(3) + clip)


def paper_mabry():
    w, h = 820, 460
    lines = "".join(f'<line x1="{PAD}" y1="{PAD+70+k*48}" x2="{w-PAD}" y2="{PAD+70+k*48}" stroke="#8fa3b5" stroke-width="2" opacity=".55"/>' for k in range(8))
    extra = lines + f'<line x1="{PAD+70}" y1="{PAD}" x2="{PAD+70}" y2="{h}" stroke="#c0605a" stroke-width="2" opacity=".5"/>'
    clip, body = sheet(w, h, "#EEE7D6", extra, 5, torn_top=True)
    return svg(w, h, body, paper_defs(5) + clip)


def paper_newspaper():
    w, h = 1100, 1400
    x0, x1 = PAD + 50, w - PAD - 50
    extra = (f'<rect x="{x0}" y="{PAD+40}" width="{x1-x0}" height="6" fill="{INK}" opacity=".85"/>'
             f'<rect x="{x0}" y="{PAD+52}" width="{x1-x0}" height="2" fill="{INK}" opacity=".85"/>'
             f'<rect x="{x0}" y="{h-PAD-60}" width="{x1-x0}" height="2" fill="{INK}" opacity=".6"/>'
             f'<rect x="{PAD}" y="{PAD}" width="{w}" height="{h}" fill="#b49a6e" opacity=".06"/>'
             f'<path d="M{w/2},{PAD} v{h}" stroke="#7a5a3a" stroke-width="3" opacity=".12"/>')
    clip, body = sheet(w, h, "#ECE3CF", extra, 9)
    return svg(w, h, body, paper_defs(9) + clip)


def news_masthead():
    w, h = 1000, 150
    b = (f'<text x="{w/2}" y="86" text-anchor="middle" font-family="Lora" font-weight="700" font-size="76" fill="{INK}" letter-spacing="6">TIN SELLOWE</text>'
         f'<rect x="40" y="112" width="{w-80}" height="5" fill="{INK}"/><rect x="40" y="123" width="{w-80}" height="2" fill="{INK}"/>'
         f'<rect x="40" y="10" width="{w-80}" height="2" fill="{INK}"/>')
    return svg(w, h, b)


NOTICE_COLORS = [("cream", "#F2E8D2"), ("blue", "#DCE2E2"), ("green", "#E2E4CC"), ("pink", "#EED9CC")]


def notice(i):
    name, c = NOTICE_COLORS[i]
    w, h = 560, 700
    r = rng(40 + i)
    extra = ""
    if i % 2:
        extra += f'<path d="M{w-PAD},{PAD} l-70,0 l70,70 Z" fill="#000" opacity=".06"/>'
    clip, body = sheet(w, h, c, extra, 40 + i, torn_top=(i == 2))
    return f"notice_{name}", w, h, svg(w, h, body, paper_defs(40 + i) + clip)


def pin(kind):
    w = h = 72
    c = {"brass": ("#f2cf7a", "#a87a2e"), "red": ("#d9665a", "#7d2a20"), "blue": ("#7c9cc0", "#344a66")}[kind]
    d = (shadow("sh", 3, 5, 3, .45) + rad("pg", [(0, "#fff", .9), (.25, c[0], 1), (1, c[1], 1)], .38, .35, .6))
    b = f'<g filter="url(#sh)"><circle cx="36" cy="34" r="20" fill="url(#pg)"/></g><circle cx="30" cy="28" r="5" fill="#fff" opacity=".6"/>'
    return f"pin_{kind}", w, h, svg(w, h, b, d)


def tape():
    w, h = 200, 70
    b = (f'<g transform="rotate(-6 100 35)"><path d="M20,18 l6,4 l-6,4 l6,4 l-6,4 l6,4 l-6,4 L180,46 l-6,-4 l6,-4 l-6,-4 l6,-4 l-6,-4 l6,-4 Z" fill="#e8dcb8" opacity=".78"/></g>')
    return svg(w, h, b)


def board_cork():
    w, h = 1848, 864  # 1.5x of the 1232x576 Board_Frame
    r = rng(77)
    d = (shadow("sh", 0, 8, 10, .45) +
         '<filter id="cork" x="0" y="0" width="100%" height="100%"><feTurbulence type="fractalNoise" baseFrequency=".55" numOctaves="3" seed="9" result="n"/>'
         '<feColorMatrix in="n" type="matrix" values="0 0 0 0 .32  0 0 0 0 .2  0 0 0 0 .1  0 0 0 1.6 -.55" result="s"/>'
         '<feComposite in="s" in2="SourceGraphic" operator="in" result="si"/><feMerge><feMergeNode in="SourceGraphic"/><feMergeNode in="si"/></feMerge></filter>'
         + rad("corkv", [(0, "#000", 0), (.75, "#000", 0), (1, "#2a1a0e", .35)], r=.75))
    f = 34
    b = [f'<rect x="0" y="0" width="{w}" height="{h}" rx="10" fill="#4a3222"/>',
         planks_v(0, w, 0, f, r, base=(104, 72, 46), wmin=200, wmax=300).replace('height="34"', f'height="{f}"'),
         f'<g filter="url(#cork)"><rect x="{f}" y="{f}" width="{w-2*f}" height="{h-2*f}" fill="#B98A58"/></g>',
         f'<rect x="{f}" y="{f}" width="{w-2*f}" height="{h-2*f}" fill="url(#corkv)"/>',
         f'<rect x="{f}" y="{f}" width="{w-2*f}" height="6" fill="#2a1a0e" opacity=".45"/><rect x="{f}" y="{f}" width="6" height="{h-2*f}" fill="#2a1a0e" opacity=".35"/>',
         f'<rect x="1" y="1" width="{w-2}" height="{h-2}" rx="10" fill="none" stroke="#2a1a0e" stroke-width="3"/>',
         f'<rect x="{f-4}" y="{f-4}" width="{w-2*f+8}" height="{h-2*f+8}" fill="none" stroke="#7a5638" stroke-width="3"/>']
    for k in range(60):
        b.append(f'<circle cx="{r.uniform(f+20,w-f-20):.0f}" cy="{r.uniform(f+20,h-f-20):.0f}" r="{r.uniform(1.5,3):.1f}" fill="#4a2e18" opacity=".45"/>')
    return svg(w, h, "".join(b), d)


def notebook_page():
    w, h = 1500, 864  # 1.5x of Notebook_Page 1000x576
    r = rng(88)
    lines = "".join(f'<line x1="0" y1="{156+k*42}" x2="{w}" y2="{156+k*42}" stroke="#9fb0bf" stroke-width="2" opacity=".55"/>' for k in range(17))
    holes = "".join(f'<circle cx="38" cy="{60+k*62}" r="13" fill="#2a1d14" opacity=".85"/><path d="M14,{60+k*62} q24,-22 48,0" stroke="#8b8f96" stroke-width="5" fill="none"/>' for k in range(13))
    d = grain("g", .9, .09, 8) + rad("age", [(0, "#000", 0), (.7, "#6b4a2a", 0), (1, "#6b4a2a", .25)], r=.75)
    b = (f'<g filter="url(#g)"><rect width="{w}" height="{h}" rx="8" fill="#F3EBD6"/>{lines}'
         f'<line x1="105" y1="0" x2="105" y2="{h}" stroke="#c0605a" stroke-width="3" opacity=".7"/>'
         f'<rect width="{w}" height="{h}" rx="8" fill="url(#age)"/></g>{holes}')
    return svg(w, h, b, d)


# ---------------------------------------------------------------- FARM
def soil_bed(r, w=320, h=320):
    out = [f'<rect x="16" y="24" width="{w-32}" height="{h-40}" rx="26" fill="{SOIL_D}"/>',
           f'<rect x="16" y="16" width="{w-32}" height="{h-40}" rx="26" fill="{SOIL}"/>']
    for k in range(5):
        y = 52 + k * 52
        out.append(f'<path d="M36,{y} C110,{y-6} 210,{y+6} {w-36},{y}" stroke="{SOIL_L}" stroke-width="16" stroke-linecap="round" fill="none"/>'
                   f'<path d="M36,{y+12} C110,{y+6} 210,{y+18} {w-36},{y+12}" stroke="{SOIL_D}" stroke-width="6" stroke-linecap="round" fill="none" opacity=".6"/>')
    for k in range(40):
        out.append(f'<circle cx="{r.uniform(30,w-30):.0f}" cy="{r.uniform(26,h-40):.0f}" r="{r.uniform(1.5,4):.1f}" fill="{r.choice(["#8a6a4c","#2e1f14","#6e5038"])}"/>')
    # wooden edging
    out.append(f'<rect x="10" y="10" width="{w-20}" height="{h-28}" rx="28" fill="none" stroke="#7a5a3a" stroke-width="10"/>'
               f'<rect x="10" y="10" width="{w-20}" height="{h-28}" rx="28" fill="none" stroke="#a88058" stroke-width="3"/>')
    return "".join(out)


def sprout(x, y, s, c=LEAF):
    return (f'<g transform="translate({x},{y}) scale({s})"><path d="M0,0 C-4,-14 -14,-22 -22,-22 C-16,-12 -8,-6 0,0 Z" fill="{c}"/>'
            f'<path d="M0,0 C4,-16 14,-24 22,-24 C16,-12 8,-4 0,0 Z" fill="{LEAF_D}"/><path d="M0,2 v-10" stroke="{LEAF_D}" stroke-width="3"/></g>')


def carrot_top(x, y, s):
    return (f'<g transform="translate({x},{y}) scale({s})">'
            f'<ellipse cx="0" cy="6" rx="14" ry="7" fill="{CARROT}"/><ellipse cx="-3" cy="4" rx="6" ry="3" fill="#f2a564"/>'
            f'<path d="M0,4 C-10,-24 -26,-36 -34,-50 C-16,-42 -6,-24 0,0 Z" fill="{LEAF}"/>'
            f'<path d="M0,4 C2,-28 -2,-48 6,-66 C12,-46 6,-22 2,2 Z" fill="{LEAF_D}"/>'
            f'<path d="M0,4 C10,-22 24,-30 34,-44 C26,-26 12,-12 2,4 Z" fill="{LEAF}"/>'
            f'<path d="M0,4 C-4,-18 -10,-30 -8,-46" stroke="{OLIVE}" stroke-width="4" fill="none"/></g>')


def plot(state):
    w = h = 320
    r = rng(100 + ["empty", "seeded", "growing", "ready"].index(state))
    b = [soil_bed(r)]
    rows = [52 + k * 52 for k in range(5)]
    for y in rows:
        for k in range(4):
            x = 70 + k * 60 + r.uniform(-6, 6)
            if state == "seeded":
                b.append(f'<ellipse cx="{x:.0f}" cy="{y+2}" rx="12" ry="7" fill="{SOIL_D}"/><circle cx="{x-3:.0f}" cy="{y}" r="3" fill="#d9c49a"/>')
            elif state == "growing":
                b.append(sprout(x, y + 6, r.uniform(.9, 1.2)))
            elif state == "ready":
                b.append(carrot_top(x, y + 6, r.uniform(.68, .8)))
    if state == "ready":
        b.append(f'<circle cx="276" cy="44" r="26" fill="{GOLD}" stroke="{INK}" stroke-width="4"/><path d="M264,44 l9,9 l16,-18" stroke="{INK}" stroke-width="6" fill="none" stroke-linecap="round" stroke-linejoin="round"/>')
    return f"plot_{state}", w, h, svg(w, h, "".join(b), shadow("sh"))


def farm_card():
    w, h = 1350, 630
    r = rng(120)
    d = paper_defs(12) + shadow("sh2", 0, 14, 16, .5)
    inner = f'<rect x="40" y="40" width="{w-80}" height="{h-80}" rx="10" fill="{PAPER}"/>'
    b = (f'<g filter="url(#sh2)"><rect x="8" y="8" width="{w-16}" height="{h-24}" rx="18" fill="#5e412b"/></g>'
         + planks_v(8, w - 8, 8, h - 16, r, base=(110, 76, 50), wmin=140, wmax=220).replace('<rect x="8"', '<rect x="8"')
         + f'<g filter="url(#g)">{inner}<rect x="40" y="40" width="{w-80}" height="{h-80}" rx="10" fill="url(#age)"/></g>'
         + f'<rect x="40" y="40" width="{w-80}" height="{h-80}" rx="10" fill="none" stroke="#3a281b" stroke-width="4"/>'
         + "".join(f'<circle cx="{x}" cy="{y}" r="10" fill="url(#brass)" stroke="#5a3f1c" stroke-width="2"/>' for x in (24, w - 24) for y in (24, h - 32)))
    return svg(w, h, b, d)


def icon_carrot(size=128):
    return svg(size, size, f'<g transform="scale({size/128})">{carrot(70, 40, 1.15, 28)}</g>', shadow("sh"))


# ---------------------------------------------------------------- NIGHT MAP (812x576 card @2x)
def segdist(px, py, a, b):
    (ax, ay), (bx, by) = a, b
    dx, dy = bx - ax, by - ay
    t = max(0, min(1, ((px - ax) * dx + (py - ay) * dy) / (dx * dx + dy * dy)))
    return math.hypot(px - ax - t * dx, py - ay - t * dy)


NODES = {"home": (110, 330), "well": (260, 200), "market": (440, 330), "church": (450, 140),
         "bridge": (690, 150), "river": (340, 500), "oak": (700, 400)}
EDGES = [("home", "river"), ("river", "oak"), ("home", "well"), ("well", "market"), ("market", "oak"),
         ("well", "church"), ("market", "church"), ("church", "bridge"), ("bridge", "oak")]


def map_night():
    S = 2; w, h = 812 * S, 576 * S
    r = rng(130)
    d = (grain("g", .8, .07, 4) + shadow("sh", 0, 6, 6, .5) + blur("b8", 8)
         + rad("lamp", [(0, "#ffcf6a", .85), (.35, "#ffb04a", .3), (1, "#ffb04a", 0)])
         + rad("vig", [(0, "#000", 0), (.65, "#000", 0), (1, "#05070c", .6)], r=.75)
         + rad("moonwash", [(0, "#9fb1cc", .22), (1, "#9fb1cc", 0)], .8, .1, .8)
         + lin("water", [(0, "#2d3e5c", 1), (1, "#1e2a42", 1)], 0, 0, 1, 1))
    P = lambda k: (NODES[k][0] * S, NODES[k][1] * S)
    b = [f'<rect width="{w}" height="{h}" fill="#26303a"/>']
    # meadow texture
    for k in range(260):
        b.append(f'<circle cx="{r.uniform(0,w):.0f}" cy="{r.uniform(0,h):.0f}" r="{r.uniform(20,70):.0f}" fill="{r.choice(["#2b3640","#222b33","#2f3a3c","#283238"])}" opacity=".7"/>')
    # river along bottom
    b.append(f'<path d="M0,{1040} C300,{960} 520,{1080} 820,{1050} C1100,{1020} 1300,{1090} {w},{1010} L{w},{h} L0,{h} Z" fill="url(#water)"/>'
             f'<path d="M0,{1040} C300,{960} 520,{1080} 820,{1050} C1100,{1020} 1300,{1090} {w},{1010}" stroke="#4e5f6e" stroke-width="10" fill="none"/>')
    # stream from top under the bridge to right edge
    b.append(f'<path d="M1180,0 C1240,120 1330,260 1420,320 C1500,370 1560,420 {w},470 L{w},560 C1540,520 1470,450 1380,400 C1280,340 1200,180 1120,0 Z" fill="url(#water)"/>')
    for k in range(30):
        x = r.uniform(80, w - 80); y = r.uniform(1060, 1140)
        b.append(f'<rect x="{x:.0f}" y="{y:.0f}" width="{r.uniform(20,60):.0f}" height="3" fill="#9fb1cc" opacity=".35"/>')
    # reeds along river path
    for k in range(60):
        x = r.uniform(0, w); y = 1000 + r.uniform(-30, 10) + (60 if x > 820 else 0) * 0
        b.append(f'<path d="M{x:.0f},{y:.0f} q-4,-20 2,-38 M{x+6:.0f},{y:.0f} q6,-18 2,-30" stroke="#4a5a3a" stroke-width="3" fill="none"/>')
    # dirt roads under each edge
    for a, c in EDGES:
        (x1, y1), (x2, y2) = P(a), P(c)
        mx, my = (x1 + x2) / 2 + r.uniform(-30, 30), (y1 + y2) / 2 + r.uniform(-30, 30)
        b.append(f'<path d="M{x1},{y1} Q{mx:.0f},{my:.0f} {x2},{y2}" stroke="#4b4a44" stroke-width="34" stroke-linecap="round" fill="none" opacity=".55"/>'
                 f'<path d="M{x1},{y1} Q{mx:.0f},{my:.0f} {x2},{y2}" stroke="#5c5a50" stroke-width="18" stroke-linecap="round" fill="none" opacity=".45" stroke-dasharray="2 18"/>')
    # scattered houses
    for k in range(60):
        x = r.uniform(60, w - 60); y = r.uniform(60, 940)
        if any(math.hypot(x - P(n)[0], y - P(n)[1]) < 170 for n in NODES): continue
        if any(segdist(x + 30, y + 10, P(a), P(c)) < 90 for a, c in EDGES): continue
        if x > 1120 and y < 560: continue
        hw = r.uniform(50, 80)
        b.append(f'<g filter="url(#sh)"><rect x="{x:.0f}" y="{y:.0f}" width="{hw:.0f}" height="{hw*.7:.0f}" fill="#4a4038"/>'
                 f'<path d="M{x-6:.0f},{y:.0f} L{x+hw/2:.0f},{y-hw*.45:.0f} L{x+hw+6:.0f},{y:.0f} Z" fill="#5e3a2c"/></g>')
        if r.random() < .35:
            b.append(f'<rect x="{x+hw*.3:.0f}" y="{y+hw*.2:.0f}" width="10" height="12" fill="#f2b552"/>')
    def dy(k): x, y = P(k); return x, y - (70 if k == 'church' else 100)
    # HOME: cottage + small field
    x, y = dy("home")
    b.append(f'<g filter="url(#sh)"><rect x="{x-60}" y="{y-30}" width="120" height="80" fill="#6b5844"/><path d="M{x-74},{y-30} L{x},{y-90} L{x+74},{y-30} Z" fill="#7a4632"/>'
             f'<rect x="{x-14}" y="{y+6}" width="28" height="44" fill="#3a281b"/><rect x="{x+26}" y="{y-12}" width="20" height="20" fill="#f2b552"/></g>')
    for k in range(4):
        b.append(f'<path d="M{x-150},{y+40+k*16} h80" stroke="#4e3a28" stroke-width="8" stroke-linecap="round"/>')
    # WELL
    x, y = dy("well")
    b.append(f'<circle cx="{x}" cy="{y}" r="110" fill="url(#lamp)" opacity=".55"/>'
             f'<g filter="url(#sh)"><ellipse cx="{x}" cy="{y+20}" rx="54" ry="22" fill="#6f6a5c"/><rect x="{x-54}" y="{y-14}" width="108" height="34" fill="#8f8a7a"/>'
             f'<ellipse cx="{x}" cy="{y-14}" rx="54" ry="20" fill="#7d786a"/><ellipse cx="{x}" cy="{y-14}" rx="40" ry="13" fill="#141820"/>'
             f'<path d="M{x-60},{y-56} L{x},{y-92} L{x+60},{y-56} Z" fill="#6b3f2c"/><rect x="{x-50}" y="{y-60}" width="8" height="50" fill="#4a3324"/><rect x="{x+42}" y="{y-60}" width="8" height="50" fill="#4a3324"/></g>')
    # MARKET stalls + lanterns
    x, y = dy("market")
    b.append(f'<circle cx="{x}" cy="{y}" r="150" fill="url(#lamp)" opacity=".6"/>')
    for k, (ox, c) in enumerate([(-90, "#8c3a2b"), (0, "#a8874a"), (90, "#5a6a4a")]):
        b.append(f'<g filter="url(#sh)"><rect x="{x+ox-36}" y="{y-10}" width="72" height="44" fill="#4a3a2c"/>'
                 f'<path d="M{x+ox-44},{y-10} L{x+ox-36},{y-44} L{x+ox+36},{y-44} L{x+ox+44},{y-10} Z" fill="{c}"/>' +
                 "".join(f'<path d="M{x+ox-44+j*22},{y-10} q5,10 11,0" fill="{c}"/>' for j in range(4)) + '</g>')
    for ox in (-130, 130):
        b.append(f'<line x1="{x+ox}" y1="{y+30}" x2="{x+ox}" y2="{y-60}" stroke="#2a1d14" stroke-width="5"/><rect x="{x+ox-9}" y="{y-74}" width="18" height="22" rx="4" fill="#ffd27a"/>')
    # CHURCH
    x, y = dy("church")
    b.append(f'<g filter="url(#sh)"><rect x="{x-70}" y="{y-30}" width="140" height="80" fill="#5a5a5e"/><path d="M{x-80},{y-30} L{x},{y-80} L{x+80},{y-30} Z" fill="#454a56"/>'
             f'<rect x="{x+40}" y="{y-110}" width="44" height="160" fill="#62626a"/><path d="M{x+34},{y-110} L{x+62},{y-180} L{x+90},{y-110} Z" fill="#454a56"/>'
             f'<rect x="{x+58}" y="{y-206}" width="8" height="30" fill="#c9c2a8"/><rect x="{x+50}" y="{y-196}" width="24" height="7" fill="#c9c2a8"/>'
             f'<path d="M{x-20},{y+50} v-34 q20,-24 40,0 v34 Z" fill="#2a2420"/><circle cx="{x+62}" cy="{y-70}" r="10" fill="#f2b552"/></g>')
    # BRIDGE over the stream
    x, y = dy("bridge")
    b.append(f'<g filter="url(#sh)" transform="rotate(28 {x} {y+60})"><rect x="{x-110}" y="{y+30}" width="220" height="60" rx="10" fill="#6f7280"/>'
             f'<rect x="{x-110}" y="{y+26}" width="220" height="10" fill="#8a8e9c"/><rect x="{x-110}" y="{y+84}" width="220" height="10" fill="#8a8e9c"/>' +
             "".join(f'<rect x="{x-100+k*40}" y="{y+40}" width="30" height="40" fill="#7c808e" opacity=".6"/>' for k in range(5)) + '</g>')
    b.append(f'<g filter="url(#sh)"><rect x="{x+90}" y="{y-40}" width="60" height="50" fill="#4a4038"/><path d="M{x+84},{y-40} L{x+120},{y-70} L{x+156},{y-40} Z" fill="#5e3a2c"/>'
             f'<circle cx="{x+120}" cy="{y-10}" r="60" fill="url(#lamp)" opacity=".7"/></g>')  # guard post (risk revealed by F08)
    # RIVER PATH marker: stepping stones
    x, y = P("river")
    for k in range(7):
        b.append(f'<ellipse cx="{x-150+k*50}" cy="{y+40+(k%2)*10}" rx="18" ry="10" fill="#6f6a5c"/>')
    # OAK with hollow
    x, y = dy("oak")
    oak = [f'<path d="M{x-14},{y+60} L{x-8},{y-10} L{x+8},{y-10} L{x+14},{y+60} Z" fill="#3a2a1e"/>']
    for k in range(22):
        a = r.uniform(0, math.tau); dd = r.uniform(0, 70)
        oak.append(f'<circle cx="{x+math.cos(a)*dd*1.2:.0f}" cy="{y-60+math.sin(a)*dd*.8:.0f}" r="{r.uniform(34,52):.0f}" fill="{r.choice(["#2e3a26","#26321f","#34422a"])}"/>')
    b.append(f'<g filter="url(#sh)">{"".join(oak)}</g><ellipse cx="{x}" cy="{y+30}" rx="8" ry="13" fill="#0a0c10"/>')
    b.append(f'<rect width="{w}" height="{h}" fill="url(#moonwash)"/><rect width="{w}" height="{h}" fill="url(#vig)"/>')
    # compass
    b.append(f'<g transform="translate({w-110},{h-260})" opacity=".8"><circle r="54" fill="none" stroke="#c9b994" stroke-width="3"/>'
             f'<path d="M0,-70 L14,0 L0,70 L-14,0 Z" fill="#c9b994"/><path d="M0,-70 L14,0 L-14,0 Z" fill="{GOLD}"/>'
             f'<text y="-80" text-anchor="middle" font-family="Lora" font-weight="700" font-size="28" fill="#e8d6ad">B</text></g>')
    return svg(w, h, f'<g filter="url(#g)">{"".join(b)}</g>', d)


def node_plate(state):
    w, h = 296, 84
    fill, stroke, sw = {"normal": ("#E8DBBE", "#4A3B2F", 4), "route": ("#F2DFA8", GOLD, 6),
                        "disabled": ("#9C9488", "#5a524a", 4), "home": ("#D9CBB0", "#4A3B2F", 4)}[state]
    b = (f'<g filter="url(#sh)"><rect x="6" y="6" width="{w-12}" height="{h-16}" rx="14" fill="{fill}"/></g>'
         f'<rect x="6" y="6" width="{w-12}" height="{h-16}" rx="14" fill="none" stroke="{stroke}" stroke-width="{sw}"/>'
         f'<rect x="14" y="14" width="{w-28}" height="{h-32}" rx="9" fill="none" stroke="{stroke}" stroke-width="1.5" opacity=".45"/>')
    if state == "disabled":
        b += f'<path d="M20,{h-14} L{w-20},14" stroke="#5a524a" stroke-width="3" opacity=".5"/>'
    return f"node_plate_{state}", w, h, svg(w, h, b, shadow("sh", 0, 4, 4, .45))


def risk_icon(level):
    s = 128
    col = {"safe": "#6E7446", "low": "#B99A3E", "mid": "#C4702E", "high": "#8C3A2B", "unknown": "#6b6560"}[level]
    inner = {
        "safe": '<path d="M42,66 l16,16 l30,-34" stroke="#F4ECD8" stroke-width="11" fill="none" stroke-linecap="round" stroke-linejoin="round"/>',
        "low": '<rect x="58" y="40" width="12" height="38" rx="6" fill="#F4ECD8"/><circle cx="64" cy="92" r="7" fill="#F4ECD8"/>',
        "mid": '<rect x="44" y="40" width="12" height="38" rx="6" fill="#F4ECD8"/><circle cx="50" cy="92" r="7" fill="#F4ECD8"/><rect x="72" y="40" width="12" height="38" rx="6" fill="#F4ECD8"/><circle cx="78" cy="92" r="7" fill="#F4ECD8"/>',
        "high": '<path d="M28,64 C44,40 84,40 100,64 C84,88 44,88 28,64 Z" fill="#F4ECD8"/><circle cx="64" cy="64" r="13" fill="#8C3A2B"/><circle cx="64" cy="64" r="5" fill="#2a1410"/>',
        "unknown": '<path d="M48,50 q0,-18 16,-18 q16,0 16,16 q0,12 -12,16 q-4,2 -4,10" stroke="#F4ECD8" stroke-width="10" fill="none" stroke-linecap="round"/><circle cx="64" cy="94" r="7" fill="#F4ECD8"/>',
    }[level]
    b = f'<g filter="url(#sh)"><circle cx="64" cy="62" r="48" fill="{col}"/></g><circle cx="64" cy="62" r="48" fill="none" stroke="#2A221C" stroke-width="5"/><circle cx="64" cy="62" r="40" fill="none" stroke="#F4ECD8" stroke-width="2" opacity=".35"/>{inner}'
    return f"risk_{level}", s, s, svg(s, s, b, shadow("sh", 0, 4, 4, .4))


# ---------------------------------------------------------------- UI
def dialogue_box():
    w, h = 2464, 440  # 2x of Shop_DialogueBox 1232x220; 9-slice border = 56px
    d = paper_defs(21) + lin("wood", [(0, "#5a3f2b", 1), (1, "#3a281b", 1)])
    corner = lambda x, y, sx, sy: (f'<g transform="translate({x},{y}) scale({sx},{sy})"><path d="M0,0 h44 v8 h-36 v36 h-8 Z" fill="{GOLD}"/>'
                                   f'<circle cx="18" cy="18" r="6" fill="{GOLD}"/></g>')
    b = (f'<g filter="url(#sh)"><rect x="8" y="8" width="{w-16}" height="{h-24}" rx="22" fill="url(#wood)"/></g>'
         f'<g filter="url(#g)"><rect x="30" y="30" width="{w-60}" height="{h-68}" rx="12" fill="#F1E6CC"/>'
         f'<rect x="30" y="30" width="{w-60}" height="{h-68}" rx="12" fill="url(#age)"/></g>'
         f'<rect x="30" y="30" width="{w-60}" height="{h-68}" rx="12" fill="none" stroke="#2A221C" stroke-width="4"/>'
         f'<rect x="42" y="42" width="{w-84}" height="{h-92}" rx="8" fill="none" stroke="{EDGE}" stroke-width="2"/>'
         + corner(14, 14, 1, 1) + corner(w - 14, 14, -1, 1) + corner(14, h - 30, 1, -1) + corner(w - 14, h - 30, -1, -1))
    return svg(w, h, b, d)


def name_tag():
    w, h = 420, 84
    b = (f'<g filter="url(#sh)"><path d="M20,8 H{w-20} L{w-4},{h/2-4} L{w-20},{h-16} H20 L4,{h/2-4} Z" fill="#2A221C"/></g>'
         f'<path d="M26,16 H{w-26} L{w-14},{h/2-4} L{w-26},{h-24} H26 L14,{h/2-4} Z" fill="none" stroke="{GOLD}" stroke-width="3"/>')
    return svg(w, h, b, shadow("sh", 0, 5, 5, .45))


def choice_btn(state):
    w, h = 760, 96
    fill, stroke, sw, dy = {"normal": (TAN, "#4A3B2F", 3, 0), "hover": ("#EFE2C2", GOLD, 5, 0),
                            "pressed": ("#CDBD99", "#4A3B2F", 3, 3), "disabled": ("#B9B1A2", "#7a7064", 2, 0)}[state]
    b = (f'<g filter="url(#sh)"><rect x="8" y="{6+dy}" width="{w-16}" height="{h-20}" rx="14" fill="{fill}"/></g>'
         f'<rect x="8" y="{6+dy}" width="{w-16}" height="{h-20}" rx="14" fill="none" stroke="{stroke}" stroke-width="{sw}"/>')
    if state == "hover":
        b += f'<path d="M30,{h/2-4} l14,-12 v24 Z" fill="{GOLD}"/>'
    return f"btn_choice_{state}", w, h, svg(w, h, b, shadow("sh", 0, 5 - dy, 5, .35))


def btn(kind):
    w, h = 400, 104
    fill, stroke, hl = {"primary": (RED, "#4a1a12", "#b85a48"), "secondary": (PAPER, "#4A3B2F", "#fff"), "night": ("#3a332c", "#1a1612", "#5a4f44")}[kind]
    b = (f'<g filter="url(#sh)"><rect x="8" y="8" width="{w-16}" height="{h-24}" rx="18" fill="{fill}"/></g>'
         f'<rect x="8" y="8" width="{w-16}" height="{(h-24)/2}" rx="18" fill="{hl}" opacity=".18"/>'
         f'<rect x="8" y="8" width="{w-16}" height="{h-24}" rx="18" fill="none" stroke="{stroke}" stroke-width="4"/>'
         f'<rect x="16" y="16" width="{w-32}" height="{h-40}" rx="12" fill="none" stroke="{GOLD if kind!="secondary" else EDGE}" stroke-width="2" opacity=".6"/>')
    return f"btn_{kind}", w, h, svg(w, h, b, shadow("sh", 0, 6, 6, .4))


def hud_bar():
    w, h = 2560, 120
    r = rng(150)
    b = (planks_v(0, w, 0, h, r, base=(52, 40, 32), var=6, wmin=300, wmax=420, grain_op=.12)
         + f'<rect width="{w}" height="{h}" fill="#000" opacity=".25"/>'
         + f'<rect y="{h-10}" width="{w}" height="4" fill="{GOLD}" opacity=".85"/><rect y="{h-6}" width="{w}" height="6" fill="#120d09"/>')
    return svg(w, h, b)


def panel(kind):
    w, h = (808, 1152) if kind == "night" else (1600, 1000)
    fill = "#2F2924" if kind == "night" else "#F1E6CC"
    d = paper_defs(31 if kind == "night" else 33)
    b = (f'<g filter="url(#sh)"><rect x="12" y="12" width="{w-24}" height="{h-36}" rx="20" fill="{"#1e1a16" if kind=="night" else "#5e412b"}"/></g>'
         f'<g filter="url(#g)"><rect x="30" y="30" width="{w-60}" height="{h-72}" rx="12" fill="{fill}"/>'
         + (f'<rect x="30" y="30" width="{w-60}" height="{h-72}" rx="12" fill="url(#age)"/>' if kind != "night" else "") + '</g>'
         f'<rect x="30" y="30" width="{w-60}" height="{h-72}" rx="12" fill="none" stroke="{GOLD if kind=="night" else "#2A221C"}" stroke-width="{2 if kind=="night" else 4}" opacity="{.6 if kind=="night" else 1}"/>')
    return f"panel_{kind}", w, h, svg(w, h, b, d)


def susp_meter(level):
    w, h = 360, 72
    n = {"low": 1, "mid": 2, "high": 3}[level]; col = {"low": "#6E7446", "mid": "#C4702E", "high": "#8C3A2B"}[level]
    b = (f'<rect x="4" y="4" width="{w-8}" height="{h-8}" rx="{(h-8)/2}" fill="#2A221C" stroke="#4A3B2F" stroke-width="3"/>'
         f'<g transform="translate(40,36)"><path d="M-24,0 C-14,-16 14,-16 24,0 C14,16 -14,16 -24,0 Z" fill="{LIGHT}"/><circle r="8" fill="{col}"/></g>' +
         "".join(f'<rect x="{84+k*90}" y="24" width="80" height="24" rx="12" fill="{col if k<n else "#4A3B2F"}"/>' for k in range(3)))
    return f"meter_suspicion_{level}", w, h, svg(w, h, b)


# ---------------------------------------------------------------- ICONS (128px)
def icons():
    S = 128; sh = shadow("sh", 0, 4, 4, .4)
    br = lin("brass", [(0, "#f2cf7a", 1), (.5, "#c49a3e", 1), (1, "#8a6526", 1)], 0, 0, 1, 1)
    out = {}
    out["icon_coin"] = (f'<g filter="url(#sh)"><circle cx="64" cy="62" r="42" fill="url(#brass)"/></g><circle cx="64" cy="62" r="32" fill="none" stroke="#8a6526" stroke-width="4"/>'
                        f'<rect x="52" y="50" width="24" height="24" fill="#5a3f1c" opacity=".85"/><rect x="52" y="50" width="24" height="24" fill="none" stroke="#f2cf7a" stroke-width="2" opacity=".6"/>', br + sh)
    out["icon_coins"] = ("".join(f'<ellipse cx="{50+k*0}" cy="{92-k*12}" rx="34" ry="12" fill="url(#brass)" stroke="#7a5a22" stroke-width="3"/>' for k in range(5)) +
                         f'<ellipse cx="92" cy="96" rx="26" ry="10" fill="url(#brass)" stroke="#7a5a22" stroke-width="3"/>', br)
    out["icon_ap"] = (f'<g filter="url(#sh)"><rect x="34" y="16" width="60" height="10" rx="4" fill="#5e412b"/><rect x="34" y="100" width="60" height="10" rx="4" fill="#5e412b"/>'
                      f'<path d="M42,26 h44 c0,24 -18,30 -18,37 c0,7 18,13 18,37 h-44 c0,-24 18,-30 18,-37 c0,-7 -18,-13 -18,-37 Z" fill="#EDE3D1" stroke="#5e412b" stroke-width="4"/>'
                      f'<path d="M50,38 h28 c-2,10 -12,16 -14,22 c-2,-6 -12,-12 -14,-22 Z M48,96 c4,-14 14,-16 16,-20 c2,4 12,6 16,20 Z" fill="{GOLD}"/></g>', sh)
    out["icon_suspicion"] = (f'<g filter="url(#sh)"><path d="M12,64 C34,30 94,30 116,64 C94,98 34,98 12,64 Z" fill="#EDE3D1" stroke="{INK}" stroke-width="5"/>'
                             f'<circle cx="64" cy="64" r="22" fill="{RED}"/><circle cx="64" cy="64" r="9" fill="#1a0e0a"/><circle cx="56" cy="56" r="5" fill="#fff" opacity=".7"/></g>', sh)
    out["icon_sun"] = (f'<g filter="url(#sh)"><circle cx="64" cy="64" r="26" fill="{GOLD}"/>' +
                       "".join(f'<rect x="61" y="14" width="6" height="18" rx="3" fill="{GOLD}" transform="rotate({k*45} 64 64)"/>' for k in range(8)) + '</g>', sh)
    out["icon_moon"] = (f'<g filter="url(#sh)"><path d="M78,20 A44,44 0 1 0 108,84 A36,36 0 1 1 78,20 Z" fill="#E8E2C8"/></g>'
                        f'<circle cx="52" cy="70" r="6" fill="#cfc7a8"/><circle cx="66" cy="94" r="4" fill="#cfc7a8"/>', sh)
    out["icon_calendar"] = (f'<g filter="url(#sh)"><rect x="18" y="24" width="92" height="86" rx="8" fill="{PAPER}"/><rect x="18" y="24" width="92" height="26" rx="8" fill="{RED}"/></g>'
                            f'<rect x="34" y="14" width="8" height="20" rx="4" fill="{INK}"/><rect x="86" y="14" width="8" height="20" rx="4" fill="{INK}"/>' +
                            "".join(f'<rect x="{30+(k%4)*20}" y="{60+(k//4)*16}" width="12" height="10" fill="{MUTED}"/>' for k in range(12)), sh)
    out["icon_notebook"] = (f'<g filter="url(#sh)"><rect x="26" y="14" width="78" height="100" rx="6" fill="#6b3524"/><rect x="26" y="14" width="14" height="100" rx="4" fill="#3a1912"/>'
                            f'<rect x="50" y="34" width="44" height="20" fill="{PAPER}" opacity=".9"/><rect x="96" y="14" width="8" height="100" fill="#e8dcc0"/></g>'
                            f'<path d="M80,14 v34 l8,-8 l8,8 v-34" fill="{GOLD}"/>', sh)
    out["icon_letter"] = (f'<g filter="url(#sh)"><rect x="12" y="30" width="104" height="70" rx="4" fill="#EFE4CC"/></g>'
                          f'<path d="M12,30 L64,70 L116,30" stroke="#b8a580" stroke-width="4" fill="none"/>'
                          f'<circle cx="64" cy="70" r="14" fill="{RED}"/><circle cx="64" cy="70" r="8" fill="none" stroke="#5a1a12" stroke-width="2"/>', sh)
    out["icon_board"] = (f'<g filter="url(#sh)"><rect x="16" y="22" width="96" height="76" rx="4" fill="#B98A58" stroke="#5e412b" stroke-width="6"/>'
                         f'<rect x="28" y="34" width="34" height="42" fill="{PAPER}" transform="rotate(-6 45 55)"/><rect x="66" y="40" width="32" height="38" fill="#DCE2E2" transform="rotate(5 82 59)"/></g>'
                         f'<circle cx="45" cy="36" r="5" fill="{RED}"/><circle cx="82" cy="42" r="5" fill="{GOLD}"/><rect x="28" y="98" width="8" height="18" fill="#5e412b"/><rect x="92" y="98" width="8" height="18" fill="#5e412b"/>', sh)
    out["icon_shrike"] = (f'<g filter="url(#sh)"><path d="M18,74 C30,52 50,40 72,42 C86,30 100,30 108,40 L120,44 L106,50 C104,64 94,76 76,80 L52,82 L14,104 L30,82 Z" fill="#5a5f66"/>'
                          f'<path d="M72,42 C86,30 100,30 108,40 L120,44 L106,50 C102,46 92,44 84,48 Z" fill="#9aa0a6"/>'
                          f'<path d="M80,44 L108,46" stroke="#111" stroke-width="7" stroke-linecap="round"/><circle cx="96" cy="44" r="3" fill="#fff"/>'
                          f'<path d="M44,62 C56,58 70,60 80,66" stroke="#2a2d31" stroke-width="5" fill="none"/></g>', sh)
    out["icon_handkerchief"] = (f'<g filter="url(#sh)"><path d="M18,30 L104,20 L110,104 L24,110 Z" fill="#F4F0E6"/></g>'
                                f'<path d="M26,38 L98,30 L102,96 L32,102 Z" fill="none" stroke="#8fa3b5" stroke-width="2" stroke-dasharray="5 4"/>'
                                f'<g transform="translate(46,58) scale(.42)"><path d="M18,74 C30,52 50,40 72,42 C86,30 100,30 108,40 L120,44 L106,50 C104,64 94,76 76,80 L52,82 L14,104 L30,82 Z" fill="#4E6070"/><path d="M80,44 L108,46" stroke="#111" stroke-width="7"/></g>', sh)
    out["icon_seal_shrike"] = (f'<g filter="url(#sh)"><path d="M64,14 C80,16 86,10 98,22 C110,34 106,42 112,56 C116,72 108,80 108,94 C96,108 88,104 74,112 C58,116 50,108 36,108 C22,100 22,90 16,76 C12,60 20,52 18,38 C28,24 36,24 48,16 Z" fill="#7d2a20"/></g>'
                               f'<circle cx="64" cy="64" r="36" fill="none" stroke="#5a1a12" stroke-width="4"/>'
                               f'<g transform="translate(30,32) scale(.55)"><path d="M18,74 C30,52 50,40 72,42 C86,30 100,30 108,40 L120,44 L106,50 C104,64 94,76 76,80 L52,82 L14,104 L30,82 Z" fill="#5a1a12"/></g>', sh)
    out["icon_seal_straka"] = (f'<g filter="url(#sh)"><path d="M64,14 C80,16 86,10 98,22 C110,34 106,42 112,56 C116,72 108,80 108,94 C96,108 88,104 74,112 C58,116 50,108 36,108 C22,100 22,90 16,76 C12,60 20,52 18,38 C28,24 36,24 48,16 Z" fill="#3c4f66"/></g>'
                               f'<circle cx="64" cy="64" r="36" fill="none" stroke="#24344a" stroke-width="4"/>'
                               f'<path d="M64,38 L72,58 L94,60 L76,74 L82,96 L64,84 L46,96 L52,74 L34,60 L56,58 Z" fill="#24344a"/>', sh)
    out["icon_oak"] = (f'<g filter="url(#sh)"><path d="M56,118 L60,70 L68,70 L72,118 Z" fill="#4a3324"/>' +
                       "".join(f'<circle cx="{cx}" cy="{cy}" r="{rr}" fill="{c}"/>' for cx, cy, rr, c in [(64, 44, 30, OLIVE_D), (40, 56, 22, OLIVE_D), (88, 56, 22, OLIVE_D), (58, 38, 22, OLIVE), (78, 44, 18, OLIVE), (44, 50, 14, OLIVE)]) +
                       f'<ellipse cx="64" cy="98" rx="5" ry="8" fill="#1a120b"/></g>', sh)
    out["icon_lantern"] = (rad("lg", [(0, "#ffcf6a", .7), (1, "#ffcf6a", 0)]) and
                           (f'<circle cx="64" cy="68" r="56" fill="url(#lg)"/><g filter="url(#sh)"><path d="M50,18 q14,-14 28,0" stroke="{INK}" stroke-width="5" fill="none"/>'
                            f'<path d="M42,26 h44 l-6,12 h-32 Z" fill="{INK}"/><rect x="44" y="38" width="40" height="56" rx="6" fill="#ffd27a"/>'
                            f'<rect x="44" y="38" width="40" height="56" rx="6" fill="none" stroke="{INK}" stroke-width="5"/><line x1="64" y1="38" x2="64" y2="94" stroke="{INK}" stroke-width="3"/>'
                            f'<path d="M38,94 h52 l-8,10 h-36 Z" fill="{INK}"/></g>', sh + rad("lg", [(0, "#ffcf6a", .7), (1, "#ffcf6a", 0)])))
    out["icon_home"] = (f'<g filter="url(#sh)"><rect x="28" y="56" width="72" height="54" fill="#d9c6a2"/><path d="M16,60 L64,20 L112,60 Z" fill="#7a4632"/>'
                        f'<rect x="56" y="76" width="18" height="34" fill="#5a3b26"/><rect x="82" y="68" width="14" height="14" fill="#f2b552"/></g>', sh)
    out["icon_stay_home"] = (f'<g filter="url(#sh)"><rect x="28" y="56" width="72" height="54" fill="#4a4038"/><path d="M16,60 L64,20 L112,60 Z" fill="#5e3a2c"/>'
                             f'<rect x="56" y="76" width="18" height="34" fill="#2a1d14"/><rect x="82" y="68" width="14" height="14" fill="#f2b552"/></g>'
                             f'<path d="M92,16 A16,16 0 1 0 108,34 A12,12 0 1 1 92,16 Z" fill="#E8E2C8"/>', sh)
    out["icon_bell"] = (f'<g filter="url(#sh)"><path d="M64,20 c-22,2 -26,30 -28,56 l-8,12 h72 l-8,-12 c-2,-26 -6,-54 -28,-56 Z" fill="url(#brass)"/>'
                        f'<circle cx="64" cy="98" r="9" fill="#8a6526"/><rect x="58" y="10" width="12" height="12" rx="4" fill="#8a6526"/></g>', br + sh)
    out["icon_carrot"] = (f'<g filter="url(#sh)">{carrot(70, 40, 1.15, 28)}</g>', sh)
    out["icon_rent"] = (f'<g filter="url(#sh)"><rect x="20" y="22" width="76" height="92" rx="4" fill="#EEE7D6" transform="rotate(-6 58 68)"/></g>' +
                        "".join(f'<line x1="30" y1="{44+k*14}" x2="82" y2="{40+k*14}" stroke="#8fa3b5" stroke-width="3"/>' for k in range(5)) +
                        f'<g filter="url(#sh)"><circle cx="92" cy="92" r="24" fill="url(#brass)"/><rect x="85" y="85" width="14" height="14" fill="#5a3f1c" opacity=".85"/></g>', br + sh)
    return [(n, S, S, svg(S, S, b, d)) for n, (b, d) in out.items()]


if __name__ == "__main__":
    import sys
    want = sys.argv[1:]
    jobs = []
    def add(group, name, w, h, s):
        if not want or group in want: jobs.append((name, w, h, s, group))
    add("Paper", "paper_letter", 900, 1200, paper_letter())
    add("Paper", "paper_mabry", 820, 460, paper_mabry())
    add("Paper", "paper_newspaper", 1100, 1400, paper_newspaper())
    add("Paper", "news_masthead", 1000, 150, news_masthead())
    for i in range(4): add("Paper", *notice(i))
    for k in ("brass", "red", "blue"): add("Paper", *pin(k))
    add("Paper", "tape", 200, 70, tape())
    add("Paper", "board_cork", 1848, 864, board_cork())
    add("Paper", "notebook_page", 1500, 864, notebook_page())
    for st in ("empty", "seeded", "growing", "ready"): add("Farm", *plot(st))
    add("Farm", "farm_card", 1350, 630, farm_card())
    add("Map", "map_night", 1624, 1152, map_night())
    for st in ("normal", "route", "disabled", "home"): add("Map", *node_plate(st))
    for lv in ("safe", "low", "mid", "high", "unknown"): add("Map", *risk_icon(lv))
    add("UI", "dialogue_box", 2464, 440, dialogue_box())
    add("UI", "name_tag", 420, 84, name_tag())
    for st in ("normal", "hover", "pressed", "disabled"): add("UI", *choice_btn(st))
    for k in ("primary", "secondary", "night"): add("UI", *btn(k))
    add("UI", "hud_bar", 2560, 120, hud_bar())
    for k in ("night", "ending"): add("UI", *panel(k))
    for lv in ("low", "mid", "high"): add("UI", *susp_meter(lv))
    for n, w, h, s in icons(): add("Icons", n, w, h, s)
    render(jobs)
