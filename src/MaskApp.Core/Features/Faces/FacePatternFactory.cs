namespace MaskApp.Core.Features.Faces;

public static class FacePatternFactory
{
    public static IReadOnlyList<FacePattern> CreateBuiltIns()
    {
        var now = DateTimeOffset.UnixEpoch;
        return
        [
            Create(FaceEmotion.Happy, "Happy", 1, new FaceColor(0xFA, 0xCC, 0x15), DrawHappy, now),
            Create(FaceEmotion.Sad, "Sad", 2, new FaceColor(0x60, 0xA5, 0xFA), DrawSad, now),
            Create(FaceEmotion.Angry, "Angry", 3, new FaceColor(0xEF, 0x44, 0x44), DrawAngry, now),
            Create(FaceEmotion.Surprised, "Surprised", 4, new FaceColor(0x52, 0xE3, 0xFF), DrawSurprised, now),
            Create(FaceEmotion.Meh, "Meh", 5, new FaceColor(0xE5, 0xE7, 0xEB), DrawMeh, now),
            Create(FaceEmotion.Wink, "Wink", 6, new FaceColor(0xF4, 0x72, 0xB6), DrawWink, now),
            CreateCharacter("cool-shades", "Cool Shades", DrawCoolShades, now),
            CreateCharacter("heart-eyes", "Heart Eyes", DrawHeartEyes, now),
            CreateCharacter("starstruck", "Starstruck", DrawStarstruck, now),
            CreateCharacter("big-laugh", "Big Laugh", DrawBigLaugh, now),
            CreateCharacter("tongue-out", "Tongue Out", DrawTongueOut, now),
            CreateCharacter("vampire", "Vampire", DrawVampire, now),
            CreateCharacter("robot", "Robot", DrawRobot, now),
            CreateCharacter("alien", "Alien", DrawAlien, now),
            CreateCharacter("cat", "Pixel Cat", DrawCat, now),
            CreateCharacter("puppy", "Pixel Puppy", DrawPuppy, now),
            CreateCharacter("frog", "Pixel Frog", DrawFrog, now),
            CreateCharacter("panda", "Pixel Panda", DrawPanda, now),
            CreateCharacter("skull", "Neon Skull", DrawSkull, now),
            CreateCharacter("ghost", "Tiny Ghost", DrawGhost, now),
            CreateCharacter("little-devil", "Little Devil", DrawLittleDevil, now),
            CreateCharacter("clown", "Silly Clown", DrawClown, now),
            CreateCharacter("pirate", "Pixel Pirate", DrawPirate, now),
            CreateCharacter("ninja", "Pixel Ninja", DrawNinja, now),
            CreateCharacter("cowboy", "Pixel Cowboy", DrawCowboy, now),
            CreateCharacter("mustache", "Fancy Mustache", DrawMustache, now),
            CreateCharacter("dj", "Rave DJ", DrawDj, now),
            CreateCharacter("three-eyed-monster", "Three-Eyed Monster", DrawThreeEyedMonster, now),
            CreateCharacter("cyclops", "Happy Cyclops", DrawCyclops, now),
            CreateCharacter("sleepy", "Sleepy Face", DrawSleepy, now)
        ];
    }

    public static FacePattern CreateBlank(string name = "Custom Face", int preferredSlot = 7) =>
        new FacePattern
        {
            Id = $"face-{Guid.NewGuid():N}",
            DisplayName = name,
            Source = FacePatternSource.Custom,
            Emotion = FaceEmotion.Custom,
            PreferredSlot = preferredSlot,
            Pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray()
        }.Normalize();

    private static FacePattern Create(
        FaceEmotion emotion,
        string name,
        int slot,
        FaceColor color,
        Action<Canvas, FaceColor> draw,
        DateTimeOffset timestamp)
    {
        var canvas = new Canvas();
        draw(canvas, color);
        return new FacePattern
        {
            Id = $"built-in-smiley-{emotion.ToString().ToLowerInvariant()}",
            DisplayName = $"{name} Smiley",
            Emotion = emotion,
            Source = FacePatternSource.BuiltIn,
            PreferredSlot = slot,
            IsFavorite = true,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
            Pixels = canvas.Pixels
        }.Normalize(timestamp);
    }

