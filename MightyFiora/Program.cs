using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers;
using LeagueSharp.SDK.Core.Events;
using LeagueSharp.SDK.Core.Extensions.SharpDX;
using LeagueSharp.SDK.Core.IDrawing;
using LeagueSharp.SDK.Core.UI;
using LeagueSharp.SDK.Core.UI.IMenu.Skins;
using LeagueSharp.SDK.Core.UI.INotifications;
using LeagueSharp.SDK.Core.Utils;
using SharpDX;
using SharpDX.Direct3D9;

using Color = System.Drawing.Color;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

namespace MightyFiora
{
    //7/12/2015 whats left:
    // RLOGIC (enemy count check, r overkill check, r safety check, perhaps red buff combo? laneclear, lasthit and harass also jungle cause fiora jungle op amirite/????
    class Program
    {
        public static Spell Q, W, E, R;
        public static Menu Config;
        public static float Q1, Q2, Lastaa;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        static void Main(string[] args)
        {
            Load.OnLoad += OnLoad;
        }

        private static void OnLoad(object sender, EventArgs e)
        {
            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 400f);

            Config = new Menu("Mighty Fiora", "Mighty Fiora", true);
            Config.Add(new MenuSeparator("Mighty Fiora", "Mighty Fiora"));
            Bootstrap.Init(new string[] { });

            var combo = Config.Add(new Menu("combo", "Combo Settings"));
            var spell = Config.Add(new Menu("spell", "Spell Settings"));


            combo.Add(new MenuSeparator("Combo Menu", "Combo Menu"));
            combo.Add(new MenuBool("UseQ", "Use Q", true));
            combo.Add(new MenuBool("UseW", "Use W", true));
            combo.Add(new MenuBool("UseE", "Use E", true));
            combo.Add(new MenuBool("UseR", "Use R", true));

            //Advanced Spell Settings
            spell.Add(new MenuSeparator("Advanced Q Settings", "Advanced Q Settings"));
            spell.Add(new MenuBool("qgapcloseonly", "Use [Q] for Gapclosing only", false));
            spell.Add(new MenuBool("qgapclosem", "Gapclose using Enemy Minions?", true));
            spell.Add(new MenuBool("qgapclosec", "Gapclose using Enemy Champs?", true));
            spell.Add((new MenuSlider("qgapcloserange", "Gapclose Range", 300, 0, 600)));
            spell.Add(new MenuSeparator("Advanced W Settings", "Advanced W Settings"));
            spell.Add(new MenuBool("autow", "Use [W] on Autoattacks", true));
            spell.Add(new MenuSlider("wdelay", "Cast Delay in Miliseconds", 80, 0, 1000));
            spell.Add(new MenuSeparator("Advanced R Settings", "Advanced R Settings"));
            spell.Add(new MenuBool("UseRF", "Use [R] Finisher", true));
            spell.Add(new MenuBool("UseRdangerous", "Auto [R] on Dangerous", true));
            spell.Add(new MenuBool("UseRaoe", "Auto [R] on X amount of Enemies", true));
            spell.Add((new MenuSlider("rcount", "Enemy Count", 4, 0, 5)));
            spell.Add(new MenuSeparator("Killsteal Settings", "Killsteal Settings"));
            spell.Add((new MenuBool("UseQKS", "Use [Q] for Killstealing", true)));
            spell.Add((new MenuBool("UseRKS", "Use [R] for Killstealing", true)));

            var drawing = Config.Add(new Menu("draw", "Draw Settings"));
            drawing.Add(new MenuSeparator("Draw Menu", "Draw Menu"));
            drawing.Add(new MenuBool("disable", "Disable all drawings", false));
            drawing.Add(new MenuList<string>("drawmode", "Drawing Mode:", objects: new[] { "Normal", "Custom" }));
            drawing.Add(new MenuBool("disableq", "[Q] draw", true));
            drawing.Add(new MenuBool("disableqp", "[Q] Gapclose draw", true));
            drawing.Add(new MenuBool("disablee", "[E] draw", true));
            drawing.Add(new MenuBool("disabler", "[R] draw", true));
            drawing.Add(new MenuSeparator("Color Settings", "Color Settings"));

            //I'll call your parents if you copy this. KappaHD
            //Do you need an Onii-chan or a Senpai? Feel free to contact me on Skype: djkleeven
            drawing.Add(new MenuColor("drawq", "[Q] Range Draw Color", new ColorBGRA(32, 20, 10, 255)));
            drawing.Add(new MenuColor("drawqp", "[Q] Gapclose Draw Color", new ColorBGRA(125, 20, 10, 255)));
            drawing.Add(new MenuColor("drawe", "[E] Range Draw Color", new ColorBGRA(32, 20, 10, 255)));
            drawing.Add(new MenuColor("drawr", "[R] Range Draw Color", new ColorBGRA(32, 20, 10, 255)));

