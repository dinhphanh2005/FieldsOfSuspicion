"""Six 1920x1080 backgrounds for Fields of Suspicion."""
from common import *

W, H = 1920, 1080


def planks_v(x0, x1, y0, y1, r, base=(110, 76, 50), var=14, wmin=80, wmax=118, seam="#2b1d13", grain_op=.18):
    out = []; x = x0
    while x < x1:
        w = r.randint(wmin, wmax); w = min(w, x1 - x)
        d = r.randint(-var, var)
        c = f"rgb({base[0]+d},{base[1]+int(d*.75)},{base[2]+int(d*.5)})"
        out.append(f'<rect x="{x}" y="{y0}" width="{w}" height="{y1-y0}" fill="{c}"/>')
        for _ in range(r.randint(3, 6)):
            gx = x + r.uniform(6, w - 6); amp = r.uniform(2, 7)
            out.append(f'<path d="M{gx:.0f},{y0} C{gx+amp:.0f},{y0+(y1-y0)*.3:.0f} {gx-amp:.0f},{y0+(y1-y0)*.6:.0f} {gx+amp*.5:.0f},{y1}" '
                       f'stroke="#2b1d13" stroke-width="{r.uniform(.8,1.8):.1f}" fill="none" opacity="{grain_op*r.uniform(.5,1):.2f}"/>')
        if r.random() < .35:
            ky = r.uniform(y0 + 60, y1 - 60)
            out.append(f'<ellipse cx="{x+w*r.uniform(.3,.7):.0f}" cy="{ky:.0f}" rx="5" ry="9" fill="#2b1d13" opacity=".35"/>')
        out.append(f'<rect x="{x}" y="{y0}" width="3" height="{y1-y0}" fill="{seam}" opacity=".85"/>')
        out.append(f'<rect x="{x+3}" y="{y0}" width="2" height="{y1-y0}" fill="#fff" opacity=".05"/>')
        x += w
    return "".join(out)


def planks_h(x0, x1, y0, y1, r, base=(92, 62, 40), var=10, hmin=34, hmax=48):
    out = []; y = y0
    while y < y1:
        h = r.randint(hmin, hmax); h = min(h, y1 - y); d = r.randint(-var, var)
        out.append(f'<rect x="{x0}" y="{y}" width="{x1-x0}" height="{h}" fill="rgb({base[0]+d},{base[1]+int(d*.7)},{base[2]+int(d*.5)})"/>')
        for _ in range(4):
            gy = y + r.uniform(4, h - 4)
            out.append(f'<path d="M{x0},{gy:.0f} C{x0+(x1-x0)*.3:.0f},{gy+r.uniform(-4,4):.0f} {x0+(x1-x0)*.7:.0f},{gy+r.uniform(-4,4):.0f} {x1},{gy:.0f}" stroke="#24180f" stroke-width="1" fill="none" opacity=".16"/>')
        out.append(f'<rect x="{x0}" y="{y}" width="{x1-x0}" height="2" fill="#1d130c" opacity=".8"/>')
        y += h
    return "".join(out)


def herb_bunch(x, y, r, length=120, color=LEAF):
    out = [f'<line x1="{x}" y1="{y}" x2="{x}" y2="{y+30}" stroke="#c9b48a" stroke-width="2"/>']
    for i in range(14):
        a = r.uniform(-18, 18); l = r.uniform(.6, 1) * length
        ex = x + math.sin(math.radians(a)) * l; ey = y + 30 + math.cos(math.radians(a)) * l
        out.append(f'<path d="M{x},{y+34} Q{(x+ex)/2+r.uniform(-6,6):.0f},{(y+34+ey)/2:.0f} {ex:.0f},{ey:.0f}" stroke="{OLIVE_D}" stroke-width="2" fill="none"/>')
        for t in (.45, .7, .92):
            lx = x + (ex - x) * t; ly = y + 34 + (ey - y - 34) * t
            out.append(f'<ellipse cx="{lx:.0f}" cy="{ly:.0f}" rx="{r.uniform(5,9):.0f}" ry="{r.uniform(3,5):.0f}" '
                       f'transform="rotate({r.uniform(-60,60):.0f} {lx:.0f} {ly:.0f})" fill="{r.choice([color, OLIVE, OLIVE_D])}" opacity=".95"/>')
    out.append(f'<rect x="{x-9}" y="{y+28}" width="18" height="9" rx="3" fill="#b0905a"/>')
    return "".join(out)


def garlic_string(x, y, n, r):
    out = [f'<path d="M{x},{y} L{x},{y+n*30+10}" stroke="#c9b48a" stroke-width="3"/>']
    for i in range(n):
        gx = x + (14 if i % 2 else -14); gy = y + 24 + i * 30
        out.append(f'<path d="M{gx},{gy-18} C{gx+16},{gy-8} {gx+18},{gy+12} {gx},{gy+16} C{gx-18},{gy+12} {gx-16},{gy-8} {gx},{gy-18} Z" fill="#efe4cc"/>'
                   f'<path d="M{gx},{gy-16} C{gx+4},{gy-4} {gx+4},{gy+8} {gx},{gy+15}" stroke="#c8b591" stroke-width="1.5" fill="none"/>')
    return "".join(out)


def vignette(id_="vig", strength=.55):
    return rad(id_, [(0, "#000", 0), (.62, "#000", 0), (1, "#0d0805", strength)], r=.75)


