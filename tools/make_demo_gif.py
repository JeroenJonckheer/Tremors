"""Renders docs/demo.gif: the same trembling hand, without Tremors (left) and with
Tremors (right). The right-hand cursor comes from the real filter, via tools/DemoTrace.

Usage:
    dotnet run --project tools/DemoTrace -- trace.csv
    python tools/make_demo_gif.py trace.csv docs/demo.gif
"""
import csv
import sys
from PIL import Image, ImageDraw, ImageFont

SCALE = 2                     # draw at 2x, scale down for smooth edges
PANEL_W, PANEL_H = 480, 300   # same coordinate space as DemoTrace
GAP, TOP, SIDE, BOTTOM = 24, 56, 24, 22
FRAME_MS = 40
CLICK_MS = 4600
END_MS = 5000
HOLD_LAST_MS = 1800
TRAIL_MS = 1000

NAVY = (28, 52, 78)
WINDOW = (250, 251, 252)
TITLE_BAR = (232, 236, 240)
TEXT = (60, 66, 74)
FAINT = (214, 219, 225)
CORAL = (226, 96, 80)
MINT = (60, 190, 140)
WHITE = (255, 255, 255)

CHECKBOXES = [(119, "Send me a copy"), (75, "Show notifications"), (163, "Large text")]
TARGET_Y = 118
BOX_X, BOX = 240, 22


def font(size, bold=False):
    name = "segoeuib.ttf" if bold else "segoeui.ttf"
    try:
        return ImageFont.truetype(f"C:/Windows/Fonts/{name}", size * SCALE)
    except OSError:
        return ImageFont.load_default()


TITLE_FONT, LABEL_FONT, HEADER_FONT = font(13), font(15), font(18, bold=True)
ARROW = [(0, 0), (0, 0.895), (0.211, 0.684), (0.368, 1.0), (0.474, 0.947), (0.316, 0.632), (0.632, 0.632)]


def s(v):
    return v * SCALE


def load(path):
    with open(path, newline="") as f:
        return [(int(r["time_ms"]), float(r["raw_x"]), float(r["raw_y"]), float(r["filtered_x"]), float(r["filtered_y"]))
                for r in csv.DictReader(f)]


def at(rows, ms):
    return min(rows, key=lambda r: abs(r[0] - ms))


def window(d, ox, oy, checked):
    d.rounded_rectangle([s(ox), s(oy), s(ox + PANEL_W), s(oy + PANEL_H)], radius=s(10), fill=WINDOW)
    d.rounded_rectangle([s(ox), s(oy), s(ox + PANEL_W), s(oy + 30)], radius=s(10), fill=TITLE_BAR)
    d.rectangle([s(ox), s(oy + 20), s(ox + PANEL_W), s(oy + 30)], fill=TITLE_BAR)
    d.text((s(ox + 14), s(oy + 7)), "Settings", font=TITLE_FONT, fill=TEXT)
    for y, label in CHECKBOXES:
        y0 = oy + y - BOX // 2 + (TARGET_Y - 119)
        x0 = ox + BOX_X
        is_target = y == 119
        fill = MINT if (is_target and checked) else WHITE
        d.rounded_rectangle([s(x0), s(y0), s(x0 + BOX), s(y0 + BOX)], radius=s(4), fill=fill,
                            outline=MINT if (is_target and checked) else (150, 158, 168), width=s(2))
        if is_target and checked:
            d.line([(s(x0 + 5), s(y0 + 11)), (s(x0 + 9), s(y0 + 16)), (s(x0 + 17), s(y0 + 6))], fill=WHITE, width=s(3))
        d.text((s(x0 + BOX + 12), s(y0 + 1)), label, font=LABEL_FONT, fill=TEXT)
    for i, w in enumerate((130, 90, 150)):  # placeholder text on the left
        d.rounded_rectangle([s(ox + 30), s(oy + 66 + i * 44), s(ox + 30 + w), s(oy + 76 + i * 44)], radius=s(5), fill=FAINT)