            Config.Add(new MenuButton("resetAll", "Settings", "Reset All Settings")
            {
                Action = () =>
                {
                    Config.RestoreDefault();
                }
            });

            Config.Attach();
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Orbwalker.OnAction += OnAction;
            Drawing.OnDraw += OnDraw;

        }

        private static void GapcloserMM()
        {
            //Minions and Heroes @Look at BMAkali
        }
        private static void OnDraw(EventArgs args)
        {
            
            var mode = Config["draw"]["drawmode"].GetValue<MenuList<string>>();
            if (Config["draw"]["disable"].GetValue<MenuBool>().Value) return;
            var gapcloserange = Config["spell"]["qgapcloserange"].GetValue<MenuSlider>().Value;
            switch (mode.Index)
            {
                case 0:
                {
                    if (Q.Level >= 1 && Config["draw"]["disableq"].GetValue<MenuBool>().Value)
                        Drawing.DrawCircle(GameObjects.Player.Position, Q.Range,
                            Color.FromArgb((int) Config["draw"]["drawq"].GetValue<MenuColor>().Color));

                    if (Q.Level >= 1 && Config["draw"]["disableqp"].GetValue<MenuBool>().Value)
                        Drawing.DrawCircle(GameObjects.Player.Position, gapcloserange,
                            Color.FromArgb((int) Config["draw"]["drawqp"].GetValue<MenuColor>().Color));

                    if (E.Level >= 1 && Config["draw"]["disablee"].GetValue<MenuBool>().Value)
                        Drawing.DrawCircle(GameObjects.Player.Position, E.Range,
                            Color.FromArgb((int) Config["draw"]["drawe"].GetValue<MenuColor>().Color));

                    if (R.Level >= 1 && Config["draw"]["disabler"].GetValue<MenuBool>().Value)
                        Drawing.DrawCircle(GameObjects.Player.Position, R.Range,
                            Color.FromArgb((int) Config["draw"]["drawr"].GetValue<MenuColor>().Color));

                    var pos1 = Drawing.WorldToScreen(Player.Position);
                    var target = TargetSelector.GetTarget(2100);

                          //RDMG please dont return 0 i'll fucking rek myself
                        Drawing.DrawText(pos1.X + 60, pos1.Y + 40, Color.LawnGreen, Rdmg(target).ToString());
                         //Check if EnemiesInRange actually works.
                        Drawing.DrawText(pos1.X + 60, pos1.Y + 60, Color.LawnGreen, EnemiesInRange(target.Position, 2000).ToString());
                    }
                    break;
                }
            }
        
        

        private static void ComboLogic() //wow such logic much amaze
        {
            var target = TargetSelector.GetTarget(Q.Range*2);

            KoreanCombo(target);

            if (R.IsReady())
                Rlogic();
        }
        private static void KoreanCombo(Obj_AI_Hero target)
        {
          var gapclose = Config["spell"]["qgapcloseonly"].GetValue<MenuBool>().Value;
            var gapcloserange = Config["spell"]["qgapcloserange"].GetValue<MenuSlider>().Value;

        //Q A E A HYDRA A Q A
        //Q A E A Hydra Q A A cause the slow action so Q can't use complete !
        //Q A E A A Q A Hydra A if have red buff
        //Q A E Hydra R Q A (Can chase enemy when after R)﻿  if Normal combo doesn't kill (only if R in Combo is enabled)

            //First Q cast
            if (Config["combo"]["UseQ"].GetValue<MenuBool>().Value)
            {
                if (Q.IsReady() && gapclose && Player.Distance(target.Position) >= gapcloserange)
                    Q.Cast(target);

                if (Q.IsReady() && Q1 == 0 && !gapclose)
                {
                    Q.Cast(target);
                    Items.UseItem(3142);
                }
                if (Q1 == 1 && Player.Distance(target.Position) >= 350 && !gapclose && !Player.IsDashing()) //Some bugged shit up in this crib, Q isn't a dash? ok.
                {
                    Q.Cast(target);
                }
            }

        }
        private static float EnemiesInRange(Vector3 position, int range)
        {
            float enemycount = 0;
            var enemies = GameObjects.EnemyHeroes.Where(a => a.IsValid && a.Distance(position) < range).ToList();
            foreach (var enemy in enemies.Where(a => !a.IsDead && !a.IsMe))
            {
                enemycount += 1; return enemycount;
            }
            return enemycount;
        }
        private static double Rdmg (Obj_AI_Hero target)
        {
            double dmg = 0;
            if (R.Level == 1)
            {
                var dmg1 = Player.CalculateDamage(target, DamageType.Physical, 125 + 0.9*Player.FlatPhysicalDamageMod);
                dmg += dmg1 * (0.25 * (6 - EnemiesInRange(target.Position, 500)) + 1);
                if (dmg > 320 && EnemiesInRange(target.Position, 500) < 1)
                    dmg = 320;

                return dmg;
            }
            if (R.Level == 2)
            {
                var dmg1 = Player.CalculateDamage(target, DamageType.Physical, 255 + 0.9 * Player.FlatPhysicalDamageMod);
                dmg += dmg1 * (0.25 * (6 - EnemiesInRange(target.Position, 500)) + 1);
                if (dmg > 660 && EnemiesInRange(target.Position, 500) < 1)
                    dmg = 660;

                return dmg;
            }
            if (R.Level == 3)
            {
                var dmg1 = Player.CalculateDamage(target, DamageType.Physical, 385 + 0.9 * Player.FlatPhysicalDamageMod);
                dmg += dmg1*(0.25 * (6 - EnemiesInRange(target.Position, 500)) + 1);

                if (dmg > 1000 && EnemiesInRange(target.Position, 500) < 1)
                    dmg = 1000;

                return dmg;
            }

            return dmg;
        }