# ---------------------------------------------------------------- SHOP
def bg_shop():
    r = rng(11)
    defs = (vignette() + grain("g", .85, .08) + blur("b30", 30) + blur("b8", 8) + blur("b3", 3) + shadow("sh", 0, 10, 10, .45)
            + lin("wallLight", [(0, "#f2c98a", .0), (.5, "#f2c98a", .10), (1, "#f2c98a", 0)], 0, 0, 1, 0)
            + lin("sky", [(0, "#f6e2b4", 1), (1, "#e8b77c", 1)])
            + lin("shaft", [(0, "#ffe7b0", .38), (1, "#ffe7b0", 0)], 1, 0, 0, 1)
            + lin("ctop", [(0, "#a77b50", 1), (1, "#7c5636", 1)])
            + rad("glow", [(0, "#ffd99a", .35), (1, "#ffd99a", 0)], .5, .4, .55)
            + lin("brass", [(0, "#f2cf7a", 1), (.5, "#c49a3e", 1), (1, "#8a6526", 1)], 0, 0, 1, 1))
    b = []
    b.append(f'<rect width="{W}" height="{H}" fill="#4a3324"/>')
    b.append(planks_v(0, W, 70, 840, r, base=(116, 82, 54)))
    b.append(f'<rect width="{W}" height="{H}" fill="url(#wallLight)"/>')
    # wainscot rail
    b.append(f'<rect x="0" y="610" width="{W}" height="16" fill="#3a271a"/><rect x="0" y="606" width="{W}" height="4" fill="#9c7650" opacity=".6"/>')
    # ceiling beam
    b.append(f'<rect x="0" y="0" width="{W}" height="78" fill="#2c1f16"/><rect x="0" y="70" width="{W}" height="10" fill="#1d140e"/>'
             f'<rect x="0" y="60" width="{W}" height="4" fill="#6b4c33" opacity=".7"/>')
    for x in (110, 640, 1280, 1820):
        b.append(f'<rect x="{x-30}" y="0" width="60" height="96" fill="#251a12"/><rect x="{x-30}" y="0" width="6" height="96" fill="#5a3f2b" opacity=".6"/>')
    # ---- right window with street view
    wx, wy, ww, wh = 1400, 210, 330, 380
    b.append(f'<rect x="{wx-26}" y="{wy-26}" width="{ww+52}" height="{wh+60}" fill="#2e2017" filter="url(#sh)"/>')
    b.append(f'<rect x="{wx}" y="{wy}" width="{ww}" height="{wh}" fill="url(#sky)"/>')
    # outside: houses
    hz = wy + wh * .62
    for i, (hx, hw, hh, c) in enumerate([(wx-20, 120, 120, "#c99e72"), (wx+90, 110, 150, "#b98d63"), (wx+190, 150, 110, "#c5996b")]):
        b.append(f'<rect x="{hx}" y="{hz-hh}" width="{hw}" height="{hh+200}" fill="{c}"/>'
                 f'<path d="M{hx-12},{hz-hh} L{hx+hw/2},{hz-hh-60} L{hx+hw+12},{hz-hh} Z" fill="#9a6e4c"/>'
                 f'<rect x="{hx+hw*.3}" y="{hz-hh+30}" width="22" height="30" fill="#7d5a3e"/>')
    b.append(f'<rect x="{wx}" y="{hz+40}" width="{ww}" height="{wh}" fill="#b59a74"/>')
    b.append(f'<rect x="{wx}" y="{wy}" width="{ww}" height="{wh}" fill="#fff4dc" opacity=".28"/>')
    # mullions
    b.append(f'<rect x="{wx+ww/2-7}" y="{wy}" width="14" height="{wh}" fill="#3a281b"/><rect x="{wx}" y="{wy+wh/2-7}" width="{ww}" height="14" fill="#3a281b"/>')
    b.append(f'<rect x="{wx}" y="{wy}" width="{ww}" height="{wh}" fill="none" stroke="#3a281b" stroke-width="14"/>')
    b.append(f'<rect x="{wx-34}" y="{wy+wh+18}" width="{ww+68}" height="20" fill="#5e412b"/><rect x="{wx-34}" y="{wy+wh+18}" width="{ww+68}" height="4" fill="#a7805a"/>')
    # pot of herbs on sill
    b.append(f'<path d="M{wx+40},{wy+wh+18} l10,-44 h52 l10,44 Z" fill="#a4573a"/>{herb_bunch(wx+76, wy+wh-150, r, 40)}')
    # curtains
    b.append(f'<path d="M{wx-30},{wy-30} C{wx+10},{wy+120} {wx-10},{wy+260} {wx+20},{wy+wh+10} L{wx-40},{wy+wh+10} L{wx-40},{wy-30} Z" fill="#7a3d2c" opacity=".92"/>'
             f'<path d="M{wx+ww+30},{wy-30} C{wx+ww-10},{wy+120} {wx+ww+10},{wy+260} {wx+ww-20},{wy+wh+10} L{wx+ww+40},{wy+wh+10} L{wx+ww+40},{wy-30} Z" fill="#7a3d2c" opacity=".92"/>'
             f'<rect x="{wx-60}" y="{wy-40}" width="{ww+120}" height="12" rx="6" fill="#3a271a"/>')
    # ---- center back door
    dx, dy, dw, dh = 860, 250, 270, 590
    b.append(f'<rect x="{dx-26}" y="{dy-26}" width="{dw+52}" height="{dh+26}" fill="#2a1d14" filter="url(#sh)"/>')
    b.append(planks_v(dx, dx + dw, dy, dy + dh, rng(5), base=(98, 66, 42), wmin=60, wmax=72))
    b.append(f'<rect x="{dx+40}" y="{dy+40}" width="{dw-80}" height="150" fill="url(#sky)"/>'
             f'<rect x="{dx+40}" y="{dy+150}" width="{dw-80}" height="40" fill="#c49a6c"/>'
             f'<rect x="{dx+dw/2-5}" y="{dy+40}" width="10" height="150" fill="#3a281b"/>'
             f'<rect x="{dx+40}" y="{dy+40}" width="{dw-80}" height="150" fill="none" stroke="#3a281b" stroke-width="10"/>')
    for yy in (dy + 260, dy + 470):
        b.append(f'<rect x="{dx+10}" y="{yy}" width="{dw-20}" height="18" fill="#3c2919"/>')
    b.append(f'<circle cx="{dx+dw-36}" cy="{dy+330}" r="11" fill="url(#brass)"/>')
    # bell
    b.append(f'<path d="M{dx+dw+30},{dy-20} h40 v10 h-24 v14" stroke="#2a1d14" stroke-width="6" fill="none"/>'
             f'<path d="M{dx+dw+46},{dy+4} c-18,2 -20,20 -22,38 h44 c-2,-18 -4,-36 -22,-38 Z" fill="url(#brass)"/>'
             f'<circle cx="{dx+dw+46}" cy="{dy+46}" r="5" fill="#8a6526"/>')
    # calendar / notice on wall left of door
    b.append(f'<g transform="rotate(-2 700 330)"><rect x="640" y="250" width="120" height="160" fill="{PAPER}" filter="url(#sh)"/>'
             f'<rect x="640" y="250" width="120" height="34" fill="{RED}"/>' +
             "".join(f'<rect x="{652+(i%5)*21}" y="{298+(i//5)*20}" width="14" height="12" fill="{MUTED}" opacity=".45"/>' for i in range(25)) +
             f'<circle cx="700" cy="244" r="6" fill="#7d5a3e"/></g>')
    # hanging lantern above center-left
    b.append(f'<line x1="640" y1="78" x2="640" y2="150" stroke="#1d140e" stroke-width="3"/>'
             f'<circle cx="640" cy="196" r="90" fill="url(#glow)"/>'
             f'<path d="M616,150 h48 l-6,14 h-36 Z" fill="#2a1d14"/><rect x="618" y="164" width="44" height="58" rx="6" fill="#ffd27a" opacity=".9"/>'
             f'<rect x="618" y="164" width="44" height="58" rx="6" fill="none" stroke="#2a1d14" stroke-width="5"/><line x1="640" y1="164" x2="640" y2="222" stroke="#2a1d14" stroke-width="3"/>'
             f'<path d="M612,222 h56 l-8,10 h-40 Z" fill="#2a1d14"/>')
    # ---- left shelving
    sx0, sx1 = 40, 560
    b.append(f'<rect x="{sx0-10}" y="140" width="{sx1-sx0+20}" height="700" fill="#33241a" filter="url(#sh)"/>')
    b.append(planks_v(sx0, sx1, 150, 840, rng(8), base=(70, 48, 32), var=8))
    for i, sy in enumerate((310, 490, 670)):
        b.append(f'<rect x="{sx0-14}" y="{sy}" width="{sx1-sx0+28}" height="20" fill="#8a6440"/><rect x="{sx0-14}" y="{sy+20}" width="{sx1-sx0+28}" height="8" fill="#2a1d14" opacity=".7"/>')
    # top shelf: jars + onions
    for i, jx in enumerate(range(70, 300, 66)):
        hh = r.randint(70, 110); c = r.choice(["#b88a4a", "#8fa07a", "#c9a46c", "#9f6c55"])
        b.append(f'<rect x="{jx}" y="{310-hh}" width="50" height="{hh}" rx="8" fill="{c}" opacity=".9"/>'
                 f'<rect x="{jx+4}" y="{310-hh-10}" width="42" height="14" rx="3" fill="#5a3f2b"/>'
                 f'<rect x="{jx+8}" y="{310-hh+14}" width="8" height="{hh-30}" rx="4" fill="#fff" opacity=".22"/>'
                 f'<rect x="{jx+8}" y="{310-hh*0.55}" width="34" height="24" fill="{PAPER}" opacity=".85"/>')
    for k in range(7):
        b.append(onion(360 + k * 26 + r.uniform(-4, 4), 290 - (k % 2) * 14, 16))
    for k in range(4):
        b.append(onion(470 + k * 22, 262 + (k % 2) * 10, 14, "#b07a4a"))
    # middle shelf: baskets of carrots
    for bx in (70, 310):
        for k in range(9):
            b.append(carrot(bx + 20 + k * 22 + r.uniform(-4, 4), 418 + r.uniform(-8, 6), .9, r.uniform(-30, 30)))
        b.append(basket(bx, 430, 210, 60))
    # bottom shelf: sacks and cabbages
    for k, (cx, rr) in enumerate([(110, 46), (200, 40), (280, 44)]):
        b.append(f'<circle cx="{cx}" cy="{670-rr}" r="{rr}" fill="{OLIVE}"/><path d="M{cx-rr*.8},{670-rr} Q{cx},{670-rr*2.1} {cx+rr*.8},{670-rr}" stroke="{OLIVE_D}" stroke-width="5" fill="none"/>'
                 f'<circle cx="{cx-rr*.3}" cy="{670-rr*1.3}" r="{rr*.35}" fill="#9cad6c" opacity=".7"/>')
    b.append(f'<path d="M360,670 C350,600 370,560 410,548 L500,548 C540,560 556,600 548,670 Z" fill="#c7ad7e"/>'
             f'<path d="M392,552 Q455,520 516,552" stroke="#8e7650" stroke-width="6" fill="none"/>'
             f'<path d="M380,600 Q450,590 530,604" stroke="#a88f63" stroke-width="3" fill="none" opacity=".7"/>')
    # herbs + garlic hanging from beam
    for hx in (180, 300, 420):
        b.append(herb_bunch(hx, 80, r, 90))
    b.append(garlic_string(1810, 80, 6, r))
    b.append(herb_bunch(1290, 80, r, 80))
    # bench + sacks under window
    b.append(f'<rect x="1360" y="720" width="460" height="22" fill="#6e4d32"/><rect x="1380" y="742" width="20" height="100" fill="#4a3324"/><rect x="1780" y="742" width="20" height="100" fill="#4a3324"/>')
    b.append(f'<path d="M1400,720 C1390,650 1410,610 1450,600 L1540,600 C1580,610 1596,650 1588,720 Z" fill="#c7ad7e"/>'
             f'<path d="M1610,720 C1600,660 1615,630 1650,622 L1720,622 C1752,630 1764,662 1758,720 Z" fill="#b89c6c"/>')
    for k in range(6):
        b.append(f'<ellipse cx="{1440+k*18}" cy="{600-(k%2)*8}" rx="16" ry="12" fill="#a67f52"/>')
    # light shafts from window
    b.append(f'<g filter="url(#b30)"><path d="M{wx},{wy} L{wx+ww},{wy} L{wx+ww-420},{H} L{wx-760},{H} Z" fill="url(#shaft)"/></g>')
    # ---- counter
    cy = 832
    b.append(f'<rect x="0" y="{cy}" width="{W}" height="{H-cy}" fill="#3f2a1b"/>')
    b.append(planks_h(0, W, cy + 50, H, rng(21), base=(96, 64, 40)))
    b.append(f'<rect x="0" y="{cy}" width="{W}" height="50" fill="url(#ctop)"/><rect x="0" y="{cy}" width="{W}" height="4" fill="#d6ad78" opacity=".7"/>'
             f'<rect x="0" y="{cy+50}" width="{W}" height="12" fill="#1d130c" opacity=".7"/>')
    # scale on counter (left)
    b.append(f'<g filter="url(#sh)"><rect x="170" y="{cy-16}" width="140" height="22" rx="4" fill="url(#brass)"/>'
             f'<rect x="234" y="{cy-170}" width="12" height="160" fill="url(#brass)"/>'
             f'<rect x="120" y="{cy-176}" width="240" height="10" rx="5" fill="url(#brass)"/>'
             f'<path d="M126,{cy-170} l-30,70 h72 Z M354,{cy-170} l-30,70 h72 Z" fill="none" stroke="#8a6526" stroke-width="2"/>'
             f'<path d="M86,{cy-100} h100 q-10,26 -50,26 q-40,0 -50,-26 Z M314,{cy-100} h100 q-10,26 -50,26 q-40,0 -50,-26 Z" fill="url(#brass)"/></g>')
    b.append(carrot(140, cy - 128, .7, -80, True) + carrot(352, cy - 124, .7, 80, True))
    # crate of carrots (right)
    b.append(f'<g filter="url(#sh)">')
    for k in range(13):
        b.append(carrot(1520 + k * 22 + r.uniform(-5, 5), cy - 150 + r.uniform(-10, 8), 1.05, r.uniform(-25, 25)))
    b.append(f'<rect x="1500" y="{cy-110}" width="320" height="114" fill="#9a7148"/>'
             f'<rect x="1500" y="{cy-110}" width="320" height="12" fill="#c49a68"/><rect x="1500" y="{cy-56}" width="320" height="8" fill="#6e4d32"/>'
             f'<rect x="1500" y="{cy-110}" width="14" height="114" fill="#6e4d32"/><rect x="1806" y="{cy-110}" width="14" height="114" fill="#6e4d32"/>'
             f'<rect x="1600" y="{cy-90}" width="120" height="30" fill="{PAPER}" opacity=".85"/></g>')
    # coins + ledger
    b.append(f'<g filter="url(#sh)"><rect x="1250" y="{cy-30}" width="170" height="30" rx="3" fill="#5a2e22"/><rect x="1256" y="{cy-34}" width="160" height="10" fill="{PAPER}"/></g>')
    for k in range(5):
        b.append(f'<ellipse cx="{1180}" cy="{cy-6-k*7}" rx="22" ry="8" fill="url(#brass)" stroke="#7a5a22" stroke-width="1.5"/>')
    b.append(f'<ellipse cx="1130" cy="{cy-4}" rx="22" ry="8" fill="url(#brass)" stroke="#7a5a22" stroke-width="1.5"/>')
    # atmosphere
    b.append(f'<rect width="{W}" height="{H}" fill="#ffb46b" opacity=".06"/>')
    b.append(f'<rect width="{W}" height="{H}" fill="url(#vig)"/>')
    return svg(W, H, f'<g filter="url(#g)">{"".join(b)}</g>', defs)


