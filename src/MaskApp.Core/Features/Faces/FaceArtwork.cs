namespace MaskApp.Core.Features.Faces;

internal static class FaceArtwork
{
    private static readonly FaceColor Dark = new(0x05, 0x07, 0x0D);
    private static readonly FaceColor Ink = new(0x0F, 0x17, 0x2A);
    private static readonly FaceColor White = new(0xF8, 0xFA, 0xFC);
    private static readonly FaceColor Bone = new(0xE7, 0xE5, 0xD8);
    private static readonly FaceColor Silver = new(0x94, 0xA3, 0xB8);
    private static readonly FaceColor Gray = new(0x47, 0x55, 0x69);
    private static readonly FaceColor Yellow = new(0xFA, 0xCC, 0x15);
    private static readonly FaceColor Gold = new(0xF5, 0x9E, 0x0B);
    private static readonly FaceColor Orange = new(0xF9, 0x73, 0x16);
    private static readonly FaceColor Tan = new(0xD9, 0x77, 0x45);
    private static readonly FaceColor Brown = new(0x78, 0x3F, 0x27);
    private static readonly FaceColor Red = new(0xEF, 0x44, 0x44);
    private static readonly FaceColor DeepRed = new(0x7F, 0x1D, 0x1D);
    private static readonly FaceColor Pink = new(0xF4, 0x72, 0xB6);
    private static readonly FaceColor Purple = new(0xA8, 0x55, 0xF7);
    private static readonly FaceColor DeepPurple = new(0x58, 0x21, 0x8C);
    private static readonly FaceColor Cyan = new(0x52, 0xE3, 0xFF);
    private static readonly FaceColor Blue = new(0x60, 0xA5, 0xFA);
    private static readonly FaceColor DeepBlue = new(0x1E, 0x3A, 0x8A);
    private static readonly FaceColor Green = new(0x22, 0xC5, 0x5E);
    private static readonly FaceColor Lime = new(0xA3, 0xE6, 0x35);
    private static readonly FaceColor DeepGreen = new(0x14, 0x53, 0x2D);
    private static readonly FaceColor PureWhite = new(0xFF, 0xFF, 0xFF);
    private static readonly FaceColor PureRed = new(0xFF, 0x00, 0x00);
    private static readonly FaceColor PureBlue = new(0x00, 0x00, 0xFF);
    private static readonly FaceColor PureGreen = new(0x00, 0xFF, 0x00);
    private static readonly FaceColor PureCyan = new(0x00, 0xFF, 0xFF);
    private static readonly FaceColor PureYellow = new(0xFF, 0xFF, 0x00);
    private static readonly FaceColor PureMagenta = new(0xFF, 0x00, 0xFF);
    private static readonly FaceColor PureOrange = new(0xFF, 0x80, 0x00);
    private static readonly FaceColor PureLime = new(0x80, 0xFF, 0x00);
    private static readonly FaceColor CalibrationGray = new(0x40, 0x40, 0x40);

    public static void Draw(string artworkId, FaceArtCanvas canvas)
    {
        switch (artworkId)
        {
            case "happy": DrawHappy(canvas); break;
            case "sad": DrawSad(canvas); break;
            case "angry": DrawAngry(canvas); break;
            case "surprised": DrawSurprised(canvas); break;
            case "meh": DrawMeh(canvas); break;
            case "wink": DrawWink(canvas); break;
            case "cool-shades": DrawCoolShades(canvas); break;
            case "heart-eyes": DrawHeartEyes(canvas); break;
            case "starstruck": DrawStarstruck(canvas); break;
            case "big-laugh": DrawBigLaugh(canvas); break;
            case "tongue-out": DrawTongueOut(canvas); break;
            case "vampire": DrawVampire(canvas); break;
            case "robot": DrawRobot(canvas); break;
            case "alien": DrawAlien(canvas); break;
            case "cat": DrawCat(canvas); break;
            case "puppy": DrawPuppy(canvas); break;
            case "frog": DrawFrog(canvas); break;
            case "panda": DrawPanda(canvas); break;
            case "skull": DrawSkull(canvas); break;
            case "ghost": DrawGhost(canvas); break;
            case "little-devil": DrawLittleDevil(canvas); break;
            case "clown": DrawClown(canvas); break;
            case "pirate": DrawPirate(canvas); break;
            case "ninja": DrawNinja(canvas); break;
            case "cowboy": DrawCowboy(canvas); break;
            case "mustache": DrawMustache(canvas); break;
            case "dj": DrawDj(canvas); break;
            case "three-eyed-monster": DrawThreeEyedMonster(canvas); break;
            case "cyclops": DrawCyclops(canvas); break;
            case "sleepy": DrawSleepy(canvas); break;
            case "mask-calibration": DrawMaskCalibration(canvas); break;
            case "holy-priest-cross": DrawHolyPriestMask(canvas, PureWhite, FaceColor.Black); break;
            case "holy-priest-inverted": DrawHolyPriestMask(canvas, FaceColor.Black, PureWhite); break;
            case "holy-priest-red": DrawHolyPriestMask(canvas, PureRed, FaceColor.Black); break;
            case "holy-priest-blue": DrawHolyPriestMask(canvas, PureBlue, FaceColor.Black); break;
            case "holy-priest-gold": DrawHolyPriestMask(canvas, PureYellow, FaceColor.Black); break;
            case "holy-priest-blackout": canvas.FillRect(0, 0, FacePattern.Width, FacePattern.Height, FaceColor.Black); break;
            default: throw new ArgumentOutOfRangeException(nameof(artworkId), artworkId, "Unknown face artwork.");
        }
    }

