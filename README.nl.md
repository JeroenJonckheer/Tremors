<div align="center">

<img src="docs/icon.png" width="96" alt="">

# Tremors

**Een stabiele muis voor trillende handen.**
Gratis, open source en privé. Eén klein programma voor Windows.

[**Download Tremors.exe**](https://github.com/JeroenJonckheer/Tremors/releases/latest) · [English](README.md) · [Hoe het werkt (Engels)](docs/how-it-works.md)

</div>

---

Naarmate we ouder worden gaat de fijne motoriek achteruit, terwijl we de computer net
zo hard nodig hebben als altijd. Een trillende hand laat de cursor schudden, klikken
komen naast de knop terecht en de pagina scrolt per ongeluk.

Tremors zit rustig in de hoek van het scherm en filtert de trilling uit elke
muisbeweging, nog voordat Windows die ziet. Kleine, trillende bewegingen worden
opgevangen. Bewuste bewegingen gaan gewoon waar u ze wilt hebben.

Tremors is gemaakt voor Nico, zodat hij zijn computer kan blijven gebruiken. Het is
gratis voor iedereen die hetzelfde nodig heeft.

## Wat het doet

- **Houdt de cursor stil.** Trillingen en plotselinge schokken worden weggefilterd; wie stilhoudt op een knop,
  houdt ook de cursor stil.
- **Klikken blijven op hun doel.** Op het moment van klikken stopt de cursor met glijden.
- **Houdt het scrollwiel rustig.** Een trillend heen-en-weer wordt genegeerd, zodat een
  pagina niet op en neer springt.
- **Eenvoudig in te stellen.** Eén schuifje voor de sterkte, en twee om fijn af te stemmen.
- **Spreekt uw taal.** Nederlands, Engels, Frans of Duits, net als Windows.
- **Verder niets.** Geen account, geen internet, geen reclame, geen gegevens verzamelen.

## Installeren

1. Download [**Tremors.exe**](https://github.com/JeroenJonckheer/Tremors/releases/latest).
2. Zet het bestand op een vaste plek, bijvoorbeeld in `Documenten` of in `C:\Program Files\Tremors`.
3. Dubbelklik erop. Naast de klok verschijnt een rond icoon met een groene stip, en de
   muis wordt meteen stabiel gehouden.
4. Klik op het icoon en vink **Starten met Windows** aan, zodat het altijd aanstaat.

Er hoeft niets geïnstalleerd te worden: geen beheerdersrechten en geen .NET nodig.

> **Windows kan de eerste keer waarschuwen** ("Windows heeft uw pc beveiligd").
> Tremors is niet ondertekend met een betaald certificaat. Klik op **Meer informatie** →
> **Toch uitvoeren**. De download is te controleren met het bestand `Tremors.exe.sha256`
> op de releasepagina, en de [broncode](src/Tremors) staat hier helemaal open.

## Gebruik

| Waar | Wat |
|---|---|
| **Linksklik** op het icoon | Opent het instellingenvenster |
| **Rechtsklik** op het icoon | Menu: **Actief** (aan/uit), **Instellingen**, **Starten met Windows**, **Tremors stoppen** |
| Groene stip | Tremors houdt de muis stabiel |
| Grijze stip | Tremors staat op pauze |

Het instellingenvenster:

| Instelling | Betekenis |
|---|---|
| **Sterkte** (0-100) | Hoeveel trilling wordt weggefilterd. Begin bij 70. Trilt de cursor nog, zet hem hoger; voelt de muis traag, zet hem lager. 0 zet het filter uit. |
| **Muissnelheid** (10-100%) | Hoe ver de cursor gaat bij een handbeweging. Lager maakt de trilling kleiner op het scherm; u beweegt de muis dan wat verder. Probeer 50-70% bij een sterke trilling. |
| **Stilstandzone** (0-30 px) | De cursor blijft staan tot de hand verder beweegt dan dit. Zet hem hoger als de cursor nog springt terwijl u stil probeert te houden op een knop. |
| **Scrollwiel stabiel houden** | Negeert een scroll de andere kant op vlak na een scroll; dat is meestal een trilling. |
| **Starten met Windows** | Start Tremors vanzelf bij het aanmelden. |

Elke wijziging werkt meteen en wordt onthouden.

Zolang Tremors aanstaat, zet het de muisversnelling van Windows uit ("Verbeterde
aanwijzerprecisie"), want die maakt snelle, trillende bewegingen groter. Uw eigen
instelling komt terug zodra Tremors gepauzeerd of afgesloten wordt.

**De juiste instellingen vinden:** begin met sterkte 70. Komen schokken nog door? Zet
eerst de muissnelheid lager, dan de stilstandzone hoger, en dan pas de sterkte. Kies de
laagste waarden waarbij de cursor rustig blijft; dan reageert de muis zo direct mogelijk.

### Tips voor mantelzorgers en familie

- Stel het één keer in, vink **Starten met Windows** aan, en wie de computer gebruikt
  hoeft er nooit meer aan te denken.
- Een zware muis of een trackball helpt ook; Tremors werkt met allebei.
- Grotere tekst (Instellingen → Toegankelijkheid → Tekengrootte) en een grotere
  muisaanwijzer (Instellingen → Toegankelijkheid → Muisaanwijzer) maken knoppen
  makkelijker te raken.

## Privacy

Tremors leest muisbewegingen om ze te filteren, en verder niets. Het leest het
toetsenbord niet, het bewaart of verstuurt niets en maakt nooit verbinding met internet.
Het enige bestand dat het schrijft zijn de instellingen: `%AppData%\Tremors\settings.json`.

## Beperkingen

- Windows 10 en 11, 64-bit.
- Op het aanmeldscherm van Windows en in de vraag "Wilt u toestaan dat deze app…" (UAC)
  werkt het niet; daar laat Windows geen enkel programma toe.
- In vensters die als administrator draaien kan de beweging ongefilterd zijn, tenzij
  Tremors ook als administrator draait.
- Games die de muis rechtstreeks uitlezen (raw input) worden niet gefilterd.

## Zelf bouwen

Zie de [Engelse README](README.md#build-from-source).

## Licentie

[MIT](LICENSE): vrij te gebruiken, te delen en aan te passen, ook commercieel.
Tremors is en blijft gratis.