# ---------------------------------------------------------------- BOARD (village square, morning)
def timber_house(x, base, w, h, roof, wall, r, lit=False, door=True):
    out = [f'<rect x="{x}" y="{base-h}" width="{w}" height="{h}" fill="{wall}"/>']
    # half-timbering
    tc = "#4a3324"
    out.append(f'<rect x="{x}" y="{base-h}" width="{w}" height="{h}" fill="none" stroke="{tc}" stroke-width="10"/>')
    out.append(f'<rect x="{x}" y="{base-h*0.55}" width="{w}" height="9" fill="{tc}"/>')
    for k in range(1, 4):
        xx = x + w * k / 4
        out.append(f'<rect x="{xx-4}" y="{base-h}" width="8" height="{h}" fill="{tc}"/>')
    out.append(f'<path d="M{x},{base-h*0.55} L{x+w/4},{base-h}" stroke="{tc}" stroke-width="7"/><path d="M{x+w},{base-h*0.55} L{x+w*3/4},{base-h}" stroke="{tc}" stroke-width="7"/>')
    for k in range(4):
        wx = x + w * (k + .5) / 4 - 16
        win = "#f2cf7e" if lit else "#4b5a63"
        out.append(f'<rect x="{wx}" y="{base-h*0.88}" width="32" height="38" fill="{win}"/><rect x="{wx+14}" y="{base-h*0.88}" width="4" height="38" fill="{tc}"/>')
    if door:
        out.append(f'<rect x="{x+w*0.4}" y="{base-h*0.42}" width="{w*0.2}" height="{h*0.42}" fill="#5a3b26"/><path d="M{x+w*0.4},{base-h*0.42} q{w*0.1},-24 {w*0.2},0" fill="#5a3b26"/>')
    out.append(f'<path d="M{x-24},{base-h+4} L{x+w/2},{base-h-roof} L{x+w+24},{base-h+4} Z" fill="#6b3f2c"/>')
    for k in range(5):
        yy = base - h + 4 - roof * (k + 1) / 6
        half = (w / 2 + 24) * (1 - (k + 1) / 6)
        out.append(f'<line x1="{x+w/2-half:.0f}" y1="{yy:.0f}" x2="{x+w/2+half:.0f}" y2="{yy:.0f}" stroke="#4e2d1f" stroke-width="3" opacity=".6"/>')
    out.append(f'<rect x="{x+w*0.68}" y="{base-h-roof*0.75}" width="26" height="{roof*0.5}" fill="#6b5a4c"/>')
    return "".join(out)


