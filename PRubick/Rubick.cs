using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;

using SharpDX;
using SharpDX.Direct3D9;

namespace PRubick
{
    internal class Rubick
    {

        #region Fields

        private static string VERSION = "v2.0.0.0";
        private static bool menuOn = true;
        private static bool loaded = false;

        private static EzElement stealIfHave;
        private static EzElement enabled;

        private static EzGUI gui;
        private static EzElement spcat = null;
        private static EzElement heroes = null;

        private static Hero myHero = null;
        private static Ability spellSteal = null;
        private static int[] castRange = new int[] {
			1000, 1400
		};

        private static Dictionary<string, string> abilitiesFix = new Dictionary<string, string>();
        private static List<string> includedAbilities = new List<string>();

        #endregion

        #region Init

        public static void Init()
        {
            _2DGeometry.Init(new Line(Drawing.Direct3DDevice9), new Font(
            Drawing.Direct3DDevice9,
            new FontDescription
            {
                FaceName = "Tahoma",
                Height = 15,
                OutputPrecision = FontPrecision.Outline,
                Quality = FontQuality.Proof
            }));
            //
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;

            // abilitiesFix
            abilitiesFix.Add("ancient_apparition_ice_blast_release", "ancient_apparition_ice_blast");
        }

        #endregion

        #region Wnd

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsInGame)
            {
                switch (args.Msg)
                {
                    case (uint)Utils.WindowsMessages.WM_KEYUP:
                        switch (args.WParam)
                        {
                            case 0x24:
                                menuOn = !menuOn;
                                break;
                        }
                        break;
                    case (uint)Utils.WindowsMessages.WM_LBUTTONUP:
                        EzGUI.MouseClick(gui.Main);
                        break;
                }
            }
        }

        #endregion

        #region Drawing

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (!Game.IsInGame || myHero == null) return;
            if (myHero.ClassID != ClassID.CDOTA_Unit_Hero_Rubick) return;
            if (menuOn) gui.Draw();
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            _2DGeometry.GetFont().OnResetDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            _2DGeometry.GetFont().OnLostDevice();
        }

        #endregion

        #region Update

        static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame) { loaded = false; return; }
            if (Game.GameState != GameState.Started) return;
            #region If assembly not loaded
            if (loaded == false)
            {
                gui = new EzGUI(Drawing.Width - 350, 50, "PRubick " + VERSION);
                enabled = new EzElement(ElementType.CHECKBOX, "Enabled / Активен", true);
                spcat = new EzElement(ElementType.CATEGORY, "Spell Steal / Кража скиллов", false);
                stealIfHave = new EzElement(ElementType.CHECKBOX, "Steal if no cd / Воровать если нет кд [CHECKED]", false);
                gui.AddMainElement(new EzElement(ElementType.TEXT, "Main / Главная", false));
                gui.AddMainElement(enabled);
                gui.AddMainElement(stealIfHave);
                gui.AddMainElement(new EzElement(ElementType.TEXT, "Rubick", false));
                gui.AddMainElement(spcat);
                
                myHero = ObjectMgr.LocalHero;
                spellSteal = myHero.Spellbook.SpellR;
                loaded = true;
            }
            #endregion
            if (myHero.ClassID != ClassID.CDOTA_Unit_Hero_Rubick) return;
            //
            if (enabled.isActive)
            {
                Hero[] enemies = ObjectMgr.GetEntities<Hero>().Where(x => x.Team != myHero.Team && !x.IsIllusion() && x.IsAlive && x.IsVisible ).ToArray();
                #region GUI Checks
                if (Utils.SleepCheck("GUI_ABILITIES") && heroes != null)
                {
                    foreach (EzElement element in heroes.In)
                    {
                        foreach (EzElement element2 in element.In)
                        {
                            if (element2.isActive && !includedAbilities.Contains(element2.Content)) includedAbilities.Add(element2.Content);
                            if (!element2.isActive && includedAbilities.Contains(element2.Content)) includedAbilities.Remove(element2.Content);
                        }
                    }
                    Utils.Sleep(1000, "GUI_ABILITIES");
                }

                if (heroes == null)
                {
                    heroes = new EzElement(ElementType.CATEGORY, "Heroes / Герои", false);
                    for (int i = 0; i < 21; i++)
                    {
                        var player = ObjectMgr.GetPlayerById((uint)i);
                        if (player != null && player != ObjectMgr.LocalPlayer && player.Team != myHero.Team && myHero.IsAlive && player.ConnectionState == ConnectionState.Connected && player.Hero != null && player.Handle != null)
                        {
                            var enemy = player.Hero;
                            if (enemy == null || enemy.Spellbook == null || enemy.Spellbook.Spells == null) continue;
                            var hero = new EzElement(ElementType.CATEGORY, enemy.Name.Replace("_", "").Replace("npcdotahero", ""), false);
                            foreach (Ability ability in enemy.Spellbook.Spells)
                            {
                                if (ability.AbilityBehavior == AbilityBehavior.Passive) continue;
                                if (ability.AbilityType == AbilityType.Attribute) continue;
                                bool ac = false;
                                if (ability.AbilityType == AbilityType.Ultimate) { ac = true; includedAbilities.Add(ability.Name); }
                                EzElement abElement = new EzElement(ElementType.CHECKBOX, ability.Name, ac);
                                hero.In.Add(abElement);
                            }
                            heroes.In.Add(hero);
                        }
                    }
                    spcat.In.Add(heroes);
                }
                #endregion
                foreach (Hero enemy in enemies)
                {
                    if (Utils.SleepCheck(enemy.ClassID.ToString()))
                    {
                        foreach (Ability ability in enemy.Spellbook.Spells)
                        {
                            if (includedAbilities.Contains(ability.Name) && ability.CooldownLength - ability.Cooldown <  (float)0.7 + ( Game.Ping /1000 ) && !spellOnCooldown(ability.Name) && iCanSteal(enemy) && myHero.Spellbook.SpellD.Name != ability.Name && ability.CooldownLength != 0)
                            {
                                if (stealIfHave.isActive == false && myHero.Spellbook.SpellD.Cooldown == 0 && includedAbilities.Contains(myHero.Spellbook.SpellD.Name)) continue;
                                if (spellSteal.CanBeCasted()) spellSteal.UseAbility(enemy);
                            }
                        }
                        Utils.Sleep(125, enemy.ClassID.ToString());
                    }
                }
            }
        }

        #endregion

        #region Methods

        private static bool iCanSteal(Hero hero)
        {
            switch (myHero.AghanimState())
            {
                case true:
                    if (myHero.Distance2D(hero) <= castRange[1]) return true;
                    break;
                case false:
                    if (myHero.Distance2D(hero) <= castRange[0]) return true;
                    break;
            }
            return false;
        }

        private static bool spellOnCooldown(string abilityName)
        {
            if (abilitiesFix.ContainsKey(abilityName)) abilityName = abilitiesFix[abilityName];
            Ability[] Spells = myHero.Spellbook.Spells.ToArray();
            Ability[] SpellsF = Spells.Where(x => x.Name == abilityName).ToArray();
            if (SpellsF.Length > 0)
            {
                Ability SpellF = SpellsF.First();
                if (SpellF.Cooldown > 10) return true;
                return false;
            }
            else return false;
        }

        #endregion
    }


}