    private static FacePattern CreateCharacter(
        string id,
        string name,
        Action<Canvas> draw,
        DateTimeOffset timestamp)
    {
        var canvas = new Canvas();
        draw(canvas);
        return new FacePattern
        {
            Id = $"built-in-face-{id}",
            DisplayName = name,
            Emotion = FaceEmotion.Custom,
            Source = FacePatternSource.BuiltIn,
            PreferredSlot = 7,
            IsFavorite = true,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
            Pixels = canvas.Pixels
        }.Normalize(timestamp);
    }

    private static void DrawHappy(Canvas canvas, FaceColor color)
    {
        DrawEyes(canvas, color);
        canvas.PlotMany(color, (13, 7), (14, 8), (15, 9), (16, 9), (17, 10), (18, 10), (19, 10), (20, 10), (21, 9), (22, 9), (23, 8), (24, 7));
    }

    private static void DrawSad(Canvas canvas, FaceColor color)
    {
        DrawEyes(canvas, color);
        canvas.PlotMany(color, (13, 10), (14, 9), (15, 8), (16, 8), (17, 7), (18, 7), (19, 7), (20, 7), (21, 8), (22, 8), (23, 9), (24, 10));
    }

    private static void DrawAngry(Canvas canvas, FaceColor color)
    {
        canvas.PlotMany(color, (10, 3), (11, 4), (12, 5), (24, 5), (25, 4), (26, 3));
        canvas.FillRect(10, 5, 3, 2, color);
        canvas.FillRect(24, 5, 3, 2, color);
        canvas.Line(14, 9, 23, 8, color);
    }

    private static void DrawSurprised(Canvas canvas, FaceColor color)
    {
        DrawEyes(canvas, color);
        canvas.PlotMany(color, (17, 7), (18, 7), (19, 7), (20, 7), (16, 8), (21, 8), (16, 9), (21, 9), (17, 10), (18, 10), (19, 10), (20, 10));
    }

    private static void DrawMeh(Canvas canvas, FaceColor color)
    {
        DrawEyes(canvas, color);
        canvas.Line(13, 9, 24, 9, color);
    }

    private static void DrawWink(Canvas canvas, FaceColor color)
    {
        canvas.FillRect(10, 4, 3, 2, color);
        canvas.Line(23, 5, 27, 4, color);
        canvas.PlotMany(color, (13, 8), (14, 9), (15, 10), (16, 10), (17, 10), (18, 10), (19, 10), (20, 10), (21, 10), (22, 9), (23, 8));
    }

    private static void DrawCoolShades(Canvas canvas) => canvas.Sprite(
        "..CCCCCC....CCCCCC..",
        ".CWWWWWWC..CWWWWWWC.",
        ".CWWWWWWCCCCWWWWWWC.",
        "..CCCCCC....CCCCCC..",
        "....................",
        "....Y..........Y....",
        ".....Y........Y.....",
        "......YYYYYYYY......",
        "........YYYY........");

    private static void DrawHeartEyes(Canvas canvas) => canvas.Sprite(
        "..PP..PP......PP..PP..",
        ".PPPPPPPP....PPPPPPPP.",
        ".PPPPPPPP....PPPPPPPP.",
        "..PPPPPP......PPPPPP..",
        "....PP..........PP....",
        "......................",
        "...Y..............Y...",
        "....YY..........YY....",
        "......YYYYYYYYYY......",
        ".........YYYY.........");

    private static void DrawStarstruck(Canvas canvas) => canvas.Sprite(
        "...Y.YY.Y......Y.YY.Y...",
        "....YYYY........YYYY....",
        "..YYYYYYYY....YYYYYYYY..",
        "....YYYY........YYYY....",
        "...Y.YY.Y......Y.YY.Y...",
        "........................",
        ".....C............C.....",
        "......CC........CC......",
        "........CCCCCCCC........",
        "..........CCCC..........");

    private static void DrawBigLaugh(Canvas canvas) => canvas.Sprite(
        "...CCC..........CCC...",
        "..C...C........C...C..",
        ".C.....C........C.....C.",
        "......................",
        "....WWWWWWWWWWWWWW....",
        "...WVVVVVVVVVVVVVVW...",
        "...WVVVVVVVVVVVVVVW...",
        "....WPPPPPPPPPPPPW....",
        "......WWWWWWWWWW......");

