using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.Win32;

namespace Bin11
{
    
    public static class IconRenderer
    {
        private const int Size = 32; // стандартный размер иконки трея

        public static Icon CreateIcon(double percentFull)
        {
            percentFull = Math.Clamp(percentFull, 0.0, 1.0);
            Logger.Log($"IconRenderer.CreateIcon: старт, percent={percentFull:P0}.");

            using var bmp = new Bitmap(Size, Size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                Color trackColor = GetTrackColor();
                Color fillColor = GetFillColor(percentFull);

                float strokeWidth = 4.5f;
                var rect = new RectangleF(strokeWidth / 2, strokeWidth / 2,
                    Size - strokeWidth, Size - strokeWidth);

                // Фоновое кольцо (трек) — полупрозрачное, чтобы не спорило
                // с цветной дугой поверх.
                using (var trackPen = new Pen(Color.FromArgb(90, trackColor), strokeWidth))
                {
                    trackPen.StartCap = LineCap.Round;
                    trackPen.EndCap = LineCap.Round;
                    g.DrawEllipse(trackPen, rect);
                }

                // Дуга заполнения, начиная сверху (-90°), по часовой стрелке.
                if (percentFull > 0.001)
                {
                    float sweepAngle = (float)(360.0 * percentFull);
                    using var fillPen = new Pen(fillColor, strokeWidth);
                    fillPen.StartCap = LineCap.Round;
                    fillPen.EndCap = LineCap.Round;
                    g.DrawArc(fillPen, rect, -90f, sweepAngle);
                }
            }

            Logger.Log("IconRenderer.CreateIcon: Bitmap отрисован, вызываем GetHicon()...");
            IntPtr hIcon = bmp.GetHicon();
            Logger.Log($"IconRenderer.CreateIcon: получен hIcon=0x{hIcon:X}.");
            try
            {
                // Icon.FromHandle не копирует хендл — оборачиваем в новый Icon,
                // чтобы можно было безопасно освободить hIcon после Dispose.
                using var tempIcon = Icon.FromHandle(hIcon);
                var result = (Icon)tempIcon.Clone();
                Logger.Log("IconRenderer.CreateIcon: Clone() выполнен успешно.");
                return result;
            }
            finally
            {
                NativeIconCleanup.DestroyIcon(hIcon);
                Logger.Log("IconRenderer.CreateIcon: исходный hIcon уничтожен, выходим.");
            }
        }

        
        private static Color GetTrackColor()
        {
            bool isLightTaskbar = IsSystemLightTheme();
            return isLightTaskbar ? Color.Black : Color.White;
        }

        
        private static Color GetFillColor(double percent)
        {
            if (percent < 0.5)
            {
                // 0.0 .. 0.5 -> зелёный -> жёлтый
                double t = percent / 0.5;
                return Lerp(Color.FromArgb(46, 204, 113), Color.FromArgb(241, 196, 15), t);
            }
            else
            {
                // 0.5 .. 1.0 -> жёлтый -> красный
                double t = (percent - 0.5) / 0.5;
                return Lerp(Color.FromArgb(241, 196, 15), Color.FromArgb(231, 76, 60), t);
            }
        }

        private static Color Lerp(Color a, Color b, double t)
        {
            t = Math.Clamp(t, 0, 1);
            int r = (int)(a.R + (b.R - a.R) * t);
            int gg = (int)(a.G + (b.G - a.G) * t);
            int bl = (int)(a.B + (b.B - a.B) * t);
            return Color.FromArgb(r, gg, bl);
        }

        
        public static bool IsSystemLightTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("SystemUsesLightTheme");
                if (value is int i)
                    return i != 0;
            }
            catch
            {
            }
            return false;
        }

        
        public static Color GetAccentColor()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
                var value = key?.GetValue("AccentColor");
                if (value is int argbAbgr)
                {
                    // Реестр хранит цвет в формате ABGR, а не ARGB.
                    byte a = (byte)((argbAbgr >> 24) & 0xFF);
                    byte b = (byte)((argbAbgr >> 16) & 0xFF);
                    byte gg = (byte)((argbAbgr >> 8) & 0xFF);
                    byte r = (byte)(argbAbgr & 0xFF);
                    return Color.FromArgb(a == 0 ? 255 : a, r, gg, b);
                }
            }
            catch
            {
                // fallback ниже
            }
            return Color.FromArgb(0, 120, 215); // стандартный синий Windows как fallback
        }
    }

    
    internal static class NativeIconCleanup
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr handle);
    }
}