def tree(x, base, s, r, c1=OLIVE, c2=OLIVE_D, trunk="#4a3324"):
    out = [f'<path d="M{x-14*s},{base} C{x-8*s},{base-60*s} {x-10*s},{base-110*s} {x-4*s},{base-150*s} L{x+6*s},{base-150*s} C{x+10*s},{base-100*s} {x+8*s},{base-60*s} {x+16*s},{base} Z" fill="{trunk}"/>',
           f'<path d="M{x},{base-110*s} L{x-50*s},{base-170*s} M{x},{base-120*s} L{x+56*s},{base-180*s}" stroke="{trunk}" stroke-width="{8*s}"/>']
    blobs = []
    for k in range(26):
        a = r.uniform(0, math.tau); d = r.uniform(0, 80 * s)
        blobs.append((x + math.cos(a) * d * 1.15, base - 200 * s + math.sin(a) * d * .75, r.uniform(30, 50) * s))
    for cx, cy, rr in blobs:
        out.append(f'<circle cx="{cx:.0f}" cy="{cy+rr*.25:.0f}" r="{rr:.0f}" fill="{c2}"/>')
    for cx, cy, rr in blobs:
        out.append(f'<circle cx="{cx-rr*.15:.0f}" cy="{cy-rr*.1:.0f}" r="{rr*.8:.0f}" fill="{c1}"/>')
    for cx, cy, rr in blobs[:12]:
        out.append(f'<circle cx="{cx-rr*.35:.0f}" cy="{cy-rr*.4:.0f}" r="{rr*.4:.0f}" fill="#a3ad74" opacity=".55"/>')
    return "".join(out)