    private static void DrawTongueOut(Canvas canvas) => canvas.Sprite(
        "...YY............YY...",
        "..YYYY..........YYYY..",
        "...YY............YY...",
        "......................",
        "....WWWWWWWWWWWWWW....",
        "...W..............W...",
        "...W....PPPPPP....W...",
        "....W...PPPPPP...W....",
        "......W.PPPPPP.W......",
        "........PPPPPP........");

    private static void DrawVampire(Canvas canvas) => canvas.Sprite(
        "..R.RR..........RR.R..",
        "...RRR..........RRR...",
        "....R............R....",
        "......................",
        "...WWWWWWWWWWWWWWWW...",
        "..W....W........W....W..",
        "..W....W...RR...W....W..",
        "...W...W........W...W...",
        ".....W.W........W.W.....",
        ".......W........W.......");

    private static void DrawRobot(Canvas canvas) => canvas.Sprite(
        ".........C..C.........",
        "......CCCCCCCCCC......",
        "....CC..........CC....",
        "...C..WWW....WWW..C...",
        "...C..WWW....WWW..C...",
        "...C............C...",
        "...C..Y.Y.Y.Y.Y..C...",
        "...C...Y.Y.Y.Y...C...",
        "....CC..........CC....",
        "......CCCCCCCCCC......");

    private static void DrawAlien(Canvas canvas) => canvas.Sprite(
        ".........GGGG.........",
        "......GGGGGGGGGG......",
        "....GGGGGGGGGGGGGG....",
        "...GG.CCCC....CCCC.GG...",
        "..GG..CCCC....CCCC..GG..",
        "..GG...CC......CC...GG..",
        "...GG............GG...",
        "....GG...GGGG...GG....",
        "......GGGGGGGGGG......",
        ".........GGGG.........");

    private static void DrawCat(Canvas canvas) => canvas.Sprite(
        "..P.................P..",
        ".PPP...............PPP.",
        ".P..PPPPPPPPPPPPPPP..P.",
        ".P...YY.........YY...P.",
        ".....YY.........YY.....",
        "..........P.P..........",
        "C.C.C....PPPPP....C.C.C",
        ".C.C.C...P.P.P...C.C.C.",
        ".........P...P.........");

    private static void DrawPuppy(Canvas canvas) => canvas.Sprite(
        "..OOO..............OOO..",
        ".OOOOO............OOOOO.",
        ".OO..O............O..OO.",
        "....WW..........WW....",
        "....WW..........WW....",
        ".........OOOO.........",
        "........OOOOOO........",
        "......O..PPPP..O......",
        ".......O.PPPP.O.......",
        ".........PPPP.........");

    private static void DrawFrog(Canvas canvas) => canvas.Sprite(
        "...GGGG..........GGGG...",
        "..GWWWWG........GWWWWG..",
        "..GWCCWG........GWCCWG..",
        "...GGGGGGGGGGGGGGGGGG...",
        "..GGGGGGGGGGGGGGGGGGGG..",
        "..GG....GGGGGGGG....GG..",
        "...GG..G........G..GG...",
        ".....GG..........GG.....",
        ".......GGGGGGGGGG.......");

    private static void DrawPanda(Canvas canvas) => canvas.Sprite(
        "...WWW..........WWW...",
        "..WWWWW........WWWWW..",
        ".WWWWWWWWWWWWWWWWWWWW.",
        ".WW.VVVV......VVVV.WW.",
        ".W.VVWWVV....VVWWVV.W.",
        ".W...WW........WW...W.",
        ".W.......VVVV.......W.",
        ".WW......VVVV......WW.",
        "..WWW..W......W..WWW..",
        "....WWWWWWWWWWWW....");

    private static void DrawSkull(Canvas canvas) => canvas.Sprite(
        ".......WWWWWWWW.......",
        "....WWWWWWWWWWWWWW....",
        "...WWW..........WWW...",
        "..WWW..VVVV..VVVV..WWW..",
        "..WW...VVVV..VVVV...WW..",
        "..WW......WW......WW..",
        "...WW....WWWW....WW...",
        "....WW.W.W..W.W.WW....",
        ".....W.W.W..W.W.W.....",
        "......WWWWWWWWWW......");

