from PIL import Image, ImageDraw
import random


WIDTH, HEIGHT, SCALE = 1400, 1100, 2
image = Image.new("RGB", (WIDTH * SCALE, HEIGHT * SCALE))
draw = ImageDraw.Draw(image)


def point(value):
    return round(value * SCALE)


def rect(box, fill, outline=None, width=1):
    draw.rectangle(tuple(point(value) for value in box), fill=fill, outline=outline, width=point(width))


def polygon(points, fill, outline=None, width=1):
    draw.polygon([(point(x), point(y)) for x, y in points], fill=fill)
    if outline:
        draw.line([(point(x), point(y)) for x, y in points + [points[0]]], fill=outline, width=point(width), joint="curve")


def ellipse(box, fill, outline=None, width=1):
    draw.ellipse(tuple(point(value) for value in box), fill=fill, outline=outline, width=point(width))


def line(points, fill, width=1):
    draw.line([(point(x), point(y)) for x, y in points], fill=fill, width=point(width), joint="curve")


def window(x, y, width=35, height=58, color="#23535A"):
    rect((x, y, x + width, y + height), color)
    rect((x + width / 2 - 2, y, x + width / 2 + 2, y + height), "#D7C8A8")
    rect((x, y + height / 2 - 2, x + width, y + height / 2 + 2), "#D7C8A8")


for y in range(HEIGHT):
    t = y / HEIGHT
    top = (196, 223, 216)
    bottom = (241, 225, 185)
    color = tuple(round(top[i] * (1 - t) + bottom[i] * t) for i in range(3))
    rect((0, y, WIDTH, y + 1), color)

ellipse((1010, 55, 1370, 415), "#F8D363")

rect((0, 330, 300, 815), "#77AAA4")
polygon([(0, 330), (115, 245), (300, 300), (300, 340)], "#286B6D")
rect((22, 385, 280, 412), "#D6C9A6")
for x in (35, 100, 165, 230):
    for y in (440, 525, 610, 695):
        window(x, y, 35, 54, "#28585E")

rect((290, 280, 525, 815), "#C88B69")
polygon([(270, 285), (365, 215), (540, 270), (540, 300)], "#8E5749")
for x in (315, 390, 465):
    for y in (345, 440, 535, 630):
        window(x, y, 38, 58, "#4E6461")
line([(300, 420), (520, 420)], "#E8D4AE", 8)
line([(300, 615), (520, 615)], "#E8D4AE", 8)

rect((520, 370, 810, 815), "#E8D4AE")
polygon([(500, 372), (620, 305), (825, 352), (825, 380)], "#BB795D")
for x in (550, 630, 710, 775):
    for y in (430, 525, 620):
        window(x, y, 30, 55, "#47787A")

rect((895, 285, 1400, 815), "#83B7AE")
polygon([(875, 290), (1015, 195), (1400, 255), (1400, 300)], "#286B6D")
for x in (930, 1010, 1090, 1170, 1250, 1330):
    for y in (350, 455, 560, 665):
        window(x, y, 42, 66, "#315F62")
line([(905, 430), (1400, 430)], "#E2C88D", 8)
line([(905, 640), (1400, 640)], "#E2C88D", 8)

rect((0, 782, WIDTH, 855), "#D4B989")
rect((0, 855, WIDTH, HEIGHT), "#384449")
line([(0, 856), (WIDTH, 856)], "#FFF0C6", 8)
for x in range(-80, WIDTH, 230):
    polygon([(x, 1015), (x + 115, 1015), (x + 205, 1030), (x - 5, 1030)], "#F6CB3F")

ellipse((150, 795, 1320, 930), "#243337")

ellipse((355, 715, 570, 930), "#151A1C")
ellipse((390, 750, 535, 895), "#95AAA2")
ellipse((423, 783, 502, 862), "#33464A")
ellipse((1035, 715, 1250, 930), "#151A1C")
ellipse((1070, 750, 1215, 895), "#95AAA2")
ellipse((1103, 783, 1182, 862), "#33464A")

polygon([(185, 660), (265, 620), (490, 595), (605, 485), (915, 485), (1035, 610), (1280, 645), (1375, 715), (1350, 805), (235, 805)], "#20282B")
polygon([(530, 620), (640, 505), (900, 505), (1005, 620)], "#F5C62E")
polygon([(566, 610), (656, 523), (765, 523), (765, 610)], "#356C70")
polygon([(785, 523), (891, 523), (973, 610), (785, 610)], "#356C70")
line([(777, 520), (777, 616)], "#F8E8B7", 8)
line([(642, 620), (1018, 620)], "#F8E8B7", 7)

polygon([(225, 676), (520, 635), (1010, 635), (1270, 670), (1345, 718), (1325, 761), (240, 761)], "#F3C52D")
polygon([(225, 676), (520, 635), (1010, 635), (1270, 670), (1345, 718), (1325, 761), (240, 761)], None, "#14191B", 9)
line([(770, 637), (770, 760)], "#20282B", 8)
line([(1030, 642), (1055, 760)], "#20282B", 8)
rect((600, 658, 700, 670), "#20282B")
rect((870, 658, 970, 670), "#20282B")
rect((790, 710, 805, 730), "#20282B")
rect((1060, 710, 1075, 730), "#20282B")

polygon([(1040, 642), (1270, 670), (1345, 718), (1255, 733), (1080, 704)], "#E9B827")
polygon([(185, 660), (265, 620), (410, 615), (380, 682), (230, 705)], "#20282B")
ellipse((190, 678, 248, 724), "#FFF1C7")
ellipse((1285, 682, 1350, 719), "#E98050")

rect((730, 455, 833, 488), "#F9D65C")
rect((742, 462, 759, 480), "#20282B")
rect((776, 462, 793, 480), "#20282B")
rect((810, 462, 824, 480), "#20282B")

random.seed(17)
for _ in range(2200):
    x = random.randrange(WIDTH)
    y = random.randrange(HEIGHT)
    shade = random.choice(("#FFFFFF", "#142326"))
    draw.point((point(x), point(y)), fill=shade)

image = image.resize((WIDTH, HEIGHT), Image.LANCZOS)
image.save("QrProfile/wwwroot/images/taxi-buenos-aires.png", optimize=True)