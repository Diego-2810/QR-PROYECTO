from PIL import Image, ImageDraw, ImageFilter, ImageFont
import math
import random


WIDTH, HEIGHT, SCALE = 1440, 1100, 2
SIZE = (WIDTH * SCALE, HEIGHT * SCALE)
random.seed(26)


def p(value):
    return round(value * SCALE)


def polygon(draw, points, fill, outline=None, width=1):
    scaled = [(p(x), p(y)) for x, y in points]
    draw.polygon(scaled, fill=fill)
    if outline:
        draw.line(scaled + [scaled[0]], fill=outline, width=p(width), joint="curve")


def ellipse(draw, box, fill, outline=None, width=1):
    draw.ellipse(tuple(p(value) for value in box), fill=fill, outline=outline, width=p(width))


def line(draw, points, fill, width=1):
    draw.line([(p(x), p(y)) for x, y in points], fill=fill, width=p(width), joint="curve")


def rect(draw, box, fill, outline=None, width=1):
    draw.rectangle(tuple(p(value) for value in box), fill=fill, outline=outline, width=p(width))


base = Image.new("RGB", SIZE)
base_draw = ImageDraw.Draw(base)
for y in range(HEIGHT):
    t = y / HEIGHT
    color = tuple(round(a * (1 - t) + b * t) for a, b in zip((14, 24, 31), (31, 35, 33)))
    line(base_draw, [(0, y), (WIDTH, y)], color, 1)

warm_halo = Image.new("RGBA", SIZE, (0, 0, 0, 0))
halo_draw = ImageDraw.Draw(warm_halo)
ellipse(halo_draw, (420, 120, 1170, 790), (238, 153, 57, 48))
warm_halo = warm_halo.filter(ImageFilter.GaussianBlur(p(170)))
base = Image.alpha_composite(base.convert("RGBA"), warm_halo)

bokeh = Image.new("RGBA", SIZE, (0, 0, 0, 0))
bokeh_draw = ImageDraw.Draw(bokeh)
colors = [(251, 184, 69, 150), (232, 95, 74, 120), (55, 169, 180, 115), (244, 215, 136, 105)]
for _ in range(74):
    x = random.randint(-50, WIDTH + 50)
    y = random.randint(130, 675)
    radius = random.choice((5, 7, 10, 15, 20, 27))
    color = random.choice(colors)
    ellipse(bokeh_draw, (x - radius, y - radius, x + radius, y + radius), color)
bokeh = bokeh.filter(ImageFilter.GaussianBlur(p(11)))
base = Image.alpha_composite(base, bokeh)

city_layer = Image.new("RGBA", SIZE, (0, 0, 0, 0))
city = ImageDraw.Draw(city_layer)
blocks = [(-40, 390, 175, 750), (145, 325, 360, 750), (350, 425, 520, 750), (955, 395, 1140, 750), (1115, 315, 1320, 750), (1300, 435, 1490, 750)]
for x1, y1, x2, y2 in blocks:
    rect(city, (x1, y1, x2, y2), (21, 39, 43, 210))
    rect(city, (x1 + 12, y1 + 14, x2 - 12, y1 + 24), (52, 69, 68, 150))
    for x in range(x1 + 18, x2 - 10, 29):
        for y in range(y1 + 45, y2 - 24, 42):
            if random.random() > 0.48:
                color = random.choice([(237, 177, 82, 155), (87, 156, 162, 120), (215, 123, 80, 120)])
                rect(city, (x, y, x + 7, y + 15), color)

polygon(city, [(752, 731), (786, 205), (820, 205), (854, 731)], (15, 29, 34, 235))
polygon(city, [(779, 206), (803, 158), (827, 206)], (15, 29, 34, 235))
rect(city, (797, 108, 810, 160), (15, 29, 34, 235))
city_layer = city_layer.filter(ImageFilter.GaussianBlur(p(2.2)))
base = Image.alpha_composite(base, city_layer)

street_glow = Image.new("RGBA", SIZE, (0, 0, 0, 0))
street_draw = ImageDraw.Draw(street_glow)
ellipse(street_draw, (80, 665, 1370, 1010), (219, 148, 67, 25))
street_glow = street_glow.filter(ImageFilter.GaussianBlur(p(75)))
base = Image.alpha_composite(base, street_glow)

draw = ImageDraw.Draw(base)

polygon(draw, [(0, 825), (145, 752), (330, 687), (470, 610), (650, 572), (1010, 585), (1180, 646), (1375, 760), (1440, 815), (1440, 1100), (0, 1100)], (13, 19, 22, 255))
polygon(draw, [(0, 848), (177, 766), (349, 708), (487, 633), (648, 602), (1006, 614), (1162, 666), (1375, 770), (1440, 824), (1440, 878), (1260, 805), (1080, 758), (850, 731), (565, 724), (335, 765), (140, 836)], (28, 37, 39, 255))
line(draw, [(0, 852), (180, 772), (350, 715), (490, 644), (650, 612), (1005, 625), (1163, 678), (1374, 780), (1440, 835)], (115, 129, 121, 175), 5)
line(draw, [(100, 860), (358, 781), (598, 741), (860, 748), (1100, 787), (1360, 865)], (239, 191, 99, 125), 8)