    private static void DrawGhost(Canvas canvas) => canvas.Sprite(
        "........WWWWWW........",
        ".....WWWWWWWWWWWW.....",
        "....WWW........WWW....",
        "...WWW..CC....CC..WWW...",
        "...WW...CC....CC...WW...",
        "...WW.....PPPP.....WW...",
        "...WW....PPPPPP....WW...",
        "...WW............WW...",
        "...WW.WW.WW.WW.WW.WW...",
        "....WW..WW..WW..WW....");

    private static void DrawLittleDevil(Canvas canvas) => canvas.Sprite(
        "..RR.................RR..",
        ".RRRR...............RRRR.",
        "..RRRRRRRRRRRRRRRRRRRR..",
        "....R............R....",
        "....RRR........RRR....",
        ".....RR........RR.....",
        ".........RRRR.........",
        "....W.W.RRRRRR.W.W....",
        ".....W..RRRRRR..W.....",
        ".......RRRRRRRR.......");

    private static void DrawClown(Canvas canvas) => canvas.Sprite(
        "..PPPP............PPPP..",
        ".PPPPP............PPPPP.",
        "...CC............CC...",
        "..CWWC..........CWWC..",
        "...CC....RRRR....CC...",
        ".........RRRR.........",
        "....Y............Y....",
        ".....YY.PPPPPP.YY.....",
        ".......PPPPPPPP.......",
        ".........PPPP.........");

    private static void DrawPirate(Canvas canvas) => canvas.Sprite(
        "....RRRRRRRRRRRRRR....",
        "..RRRRRRRRRRRRRRRRRR..",
        "....RRRRRRRRRRRRRR....",
        "...VVVVVVVVVV....WW...",
        "..VVVVVVVVVVV....WW..",
        "...VVVVVVVVVV.........",
        "...........O..........",
        ".....WWWWWWWWWWWW.....",
        ".....W.W.W.W.W.W......",
        ".......WWWWWWWW.......");

    private static void DrawNinja(Canvas canvas) => canvas.Sprite(
        ".....VVVVVVVVVVVV.....",
        "...VVVVVVVVVVVVVVVV...",
        "..VVV............VVV..",
        ".VVV..WWW......WWW..VVV.",
        ".VV...WW........WW...VV.",
        ".VVV..............VVV.",
        "..VVVVVVVVVVVVVVVVVV..",
        "...VVVVVVVVVVVVVVVV...",
        ".....VVVVVVVVVVVV.....");

    private static void DrawCowboy(Canvas canvas) => canvas.Sprite(
        ".......OOOOOOOO.......",
        "....OOOOOOOOOOOOOO....",
        "..OOOOOOOOOOOOOOOOOO..",
        "......OOOOOOOOOO......",
        "...CC............CC...",
        "...CC............CC...",
        "..........OO..........",
        ".....OO.OOOOOO.OO.....",
        "......OO......OO......",
        "........OOOOOO........");

    private static void DrawMustache(Canvas canvas) => canvas.Sprite(
        "...CC............CC...",
        "..CCCC..........CCCC..",
        "...CC............CC...",
        "......................",
        "..........WW..........",
        "....OOO..OOOO..OOO....",
        "...OOOOOOOOOOOOOOOO...",
        "....OOOOOO..OOOOOO....",
        "......OO......OO......");

    private static void DrawDj(Canvas canvas) => canvas.Sprite(
        "...PPPPPPPPPPPPPPPP...",
        ".PPP..............PPP.",
        ".PP..CCCC....CCCC..PP.",
        ".PP.CWWWWC..CWWWWC.PP.",
        ".PP..CCCCCCCCCCCC..PP.",
        ".PPP..............PPP.",
        "...PP....YYYY....PP...",
        ".........YYYY.........",
        "......YY......YY......",
        ".......YYYYYYYY.......");

