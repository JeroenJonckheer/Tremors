"""Renders the Tremors icon: a calm mouse pointer with two mint echoes behind it, the
tremor that is being absorbed. The echoes are offset far and drawn strongly on purpose,
so they stay visible at 16 px in the notification area. Navy and mint when active, grey
when paused.

Usage: python tools/make_icon.py <preview.png> [src/Tremors/Tremors.ico docs/icon.png]

The geometry matches TrayIcon.cs (same proportions); keep the two in sync.
"""
import sys
from PIL import Image, ImageDraw

S = 1024

# Classic pointer, tip at (0, 0), height 1.
ARROW = [(0, 0), (0, 0.895), (0.211, 0.684), (0.368, 1.0), (0.474, 0.947), (0.316, 0.632), (0.632, 0.632)]
HEIGHT = 0.52                                   # pointer height, fraction of the icon
GHOSTS = [((-0.13, 0.05), 150), ((0.10, -0.09), 230)]  # offset (fraction), alpha
ROUNDING = 0.02                                # softens the pointer's corners

NAVY, GREY = (28, 52, 78), (120, 128, 138)
WHITE, MINT, PALE = (240, 246, 248), (110, 222, 172), (210, 216, 222)


def pointer(draw, tip, color):
    h = HEIGHT * S
    pts = [(tip[0] + x * h, tip[1] + y * h) for x, y in ARROW]
    draw.polygon(pts, fill=color)
    draw.line(pts + [pts[0]], fill=color, width=int(ROUNDING * S), joint="curve")


def render(active=True):
    im = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    m = 0.02 * S
    d.ellipse([m, m, S - m, S - m], fill=(NAVY if active else GREY) + (255,))
    h = HEIGHT * S
    tip = (S / 2 - 0.632 * h / 2, S / 2 - h / 2)  # centre the pointer
    ghost = MINT if active else PALE
    for (dx, dy), alpha in GHOSTS:
        layer = Image.new("RGBA", (S, S), (0, 0, 0, 0))
        pointer(ImageDraw.Draw(layer), (tip[0] + dx * S, tip[1] + dy * S), ghost + (alpha,))
        im.alpha_composite(layer)
    pointer(ImageDraw.Draw(im), tip, WHITE + (255,))
    return im


if __name__ == "__main__":
    on, off = render(True), render(False)
    sheet = Image.new("RGBA", (620, 420), (240, 240, 240, 255))
    sheet.alpha_composite(on.resize((256, 256), Image.LANCZOS), (20, 20))
    sheet.alpha_composite(off.resize((256, 256), Image.LANCZOS), (320, 20))
    for row, bg in enumerate([(240, 240, 240, 255), (32, 32, 32, 255)]):
        strip = Image.new("RGBA", (620, 62), bg)
        for j, size in enumerate((48, 32, 24, 16)):
            strip.alpha_composite(on.resize((size, size), Image.LANCZOS), (20 + j * 70, (62 - size) // 2))
            strip.alpha_composite(off.resize((size, size), Image.LANCZOS), (320 + j * 70, (62 - size) // 2))
        sheet.alpha_composite(strip, (0, 290 + row * 62))
    sheet.save(sys.argv[1])
    if len(sys.argv) > 3:
        on.resize((256, 256), Image.LANCZOS).save(
            sys.argv[2], sizes=[(16, 16), (20, 20), (24, 24), (32, 32), (40, 40), (48, 48), (64, 64), (128, 128), (256, 256)])
        on.resize((128, 128), Image.LANCZOS).save(sys.argv[3])
    print("ok")