def bg_board():
    r = rng(23)
    defs = (vignette(strength=.45) + grain("g", .85, .07) + blur("b20", 20) + blur("b6", 6) + shadow("sh", 0, 12, 10, .4)
            + lin("sky", [(0, "#c9d6dc", 1), (.55, "#ecdcbf", 1), (1, "#f2dcb0", 1)])
            + lin("mist", [(0, "#f4ead6", 0), (.5, "#f4ead6", .75), (1, "#f4ead6", 0)])
            + lin("ground", [(0, "#8c7860", 1), (1, "#5d4b3a", 1)])
            + rad("sun", [(0, "#fff3d0", .9), (.4, "#ffe2a8", .35), (1, "#ffe2a8", 0)], .5, .5, .5))
    b = [f'<rect width="{W}" height="{H}" fill="url(#sky)"/>',
         f'<circle cx="1500" cy="250" r="420" fill="url(#sun)"/>']
    # far hills + far rooftops in haze
    b.append(f'<path d="M0,560 C300,500 600,520 900,490 C1200,460 1500,500 1920,470 L1920,700 L0,700 Z" fill="#b7b9a2" opacity=".75"/>')
    for k in range(14):
        x = k * 145 + r.uniform(-20, 20); h = r.uniform(60, 120)
        b.append(f'<rect x="{x}" y="{600-h}" width="110" height="{h+60}" fill="#a9a48e"/><path d="M{x-12},{600-h} L{x+55},{600-h-50} L{x+122},{600-h} Z" fill="#9b8f7a"/>')
    b.append(f'<path d="M760,600 L760,380 L790,340 L820,380 L820,600 Z" fill="#9f9884"/><path d="M770,345 L790,250 L810,345 Z" fill="#8f8775"/>')
    b.append(f'<rect x="0" y="470" width="{W}" height="220" fill="url(#mist)"/>')
    # mid houses left & right
    b.append(timber_house(-60, 760, 360, 300, 150, "#e3d3b4", r))
    b.append(timber_house(330, 740, 260, 230, 120, "#d9c6a2", r))
    b.append(timber_house(1380, 745, 280, 250, 130, "#e0cfae", r))
    b.append(timber_house(1660, 770, 340, 320, 160, "#d6c19c", r))
    b.append(f'<rect x="0" y="560" width="{W}" height="200" fill="url(#mist)" opacity=".45"/>')
    # ground / cobbles
    b.append(f'<rect x="0" y="730" width="{W}" height="{H-730}" fill="url(#ground)"/>')
    rows = 16
    for i in range(rows):
        t = i / rows; y = 740 + (t ** 1.6) * 360; hh = 6 + t * 26; ww = 24 + t * 70
        x = -r.uniform(0, ww)
        while x < W:
            c = r.choice(["#9b8870", "#8a7760", "#a8957a", "#7c6a55"])
            b.append(f'<rect x="{x:.0f}" y="{y:.0f}" width="{ww*r.uniform(.8,1.1):.0f}" height="{hh:.0f}" rx="{hh*.45:.0f}" fill="{c}" stroke="#4e3f31" stroke-width="{1+t*2:.1f}" opacity=".95"/>')
            x += ww * 1.08
    # well (left)
    b.append(f'<g filter="url(#sh)"><ellipse cx="370" cy="900" rx="150" ry="40" fill="#5b5a52"/>'
             f'<rect x="220" y="800" width="300" height="100" fill="#8f8a7a"/>' +
             "".join(f'<rect x="{220+(k%6)*50+(25 if (k//6)%2 else 0)}" y="{800+(k//6)*25}" width="48" height="23" rx="4" fill="{r.choice(["#a49e8a","#8b8573","#9a947f"])}" stroke="#5b564a" stroke-width="2"/>' for k in range(24)) +
             f'<ellipse cx="370" cy="800" rx="150" ry="34" fill="#6f6a5c"/><ellipse cx="370" cy="802" rx="124" ry="24" fill="#2b2a26"/>'
             f'<rect x="236" y="630" width="18" height="180" fill="#4a3324"/><rect x="486" y="630" width="18" height="180" fill="#4a3324"/>'
             f'<path d="M210,650 L370,560 L530,650 Z" fill="#6b3f2c"/><rect x="250" y="680" width="240" height="10" fill="#4a3324"/>'
             f'<line x1="370" y1="690" x2="370" y2="770" stroke="#7a6a50" stroke-width="3"/><rect x="350" y="768" width="40" height="34" rx="4" fill="#7a5a3a"/></g>')
    # tree right
    b.append(tree(1540, 860, 1.6, r))
    # NOTICE BOARD (center, empty)
    bx, by, bw, bh = 640, 330, 640, 470
    b.append(f'<g filter="url(#sh)">'
             f'<rect x="{bx+40}" y="{by+bh-10}" width="30" height="{960-by-bh+10}" fill="#4a3324"/><rect x="{bx+bw-70}" y="{by+bh-10}" width="30" height="{960-by-bh+10}" fill="#4a3324"/>'
             f'<path d="M{bx-50},{by+10} L{bx+bw/2},{by-110} L{bx+bw+50},{by+10} L{bx+bw+30},{by+30} L{bx+bw/2},{by-80} L{bx-30},{by+30} Z" fill="#5b3424"/>'
             f'<rect x="{bx-20}" y="{by}" width="{bw+40}" height="{bh+20}" fill="#5e412b"/>'
             f'<rect x="{bx}" y="{by+20}" width="{bw}" height="{bh-20}" fill="#b08a5c"/></g>')
    b.append(planks_v(bx, bx + bw, by + 20, by + bh, rng(31), base=(168, 128, 86), var=10, wmin=70, wmax=90, grain_op=.14))
    b.append(f'<rect x="{bx}" y="{by+20}" width="{bw}" height="{bh-20}" fill="none" stroke="#3e2a1b" stroke-width="10"/>')
    for (px, py) in [(bx+120, by+80), (bx+430, by+120), (bx+250, by+300), (bx+520, by+360)]:
        b.append(f'<circle cx="{px}" cy="{py}" r="4" fill="#3e2a1b" opacity=".6"/>')  # old pin holes
    b.append(f'<rect x="{bx+bw/2-120}" y="{by-40}" width="240" height="44" rx="4" fill="#3e2a1b"/>'
             f'<text x="{bx+bw/2}" y="{by-8}" text-anchor="middle" font-family="Lora" font-weight="700" font-size="26" fill="#e8d6ad" letter-spacing="4">SELLOWE</text>')
    # morning light + mist foreground
    b.append(f'<g filter="url(#b20)"><path d="M1500,0 L1920,0 L1100,1080 L500,1080 Z" fill="#fff1cf" opacity=".12"/></g>')
    b.append(f'<rect x="0" y="880" width="{W}" height="200" fill="url(#mist)" opacity=".35"/>')
    b.append(f'<rect width="{W}" height="{H}" fill="url(#vig)"/>')
    return svg(W, H, f'<g filter="url(#g)">{"".join(b)}</g>', defs)


# ---------------------------------------------------------------- NEWS (kitchen table top-down, morning)
def table_top(r, base=(132, 94, 62), dark=False):
    out = []
    y = -20
    while y < H + 20:
        h = r.randint(150, 190); d = r.randint(-12, 12)
        out.append(f'<rect x="0" y="{y}" width="{W}" height="{h}" fill="rgb({base[0]+d},{base[1]+int(d*.7)},{base[2]+int(d*.5)})"/>')
        for _ in range(9):
            gy = y + r.uniform(8, h - 8); a = r.uniform(-10, 10)
            out.append(f'<path d="M0,{gy:.0f} C{W*.25:.0f},{gy+a:.0f} {W*.6:.0f},{gy-a:.0f} {W},{gy+a*.4:.0f}" stroke="#2b1d13" stroke-width="{r.uniform(.8,2.2):.1f}" fill="none" opacity="{r.uniform(.08,.2):.2f}"/>')
        if r.random() < .5:
            kx = r.uniform(100, W - 100); ky = y + h * r.uniform(.3, .7)
            out.append(f'<ellipse cx="{kx:.0f}" cy="{ky:.0f}" rx="26" ry="10" fill="none" stroke="#2b1d13" stroke-width="2" opacity=".25"/>'
                       f'<ellipse cx="{kx:.0f}" cy="{ky:.0f}" rx="10" ry="4" fill="#2b1d13" opacity=".3"/>')
        out.append(f'<rect x="0" y="{y+h-3}" width="{W}" height="4" fill="#1d130c" opacity=".75"/><rect x="0" y="{y+h+1}" width="{W}" height="2" fill="#fff" opacity=".06"/>')
        y += h
    return "".join(out)