        private static void Rlogic()
        {
            var target = TargetSelector.GetTarget(400);

            if (Config["spells"]["UseRF"].GetValue<MenuBool>().Value)
            {
                if (R.IsReady() && target.Health <= Rdmg(target)) //needs ignite check since you can check summoners in ult so ult + ignite kill is fine but if ult is enough no ignitos :3
                    R.Cast(target);
            }

        }
        private static double Overkillcheck(Obj_AI_Hero target)
        {
            //needs ignite with health + duration check +++++
            double dmg = 0;
            var tiamat = Items.HasItem(3074) && Items.CanUseItem(3074);
            var hydra = Items.HasItem(3077) && Items.CanUseItem(3077);
            var qdmg = Player.GetSpellDamage(target, SpellSlot.Q);
            var aa = Player.GetAutoAttackDamage(target);

            //Shit ain't done yet mate.
            return 100;

        }
        private static void OnAction(object sender, Orbwalker.OrbwalkerActionArgs orbwalk)
        {
            var target = TargetSelector.GetTarget(Q.Range * 2);
            if (!target.IsValidTarget())
                return;

            var gapclose = Config["spell"]["qgapcloseonly"].GetValue<MenuBool>().Value;

            if (orbwalk.Type == OrbwalkerType.AfterAttack && Q1 == 1 && Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk)
            {
                E.Cast();
                Player.IssueOrder(GameObjectOrder.AutoAttack, target);
            }
            if (orbwalk.Type == OrbwalkerType.AfterAttack && Q1 == 1 && Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk && !E.IsReady())
            {
                Q1 = 0;
            }
            if (Q1 == 0 && !E.IsReady() && Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk && Qcast2() && orbwalk.Type == OrbwalkerType.AfterAttack && !gapclose)
            {
                Q.Cast(target);
                Player.IssueOrder(GameObjectOrder.AutoAttack, target);
            }
            if (orbwalk.Type == OrbwalkerType.AfterAttack && Q1 == 0 && !Q.IsReady() && Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk)
            {
                E.Cast();
                Player.IssueOrder(GameObjectOrder.AutoAttack, target);
            }
            if (orbwalk.Type == OrbwalkerType.AfterAttack && Q1 == 0 && !E.IsReady() && !Q.IsReady() && Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk)
            {
                Items.UseItem(3074);
                Items.UseItem(3077);
            }
        }
        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs spell)
        {
            var delayvalue = Config["spell"]["wdelay"].GetValue<MenuSlider>().Value;
            //Simple [W] Logic
            var target = TargetSelector.GetTarget(200);
            if (spell.SData.Name.ToLower().Contains("basicattack") && spell.Target.IsMe && !sender.IsMinion &&
                sender.IsEnemy && !spell.SData.Name.ToLower().Contains("turret"))
                DelayAction.Add(delayvalue, () => W.Cast());

            if (spell.SData.Name == "FioraQ")
            {
                Q1 = 1;
            }
            if (spell.SData.Name == "FioraQ" && Q1 == 1 && Player.HasBuff("fioraqcd"))
            {
                Items.UseItem(3074);
                Items.UseItem(3077);
                Player.IssueOrder(GameObjectOrder.AutoAttack, target);
            }

            //AA reset [E]
            if (spell.SData.Name.Contains("FioraFlurry") || spell.SData.Name == "ItemTiamatCleave")
            {
                Orbwalker.ResetAutoAttackTimer();
            }
            if (spell.SData.Name.ToLower().Contains("fiorabasicattack") || spell.SData.Name.ToLower().Contains("fiorabasicattack2"))
            {
                Lastaa = Environment.TickCount;
            }
        }
        public static bool Qcast2()
        {
            return Environment.TickCount + Game.Ping / 2 + 25 >= Lastaa + Player.AttackDelay * 1000;
        }
        private static void OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                    ComboLogic();
                    break;
                case OrbwalkerMode.LastHit:
                    break;
                case OrbwalkerMode.LaneClear:
                    break;
                case OrbwalkerMode.Hybrid:
                    break;
            }
            if (!Player.HasBuff("fioraqcd"))
            {
                Q1 = 0;
            }
        
        }
    }
}
