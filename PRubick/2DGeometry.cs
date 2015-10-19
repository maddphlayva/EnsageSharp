using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D9;

namespace PRubick
{
    internal class _2DGeometry
    {
        #region Fields
        private static Line line;
        private static Font font;
        #endregion

        public static void Init(Line _line, Font _font)
        {
            line = _line;
            font = _font;
        }

        #region Methods
        public static void DrawLine(float x1, float y1, float x2, float y2, float w, ColorBGRA Color)
        {
            Vector2[] vLine = new Vector2[2] { new Vector2(x1, y1), new Vector2(x2, y2) };

            line.GLLines = true;
            line.Antialias = true;
            line.Width = w;

            line.Begin();
            line.Draw(vLine, Color);
            line.End();

        }

        public static void DrawFilledBox(float x, float y, float w, float h, ColorBGRA Color)
        {
            Vector2[] vLine = new Vector2[2];

            line.GLLines = true;
            line.Antialias = false;
            line.Width = w;

            vLine[0].X = x + w / 2;
            vLine[0].Y = y;
            vLine[1].X = x + w / 2;
            vLine[1].Y = y + h;

            line.Begin();
            line.Draw(vLine, Color);
            line.End();
        }

        public static void DrawBox(float x, float y, float w, float h, float px, ColorBGRA Color)
        {
            DrawLine(x, y, x + w, y, 1, new ColorBGRA(0,0,0, 255));
            DrawLine(x, y, x, y + h, 1, new ColorBGRA(0, 0, 0, 255));
            DrawLine(x, y + h, x + w, y + h, 1, new ColorBGRA(0, 0, 0, 255));
            DrawLine(x + w, y, x + w, y + h, 1, new ColorBGRA(0, 0, 0, 255));
            DrawFilledBox(x, y + h, w, px, Color);
            DrawFilledBox(x - px, y, px, h, Color);
            DrawFilledBox(x, y - px, w, px, Color);
            DrawFilledBox(x + w, y, px, h, Color);
            DrawFilledBox(x, y, w, h, Color);
        }

        #region DrawText
        public static void DrawShadowText(string text, float x, float y, ColorBGRA color)
        {
            //font.DrawText(null, text, (int)x, (int)y - 1, color);
            font.DrawText(null, text, (int)x, (int)y, color);
        }
        #endregion

        public static Font GetFont()
        {
            return font;
        }
        #endregion
    }
}