def cup_top(x, y, s=1, tea="#9a5a2a"):
    return (f'<g transform="translate({x},{y}) scale({s})" filter="url(#sh)">'
            f'<circle r="120" fill="#efe6d4"/><circle r="104" fill="#e2d6bf"/><circle r="76" fill="#f6efe2"/>'
            f'<circle r="62" fill="{tea}"/><circle r="62" fill="url(#teaShine)"/>'
            f'<path d="M74,-18 C120,-30 128,30 74,22" stroke="#f6efe2" stroke-width="16" fill="none"/>'
            f'<circle r="76" fill="none" stroke="#c8b99e" stroke-width="3"/></g>')


def candle_top(x, y, s=1, lit=False):
    out = (f'<g transform="translate({x},{y}) scale({s})">'
           f'<circle r="64" fill="#8a6a3c" filter="url(#sh)"/><circle r="56" fill="#b08a4c"/><circle r="50" fill="#9c7a42"/>'
           f'<path d="M58,0 h40 a14,14 0 0 1 0,28 h-34" fill="none" stroke="#9c7a42" stroke-width="10"/>'
           f'<circle r="30" fill="#efe3c4"/><circle r="30" fill="none" stroke="#d6c6a0" stroke-width="4"/>'
           f'<path d="M-30,6 q-10,14 -4,26 q6,-10 8,-22 Z" fill="#efe3c4"/><circle r="4" fill="#2a1d14"/>')
    if lit:
        out += '<circle r="22" fill="#ffcf6a" opacity=".85"/><circle r="10" fill="#fff6d6"/>'
    return out + '</g>'


def bg_news():
    r = rng(41)
    defs = (vignette(strength=.6) + grain("g", .85, .08) + blur("b40", 40) + shadow("sh", 10, 16, 12, .45)
            + rad("teaShine", [(0, "#fff", .0), (.7, "#fff", 0), (.85, "#fff", .18), (1, "#fff", 0)], .4, .4, .6)
            + lin("win", [(0, "#ffefc8", .55), (1, "#ffefc8", .0)], 0, 0, 1, 1))
    b = [table_top(r)]
    # window light patches (top-left → slanted)
    b.append('<g filter="url(#b40)">' + "".join(
        f'<path d="M{x},{y} l300,-40 l60,240 l-300,40 Z" fill="url(#win)"/>' for x, y in [(160, 140), (500, 90), (220, 440), (560, 390)]) + '</g>')
    b.append(cup_top(1650, 260))
    b.append(candle_top(260, 860, 1.0))
    # carrots bunch bottom-right
    for k in range(5):
        b.append(f'<g filter="url(#sh)">{carrot(1580+k*40, 760+k*8, 1.6, -60+k*6)}</g>')
    # bread half top-left
    b.append(f'<g filter="url(#sh)"><ellipse cx="230" cy="190" rx="170" ry="96" fill="#c88c46"/><ellipse cx="230" cy="190" rx="150" ry="80" fill="#d9a35c"/>' +
             "".join(f'<path d="M{150+k*55},130 q20,60 0,120" stroke="#a96c30" stroke-width="8" fill="none" opacity=".7"/>' for k in range(3)) + '</g>')
    # a cloth napkin + spoon
    b.append(f'<g filter="url(#sh)" transform="rotate(14 1700 980)"><rect x="1560" y="920" width="280" height="200" fill="#c8bba0"/>' +
             "".join(f'<line x1="1560" y1="{930+k*24}" x2="1840" y2="{930+k*24}" stroke="{RED}" stroke-width="5" opacity=".5"/>' for k in range(8)) + '</g>')
    b.append(f'<g filter="url(#sh)" transform="rotate(-30 1480 300)"><ellipse cx="1480" cy="300" rx="18" ry="30" fill="#b9b2a2"/><rect x="1476" y="326" width="8" height="120" rx="4" fill="#b9b2a2"/></g>')
    b.append(f'<rect width="{W}" height="{H}" fill="#ffcf8a" opacity=".05"/><rect width="{W}" height="{H}" fill="url(#vig)"/>')
    return svg(W, H, f'<g filter="url(#g)">{"".join(b)}</g>', defs)


# ---------------------------------------------------------------- NOTEBOOK desk at night
def bg_notebook():
    r = rng(51)
    defs = (vignette(strength=.85) + grain("g", .85, .08) + shadow("sh", 10, 16, 12, .55)
            + rad("cglow", [(0, "#ffcf7a", .55), (.35, "#e8963e", .22), (1, "#e8963e", 0)], .5, .5, .5)
            + rad("teaShine", [(0, "#fff", 0), (1, "#fff", 0)])
            + lin("leather", [(0, "#6b3524", 1), (1, "#4a2219", 1)], 0, 0, 1, 1))
    b = [table_top(r, base=(82, 56, 38)), f'<rect width="{W}" height="{H}" fill="#140c08" opacity=".35"/>']
    b.append(f'<circle cx="250" cy="300" r="900" fill="url(#cglow)"/>')
    b.append(candle_top(250, 300, 1.35, lit=True))
    # ink pot + quill
    b.append(f'<g filter="url(#sh)"><circle cx="260" cy="760" r="70" fill="#1f2a33"/><circle cx="260" cy="760" r="48" fill="#2f3d48"/><circle cx="260" cy="760" r="30" fill="#0a0d10"/>'
             f'<circle cx="244" cy="742" r="10" fill="#fff" opacity=".25"/></g>')
    b.append(f'<g filter="url(#sh)" transform="rotate(-38 360 620)"><path d="M360,620 C380,500 420,420 470,360 C440,460 410,540 368,626 Z" fill="#e9e0cd"/>'
             f'<path d="M362,620 C390,520 430,430 470,360" stroke="#b8ab92" stroke-width="2" fill="none"/>' +
             "".join(f'<path d="M{380+k*8},{560-k*18} l-22,-6" stroke="#cfc4ad" stroke-width="2"/>' for k in range(9)) +
             f'<path d="M360,620 l-6,46 l10,-44 Z" fill="#2a1d14"/></g>')
    # closed leather notebook pushed right
    b.append(f'<g filter="url(#sh)" transform="rotate(8 1640 620)"><rect x="1440" y="380" width="420" height="540" rx="14" fill="url(#leather)"/>'
             f'<rect x="1440" y="380" width="40" height="540" rx="10" fill="#3a1912"/>'
             f'<rect x="1500" y="420" width="330" height="460" rx="8" fill="none" stroke="#8a4a32" stroke-width="3" stroke-dasharray="8 6"/>'
             f'<rect x="1846" y="380" width="14" height="540" fill="#e8dcc0" opacity=".85"/>'
             f'<path d="M1840,600 h90 a16,16 0 0 1 0,32 h-90 Z" fill="#3a1912"/><circle cx="1918" cy="616" r="8" fill="#c49a3e"/></g>')
    # loose papers + wax
    b.append(f'<g filter="url(#sh)" transform="rotate(-6 1500 180)"><rect x="1320" y="80" width="360" height="220" fill="#d8ccb0" opacity=".9"/>' +
             "".join(f'<line x1="1350" y1="{120+k*26}" x2="{1640-r.randint(0,90)}" y2="{120+k*26}" stroke="#6b5a48" stroke-width="3" opacity=".35"/>' for k in range(6)) + '</g>')
    b.append(f'<circle cx="1260" cy="930" r="34" fill="#7d2a20" filter="url(#sh)"/><circle cx="1260" cy="930" r="22" fill="#9a3a2a"/>')
    b.append(f'<rect width="{W}" height="{H}" fill="url(#vig)"/>')
    return svg(W, H, f'<g filter="url(#g)">{"".join(b)}</g>', defs)