def pointer(d, x, y, ox, oy):
    h = 22
    pts = [(s(ox + x + px * h), s(oy + y + py * h)) for px, py in ARROW]
    d.polygon(pts, fill=WHITE)
    d.line(pts + [pts[0]], fill=(30, 30, 30), width=s(1) + 1, joint="curve")


def trail(d, points, ox, oy, color):
    for i in range(1, len(points)):
        alpha = 0.2 + 0.8 * i / len(points)
        c = tuple(int(WINDOW[k] + (color[k] - WINDOW[k]) * alpha) for k in range(3))
        (x0, y0), (x1, y1) = points[i - 1], points[i]
        d.line([(s(ox + x0), s(oy + y0)), (s(ox + x1), s(oy + y1))], fill=c, width=s(3))


def ripple(d, x, y, ox, oy, ms, color):
    age = ms - CLICK_MS
    if age < 0:
        return
    r = 6 + age * 0.05
    a = max(0.0, 1 - age / 400)
    c = tuple(int(WINDOW[k] + (color[k] - WINDOW[k]) * a) for k in range(3))
    d.ellipse([s(ox + x - r), s(oy + y - r), s(ox + x + r), s(oy + y + r)], outline=c, width=s(3))


def on_target(x, y):
    y0 = TARGET_Y - BOX // 2
    return BOX_X <= x <= BOX_X + BOX and y0 <= y <= y0 + BOX


def frame(rows, ms):
    width = SIDE * 2 + PANEL_W * 2 + GAP
    height = TOP + PANEL_H + BOTTOM
    im = Image.new("RGB", (s(width), s(height)), NAVY)
    d = ImageDraw.Draw(im)
    click = at(rows, CLICK_MS)
    panels = [
        ("Without Tremors", CORAL, 1, 2, SIDE),
        ("With Tremors", MINT, 3, 4, SIDE + PANEL_W + GAP),
    ]
    for title, color, ix, iy, ox in panels:
        d.ellipse([s(ox + 2), s(22), s(ox + 14), s(34)], fill=color)
        d.text((s(ox + 22), s(16)), title, font=HEADER_FONT, fill=WHITE)
        hit = on_target(click[ix], click[iy])
        window(d, ox, TOP, checked=ms >= CLICK_MS and hit)
        points = [(r[ix], r[iy]) for r in rows if ms - TRAIL_MS <= r[0] <= ms]
        trail(d, points, ox, TOP, color)
        if ms >= CLICK_MS:
            ripple(d, click[ix], click[iy], ox, TOP, ms, color)
        now = at(rows, ms)
        pointer(d, now[ix], now[iy], ox, TOP)
        if ms >= CLICK_MS + 150:
            verdict = "Clicked" if hit else "Missed the box"
            d.rounded_rectangle([s(ox + 30), s(TOP + 236), s(ox + 30 + 14 + d.textlength(verdict, font=LABEL_FONT) / SCALE), s(TOP + 266)],
                                radius=s(6), fill=color)
            d.text((s(ox + 37), s(TOP + 240)), verdict, font=LABEL_FONT, fill=WHITE)
    return im.resize((width, height), Image.LANCZOS)


def main(trace, out):
    rows = load(trace)
    times = list(range(0, END_MS + 1, FRAME_MS))
    frames = [frame(rows, ms) for ms in times]
    palette = frames[-1].quantize(colors=128, method=Image.Quantize.MEDIANCUT)
    frames = [f.quantize(palette=palette, dither=Image.Dither.NONE) for f in frames]
    durations = [FRAME_MS] * (len(frames) - 1) + [HOLD_LAST_MS]
    frames[0].save(out, save_all=True, append_images=frames[1:], duration=durations, loop=0, optimize=True)
    print(out, len(frames), "frames")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
