"""Renders docs/phone-guide.nl.pdf: a printable A4 guide (Dutch) for setting up an iPhone or
Android phone for a trembling hand, using the settings that phones already have built in.

Usage:
    python tools/make_phone_guide.py docs/phone-guide.nl.pdf
"""
import os
import sys
from reportlab.lib.pagesizes import A4
from reportlab.lib.units import mm
from reportlab.lib import colors
from reportlab.lib.styles import ParagraphStyle
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (BaseDocTemplate, Frame, PageTemplate, Paragraph, Spacer,
                                Table, TableStyle, Image, PageBreak)

OUT = sys.argv[1]
ICON = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "docs", "icon.png")

pdfmetrics.registerFont(TTFont("Segoe", r"C:\Windows\Fonts\segoeui.ttf"))
pdfmetrics.registerFont(TTFont("Segoe-Bold", r"C:\Windows\Fonts\segoeuib.ttf"))
pdfmetrics.registerFont(TTFont("Segoe-Light", r"C:\Windows\Fonts\segoeuil.ttf"))
from reportlab.lib.fonts import addMapping
addMapping("Segoe", 0, 0, "Segoe"); addMapping("Segoe", 1, 0, "Segoe-Bold")

NAVY = colors.HexColor("#1C344E")
MINT = colors.HexColor("#6EDEAC")
MINT_BG = colors.HexColor("#E9F8F1")
GREY = colors.HexColor("#5A6470")
LINE = colors.HexColor("#D5DBE1")

body = ParagraphStyle("body", fontName="Segoe", fontSize=12, leading=17, textColor=colors.black)
small = ParagraphStyle("small", parent=body, fontSize=11.5, leading=15.5, textColor=GREY)
title = ParagraphStyle("title", fontName="Segoe-Bold", fontSize=26, leading=30, textColor=NAVY)
lead = ParagraphStyle("lead", parent=body, fontSize=13.5, leading=19, textColor=GREY)
h1 = ParagraphStyle("h1", fontName="Segoe-Bold", fontSize=18, leading=22, textColor=NAVY,
                    spaceBefore=10, spaceAfter=4)
h2 = ParagraphStyle("h2", fontName="Segoe-Bold", fontSize=13, leading=17, textColor=NAVY,
                    spaceBefore=10, spaceAfter=4)
path = ParagraphStyle("path", parent=body, fontSize=12, leading=17, textColor=NAVY)
num = ParagraphStyle("num", fontName="Segoe-Bold", fontSize=15, leading=18, textColor=NAVY,
                     alignment=1)

W = A4[0] - 36 * mm  # text width


def box(flowables, bg=MINT_BG, border=MINT):
    t = Table([[flowables]], colWidths=[W])
    t.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, -1), bg),
        ("LINEBEFORE", (0, 0), (0, -1), 3, border),
        ("LEFTPADDING", (0, 0), (-1, -1), 10), ("RIGHTPADDING", (0, 0), (-1, -1), 10),
        ("TOPPADDING", (0, 0), (-1, -1), 8), ("BOTTOMPADDING", (0, 0), (-1, -1), 9),
    ]))
    return t


def steps(rows):
    """Numbered steps: (setting, what to choose, why)."""
    data = []
    for i, (name, choose, why) in enumerate(rows, 1):
        data.append([
            Paragraph(str(i), num),
            [Paragraph(f"<b>{name}</b>: {choose}", body), Paragraph(why, small)],
        ])
    t = Table(data, colWidths=[10 * mm, W - 10 * mm])
    t.setStyle(TableStyle([
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("LINEBELOW", (0, 0), (-1, -2), 0.5, LINE),
        ("TOPPADDING", (0, 0), (-1, -1), 7), ("BOTTOMPADDING", (0, 0), (-1, -1), 8),
        ("LEFTPADDING", (0, 0), (0, -1), 0),
    ]))
    return t


def bullets(items):
    return [Paragraph(f"•&nbsp;&nbsp;{t}", ParagraphStyle("b", parent=body, leftIndent=12,
                                                          firstLineIndent=-12, spaceAfter=4))
            for t in items]


def footer(canvas, doc):
    canvas.saveState()
    canvas.setStrokeColor(LINE)
    canvas.line(18 * mm, 14 * mm, A4[0] - 18 * mm, 14 * mm)
    canvas.setFont("Segoe", 8.5)
    canvas.setFillColor(GREY)
    canvas.drawString(18 * mm, 9.5 * mm,
                      "Tremors · gratis hulpmiddel voor mensen met een trillende hand · "
                      "github.com/JeroenJonckheer/Tremors")
    canvas.drawRightString(A4[0] - 18 * mm, 9.5 * mm, f"{doc.page}")
    canvas.restoreState()


story = []

head = Table([[Image(ICON, 17 * mm, 17 * mm),
               [Paragraph("Tremors voor de telefoon", title),
                Paragraph("Zo stelt u een iPhone of Android-telefoon in voor een trillende hand", lead)]]],
             colWidths=[22 * mm, W - 22 * mm])
head.setStyle(TableStyle([("VALIGN", (0, 0), (-1, -1), "MIDDLE"), ("LEFTPADDING", (0, 0), (-1, -1), 0)]))
story += [head, Spacer(1, 8)]

story.append(Paragraph(
    "Telefoons hebben handige instellingen voor trillende handen al ingebouwd. Ze staan alleen "
    "goed verstopt. U hoeft niets te installeren of te kopen. Het instellen kost ongeveer vijf "
    "minuten. Doe het gerust samen met een mantelzorger, buur of (klein)kind.", body))