polygon(draw, [(0, 927), (270, 855), (468, 835), (1440, 905), (1440, 1100), (0, 1100)], (16, 23, 26, 255))
polygon(draw, [(0, 931), (280, 866), (468, 846), (1440, 920), (1440, 986), (470, 920), (250, 943), (0, 1000)], (232, 180, 48, 255))
line(draw, [(0, 1003), (280, 937), (470, 922), (1440, 994)], (255, 218, 112, 200), 7)

windshield = Image.new("RGBA", SIZE, (0, 0, 0, 0))
wind = ImageDraw.Draw(windshield)
polygon(wind, [(145, 819), (347, 744), (570, 718), (905, 733), (1165, 788), (1365, 883), (1370, 1055), (140, 1055)], (17, 28, 33, 248))
polygon(wind, [(228, 827), (378, 767), (573, 744), (706, 750), (622, 913), (267, 928)], (50, 80, 84, 125))
polygon(wind, [(752, 751), (897, 757), (1113, 807), (1284, 879), (755, 912)], (75, 96, 95, 85))
line(wind, [(705, 746), (682, 914)], (9, 17, 22, 240), 14)
line(wind, [(133, 820), (370, 744), (585, 717), (910, 732), (1170, 790), (1377, 884)], (131, 146, 136, 140), 5)
windshield = windshield.filter(ImageFilter.GaussianBlur(p(1.3)))
base = Image.alpha_composite(base, windshield)

reflections = Image.new("RGBA", SIZE, (0, 0, 0, 0))
reflection = ImageDraw.Draw(reflections)
line(reflection, [(290, 789), (495, 740), (639, 731)], (240, 182, 98, 70), 8)
line(reflection, [(840, 753), (1090, 793), (1230, 848)], (221, 231, 211, 50), 7)
line(reflection, [(90, 911), (244, 875)], (248, 211, 124, 95), 12)
reflections = reflections.filter(ImageFilter.GaussianBlur(p(9)))
base = Image.alpha_composite(base, reflections)

sign_glow = Image.new("RGBA", SIZE, (0, 0, 0, 0))
glow_draw = ImageDraw.Draw(sign_glow)
ellipse(glow_draw, (350, 220, 1110, 655), (255, 176, 54, 105))
sign_glow = sign_glow.filter(ImageFilter.GaussianBlur(p(72)))
base = Image.alpha_composite(base, sign_glow)

draw = ImageDraw.Draw(base)
polygon(draw, [(645, 490), (775, 490), (806, 625), (626, 621)], (8, 14, 17, 245))
polygon(draw, [(710, 484), (801, 484), (839, 612), (709, 612)], (29, 36, 36, 245))
line(draw, [(720, 498), (790, 498), (819, 590)], (118, 127, 114, 110), 4)

polygon(draw, [(383, 335), (986, 301), (1080, 462), (443, 506)], (8, 14, 17, 255))
polygon(draw, [(408, 350), (975, 320), (1044, 446), (460, 483)], (244, 186, 54, 255))
polygon(draw, [(408, 350), (975, 320), (1044, 446), (460, 483)], None, (255, 228, 142, 230), 5)
polygon(draw, [(421, 360), (970, 332), (1018, 423), (468, 457)], (255, 218, 104, 255))
line(draw, [(440, 367), (958, 340)], (255, 244, 190, 235), 7)
line(draw, [(475, 468), (1026, 433)], (135, 80, 15, 170), 6)

font_path = "/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf"
font = ImageFont.truetype(font_path, p(102))
text_layer = Image.new("RGBA", SIZE, (0, 0, 0, 0))
text_draw = ImageDraw.Draw(text_layer)
text = "TAXI"
bounds = text_draw.textbbox((0, 0), text, font=font, stroke_width=p(1))
text_width = bounds[2] - bounds[0]
text_height = bounds[3] - bounds[1]
text_image = Image.new("RGBA", (text_width, text_height), (0, 0, 0, 0))
ImageDraw.Draw(text_image).text((0, -bounds[1]), text, font=font, fill=(20, 25, 26, 255), stroke_width=p(1), stroke_fill=(20, 25, 26, 255))
text_image = text_image.resize((p(435), p(91)), Image.LANCZOS)
text_layer.alpha_composite(text_image, (p(505), p(359)))
base = Image.alpha_composite(base, text_layer)

front = Image.new("RGBA", SIZE, (0, 0, 0, 0))
front_draw = ImageDraw.Draw(front)
polygon(front_draw, [(0, 1030), (265, 980), (470, 963), (1440, 1033), (1440, 1100), (0, 1100)], (11, 16, 19, 220))
line(front_draw, [(0, 1028), (272, 978), (470, 965), (1440, 1034)], (82, 96, 92, 130), 4)
base = Image.alpha_composite(base, front)

texture = Image.new("RGBA", SIZE, (0, 0, 0, 0))
texture_draw = ImageDraw.Draw(texture)
for _ in range(14500):
    x = random.randrange(SIZE[0])
    y = random.randrange(SIZE[1])
    alpha = random.randint(2, 12)
    color = random.choice(((255, 242, 210, alpha), (0, 0, 0, alpha)))
    texture_draw.point((x, y), fill=color)
base = Image.alpha_composite(base, texture)

image = base.convert("RGB").resize((WIDTH, HEIGHT), Image.LANCZOS)
image.save("TarjeTaxi/wwwroot/images/taxi-buenos-aires.png", optimize=True)