# ---------------------------------------------------------------- NIGHT village from hill
def bg_night():
    r = rng(61)
    defs = (vignette(strength=.7) + grain("g", .9, .06) + blur("b10", 10) + blur("b4", 4) + blur("b30", 30)
            + lin("sky", [(0, "#0f1830", 1), (.55, "#22324f", 1), (1, "#3f4d66", 1)])
            + rad("moon", [(0, "#f6efd2", .55), (.25, "#cfd6e6", .18), (1, "#cfd6e6", 0)], .5, .5, .5)
            + rad("lamp", [(0, "#ffcf6a", .9), (.3, "#ffb04a", .35), (1, "#ffb04a", 0)], .5, .5, .5)
            + lin("river", [(0, "#2c3c58", 1), (1, "#1a2438", 1)])
            + lin("fog", [(0, "#8d9ab3", 0), (.5, "#8d9ab3", .22), (1, "#8d9ab3", 0)]))
    b = [f'<rect width="{W}" height="{H}" fill="url(#sky)"/>']
    for _ in range(260):
        x = r.uniform(0, W); y = r.uniform(0, 560) ** 1.0; s = r.choice([1, 1, 1, 1.5, 2])
        b.append(f'<circle cx="{x:.0f}" cy="{y*.9:.0f}" r="{s}" fill="#f2ecd8" opacity="{r.uniform(.25,.9):.2f}"/>')
    b.append(f'<circle cx="1480" cy="200" r="260" fill="url(#moon)"/><circle cx="1480" cy="200" r="54" fill="#efe8cf"/>'
             f'<circle cx="1462" cy="186" r="12" fill="#d8d0b4" opacity=".6"/><circle cx="1500" cy="220" r="8" fill="#d8d0b4" opacity=".6"/>')
    # far hills
    b.append(f'<path d="M0,560 C260,500 520,540 780,500 C1060,456 1340,520 1600,488 C1760,470 1860,486 1920,480 L1920,760 L0,760 Z" fill="#26324a"/>')
    b.append(f'<path d="M0,620 C300,590 600,610 900,585 C1200,560 1500,600 1920,580 L1920,760 L0,760 Z" fill="#1d2840"/>')
    # river through valley
    b.append(f'<path d="M-20,760 C300,720 700,760 1000,720 C1300,690 1600,700 1940,680 L1940,770 C1600,780 1300,770 1000,800 C700,830 300,800 -20,840 Z" fill="url(#river)"/>')
    b.append(f'<g filter="url(#b4)">' + "".join(f'<rect x="{1440+r.uniform(-60,60):.0f}" y="{700+k*14}" width="{r.uniform(30,90):.0f}" height="3" fill="#cfd6e6" opacity=".45"/>' for k in range(7)) + '</g>')
    # stone bridge (north/right)
    b.append(f'<path d="M1180,700 L1500,690 L1500,720 Q1440,690 1380,722 Q1320,694 1260,724 Q1220,700 1180,728 Z" fill="#3a4258"/>'
             f'<rect x="1176" y="684" width="328" height="12" fill="#4a5470"/>' +
             "".join(f'<rect x="{1190+k*40}" y="672" width="8" height="14" fill="#4a5470"/>' for k in range(8)))
    # village silhouettes
    houses = []
    x = 120
    while x < 1520:
        w = r.randint(70, 130); h = r.randint(60, 120); base = 700 + r.randint(-6, 10)
        houses.append((x, base, w, h)); x += w + r.randint(4, 30)
    for (x, base, w, h) in houses:
        b.append(f'<rect x="{x}" y="{base-h}" width="{w}" height="{h}" fill="#141b2c"/><path d="M{x-10},{base-h} L{x+w/2},{base-h-r.randint(36,60)} L{x+w+10},{base-h} Z" fill="#10172a"/>')
        for k in range(r.randint(0, 2)):
            wx = x + r.uniform(12, w - 30); wy = base - h + r.uniform(18, h - 30)
            b.append(f'<rect x="{wx:.0f}" y="{wy:.0f}" width="14" height="16" fill="#f2b552" opacity=".9"/>'
                     f'<circle cx="{wx+7:.0f}" cy="{wy+8:.0f}" r="40" fill="url(#lamp)" opacity=".45"/>')
    # church tower
    b.append(f'<rect x="560" y="470" width="70" height="240" fill="#121a2c"/><path d="M550,474 L595,340 L640,474 Z" fill="#0f1628"/>'
             f'<rect x="590" y="300" width="10" height="44" fill="#0f1628"/><rect x="580" y="312" width="30" height="8" fill="#0f1628"/>'
             f'<path d="M580,520 q15,-22 30,0 v30 h-30 Z" fill="#f2b552" opacity=".8"/>')
    # market lanterns (center)
    for lx in (860, 920, 980):
        b.append(f'<circle cx="{lx}" cy="660" r="30" fill="url(#lamp)"/><circle cx="{lx}" cy="660" r="4" fill="#ffe2a0"/>')
    b.append(f'<rect x="0" y="600" width="{W}" height="200" fill="url(#fog)"/>')
    # foreground hill
    b.append(f'<path d="M0,850 C300,810 700,830 1000,860 C1300,890 1600,860 1920,830 L1920,1080 L0,1080 Z" fill="#0e1424"/>')
    for k in range(160):
        gx = r.uniform(0, W); gy = 860 + r.uniform(0, 220)
        b.append(f'<path d="M{gx:.0f},{gy:.0f} q{r.uniform(-6,6):.0f},-{r.uniform(10,26):.0f} {r.uniform(-4,10):.0f},-{r.uniform(18,34):.0f}" stroke="#1c2538" stroke-width="2" fill="none"/>')
    # big oak (east, right edge) with the dead-drop hollow
    oak = [f'<path d="M1690,1080 C1700,980 1720,880 1716,760 C1700,700 1650,650 1600,630 C1660,640 1710,670 1736,700 C1740,640 1730,590 1700,540 C1750,580 1770,640 1772,700 C1800,640 1850,600 1920,590 L1920,1080 Z" fill="#080c18"/>']
    for k in range(46):
        a = r.uniform(math.pi * .9, math.pi * 2.1); d = r.uniform(40, 300)
        cx = 1800 + math.cos(a) * d * 1.1; cy = 470 + math.sin(a) * d * .7
        if cx > 1520:
            oak.append(f'<circle cx="{cx:.0f}" cy="{cy:.0f}" r="{r.uniform(50,95):.0f}" fill="#080c18"/>')
    for k in range(18):
        a = r.uniform(math.pi * 1.1, math.pi * 1.6); d = r.uniform(120, 300)
        cx = 1800 + math.cos(a) * d * 1.1; cy = 470 + math.sin(a) * d * .7
        if cx > 1540:
            oak.append(f'<circle cx="{cx:.0f}" cy="{cy:.0f}" r="{r.uniform(18,34):.0f}" fill="#151d33" opacity=".55"/>')
    b.append("".join(oak))
    b.append(f'<ellipse cx="1760" cy="900" rx="26" ry="40" fill="#020308"/><path d="M1734,900 a26,40 0 0 1 52,0" stroke="#2a3552" stroke-width="3" fill="none"/>')
    b.append(f'<rect width="{W}" height="{H}" fill="url(#vig)"/>')
    return svg(W, H, f'<g filter="url(#g)">{"".join(b)}</g>', defs)