story.append(Spacer(1, 8))
story.append(box([
    Paragraph("<b>Kunt u een instelling niet vinden?</b>", body),
    Paragraph("Open <b>Instellingen</b>, tik bovenaan op het zoekveld (het vergrootglas) "
              "en typ het zoekwoord dat hieronder bij elke stap staat.", body),
]))

# iPhone
story.append(Paragraph("iPhone", h1))
story.append(Paragraph("Ga naar <b>Instellingen</b> → <b>Toegankelijkheid</b> → <b>Aanraken</b> → "
                       "<b>Aanpassingen voor aanraken</b> en zet <b>Aanpassingen voor aanraken</b> aan. "
                       "Stel daarna deze drie dingen in:", path))
story.append(Spacer(1, 2))
story.append(steps([
    ("Houdduur", "aan, begin met 0,3 seconden",
     "Een tik telt pas als u even blijft staan. Korte, trillende tikjes tellen niet mee. "
     "Reageert de telefoon te traag? Zet de tijd lager. Zoekwoord: <b>houdduur</b>"),
    ("Negeer herhaling", "aan, begin met 0,5 seconden",
     "Meerdere tikjes vlak na elkaar tellen als één tik. Zoekwoord: <b>herhaling</b>"),
    ("Tikhulp", "kies <b>Gebruik eerste aanraaklocatie</b>",
     "De plek waar uw vinger als eerste neerkomt telt. Wegglijden tijdens het tikken "
     "maakt dan niet meer uit. Zoekwoord: <b>tikhulp</b>"),
]))
story.append(Paragraph("Ook handig op de iPhone", h2))
story += bullets([
    "<b>Schudden om te herstellen</b> uitzetten (Toegankelijkheid → Aanraken). Anders kan een "
    "trillende hand per ongeluk getypte tekst wissen.",
    "<b>Tik op achterkant</b> uitzetten (Toegankelijkheid → Aanraken → Tik op achterkant): "
    "zet Dubbeltik en Drietik op Geen.",
    "<b>Grotere tekst en knoppen</b>: Instellingen → Beeldscherm en helderheid → "
    "Tekstgrootte en Weergavezoom.",
])

story.append(PageBreak())

# Android
story.append(Paragraph("Android (Samsung, Google Pixel en andere)", h1))
story.append(Paragraph(
    "De namen verschillen per merk. Hieronder ziet u ze voor Samsung. Heeft u een ander merk, "
    "gebruik dan het zoekwoord. Niet elke telefoon heeft alle drie de instellingen.", body))
story.append(Spacer(1, 6))
story.append(Paragraph("Samsung: <b>Instellingen</b> → <b>Toegankelijkheid</b> → "
                       "<b>Interactie en handigheid</b>", path))
story.append(Spacer(1, 2))
story.append(steps([
    ("Tikduur", "aan, begin met 0,3 seconden",
     "Een tik telt pas als u even blijft staan. Korte, trillende tikjes tellen niet mee. "
     "Zoekwoord: <b>tikduur</b>"),
    ("Herhaalde aanrakingen negeren", "aan, begin met 0,5 seconden",
     "Meerdere tikjes vlak na elkaar tellen als één tik. Zoekwoord: <b>herhaalde</b>"),
    ("Vertraging voor aanraken en vasthouden", "kies <b>lang</b>",
     "Een trillende tik wordt dan niet per ongeluk gezien als lang indrukken. "
     "Zoekwoord: <b>vasthouden</b>"),
]))
story.append(Paragraph("Ook handig op Android", h2))
story += bullets([
    "<b>Grotere tekst en knoppen</b>: Instellingen → Scherm → Lettergrootte en Schermzoom. "
    "Zoekwoord: <b>lettergrootte</b>",
    "<b>Tijd om te reageren</b> langer zetten, zodat meldingen niet te snel verdwijnen. "
    "Zoekwoord: <b>tijd</b>",
])

# General tips
story.append(Paragraph("Tips voor elke telefoon", h1))
story += bullets([
    "<b>Leg de telefoon neer.</b> Op tafel of in een standaard trilt hij niet mee. Laat uw pols "
    "of elleboog op tafel rusten terwijl u tikt.",
    "<b>Gebruik een tekenpen (stylus).</b> Een pen met wat gewicht houdt u vast zoals een "
    "gewone pen. Dat is vaak veel rustiger dan tikken met een vinger.",
    "<b>Een hoes met grip of een ring</b> op de achterkant geeft meer houvast.",
    "<b>Praat in plaats van te typen.</b> Tik op het microfoontje op het toetsenbord en spreek "
    "uw bericht in. Of vraag Siri of de Google Assistent om iemand te bellen.",
])
story.append(Spacer(1, 10))
story.append(box([
    Paragraph("<b>Werkt u ook op een Windows-computer?</b>", body),
    Paragraph("Tremors is een gratis programma dat de trilling uit de muisbeweging filtert. "
              "Download: <b>github.com/JeroenJonckheer/Tremors</b>", body),
]))
story.append(Spacer(1, 8))
story.append(Paragraph("De namen van instellingen kunnen per telefoon en per versie iets verschillen. "
                       "Gebruik bij twijfel het zoekveld in Instellingen.", small))

doc = BaseDocTemplate(OUT, pagesize=A4, leftMargin=18 * mm, rightMargin=18 * mm,
                      topMargin=16 * mm, bottomMargin=20 * mm,
                      title="Tremors voor de telefoon", author="Jeroen Jonckheer",
                      subject="Een iPhone of Android-telefoon instellen voor een trillende hand")
doc.addPageTemplates([PageTemplate(frames=[Frame(doc.leftMargin, doc.bottomMargin, doc.width,
                                                 doc.height, id="f")], onPage=footer)])
doc.build(story)
print("ok", OUT)
