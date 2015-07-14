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
    internal class Program
    {
        public static Spell Q, W, E, R;
        public static Menu Config;
        public static float Q1, Lastaa;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            if (Player.ChampionName != "Fiora")
                return;

            Load.OnLoad += OnLoad;

        }

        private static void OnLoad(object sender, EventArgs e)
        {
            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 400f);

            Q.SetTargetted(0f, 1000f, Player.Position);

            Config = new Menu("Mighty Fiora", "Mighty Fiora", true);
            Config.Add(new MenuSeparator("Mighty Fiora", "Mighty Fiora"));
            Bootstrap.Init(new string[] {});

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
            spell.Add((new MenuSlider("qgapcloserange", "Gapclose Range", 300, 0, 600)));
            spell.Add(new MenuSeparator("Advanced W Settings", "Advanced W Settings"));
            spell.Add(new MenuBool("autow", "Use [W] on Autoattacks", true));
            spell.Add(new MenuSlider("wdelay", "Cast Delay in Miliseconds", 80, 0, 1000));
            spell.Add(new MenuSeparator("Advanced R Settings", "Advanced R Settings"));

            spell.Add(new MenuBool("UseRF", "Use [R] on Killable", true));
            spell.Add(new MenuKeyBind("forceR", "Force [R] Toggle (will R after Combo)", Keys.J, KeyBindType.Toggle));

            spell.Add(new MenuBool("rhp", "Auto [R] if HP <= %", true));
            spell.Add((new MenuSlider("rhp%", "Player HP %", 30, 0, 100)));

            spell.Add(new MenuBool("rAOE", "Auto [R] on X amount of Enemies", true));
            spell.Add((new MenuSlider("rcount", "Enemy Count", 4, 0, 5)));

            var harass = Config.Add(new Menu("harass", "Harass Settings"));
            harass.Add(new MenuSeparator("Harass Menu", "Harass Menu"));
            harass.Add(new MenuBool("harrQ", "Use Q", true));
            harass.Add(new MenuBool("harrE", "Use E", true));

            var laneclear = Config.Add(new Menu("laneclear", "Laneclear Settings"));
            laneclear.Add(new MenuSeparator("Coming Soon", "Coming Soon"));

            var drawing = Config.Add(new Menu("draw", "Draw Settings"));
            drawing.Add(new MenuSeparator("Draw Menu", "Draw Menu"));
            drawing.Add(new MenuBool("disable", "Disable all drawings", false));
            drawing.Add(new MenuList<string>("drawmode", "Drawing Mode:", objects: new[] {"Normal", "Custom"}));
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
            drawing.Add(new MenuSeparator("Misc Settings", "Misc Settings"));
            drawing.Add(new MenuBool("dmgindic", "Damage Indicator", true));
            drawing.Add(new MenuBool("targets", "Draw Selected Target", true));



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
            Obj_AI_Hero.OnBuffRemove += Tiamat;
            Obj_AI_Hero.OnBuffAdd += Qbuff;
            Drawing.OnEndScene += DamageIn;

        }

        private static void DamageIn(EventArgs args)
        {

            if (Config["draw"]["dmgindic"].GetValue<MenuBool>().Value)
            {
                DamageIndicator.DamageToUnit = Fullcombodmg;
                DamageIndicator.Color = Color.LawnGreen;
                DamageIndicator.Enabled = true;
            }
            else
            {
                DamageIndicator.Enabled = false;
            }
        }

        private static void Qbuff(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (sender.IsMe && args.Buff.Name == "fioraqcd")
            {
                Q1 = 1;
            }
        }

        private static void Tiamat(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args)
        {
            if (sender.IsMe && args.Buff.Name == "fioraqcd")
            {
                Q1 = 0;

                var target = TargetSelector.GetTarget(Q.Range*2);
                if (target.IsValidTarget(280))
                {
                    Items.UseItem(3074);
                    Items.UseItem(3077);
                }
            }

        }

        public static float Fullcombodmg(Obj_AI_Base target)
        {
            var aa = Player.GetAutoAttackDamage(target);
            double dmg = aa * (1 + Player.Crit); 
            if (Q.IsReady() && Q1 == 0)
                dmg += aa + Qdmg(target) * 2;
            if (E.IsReady())
                dmg += aa;
            if (Items.CanUseItem(3074) || Items.CanUseItem(3077))
                dmg += aa + aa*0.60;
            if (R.IsReady())
                dmg += Rdmg(target);

            return (float)dmg;
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
                    var dtarget = TargetSelector.GetTarget(Q.Range * 2);
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


                    if (Config["spell"]["forceR"].GetValue<MenuKeyBind>().Active)
                    {
                        Drawing.DrawText(pos1.X - 45, pos1.Y + 40, Color.White, "Force [R]:");
                        Drawing.DrawText(pos1.X + 30, pos1.Y + 40, Color.LawnGreen, "On");
                    }
                    else
                    {
                        Drawing.DrawText(pos1.X - 45, pos1.Y + 40, Color.White, "Force [R]:");
                        Drawing.DrawText(pos1.X + 30, pos1.Y + 40, Color.Tomato, "Off");
                    }


                    var targets = TargetSelector.GetSelectedTarget();

                    var pos2 = Drawing.WorldToScreen(targets.Position);
                    if (Config["draw"]["targets"].GetValue<MenuBool>().Value)
                    {
                        Drawing.DrawCircle(targets.Position, targets.BoundingRadius + 50, Color.Tomato);
                        Drawing.DrawCircle(targets.Position, targets.BoundingRadius + 49, Color.DarkRed);
                        Drawing.DrawText(pos2.X, pos2.Y, Color.LawnGreen, "Current Target");
                    }


                    //RDMG please dont return 0 i'll fucking rek myself
                       // Drawing.DrawText(pos1.X + 60, pos1.Y + 40, Color.LawnGreen, Rdmg(target).ToString());
                         //Check if EnemiesInRange actually works.
                        //Drawing.DrawText(pos1.X + 60, pos1.Y + 60, Color.LawnGreen, EnemiesInRange(target.Position, 2000).ToString());
                }
                    break;
                }
            }


        private static void Laneclear()
        {
            var minions =
                GameObjects.EnemyMinions.Where(m => m.IsValid && m.Distance(Player) < Q.Range).ToList();

            var laneE = Config["laneclear"]["laneE"].GetValue<MenuBool>().Value;
            var laneQ = Config["laneclear"]["laneQ"].GetValue<MenuBool>().Value;
            var laneQL = Config["laneclear"]["laneEL"].GetValue<MenuBool>().Value;

            foreach (var minion in minions)
            {

                if 
                    (Q.IsReady()
                    && minion.IsValidTarget(Q.Range))
                {
                    Q.Cast(minion);
                }
            }
        }

        private static void ComboLogic() //wow such logic much amaze
        {
            var target = TargetSelector.GetTarget(Q.Range*2);
            if (target == null)
                return;

            KoreanCombo(target);

            if (R.IsReady() && Config["combo"]["UseR"].GetValue<MenuBool>().Value)
                Rlogic();
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range*2);
            if (target == null)
                return;
            if (Config["harass"]["harrQ"].GetValue<MenuBool>().Value)
            {

                if (Q.IsReady() && Q1 == 0 && target.IsValidTarget(Q.Range))
                {
                    Orbwalker.ResetAutoAttackTimer();
                    Q.Cast(target);
                    Items.UseItem(3142);
                    Player.IssueOrder(GameObjectOrder.AutoAttack, target);
                }
                if (Q1 == 1 && Player.Distance(target.Position) >= 300 && !Player.IsDashing())
                {
                    Q.Cast(target);
                }
            }
        }
        private static void KoreanCombo(Obj_AI_Hero target)
        {
            if (target == null)
                return;

          var gapclose = Config["spell"]["qgapcloseonly"].GetValue<MenuBool>().Value;
            var gapcloserange = Config["spell"]["qgapcloserange"].GetValue<MenuSlider>().Value;

        //Q A E A HYDRA A Q A
        //Q A E A Hydra Q A A cause the slow action so Q can't use complete !
        //Q A E A A Q A Hydra A if have red buff

            //First Q cast
            if (Config["combo"]["UseQ"].GetValue<MenuBool>().Value)
            {
                if (Q.IsReady() && gapclose && Player.Distance(target.Position) >= gapcloserange)
                    Q.Cast(target);

                if (Q.IsReady() && Q1 == 0 && !gapclose && target.IsValidTarget(Q.Range))
                {
                    Orbwalker.ResetAutoAttackTimer();
                    Q.Cast(target);
                    Items.UseItem(3142);
                    Player.IssueOrder(GameObjectOrder.AutoAttack, target);
                }
                if (Q1 == 1 && Player.Distance(target.Position) >= 300 && !gapclose && !Player.IsDashing()) //Some bugged shit up in this crib, Q isn't a dash? ok.
                {
                    Q.Cast(target);
                }

               // if (Q1 == 1 && Player.Distance(target.Position) > Player.GetRealAutoAttackRange() && !gapclose && !Player.IsDashing() 
                  //  && target.FlatMovementSpeedMod > Player.FlatMovementSpeedMod && target.IsMoving && !target.IsFacing(Player))
                //{
                   // Q.Cast(target);
                //}
            }

        }
        private static float EnemiesInRange(Vector3 position, int range)
        {
            var enemies = GameObjects.EnemyHeroes.Where(a => a.IsValid && a.Distance(position) < range && a.IsEnemy).ToList();                     
            return enemies.Count;
        }

        private static double Qdmg(Obj_AI_Base target)
        {
            return
               Player.CalculateDamage(target, DamageType.Physical,
                     new[] { 40, 65, 90, 115, 140 }[Program.Q.Level - 1] + 0.6 * Player.FlatPhysicalDamageMod);
        }
        private static double Rdmg (Obj_AI_Base target)
        {
            var aa = Player.GetAutoAttackDamage(target);

            double dmg = 0;
            if (R.Level == 1)
            {
                var dmg1 = Player.CalculateDamage(target, DamageType.Physical, 125 + 0.9*Player.FlatPhysicalDamageMod);
                dmg += dmg1 * (0.40 * (6 - EnemiesInRange(target.Position, 500)) + 1);

                return dmg;
            }
            if (R.Level == 2)
            {
                var dmg1 = Player.CalculateDamage(target, DamageType.Physical, 255 + 0.9 * Player.FlatPhysicalDamageMod);
                dmg += dmg1 * (0.40 * (6 - EnemiesInRange(target.Position, 500)) + 1);

                return dmg;
            }
            if (R.Level == 3)
            {
                var dmg1 = Player.CalculateDamage(target, DamageType.Physical, 385 + 0.9 * Player.FlatPhysicalDamageMod);
                dmg += dmg1*(0.40 * (6 - EnemiesInRange(target.Position, 500)) + 1);


                return dmg;
            }

            return dmg;
        }

        private static void Rlogic()
        {
            var target = TargetSelector.GetTarget(400);
            if (target == null)
                return;
            if (Config["spell"]["UseRF"].GetValue<MenuBool>().Value)
            {
                if (R.IsReady() && target.Health <= Rdmg(target) && !Player.IsDashing() && Overkillcheck(target) >= target.Health && !Q.IsReady()) //needs ignite check since you can check summoners in ult so ult + ignite kill is fine but if ult is enough no ignitos :3
                    R.Cast(target);
            }
            var hpslider = Config["spell"]["rph%"].GetValue<MenuSlider>().Value;
            var ecount = Config["spell"]["rcount"].GetValue<MenuSlider>().Value;
            
            if (Config["spell"]["rhp"].GetValue<MenuBool>().Value)
            {
                if (Player.HealthPercent <= hpslider && R.IsReady())
                    R.Cast(target);
            }
            if (EnemiesInRange(target.Position, 800) >= ecount && Config["spell"]["rAOE"].GetValue<MenuBool>().Value)
                R.Cast(target);

        }
        private static double Overkillcheck(Obj_AI_Hero target)
        {
            //needs ignite with health + duration check +++++
            double dmg = 0;
            var aa = Player.GetAutoAttackDamage(target);

            //Shit ain't done yet mate.

            if (Q.IsReady() && target.IsValidTarget(Q.Range) && Q1 == 0)
                dmg += Qdmg(target) + aa;
            if (Q1 == 1 && target.IsValidTarget(Q.Range))
                dmg += Qdmg(target) + aa;
            if (E.IsReady() && Player.Distance(target.Position) <= Player.GetRealAutoAttackRange())
                dmg += aa;
            if (Items.CanUseItem(3074) || Items.CanUseItem(3077))
                dmg += aa + aa * 0.60;
            if (Player.Distance(target) <= Player.GetRealAutoAttackRange())
                dmg += aa;

            return dmg;


        }
        private static void OnAction(object sender, Orbwalker.OrbwalkerActionArgs orbwalk)
        {
            var target = TargetSelector.GetTarget(Q.Range * 2);
            if (Player.IsWindingUp)
                return;

            var UseE = Config["combo"]["UseE"].GetValue<MenuBool>().Value;
            var UseQ = Config["combo"]["UseQ"].GetValue<MenuBool>().Value;
            var harrQ = Config["harass"]["harrQ"].GetValue<MenuBool>().Value;
            var harrE = Config["harass"]["harrE"].GetValue<MenuBool>().Value;
            var gapclose = Config["spell"]["qgapcloseonly"].GetValue<MenuBool>().Value;

            if (orbwalk.Type == OrbwalkerType.AfterAttack && Q1 == 1 && Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk && UseE && !target.IsDead ||
                orbwalk.Type == OrbwalkerType.AfterAttack && Q1 == 1 && Orbwalker.ActiveMode == OrbwalkerMode.Hybrid && harrE)
            {
                E.Cast();
                Player.IssueOrder(GameObjectOrder.AutoAttack, target);
            }
            if (orbwalk.Type == OrbwalkerType.AfterAttack && Q1 == 1 && Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk && !E.IsReady())
            {
                Q1 = 0;
            }
            if (Q1 == 0 && !E.IsReady() && Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk && Qcast2() && orbwalk.Type == OrbwalkerType.AfterAttack && !gapclose && UseQ ||
                Q1 == 0 && !E.IsReady() && Orbwalker.ActiveMode == OrbwalkerMode.Hybrid && Qcast2() && orbwalk.Type == OrbwalkerType.AfterAttack && !gapclose && harrQ)
            {
                Q.Cast(target);
                Player.IssueOrder(GameObjectOrder.AutoAttack, target);
            }
            if (orbwalk.Type == OrbwalkerType.AfterAttack && Q1 == 0 && !Q.IsReady() && Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk && UseE && !target.IsDead||
                orbwalk.Type == OrbwalkerType.AfterAttack && Q1 == 0 && !Q.IsReady() && Orbwalker.ActiveMode == OrbwalkerMode.Hybrid && harrE)
            {
                E.Cast();
            }
            if (orbwalk.Type == OrbwalkerType.AfterAttack && !E.IsReady() && 
                !Q.IsReady() && Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk && Config["spell"]["forceR"].GetValue<MenuKeyBind>().Active)
            {
                R.Cast(target);
            }
        }
        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs spell)
        {
            var delayvalue = Config["spell"]["wdelay"].GetValue<MenuSlider>().Value;
            //Simple [W] Logic
            if (spell.SData.Name.ToLower().Contains("basicattack") && spell.Target.IsMe && !sender.IsMinion &&
                sender.IsEnemy && !spell.SData.Name.ToLower().Contains("turret"))
                DelayAction.Add(delayvalue, () => W.Cast());

            if (spell.SData.Name == "FioraQ")
            {
                Q1 = 1;
            }
            //AA reset [E]
            if (spell.SData.Name.Contains("FioraFlurry") || spell.SData.Name == "ItemTiamatCleave")
            {
                Orbwalker.ResetAutoAttackTimer();
            }
            if (spell.SData.Name.ToLower().Contains("fiorabasicattack") 
                || spell.SData.Name.ToLower().Contains("fiorabasicattack2") || spell.SData.Name.ToLower().Contains("fioracrit"))
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
             Player.SetSkin(Player.BaseSkinName, 2);

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                    ComboLogic();
                    break;
                case OrbwalkerMode.LastHit:
                    break;
                case OrbwalkerMode.LaneClear:
                    Laneclear();
                    break;
                case OrbwalkerMode.Hybrid:
                    Harass();
                    break;
            }
        
        }
    }
}
