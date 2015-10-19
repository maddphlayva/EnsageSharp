using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common;

using SharpDX;
using SharpDX.Direct3D9;

namespace PRubick
{
    #region Help classes
    enum ElementType
    {
        CHECKBOX, BUTTON, TEXT, CATEGORY
    }
    internal class EzElement
    {
        public ElementType Type = ElementType.TEXT;
        public List<EzElement> In = new List<EzElement>();
        public string Content = "";
        public bool isActive = false;
        public bool isOpen = false;
        public Entity Attached = null;
        public string Data = null;
        public float[] Position = new float[4]{0, 0, 0, 0};
        public EzElement(ElementType _Type, string _Content, bool _Active)
        {
            Type = _Type;
            Content = _Content;
            isActive = _Active;
        }
    }
    #endregion
    internal class EzGUI
    {
        #region Fields
        private float x = 0;
        private float y = 0;
        private float w = 300;
        private float h = 250;
        private string title = "EzGUI";

        private int cachedCount = 0;

        public EzElement Main;
        #endregion

        public EzGUI(float _x, float _y, string _title)
        {
            Main = new EzElement(ElementType.CATEGORY, "MAIN_CAT", true);
            x = _x;
            y = _y;
            title = _title;
        }

        #region Drawing
        public void Draw()
        {
            DrawBase();
            int i = 0;
            int iCat = 1;
            DrawElements(Main.In, ref iCat, ref i);
        }

        public void DrawElements(List<EzElement> category, ref int iCat, ref int i)
        {
            foreach (EzElement element in category)
            {
                i++;
                DrawElement(element, i, iCat);
                if (element.Type == ElementType.CATEGORY)
                {
                    if (element.isOpen)
                    {
                        int iCat2 = iCat + 1;
                        DrawElements(element.In, ref iCat2, ref i);
                    }
                }
            }
        }

        public void DrawElement(EzElement element, int i, int incat)
        {
            byte alpha = 140;
            if (element.isActive || element.isOpen) alpha = 255;
            int xoffset = 5 * incat;
            int yoffset = 20;
            ColorBGRA color = new ColorBGRA(32, 52, 123, alpha);
            element.Position = new float[4] { x + xoffset, x + xoffset + 15, y + yoffset * i, y + yoffset * i + 13 };
            if (MouseIn(element.Position)) { color.R = 10; }
            switch (element.Type)
            {
                case ElementType.CATEGORY:
                    _2DGeometry.DrawFilledBox(element.Position[0], element.Position[2], 15, 15, color);
                    _2DGeometry.DrawShadowText("> "+element.Content, x + xoffset + 18, y + yoffset * i, new ColorBGRA(12, 0, 222, 255));
                    break;
                case ElementType.CHECKBOX:
                    _2DGeometry.DrawFilledBox(element.Position[0], element.Position[2], 15, 15, color);
                    _2DGeometry.DrawShadowText(element.Content, x + xoffset + 18, element.Position[2], new ColorBGRA(12, 0, 222, 255));
                    break;
                case ElementType.TEXT:
                    _2DGeometry.DrawShadowText(element.Content, element.Position[0], element.Position[2], new ColorBGRA(12, 0, 222, 255));
                    break;
            }
        }

        public void DrawBase()
        {
            h = 30 + (Length() * 20);
            _2DGeometry.DrawBox(x, y, w, h, 10, new ColorBGRA(209, 219, 222, 90));
            _2DGeometry.DrawShadowText(title, x + 3, y, new ColorBGRA(0, 0, 0, 255));
            _2DGeometry.DrawLine(x, y + 13, x + w, y+13, 1,  new ColorBGRA(0,0,0,213));
            _2DGeometry.DrawShadowText("EzGUI • KiKRee", x+w-85, y + h - 15, new ColorBGRA(40, 48, 51, 255));
        }
        #endregion

        #region Methods
        public void AddMainElement(EzElement en)
        {
            Main.In.Add(en);
        }

        public void Count(EzElement elem, ref int i)
        {
            foreach (EzElement element in elem.In)
            {
                i++;
                if (element.Type == ElementType.CATEGORY && element.isOpen) Count(element, ref i);
            }
        }

        public int Length()
        {
            if (Utils.SleepCheck("ezmenu_count"))
            {
                int i = 0;
                Count(Main, ref i);
                cachedCount = i;
                Utils.Sleep(125, "ezmenu_count");
                return cachedCount;
            } else return cachedCount;
        }
        #endregion

        #region Events
        public static bool MouseIn(float[] pos)
        {
            if (Game.MouseScreenPosition.X >= pos[0] && Game.MouseScreenPosition.X <= pos[1] && Game.MouseScreenPosition.Y >= pos[2] && Game.MouseScreenPosition.Y <= pos[3]) { return true; }
            else return false;
        }
        
        public static void MouseClick(EzElement e)
        {
            foreach (EzElement element in e.In)
            {
                bool mouseIn = MouseIn(element.Position);
                if (element.Type == ElementType.CATEGORY)
                {
                    if (mouseIn) element.isOpen = !element.isOpen;
                    if (element.isOpen) MouseClick(element);
                }
                else if (mouseIn) element.isActive = !element.isActive;
            }
        }
        #endregion
    }
}
