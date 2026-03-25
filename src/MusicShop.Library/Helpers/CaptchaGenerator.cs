using SkiaSharp;

namespace MusicShop.Library.Helpers;

/// <summary>
/// 驗證碼圖片產生器
/// 使用 SkiaSharp 繪製含雜訊干擾的驗證碼圖片，防止自動化工具辨識
/// </summary>
public static class CaptchaGenerator
{
    /// <summary>
    /// 可用字元（排除易混淆字元：0/O、1/I/l）
    /// </summary>
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";

    private const int ImageWidth = 150;
    private const int ImageHeight = 50;

    /// <summary>
    /// 產生驗證碼圖片
    /// </summary>
    /// <param name="length">驗證碼字元數（預設 4）</param>
    /// <returns>驗證碼文字與 PNG 圖片位元組</returns>
    public static (string Code, byte[] ImageBytes) GenerateCaptcha(int length = 4)
    {
        var random = Random.Shared;
        var code = GenerateCode(random, length);
        var imageBytes = DrawCaptchaImage(random, code);
        return (code, imageBytes);
    }

    /// <summary>
    /// 產生隨機驗證碼文字
    /// </summary>
    private static string GenerateCode(Random random, int length)
    {
        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = AllowedChars[random.Next(AllowedChars.Length)];
        }
        return new string(chars);
    }

    /// <summary>
    /// 繪製驗證碼圖片（含背景、文字扭曲、干擾線、雜點）
    /// </summary>
    private static byte[] DrawCaptchaImage(Random random, string code)
    {
        using var bitmap = new SKBitmap(ImageWidth, ImageHeight);
        using var canvas = new SKCanvas(bitmap);

        // 背景：淺色隨機色
        var bgColor = new SKColor(
            (byte)random.Next(220, 250),
            (byte)random.Next(220, 250),
            (byte)random.Next(220, 250));
        canvas.Clear(bgColor);

        // 繪製干擾線（6-8 條）
        DrawNoiseLines(canvas, random, lineCount: random.Next(6, 9));

        // 繪製驗證碼文字（每個字元獨立旋轉與著色）
        DrawText(canvas, random, code);

        // 繪製雜點（60-100 個）
        DrawNoiseDots(canvas, random, dotCount: random.Next(60, 101));

        // 編碼為 PNG
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <summary>
    /// 繪製每個字元（隨機旋轉、大小、顏色）
    /// </summary>
    private static void DrawText(SKCanvas canvas, Random random, string code)
    {
        var charWidth = (ImageWidth - 20) / code.Length;

        for (var i = 0; i < code.Length; i++)
        {
            var typeface = SKTypeface.FromFamilyName(
                "Arial",
                random.Next(2) == 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright);

            using var font = new SKFont(typeface, random.Next(24, 32));
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(
                    (byte)random.Next(20, 130),
                    (byte)random.Next(20, 130),
                    (byte)random.Next(20, 130))
            };

            var x = 10 + i * charWidth + random.Next(-3, 4);
            var y = ImageHeight / 2 + font.Size / 3 + random.Next(-4, 5);
            var angle = random.Next(-18, 19);

            canvas.Save();
            canvas.RotateDegrees(angle, x + charWidth / 2f, ImageHeight / 2f);
            canvas.DrawText(code[i].ToString(), x, y, SKTextAlign.Left, font, paint);
            canvas.Restore();
        }
    }

    /// <summary>
    /// 繪製干擾線（隨機顏色與位置）
    /// </summary>
    private static void DrawNoiseLines(SKCanvas canvas, Random random, int lineCount)
    {
        for (var i = 0; i < lineCount; i++)
        {
            using var paint = new SKPaint
            {
                IsAntialias = true,
                StrokeWidth = random.Next(1, 3),
                Color = new SKColor(
                    (byte)random.Next(100, 200),
                    (byte)random.Next(100, 200),
                    (byte)random.Next(100, 200)),
                Style = SKPaintStyle.Stroke
            };

            // 部分線條使用曲線增加辨識難度
            if (random.Next(2) == 0)
            {
                // 貝茲曲線
                using var path = new SKPath();
                path.MoveTo(random.Next(ImageWidth), random.Next(ImageHeight));
                path.CubicTo(
                    random.Next(ImageWidth), random.Next(ImageHeight),
                    random.Next(ImageWidth), random.Next(ImageHeight),
                    random.Next(ImageWidth), random.Next(ImageHeight));
                canvas.DrawPath(path, paint);
            }
            else
            {
                // 直線
                canvas.DrawLine(
                    random.Next(ImageWidth), random.Next(ImageHeight),
                    random.Next(ImageWidth), random.Next(ImageHeight),
                    paint);
            }
        }
    }

    /// <summary>
    /// 繪製雜點（增加背景雜訊）
    /// </summary>
    private static void DrawNoiseDots(SKCanvas canvas, Random random, int dotCount)
    {
        using var paint = new SKPaint
        {
            IsAntialias = false,
            Style = SKPaintStyle.Fill
        };

        for (var i = 0; i < dotCount; i++)
        {
            paint.Color = new SKColor(
                (byte)random.Next(50, 200),
                (byte)random.Next(50, 200),
                (byte)random.Next(50, 200));

            canvas.DrawPoint(random.Next(ImageWidth), random.Next(ImageHeight), paint);
        }
    }
}