    private static void DrawThreeEyedMonster(Canvas canvas) => canvas.Sprite(
        ".......GGGGGGGG.......",
        "....GGGGGGGGGGGGGG....",
        "...G.CCC..CCC..CCC.G...",
        "..GG.CWC..CWC..CWC.GG..",
        "..GG.CCC..CCC..CCC.GG..",
        "..GG..............GG..",
        "...G.W.W.W.W.W.W.W.G...",
        "....G.W.W.W.W.W.W.G....",
        ".....GGGGGGGGGGGG.....",
        ".......GG....GG.......");

    private static void DrawCyclops(Canvas canvas) => canvas.Sprite(
        ".......YYYYYYYY.......",
        "....YYYYYYYYYYYYYY....",
        "...YY....CCCC....YY...",
        "..YY....CWWWWC....YY..",
        "..YY....CWCCWC....YY..",
        "..YY....CWWWWC....YY..",
        "...YY....CCCC....YY...",
        "....YY..Y....Y..YY....",
        "......YY......YY......",
        "........YYYYYY........");

    private static void DrawSleepy(Canvas canvas) => canvas.Sprite(
        "..CCC............CCC..",
        "...CCC..........CCC...",
        ".....CCC......CCC.....",
        "......................",
        ".................VV...",
        "...............VVVV...",
        ".................VV...",
        ".....WWWWWWWWWW.....",
        "......WWWWWWWW......",
        "........WWWW........");

    private static void DrawEyes(Canvas canvas, FaceColor color)
    {
        canvas.FillRect(10, 4, 3, 2, color);
        canvas.FillRect(24, 4, 3, 2, color);
    }

    private sealed class Canvas
    {
        public FacePixel[] Pixels { get; } = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray();

        public void FillRect(int left, int top, int width, int height, FaceColor color)
        {
            for (var row = top; row < top + height; row++)
            {
                for (var column = left; column < left + width; column++)
                {
                    Plot(column, row, color);
                }
            }
        }

        public void Line(int x0, int y0, int x1, int y1, FaceColor color)
        {
            var dx = Math.Abs(x1 - x0);
            var sx = x0 < x1 ? 1 : -1;
            var dy = -Math.Abs(y1 - y0);
            var sy = y0 < y1 ? 1 : -1;
            var error = dx + dy;

            while (true)
            {
                Plot(x0, y0, color);
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                var doubledError = 2 * error;
                if (doubledError >= dy)
                {
                    error += dy;
                    x0 += sx;
                }

                if (doubledError <= dx)
                {
                    error += dx;
                    y0 += sy;
                }
            }
        }

        public void PlotMany(FaceColor color, params (int Column, int Row)[] points)
        {
            foreach (var point in points)
            {
                Plot(point.Column, point.Row, color);
            }
        }

        public void Sprite(params string[] rows)
        {
            var width = rows.Max(row => row.Length);
            var left = (FacePattern.Width - width) / 2;
            var top = (FacePattern.Height - rows.Length) / 2;

            for (var row = 0; row < rows.Length; row++)
            {
                for (var column = 0; column < rows[row].Length; column++)
                {
                    var symbol = rows[row][column];
                    if (symbol is '.' or ' ')
                    {
                        continue;
                    }

                    Plot(left + column, top + row, GetSpriteColor(symbol));
                }
            }
        }

        private static FaceColor GetSpriteColor(char symbol) => symbol switch
        {
            'B' => new FaceColor(0x60, 0xA5, 0xFA),
            'C' => new FaceColor(0x52, 0xE3, 0xFF),
            'G' => new FaceColor(0x22, 0xC5, 0x5E),
            'O' => new FaceColor(0xFB, 0x92, 0x3C),
            'P' => new FaceColor(0xF4, 0x72, 0xB6),
            'R' => new FaceColor(0xEF, 0x44, 0x44),
            'V' => new FaceColor(0xA8, 0x55, 0xF7),
            'W' => new FaceColor(0xF8, 0xFA, 0xFC),
            'Y' => new FaceColor(0xFA, 0xCC, 0x15),
            _ => FaceColor.Black
        };

        private void Plot(int column, int row, FaceColor color)
        {
            if (column < 0 || column >= FacePattern.Width || row < 0 || row >= FacePattern.Height)
            {
                return;
            }

            Pixels[(row * FacePattern.Width) + column] = new FacePixel(true, color);
        }
    }
}