# ---------------------------------------------------------------- ENDING dawn over carrot fields
def bg_ending():
    r = rng(71)
    defs = (vignette(strength=.5) + grain("g", .85, .07) + blur("b30", 30)
            + lin("sky", [(0, "#8b97ab", 1), (.45, "#d6b7a6", 1), (.75, "#efcd9e", 1), (1, "#f3dcb4", 1)])
            + rad("sun", [(0, "#fff4d8", 1), (.15, "#ffe2a8", .7), (1, "#ffd59a", 0)], .5, .5, .5)
            + lin("fog", [(0, "#f1e2c8", 0), (.5, "#f1e2c8", .8), (1, "#f1e2c8", 0)])
            + lin("soil", [(0, "#8a6a52", 1), (1, "#4a3426", 1)]))
    b = [f'<rect width="{W}" height="{H}" fill="url(#sky)"/>',
         f'<circle cx="1180" cy="560" r="520" fill="url(#sun)"/><circle cx="1180" cy="560" r="46" fill="#fff6e0"/>']
    for k in range(6):
        y = 160 + k * 60
        b.append(f'<g filter="url(#b30)"><ellipse cx="{r.uniform(200,1700):.0f}" cy="{y}" rx="{r.uniform(220,420):.0f}" ry="26" fill="#f6e0c6" opacity=".45"/></g>')
    b.append(f'<path d="M0,560 C300,520 620,548 960,520 C1300,494 1620,530 1920,510 L1920,700 L0,700 Z" fill="#a6a0a2" opacity=".8"/>')
    for k in range(12):
        x = 300 + k * 90 + r.uniform(-20, 20); h = r.uniform(30, 70)
        b.append(f'<rect x="{x}" y="{590-h}" width="70" height="{h+20}" fill="#8f8a8e"/><path d="M{x-8},{590-h} L{x+35},{590-h-34} L{x+78},{590-h} Z" fill="#857d82"/>')
    b.append(f'<path d="M820,600 L820,450 L840,420 L860,450 L860,600 Z" fill="#8a8488"/><path d="M826,424 L840,360 L854,424 Z" fill="#7f797d"/>')
    b.append(f'<rect x="0" y="520" width="{W}" height="200" fill="url(#fog)"/>')
    # field rows converging to vanishing point (1180, 600)
    b.append(f'<path d="M0,640 L1920,630 L1920,1080 L0,1080 Z" fill="url(#soil)"/>')
    vx, vy = 1180, 610
    for k in range(-14, 15):
        x2 = vx + k * 260
        b.append(f'<path d="M{vx+k*6},{vy+30} L{x2-50},1080 L{x2+50},1080 Z" fill="#6b4e3a" opacity=".6"/>')
        # carrot tops along the row
        for t in [i / 16 for i in range(2, 16)]:
            px = vx + k * 6 + (x2 - vx - k * 6) * t; py = vy + 30 + (1080 - vy - 30) * t; s = .15 + t * 1.1
            if -60 < px < W + 60:
                c = r.choice([LEAF, OLIVE, LEAF_D])
                b.append(f'<path d="M{px:.0f},{py:.0f} c{-10*s:.0f},{-30*s:.0f} {-22*s:.0f},{-38*s:.0f} {-26*s:.0f},{-46*s:.0f} M{px:.0f},{py:.0f} c{2*s:.0f},{-34*s:.0f} {2*s:.0f},{-46*s:.0f} {6*s:.0f},{-58*s:.0f} M{px:.0f},{py:.0f} c{10*s:.0f},{-26*s:.0f} {22*s:.0f},{-34*s:.0f} {26*s:.0f},{-42*s:.0f}" stroke="{c}" stroke-width="{max(1,5*s):.1f}" fill="none" stroke-linecap="round"/>')
    # fence posts left
    for k in range(8):
        x = 40 + k * 120; y = 700 + k * 4; h = 160 - k * 12
        b.append(f'<rect x="{x}" y="{y-h}" width="{16-k}" height="{h}" fill="#4a3628"/>')
    b.append(f'<path d="M40,560 L880,640" stroke="#4a3628" stroke-width="5"/><path d="M40,630 L880,690" stroke="#4a3628" stroke-width="4"/>')
    b.append(f'<rect x="0" y="600" width="{W}" height="240" fill="url(#fog)" opacity=".7"/>')
    b.append(f'<rect width="{W}" height="{H}" fill="#e7b88c" opacity=".07"/><rect width="{W}" height="{H}" fill="url(#vig)"/>')
    return svg(W, H, f'<g filter="url(#g)">{"".join(b)}</g>', defs)


if __name__ == "__main__":
    import sys
    allf = {"bg_shop": bg_shop, "bg_board": bg_board, "bg_news": bg_news, "bg_notebook": bg_notebook, "bg_night": bg_night, "bg_ending": bg_ending}
    names = sys.argv[1:] or list(allf)
    render([(n, W, H, allf[n](), "Backgrounds") for n in names])