    private static void DrawHappy(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Yellow, Gold, Orange, DeepRed);
        DrawEye(canvas, 14, 23, DeepBlue, Cyan);
        DrawEye(canvas, 32, 23, DeepBlue, Cyan);
        DrawBlush(canvas, Pink);
        DrawOpenSmile(canvas, 23, 40, 13, 9, Pink);
        canvas.FillCircle(23, 31, 2, Orange);
    }

    private static void DrawSad(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Blue, DeepBlue, Cyan, Ink);
        canvas.Line(8, 19, 18, 16, Ink, 2);
        canvas.Line(28, 16, 38, 19, Ink, 2);
        DrawEye(canvas, 14, 24, Purple, White);
        DrawEye(canvas, 32, 24, Purple, White);
        canvas.FillEllipse(34, 34, 2, 5, Cyan);
        canvas.Line(14, 45, 20, 39, Ink, 2);
        canvas.Line(20, 39, 27, 39, Ink, 2);
        canvas.Line(27, 39, 33, 45, Ink, 2);
    }

    private static void DrawAngry(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Red, DeepRed, Orange, Dark);
        canvas.FillPolygon(Ink, (6, 15), (20, 20), (18, 25), (7, 21));
        canvas.FillPolygon(Ink, (40, 15), (28, 20), (29, 25), (41, 21));
        DrawEye(canvas, 14, 24, Yellow, White);
        DrawEye(canvas, 32, 24, Yellow, White);
        canvas.FillEllipse(23, 42, 13, 7, Dark);
        DrawTeeth(canvas, 14, 39, 18, 4);
        canvas.Line(11, 34, 17, 31, Orange, 2);
        canvas.Line(35, 34, 29, 31, Orange, 2);
    }

    private static void DrawSurprised(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Cyan, DeepBlue, Blue, Ink);
        DrawEye(canvas, 14, 22, Purple, White, 7, 8);
        DrawEye(canvas, 32, 22, Purple, White, 7, 8);
        canvas.FillEllipse(23, 42, 8, 11, Ink);
        canvas.FillEllipse(23, 45, 5, 6, DeepPurple);
        canvas.FillCircle(23, 31, 2, Blue);
        canvas.Line(8, 12, 17, 9, White, 1);
        canvas.Line(29, 9, 38, 12, White, 1);
    }

    private static void DrawMeh(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Silver, Gray, White, Ink);
        canvas.FillEllipse(14, 23, 7, 5, Ink);
        canvas.FillEllipse(32, 23, 7, 5, Ink);
        canvas.FillEllipse(14, 23, 4, 2, Blue);
        canvas.FillEllipse(32, 23, 4, 2, Blue);
        canvas.Line(12, 42, 34, 42, Ink, 3);
        canvas.Line(15, 45, 31, 45, White, 1);
        canvas.FillCircle(23, 32, 2, Gray);
    }

    private static void DrawWink(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Pink, DeepPurple, Purple, Ink);
        canvas.Line(7, 23, 14, 19, Ink, 2);
        canvas.Line(14, 19, 20, 23, Ink, 2);
        DrawEye(canvas, 32, 22, Cyan, White);
        canvas.FillCircle(10, 34, 3, Red);
        DrawOpenSmile(canvas, 24, 41, 12, 8, Red);
        canvas.Line(31, 37, 38, 33, White, 2);
    }

    private static void DrawCoolShades(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Gold, Orange, Yellow, DeepRed);
        canvas.FillRect(5, 18, 17, 12, Dark);
        canvas.FillRect(24, 18, 17, 12, Dark);
        canvas.FillRect(7, 20, 13, 7, DeepBlue);
        canvas.FillRect(26, 20, 13, 7, DeepBlue);
        canvas.Line(8, 20, 17, 27, Cyan, 2);
        canvas.Line(27, 20, 36, 27, Cyan, 2);
        canvas.FillRect(20, 21, 6, 3, Ink);
        DrawOpenSmile(canvas, 23, 42, 12, 7, Pink);
    }

    private static void DrawHeartEyes(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Pink, DeepRed, Orange, DeepPurple);
        DrawHeart(canvas, 14, 23, Red);
        DrawHeart(canvas, 32, 23, Red);
        canvas.FillCircle(14, 22, 2, White);
        canvas.FillCircle(32, 22, 2, White);
        DrawOpenSmile(canvas, 23, 42, 12, 8, Pink);
        DrawBlush(canvas, Yellow);
    }

    private static void DrawStarstruck(FaceArtCanvas canvas)
    {
        DrawHead(canvas, DeepPurple, DeepBlue, Purple, Dark);
        DrawStar(canvas, 14, 22, 8, 3, Yellow);
        DrawStar(canvas, 32, 22, 8, 3, Yellow);
        canvas.FillCircle(14, 22, 2, White);
        canvas.FillCircle(32, 22, 2, White);
        DrawOpenSmile(canvas, 23, 42, 13, 9, Pink);
        canvas.PlotMany(Cyan, (7, 11), (9, 8), (37, 9), (40, 13), (6, 39), (39, 38));
    }

    private static void DrawBigLaugh(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Yellow, Orange, Gold, DeepRed);
        DrawClosedEye(canvas, 14, 23, Ink);
        DrawClosedEye(canvas, 32, 23, Ink);
        canvas.FillEllipse(23, 41, 16, 13, Dark);
        DrawTeeth(canvas, 10, 33, 26, 7);
        canvas.FillEllipse(23, 48, 10, 5, Pink);
        canvas.FillEllipse(7, 31, 2, 5, Cyan);
        canvas.FillEllipse(39, 31, 2, 5, Cyan);
    }

    private static void DrawTongueOut(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Lime, DeepGreen, Green, Ink);
        DrawClosedEye(canvas, 14, 23, Dark);
        DrawEye(canvas, 32, 22, DeepBlue, White);
        canvas.FillEllipse(23, 40, 12, 8, Dark);
        canvas.FillEllipse(23, 47, 7, 10, Pink);
        canvas.Line(23, 45, 23, 53, Red, 1);
        canvas.FillCircle(8, 34, 3, Yellow);
        canvas.FillCircle(38, 34, 3, Yellow);
    }

    private static void DrawVampire(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Bone, Gray, White, Dark);
        canvas.FillPolygon(Dark, (4, 13), (10, 3), (16, 13), (23, 4), (30, 13), (37, 3), (42, 16), (41, 23), (5, 23));
        DrawEye(canvas, 14, 25, Red, White);
        DrawEye(canvas, 32, 25, Red, White);
        canvas.FillEllipse(23, 41, 13, 8, DeepRed);
        DrawTeeth(canvas, 13, 36, 20, 4);
        canvas.FillPolygon(White, (14, 39), (19, 39), (17, 49));
        canvas.FillPolygon(White, (27, 39), (32, 39), (29, 49));
        canvas.Line(17, 49, 17, 54, Red, 2);
    }

    private static void DrawRobot(FaceArtCanvas canvas)
    {
        canvas.FillRect(5, 7, 36, 45, Ink);
        canvas.FillRect(7, 9, 32, 41, Silver);
        canvas.FillRect(3, 20, 4, 15, DeepBlue);
        canvas.FillRect(39, 20, 4, 15, DeepBlue);
        canvas.Line(23, 9, 23, 3, Gray, 2);
        canvas.FillCircle(23, 3, 3, Red);
        canvas.FillRect(10, 18, 11, 10, DeepBlue);
        canvas.FillRect(25, 18, 11, 10, DeepBlue);
        canvas.FillRect(12, 20, 7, 6, Cyan);
        canvas.FillRect(27, 20, 7, 6, Cyan);
        canvas.FillRect(12, 36, 22, 9, Dark);
        for (var column = 14; column <= 32; column += 4)
        {
            canvas.FillRect(column, 38, 2, 5, Yellow);
        }
        canvas.PlotMany(Red, (9, 12), (37, 12), (9, 47), (37, 47));
    }

    private static void DrawAlien(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Green, DeepGreen, Lime, Ink);
        canvas.FillEllipse(14, 24, 10, 13, Dark);
        canvas.FillEllipse(32, 24, 10, 13, Dark);
        canvas.FillEllipse(15, 23, 4, 7, Cyan);
        canvas.FillEllipse(31, 23, 4, 7, Cyan);
        canvas.FillCircle(16, 20, 1, White);
        canvas.FillCircle(32, 20, 1, White);
        canvas.FillEllipse(23, 44, 8, 3, Ink);
        canvas.PlotMany(DeepGreen, (21, 34), (25, 34));
        canvas.Line(17, 8, 21, 14, Cyan, 1);
        canvas.Line(29, 8, 25, 14, Cyan, 1);
    }

    private static void DrawCat(FaceArtCanvas canvas)
    {
        canvas.FillPolygon(Brown, (3, 20), (7, 2), (18, 13), (28, 13), (39, 2), (43, 20), (41, 47), (33, 56), (13, 56), (5, 47));
        canvas.FillPolygon(Pink, (8, 14), (10, 6), (16, 15));
        canvas.FillPolygon(Pink, (30, 15), (36, 6), (38, 14));
        canvas.FillEllipse(23, 34, 18, 21, Orange);
        DrawEye(canvas, 14, 27, Green, Yellow, 7, 6);
        DrawEye(canvas, 32, 27, Green, Yellow, 7, 6);
        canvas.FillEllipse(23, 38, 4, 3, Pink);
        canvas.Line(23, 40, 19, 44, Dark, 2);
        canvas.Line(23, 40, 27, 44, Dark, 2);
        canvas.Line(17, 40, 3, 37, White, 1);
        canvas.Line(17, 44, 2, 46, White, 1);
        canvas.Line(29, 40, 43, 37, White, 1);
        canvas.Line(29, 44, 44, 46, White, 1);
        canvas.Line(18, 15, 20, 21, Brown, 2);
        canvas.Line(28, 15, 26, 21, Brown, 2);
    }

    private static void DrawPuppy(FaceArtCanvas canvas)
    {
        canvas.FillEllipse(23, 31, 20, 25, Brown);
        canvas.FillEllipse(6, 25, 7, 18, DeepRed);
        canvas.FillEllipse(40, 25, 7, 18, DeepRed);
        canvas.FillEllipse(23, 31, 17, 23, Tan);
        DrawEye(canvas, 14, 26, DeepBlue, White, 6, 6);
        DrawEye(canvas, 32, 26, DeepBlue, White, 6, 6);
        canvas.FillEllipse(23, 40, 11, 10, Bone);
        canvas.FillEllipse(23, 36, 5, 4, Dark);
        canvas.Line(23, 40, 18, 44, Brown, 2);
        canvas.Line(23, 40, 28, 44, Brown, 2);
        canvas.FillEllipse(23, 49, 6, 7, Pink);
        canvas.Line(23, 48, 23, 54, Red, 1);
    }

    private static void DrawFrog(FaceArtCanvas canvas)
    {
        canvas.FillEllipse(23, 35, 21, 21, DeepGreen);
        canvas.FillEllipse(23, 34, 19, 19, Green);
        canvas.FillCircle(11, 15, 10, DeepGreen);
        canvas.FillCircle(35, 15, 10, DeepGreen);
        DrawEye(canvas, 11, 15, DeepBlue, White, 8, 8);
        DrawEye(canvas, 35, 15, DeepBlue, White, 8, 8);
        canvas.FillEllipse(23, 41, 15, 9, Ink);
        canvas.FillEllipse(23, 44, 11, 5, Lime);
        canvas.FillCircle(8, 36, 4, Yellow);
        canvas.FillCircle(38, 36, 4, Yellow);
        canvas.PlotMany(DeepGreen, (20, 31), (26, 31));
    }

    private static void DrawPanda(FaceArtCanvas canvas)
    {
        canvas.FillCircle(9, 11, 9, Dark);
        canvas.FillCircle(37, 11, 9, Dark);
        canvas.FillEllipse(23, 31, 21, 26, Gray);
        canvas.FillEllipse(23, 31, 19, 24, White);
        canvas.FillEllipse(14, 25, 10, 12, Dark);
        canvas.FillEllipse(32, 25, 10, 12, Dark);
        DrawEye(canvas, 14, 25, Green, White, 5, 6);
        DrawEye(canvas, 32, 25, Green, White, 5, 6);
        canvas.FillEllipse(23, 39, 5, 4, Dark);
        canvas.Line(23, 42, 18, 47, Dark, 2);
        canvas.Line(23, 42, 28, 47, Dark, 2);
        canvas.FillEllipse(23, 49, 6, 3, Pink);
    }

    private static void DrawSkull(FaceArtCanvas canvas)
    {
        canvas.FillEllipse(23, 27, 21, 25, Gray);
        canvas.FillEllipse(23, 27, 19, 23, Bone);
        canvas.FillRect(10, 36, 26, 17, Bone);
        canvas.FillEllipse(14, 25, 9, 11, Dark);
        canvas.FillEllipse(32, 25, 9, 11, Dark);
        canvas.FillEllipse(14, 25, 3, 5, Purple);
        canvas.FillEllipse(32, 25, 3, 5, Purple);
        canvas.FillPolygon(Dark, (23, 31), (18, 39), (22, 40), (23, 37), (24, 40), (28, 39));
        canvas.FillRect(12, 43, 22, 8, Dark);
        DrawTeeth(canvas, 13, 43, 20, 7);
        canvas.Line(8, 12, 17, 20, DeepPurple, 2);
        canvas.Line(17, 20, 14, 29, DeepPurple, 1);
        canvas.Line(37, 10, 30, 18, White, 1);
    }

    private static void DrawGhost(FaceArtCanvas canvas)
    {
        canvas.FillEllipse(23, 27, 20, 25, DeepBlue);
        canvas.FillEllipse(23, 27, 18, 23, White);
        canvas.FillRect(5, 27, 36, 23, White);
        canvas.FillPolygon(DeepBlue, (5, 49), (10, 56), (15, 49), (20, 56), (25, 49), (30, 56), (35, 49), (41, 55), (41, 45), (5, 45));
        canvas.FillPolygon(White, (6, 47), (11, 54), (16, 47), (21, 54), (26, 47), (31, 54), (36, 47), (40, 52), (40, 42), (6, 42));
        canvas.FillEllipse(14, 26, 6, 9, Dark);
        canvas.FillEllipse(32, 26, 6, 9, Dark);
        canvas.FillEllipse(23, 41, 7, 9, DeepPurple);
        canvas.FillCircle(12, 22, 2, Cyan);
        canvas.FillCircle(30, 22, 2, Cyan);
    }

    private static void DrawLittleDevil(FaceArtCanvas canvas)
    {
        canvas.FillPolygon(Yellow, (5, 18), (3, 1), (17, 13));
        canvas.FillPolygon(Yellow, (41, 18), (43, 1), (29, 13));
        DrawHead(canvas, Red, DeepRed, Orange, Dark);
        canvas.FillPolygon(Red, (4, 29), (0, 21), (2, 37));
        canvas.FillPolygon(Red, (42, 29), (46, 21), (44, 37));
        canvas.FillPolygon(Ink, (6, 18), (20, 23), (18, 28), (7, 24));
        canvas.FillPolygon(Ink, (40, 18), (28, 23), (29, 28), (41, 24));
        DrawEye(canvas, 14, 26, Yellow, White, 6, 5);
        DrawEye(canvas, 32, 26, Yellow, White, 6, 5);
        canvas.FillEllipse(23, 42, 13, 8, Dark);
        DrawTeeth(canvas, 13, 38, 20, 4);
        canvas.FillPolygon(Ink, (18, 51), (23, 57), (28, 51));
    }

    private static void DrawClown(FaceArtCanvas canvas)
    {
        canvas.FillCircle(5, 17, 8, Purple);
        canvas.FillCircle(41, 17, 8, Purple);
        canvas.FillCircle(8, 8, 7, Pink);
        canvas.FillCircle(38, 8, 7, Pink);
        DrawHead(canvas, White, Blue, Bone, DeepPurple);
        canvas.FillPolygon(Cyan, (9, 15), (19, 15), (14, 31));
        canvas.FillPolygon(Cyan, (27, 15), (37, 15), (32, 31));
        DrawEye(canvas, 14, 23, DeepBlue, White, 5, 6);
        DrawEye(canvas, 32, 23, DeepBlue, White, 5, 6);
        canvas.FillCircle(23, 33, 6, Red);
        canvas.FillEllipse(23, 44, 15, 9, DeepRed);
        DrawTeeth(canvas, 11, 39, 24, 5);
        canvas.FillEllipse(23, 49, 9, 4, Pink);
    }

    private static void DrawPirate(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Tan, Brown, Gold, Dark);
        canvas.FillPolygon(DeepRed, (3, 17), (8, 5), (38, 5), (43, 17), (36, 20), (10, 20));
        canvas.FillCircle(36, 8, 4, White);
        canvas.FillCircle(36, 8, 2, Dark);
        canvas.FillEllipse(14, 26, 9, 8, Dark);
        canvas.Line(5, 18, 22, 31, Dark, 2);
        DrawEye(canvas, 32, 26, Green, White, 6, 7);
        canvas.Line(36, 31, 31, 36, DeepRed, 2);
        canvas.FillEllipse(23, 44, 12, 8, Brown);
        canvas.FillEllipse(23, 42, 9, 5, Dark);
        DrawTeeth(canvas, 17, 39, 12, 4);
        canvas.Line(10, 48, 15, 55, Dark, 2);
        canvas.Line(36, 48, 31, 55, Dark, 2);
    }

    private static void DrawNinja(FaceArtCanvas canvas)
    {
        canvas.FillEllipse(23, 30, 22, 28, DeepPurple);
        canvas.FillEllipse(23, 31, 19, 25, Ink);
        canvas.FillPolygon(Dark, (4, 15), (19, 2), (42, 18), (38, 29), (7, 29));
        canvas.FillRect(6, 20, 34, 14, Tan);
        canvas.FillPolygon(Ink, (5, 18), (21, 27), (5, 35));
        canvas.FillPolygon(Ink, (41, 18), (25, 27), (41, 35));
        DrawEye(canvas, 14, 27, Red, White, 7, 5);
        DrawEye(canvas, 32, 27, Red, White, 7, 5);
        canvas.FillPolygon(Ink, (5, 35), (23, 29), (41, 35), (38, 56), (8, 56));
        canvas.Line(8, 42, 38, 42, Purple, 2);
        canvas.FillPolygon(Purple, (38, 10), (46, 4), (42, 19));
    }

    private static void DrawCowboy(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Tan, Brown, Gold, Dark);
        canvas.FillPolygon(Brown, (3, 16), (9, 12), (12, 1), (34, 1), (37, 12), (43, 16), (36, 20), (10, 20));
        canvas.FillRect(6, 12, 34, 6, Gold);
        DrawEye(canvas, 14, 27, DeepBlue, White, 6, 6);
        DrawEye(canvas, 32, 27, DeepBlue, White, 6, 6);
        canvas.FillEllipse(23, 37, 4, 5, Orange);
        canvas.FillPolygon(Dark, (8, 43), (19, 39), (23, 43), (27, 39), (38, 43), (30, 50), (23, 47), (16, 50));
        canvas.Line(16, 53, 30, 53, Red, 3);
        canvas.FillCircle(23, 54, 2, Yellow);
    }

    private static void DrawMustache(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Bone, Gray, White, Dark);
        canvas.FillRect(10, 1, 26, 8, Ink);
        canvas.FillRect(6, 8, 34, 5, DeepPurple);
        canvas.FillRect(13, 11, 20, 3, Purple);
        DrawEye(canvas, 14, 25, DeepBlue, White, 6, 6);
        DrawEye(canvas, 32, 25, DeepBlue, White, 6, 6);
        canvas.FillCircle(33, 25, 9, Gold);
        canvas.FillCircle(33, 25, 7, Dark);
        canvas.FillCircle(33, 25, 5, White);
        canvas.Line(40, 32, 37, 48, Gold, 1);
        canvas.FillPolygon(Brown, (5, 41), (18, 36), (23, 42), (28, 36), (41, 41), (31, 50), (23, 46), (15, 50));
        canvas.FillPolygon(DeepRed, (15, 52), (23, 48), (31, 52), (27, 57), (23, 54), (19, 57));
    }

    private static void DrawDj(FaceArtCanvas canvas)
    {
        DrawHead(canvas, DeepPurple, DeepBlue, Purple, Dark);
        canvas.FillEllipse(4, 29, 7, 18, Ink);
        canvas.FillEllipse(42, 29, 7, 18, Ink);
        canvas.FillRect(3, 24, 7, 16, Cyan);
        canvas.FillRect(36, 24, 7, 16, Pink);
        canvas.FillRect(6, 18, 34, 14, Dark);
        canvas.FillRect(8, 20, 14, 9, DeepBlue);
        canvas.FillRect(24, 20, 14, 9, DeepRed);
        canvas.Line(9, 20, 19, 28, Cyan, 2);
        canvas.Line(27, 20, 37, 28, Pink, 2);
        canvas.FillRect(11, 39, 24, 12, Ink);
        var heights = new[] { 4, 8, 6, 10, 7, 5 };
        for (var index = 0; index < heights.Length; index++)
        {
            canvas.FillRect(13 + (index * 4), 49 - heights[index], 2, heights[index], index % 2 == 0 ? Cyan : Pink);
        }
        canvas.Line(9, 9, 37, 9, Yellow, 2);
    }

    private static void DrawThreeEyedMonster(FaceArtCanvas canvas)
    {
        canvas.FillPolygon(Yellow, (8, 13), (5, 1), (17, 10));
        canvas.FillPolygon(Yellow, (38, 13), (41, 1), (29, 10));
        DrawHead(canvas, Purple, DeepPurple, Pink, Dark);
        DrawEye(canvas, 11, 25, Green, White, 6, 7);
        DrawEye(canvas, 23, 20, Cyan, White, 6, 7);
        DrawEye(canvas, 35, 25, Green, White, 6, 7);
        canvas.FillEllipse(23, 43, 15, 10, Dark);
        for (var column = 11; column <= 35; column += 6)
        {
            canvas.FillPolygon(White, (column, 37), (column + 4, 37), (column + 2, 46));
        }
        canvas.FillEllipse(23, 49, 9, 4, Green);
        canvas.PlotMany(Yellow, (8, 35), (38, 35), (11, 11), (35, 11));
    }

    private static void DrawCyclops(FaceArtCanvas canvas)
    {
        DrawHead(canvas, Blue, DeepBlue, Cyan, Ink);
        canvas.FillEllipse(23, 24, 15, 16, Dark);
        canvas.FillEllipse(23, 24, 12, 13, White);
        canvas.FillEllipse(23, 24, 7, 10, Purple);
        canvas.FillEllipse(23, 24, 3, 7, Dark);
        canvas.FillCircle(20, 20, 2, White);
        canvas.FillEllipse(23, 44, 11, 7, Ink);
        DrawTeeth(canvas, 16, 40, 14, 4);
        canvas.FillEllipse(23, 49, 6, 3, Pink);
        canvas.FillCircle(8, 36, 3, Green);
        canvas.FillCircle(38, 36, 3, Green);
    }

    private static void DrawSleepy(FaceArtCanvas canvas)
    {
        DrawHead(canvas, DeepBlue, Ink, Purple, Dark);
        canvas.FillCircle(34, 13, 8, Yellow);
        canvas.FillCircle(38, 10, 8, DeepBlue);
        DrawClosedEye(canvas, 14, 27, Cyan);
        DrawClosedEye(canvas, 32, 27, Cyan);
        canvas.Line(17, 43, 29, 43, Pink, 2);
        canvas.Line(30, 17, 36, 17, White, 1);
        canvas.Line(36, 17, 30, 23, White, 1);
        canvas.Line(30, 23, 36, 23, White, 1);
        canvas.Line(36, 28, 42, 28, Cyan, 1);
        canvas.Line(42, 28, 36, 34, Cyan, 1);
        canvas.Line(36, 34, 42, 34, Cyan, 1);
        canvas.PlotMany(White, (8, 10), (13, 7), (20, 12), (7, 39), (39, 42));
    }

    private static void DrawHolyPriestMask(
        FaceArtCanvas canvas,
        FaceColor shell,
        FaceColor cross)
    {
        canvas.FillRect(0, 0, FacePattern.Width, FacePattern.Height, shell);

        // Keep the top and side terminals inside the calibrated LED envelope;
        // the lower stem deliberately continues through the final display row.
        canvas.FillRect(20, 5, 7, FacePattern.Height - 5, cross);
        canvas.FillRect(5, 15, 36, 5, cross);

        (int Row, int Left, int Width)[] eyeApertures =
        [
            (16, 5, 11),
            (16, 30, 11),
            (17, 6, 12),
            (17, 28, 11),
            (18, 7, 11),
            (18, 28, 11),
            (19, 9, 6),
            (19, 31, 6)
        ];
        foreach (var (row, left, width) in eyeApertures)
        {
            canvas.FillRect(left, row, width, 1, FaceColor.Black);
        }
    }

    private static void DrawMaskCalibration(FaceArtCanvas canvas)
    {
        canvas.FillRect(0, 0, FacePattern.Width, FacePattern.Height, CalibrationGray);

        canvas.FillRect(0, 0, FacePattern.Width, 1, PureRed);
        canvas.FillRect(0, FacePattern.Height - 1, FacePattern.Width, 1, PureBlue);
        canvas.FillRect(0, 1, 1, FacePattern.Height - 2, PureYellow);
        canvas.FillRect(FacePattern.Width - 1, 1, 1, FacePattern.Height - 2, PureGreen);

        (int Row, FaceColor Color)[] eyeGuideRows =
        [
            (12, PureRed),
            (14, PureOrange),
            (16, PureYellow),
            (18, PureGreen),
            (20, PureCyan),
            (22, PureBlue)
        ];
        foreach (var (row, color) in eyeGuideRows)
        {
            canvas.FillRect(1, row, FacePattern.Width - 2, 1, color);
        }

        for (var column = 5; column < FacePattern.Width - 1; column += 5)
        {
            canvas.FillRect(column, 11, 1, 13, PureWhite);
        }

        canvas.FillRect(23, 11, 1, 13, PureMagenta);

        DrawCalibrationAnchor(canvas, 5, 5, PureRed);
        DrawCalibrationAnchor(canvas, 23, 5, PureGreen);
        DrawCalibrationAnchor(canvas, 40, 5, PureBlue);
        DrawCalibrationAnchor(canvas, 5, 29, PureYellow);
        DrawCalibrationAnchor(canvas, 23, 29, PureWhite);
        DrawCalibrationAnchor(canvas, 40, 29, PureCyan);
        DrawCalibrationAnchor(canvas, 5, 52, PureOrange);
        DrawCalibrationAnchor(canvas, 23, 52, PureMagenta);
        DrawCalibrationAnchor(canvas, 40, 52, PureLime);
    }

    private static void DrawCalibrationAnchor(
        FaceArtCanvas canvas,
        int centerX,
        int centerY,
        FaceColor color)
    {
        canvas.FillRect(centerX - 1, centerY - 1, 3, 3, color);
        canvas.FillRect(centerX, centerY, 1, 1, FaceColor.Black);
    }

    private static void DrawHead(
        FaceArtCanvas canvas,
        FaceColor main,
        FaceColor shadow,
        FaceColor highlight,
        FaceColor outline)
    {
        canvas.FillEllipse(23, 29, 22, 28, outline);
        canvas.FillEllipse(23, 29, 20, 26, shadow);
        canvas.FillEllipse(26, 28, 17, 24, main);
        canvas.FillEllipse(31, 15, 6, 8, highlight);
        canvas.Line(8, 46, 15, 53, shadow, 2);
        canvas.Line(38, 46, 31, 53, main, 2);
    }

    private static void DrawEye(
        FaceArtCanvas canvas,
        int centerX,
        int centerY,
        FaceColor iris,
        FaceColor sclera,
        int radiusX = 6,
        int radiusY = 7)
    {
        canvas.FillEllipse(centerX, centerY, radiusX + 1, radiusY + 1, Ink);
        canvas.FillEllipse(centerX, centerY, radiusX, radiusY, sclera);
        canvas.FillEllipse(centerX, centerY + 1, Math.Max(2, radiusX / 2), Math.Max(3, radiusY / 2), iris);
        canvas.FillEllipse(centerX, centerY + 1, 2, Math.Max(2, radiusY / 3), Dark);
        canvas.FillCircle(centerX - 1, centerY - 2, 1, White);
    }

    private static void DrawClosedEye(FaceArtCanvas canvas, int centerX, int centerY, FaceColor color)
    {
        canvas.Line(centerX - 7, centerY, centerX, centerY + 4, color, 2);
        canvas.Line(centerX, centerY + 4, centerX + 7, centerY, color, 2);
        canvas.Line(centerX - 5, centerY - 1, centerX - 8, centerY - 4, color, 1);
        canvas.Line(centerX + 5, centerY - 1, centerX + 8, centerY - 4, color, 1);
    }

    private static void DrawOpenSmile(
        FaceArtCanvas canvas,
        int centerX,
        int centerY,
        int radiusX,
        int radiusY,
        FaceColor tongue)
    {
        canvas.FillEllipse(centerX, centerY, radiusX + 1, radiusY + 1, DeepRed);
        canvas.FillEllipse(centerX, centerY, radiusX, radiusY, Dark);
        DrawTeeth(canvas, centerX - radiusX + 2, centerY - radiusY + 2, (radiusX * 2) - 4, Math.Max(3, radiusY / 2));
        canvas.FillEllipse(centerX, centerY + radiusY - 2, Math.Max(3, radiusX - 4), Math.Max(2, radiusY / 3), tongue);
    }

    private static void DrawTeeth(FaceArtCanvas canvas, int left, int top, int width, int height)
    {
        canvas.FillRect(left, top, width, height, White);
        for (var column = left + 4; column < left + width; column += 4)
        {
            canvas.Line(column, top, column, top + height - 1, Gray);
        }
    }

    private static void DrawBlush(FaceArtCanvas canvas, FaceColor color)
    {
        canvas.FillEllipse(7, 34, 4, 3, color);
        canvas.FillEllipse(39, 34, 4, 3, color);
    }

    private static void DrawHeart(FaceArtCanvas canvas, int centerX, int centerY, FaceColor color)
    {
        canvas.FillCircle(centerX - 3, centerY - 2, 4, color);
        canvas.FillCircle(centerX + 3, centerY - 2, 4, color);
        canvas.FillPolygon(color, (centerX - 7, centerY), (centerX + 7, centerY), (centerX, centerY + 10));
    }

    private static void DrawStar(
        FaceArtCanvas canvas,
        int centerX,
        int centerY,
        int outerRadius,
        int innerRadius,
        FaceColor color)
    {
        var points = Enumerable.Range(0, 10)
            .Select(index =>
            {
                var angle = (-Math.PI / 2) + (index * Math.PI / 5);
                var radius = index % 2 == 0 ? outerRadius : innerRadius;
                return (
                    X: centerX + (int)Math.Round(Math.Cos(angle) * radius),
                    Y: centerY + (int)Math.Round(Math.Sin(angle) * radius));
            })
            .ToArray();
        canvas.FillPolygon(color, points);
    }
}
