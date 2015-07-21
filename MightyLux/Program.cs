using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers;
using LeagueSharp.SDK.Core.Events;
using LeagueSharp.SDK.Core.Extensions.SharpDX;
using LeagueSharp.SDK.Core.Utils;
using MightyLux.Helpers;
using SharpDX;

using Color = System.Drawing.Color;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;


namespace MightyLux
{
    public class Program : Statics
    {
        public static void Main(string[] args)
        {
            Load.OnLoad += OnLoad;
            //var killable = GameObjects.EnemyHeroes.Where(m => m.Health d<= Rdmg(m) && !m.IsDead).ToList();
        }

        public static void OnLoad(object sender, EventArgs e)
        {
            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 3250);

            Q.SetSkillshot(0.25f, 110f, 1300f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 110f, 1200f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 280f, 1300f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1.35f, 190f, 5000000, false, SkillshotType.SkillshotLine);

            Config = new Menu("Mighty Lux", "Mighty Lux", true);
            Config.Add(new MenuSeparator("Mighty Lux", "Mighty Lux"));
            Bootstrap.Init(new string[] {});

            var spell = Config.Add(new Menu("spell", "Combo Settings"));
            var combo = Config.Add(new Menu("combo", "Spell Settings"));

            spell.Add(new MenuSeparator("Combo Menu", "Combo Menu"));
            spell.Add(new MenuBool("UseQ", "Use Q", true));
            spell.Add(new MenuBool("UseE", "Use E", true));
            spell.Add(new MenuBool("UseR", "Use R", true));

            combo.Add(new MenuSeparator("Advanced Q Settings", "Advanced Q Settings"));
            combo.Add(new MenuBool("AutoQcc", "Auto [Q] on CC'd enemies", true));
            combo.Add(new MenuSeparator("Advanced W Settings", "Advanced W Settings"));
            combo.Add(new MenuBool("AutoWturret", "Auto [W] on Turret Shots", true));
            combo.Add(new MenuSeparator("Advanced R Settings", "Advanced R Settings"));
            combo.Add(new MenuKeyBind("forceR", "Semi-Manual [R] cast", Keys.R, KeyBindType.Press));
            combo.Add(new MenuBool("UseRF", "Use [R] to Finish off enemies", true));
            combo.Add((new MenuBool("useraoe", "Use [R] if it will hit X amount of Enemies", true)));
            combo.Add((new MenuSlider("raoeslider", "Enemy Count", 4, 0, 5)));

            combo.Add(new MenuSeparator("Killsteal Settings", "Killsteal Settings"));
            combo.Add((new MenuBool("UseQKS", "Use [Q] for Killstealing", true)));
            combo.Add((new MenuBool("UseRKS", "Use [R] for Killstealing", true)));
            combo.Add(new MenuBool("UseEKS", "Use [E] for Killstealing", true));


            //Prediction
            combo.Add(new MenuSeparator("science", "Prediction Settings"));
            combo.Add(new MenuList<string>("hitchanceQ", "[Q] Hitchance",
                objects: new[] { "High", "Medium", "Low", "VeryHigh" }));
            combo.Add(new MenuList<string>("hitchanceE", "[E] Hitchance",
                objects: new[] { "High", "Medium", "Low", "VeryHigh" }));
            combo.Add(new MenuList<string>("hitchanceR", "[R] Hitchance",
                objects: new[] { "High", "Medium", "Low", "VeryHigh" }));

            var harass = Config.Add(new Menu("harass", "Harass Settings"));
            harass.Add(new MenuSeparator("Harass Menu", "Harass Menu"));
            harass.Add(new MenuBool("harrQ", "Use Q", true));
            harass.Add(new MenuBool("harrE", "Use E", true));
            harass.Add(new MenuSlider("harassmana", "Mana Percentage", 65, 0, 100));

            var laneclear = Config.Add(new Menu("laneclear", "Laneclear Settings"));
            laneclear.Add(new MenuSeparator("Laneclear Menu", "Laneclear Menu"));
            laneclear.Add(new MenuBool("laneQ", "Use Q", true));
            laneclear.Add(new MenuBool("laneE", "Use E", true));
            laneclear.Add(new MenuSeparator("[E] Settings", "[E] Settings"));
            laneclear.Add(new MenuSlider("laneclearEcount", "Minion Count", 2, 0, 10));
            laneclear.Add(new MenuSeparator("[Q] Settings", "[Q] Settings"));
            laneclear.Add(new MenuSlider("laneclearQcount", "Minion Count", 2, 0, 10));
            laneclear.Add(new MenuSeparator("Misc Settings", "Misc Settings"));
            laneclear.Add(new MenuSlider("lanemana", "Mana Percentage", 65, 0, 100));
            laneclear.Add(new MenuSlider("lanelevel", "Don't use Abilities till level", 8, 0, 18));

            var jungle = Config.Add(new Menu("jungle", "Junglesteal Settings"));
            jungle.Add(new MenuSeparator("Jungle Settings", "Junglesteal Settings"));
            jungle.Add(new MenuKeyBind("toggle", "Junglesteal with [R] (TOGGLE)", Keys.K, KeyBindType.Toggle));
            jungle.Add(new MenuBool("blue", "Blue buff", true));
            jungle.Add(new MenuBool("red", "Red buff", true));
            jungle.Add(new MenuBool("dragon", "Dragon", true));
            jungle.Add(new MenuBool("baron", "Baron", true));
            jungle.Add(new MenuList<string>("jungleteam", "[BROKEN/NOT WORKING]",  new[] {"Enemy", "Ally"}));

            var misc = Config.Add(new Menu("misc", "Misc Settings"));
            misc.Add(new MenuSeparator("sound1", "Welcome Sound Effect"));
            var drawing = Config.Add(new Menu("draw", "Draw Settings"));
            var utility = Config.Add(new Menu("util", "Utility Drawings"));
            drawing.Add(new MenuSeparator("Draw Menu", "Draw Menu"));
            drawing.Add(new MenuBool("disable", "Disable all drawings", false));
            drawing.Add(new MenuList<string>("drawmode", "Drawing Mode:",  new[] {"Normal", "Custom"}));
            drawing.Add(new MenuBool("disableq", "[Q] draw", true));
            drawing.Add(new MenuBool("disablew", "[W] draw", true));
            drawing.Add(new MenuBool("disablee", "[E] draw", true));
            drawing.Add(new MenuBool("disabler", "[R] draw", true));
            drawing.Add(new MenuSeparator("Color Settings", "Color Settings"));

            //I'll call your parents if you copy this. KappaHD
            //Do you need an Onii-chan or a Senpai? Feel free to contact me on Skype: djkleeven
            drawing.Add(new MenuColor("drawq", "[Q] Range Draw Color", new ColorBGRA(32, 20, 10, 255)));
            drawing.Add(new MenuColor("draww", "[W] Range Draw Color", new ColorBGRA(32, 20, 10, 255)));
            drawing.Add(new MenuColor("drawe", "[E] Range Draw Color", new ColorBGRA(32, 20, 10, 255)));
            drawing.Add(new MenuColor("drawr", "[R] Range Draw Color", new ColorBGRA(32, 20, 10, 255)));


            utility.Add(new MenuSeparator("Utility Drawings", "Utility Drawings"));

            utility.Add(new MenuList<string>("dmgdrawer", "Damage Indicator",  new[] { "Custom", "SDK" }));
            utility.Add(new MenuColor("dmgcolor", "Damage Indicator Color", new ColorBGRA(32, 155, 120, 255)));
            utility.Add(new MenuBool("HUD", "Heads-Up Display", true));
            utility.Add(new MenuBool("indicator", "Enemy Indicator", true));
            utility.Add(new MenuBool("orbmode", "Draw Active Orbwalk Mode", true));


            Config.Add(new MenuButton("resetAll", "Settings", "Reset All Settings")
            {
                Action = () =>
                {
                    Config.RestoreDefault();
                }
            });

            //Font


            Config.Attach();
         
            //Drawings.DrawEvent();
            Dash.OnDash += AntiGapcloser;
            Game.OnUpdate += OnUpdate;
            GameObject.OnDelete += GameObject_OnDelete;
            GameObject.OnCreate += GameObject_OnCreate;
            Obj_AI_Turret.OnAggro += Turretaggro;
            Obj_AI_Base.OnProcessSpellCast += TurretOnProcessSpellCast;
            Drawing.OnDraw += MiscDrawings;
            Drawings.DrawEvent();
        }

        public static void AntiGapcloser(object sender, Dash.DashArgs e)
        {
            throw new NotImplementedException();
        }
        public static void Turretaggro(Obj_AI_Base sender, GameObjectAggroEventArgs args)
        {
            if (!W.IsReady())
                return;
            if (sender.Target.IsMe && W.IsReady() && Config["combo"]["AutoWturret"].GetValue<MenuBool>().Value)
                W.Cast(Game.CursorPos);
        }

        public static void TurretOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!W.IsReady())
                return;

            if (!args.Target.IsMe || sender.IsAlly || sender.IsMinion)
                {
                    return;
                }

            if (W.IsReady() && Config["combo"]["AutoWturret"].GetValue<MenuBool>().Value)
                       W.Cast(Game.CursorPos);
        }

        public static void MiscDrawings(EventArgs args)
        {
            var pos1 = Drawing.WorldToScreen(Player.Position);
            if (!Player.IsDead && Config["util"]["orbmode"].GetValue<MenuBool>().Value)
            {
                Drawing.DrawText(pos1.X - 75, pos1.Y + 40, Color.Gold, "[Orbwalker Mode]:");
                Drawing.DrawText(pos1.X + 60, pos1.Y + 40, Color.LawnGreen, Orbwalker.ActiveMode.ToString());
            }
            if (Config["jungle"]["toggle"].GetValue<MenuKeyBind>().Active)
                Drawing.DrawText(pos1.X - 75, pos1.Y + 60, Color.LawnGreen, "Junglesteal Enabled");
        }

        public static float AlliesInRange(Vector3 position, int range)
        {
            var allies = GameObjects.AllyHeroes.Where(a => a.IsValid && !a.IsMe && a.Distance(position) < range).ToList();

            return allies.Count;
        }
        public static void AutoQ()
        {
            if (!Q.IsReady())
                return;

            var target = TargetSelector.GetTarget(Q.Range);
            var cc = target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Stun) ||
                     target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Suppression) || target.HasBuffOfType(BuffType.Slow);


            if (Config["combo"]["AutoQcc"].GetValue<MenuBool>().Value && Q.IsReady() && cc && Q.GetPrediction(target).Hitchance >= PredictionQ() && target.IsValidTarget(Q.Range))
                Q.Cast(target);
        }

        public static void ForceR()
        {
            var target = TargetSelector.GetTarget(R.Range);
            if (R.IsReady())
                R.Cast(target);
        }

        public static void Killsteal()
        {

         foreach (var enemy in
                  ObjectManager.Get<Obj_AI_Hero>().Where(
                       ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible && ene.Distance(Player.Position) <= R.Range))
       {
           var collision = Q.GetCollision(ObjectManager.Player.ServerPosition.ToVector2(),
           new List<Vector2> { Q.GetPrediction(enemy).CastPosition.ToVector2() });
           var getcollision = collision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);

           if (Config["combo"]["UseQKS"].GetValue<MenuBool>().Value && Q.IsReady() && enemy.Health < Qdmg(enemy) && getcollision <= 1 &&
               Q.GetPrediction(enemy).Hitchance >= PredictionQ())
               Q.Cast(enemy);

           if (Config["combo"]["UseEKS"].GetValue<MenuBool>().Value && E.IsReady() && enemy.Health < Edmg(enemy) &&
               E.GetPrediction(enemy).Hitchance >= PredictionE())
               E.Cast(enemy);

                if (Overkillcheck(enemy) > enemy.Health)
                    return;

                if (R.IsReady() && enemy.Health < Rdmg(enemy) && Config["combo"]["UseRKS"].GetValue<MenuBool>().Value &&
                    R.GetPrediction(enemy).Hitchance >= PredictionR())
                    R.Cast(enemy);
            }  
        }
        public static void RCast()
        {
            Ignite = Player.GetSpellSlot("summonerdot");

            var target = TargetSelector.GetTarget(R.Range);
            if (Overkillcheck(target) > target.Health)
                return;

            if (Config["combo"]["UseRF"].GetValue<MenuBool>().Value)
            {
                if (Rdmg(target) > target.Health && R.GetPrediction(target).Hitchance >= PredictionR())
                    R.Cast(target);

                if (Rdmg(target) + Edmg(target) > target.Health && R.GetPrediction(target).Hitchance >= PredictionR() &&
                    LuxE.Position.Distance(target.Position) < 100)
                    R.Cast(target);
            }
            //Also check if enemy as a target works better () probably will work better I guess since you find the best enemy with the most hitcounts
            foreach (var enemy in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible && ene.Distance(Player.Position) <= R.Range))
            {
                if (Config["combo"]["useraoe"].GetValue<MenuBool>().Value)
                {
                    if (R.IsReady() && R.GetPrediction(enemy).Hitchance >= PredictionR() && R.GetPrediction(enemy).AoeHitCount >= Config["combo"]["raoeslider"].GetValue<MenuSlider>().Value)
                        R.Cast(enemy);
                }
            }
        }

        public static void Junglesteal()
        {
            if (Config["jungle"]["blue"].GetValue<MenuBool>().Value) //
            {
                var blueBuff =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.BaseSkinName == "SRU_Blue")
                        .Where(x => Rdmg(x) > x.Health)
                        .FirstOrDefault(x => (x.IsAlly) || (x.IsEnemy));

                if (blueBuff != null)
                    R.Cast(blueBuff);
            }

            if (Config["jungle"]["red"].GetValue<MenuBool>().Value) //
            {
                var redBuff =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.BaseSkinName == "SRU_Red")
                        .Where(x => Rdmg(x) > x.Health)
                        .FirstOrDefault(x => (x.IsAlly) || (x.IsEnemy));

                if (redBuff != null)
                    R.Cast(redBuff);
            }

            if (Config["jungle"]["baron"].GetValue<MenuBool>().Value) //
            {
                var Baron =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.BaseSkinName == "SRU_Baron")
                        .Where(x => Rdmg(x) > x.Health)
                        .FirstOrDefault(x => (x.IsAlly) || (x.IsEnemy));

                if (Baron != null)
                    R.Cast(Baron);
            }

            if (Config["jungle"]["dragon"].GetValue<MenuBool>().Value) //
            {
                var Dragon =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.BaseSkinName == "SRU_Dragon")
                        .Where(x => Rdmg(x) > x.Health)
                        .FirstOrDefault(x => (x.IsAlly) || (x.IsEnemy));

                if (Dragon != null)
                    R.Cast(Dragon);

            }
        }     

        public static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            //Lux E has detonated :S
            if (sender.Name.Contains("Lux_Base_E") && sender.IsAlly)

            {
                LuxE = null;
                Exist = 0;
            }
        }

        public static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            //Lux E spell position (detonation check/enemy check)
            if (sender.Name.Contains("Lux_Base_E") && sender.IsAlly)
            {
                LuxE = sender;
                Exist = 1;
            }
        }

        public static void OnUpdate(EventArgs args)
        {
            //ForceR
            if (Config["combo"]["forceR"].GetValue<MenuKeyBind>().Active)
            {
                ForceR();
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                    ComboLogic();
                    if (Config["spell"]["UseR"].GetValue<MenuBool>().Value)
                    {
                        RCast();
                    }
                    break;
                case OrbwalkerMode.LastHit:
                    Lasthit();
                    break;
                case OrbwalkerMode.LaneClear:
                    Laneclear();
                    break;
                case OrbwalkerMode.Hybrid:
                    HarassLogic();
                    break;
            }
            if (Config["spell"]["UseR"].GetValue<MenuBool>().Value)
            {
                RCast();
            }
            if (Config["jungle"]["toggle"].GetValue<MenuKeyBind>().Active)
            {
                Junglesteal();
            }
            if (Q.IsReady() && Config["spell"]["UseQ"].GetValue<MenuBool>().Value)
            {
                AutoQ();
            }
            Killsteal();
        }

        public static void Lasthit()
        {
            var aaminions = GameObjects.EnemyMinions.Where(m => m.IsValid && m.Distance(Player) < Player.GetRealAutoAttackRange()).ToList();
            foreach (var minion in aaminions.Where(m => m.IsMinion && !m.IsDead && m.HasBuff("luxilluminatingfraulein")))
            {
                var passivedmg = 10 + (8 * Player.Level) + Player.FlatMagicDamageMod * 0.2 - minion.FlatMagicReduction + Player.GetAutoAttackDamage(minion);
                if (minion.Health < passivedmg)
                {
                    Player.IssueOrder(GameObjectOrder.AutoAttack, minion);
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }
            }
        }
        public static void Laneclear()
        {
         var minions = GameObjects.EnemyMinions.Where(m => m.IsValid && m.Distance(Player) < E.Range).ToList();
         var aaminions = GameObjects.EnemyMinions.Where(m => m.IsValid && m.Distance(Player) < Player.GetRealAutoAttackRange()).ToList();
         var efarmpos = E.GetCircularFarmLocation(new List<Obj_AI_Base>(minions), E.Width);

            if (Player.Level < Config["laneclear"]["lanelevel"].GetValue<MenuSlider>().Value || Player.ManaPercent <= Config["laneclear"]["lanemana"].GetValue<MenuSlider>().Value)
            {
                return;
            }
            if (efarmpos.MinionsHit >= Config["laneclear"]["laneclearEcount"].GetValue<MenuSlider>().Value &&
                E.IsReady() && Config["laneclear"]["laneE"].GetValue<MenuBool>().Value)
                E.Cast(efarmpos.Position);

            var qfarmpos = Q.GetLineFarmLocation(new List<Obj_AI_Base>(minions), Q.Width);

            if (qfarmpos.MinionsHit >= Config["laneclear"]["laneclearQcount"].GetValue<MenuSlider>().Value &&
                Q.IsReady() && Config["laneclear"]["laneQ"].GetValue<MenuBool>().Value)
                Q.Cast(qfarmpos.Position);

            foreach (var minion in aaminions.Where(m => m.IsMinion && !m.IsDead && m.HasBuff("luxilluminatingfraulein")))
            {
             var passivedmg = 10 + (8*Player.Level) + Player.FlatMagicDamageMod*0.2 - minion.FlatMagicReduction + Player.GetAutoAttackDamage(minion);
                if (minion.Health < passivedmg)
                {
                    Player.IssueOrder(GameObjectOrder.AutoAttack, minion);
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }
            }

        }
        public static double Rdmg(Obj_AI_Base target)
        {


            var passivedmg = Player.CalculateDamage(target, DamageType.Magical, 
                    10 + (8 * Player.Level) + (0.2 * Player.FlatMagicDamageMod));


            double rdmg1 = 0;
            if (R.Level == 1)
                rdmg1 += Player.CalculateDamage(target, DamageType.Magical, 300 + (0.75 * Player.FlatMagicDamageMod));
            if (R.Level == 2)
                rdmg1 += Player.CalculateDamage(target, DamageType.Magical, 400 + (0.75 * Player.FlatMagicDamageMod));
            if (R.Level == 3)
                rdmg1 += Player.CalculateDamage(target, DamageType.Magical, 599 + (0.75 * Player.FlatMagicDamageMod));

            if (target.HasBuff("luxilluminatingfraulein"))
                rdmg1 += passivedmg;

            return rdmg1;
        }
        public static double Edmg(Obj_AI_Base target)
        {
            return
              Player.CalculateDamage(target, DamageType.Magical,
                    new[] { 60, 105, 150, 195, 240 }[Program.E.Level - 1] + 0.6 * Player.FlatMagicDamageMod);
        }
        public static double Qdmg(Obj_AI_Base target)
        {
            return
              Player.CalculateDamage(target, DamageType.Magical,
                    new[] { 60, 110, 160, 210, 260 }[Program.Q.Level - 1] + 0.7 * Player.FlatMagicDamageMod);
        }

        public static float ComboDmgFull(Obj_AI_Base target)
        {
            var passivedmg = 10 + (8 * Player.Level) + Player.FlatMagicDamageMod * 0.2 - target.FlatMagicReduction;
            var passiveaa = Player.GetAutoAttackDamage(Player) + passivedmg;
            var lichdmg = Player.CalculateDamage(target, DamageType.Magical,
            (Player.BaseAttackDamage * 0.75) + ((Player.BaseAbilityDamage + Player.FlatMagicDamageMod) * 0.5));

            double dmg = Player.GetAutoAttackDamage(target);

            if (E.IsReady())
                dmg += Edmg(target);
            if (LuxE != null && target.Position.Distance(LuxE.Position) < 275)
                dmg += Edmg(target);
            if (Q.IsReady())
                dmg += Qdmg(target);
            if (target.HasBuff("luxilluminatingfraulein") && target.IsValidTarget(Player.GetRealAutoAttackRange()))
                dmg += passiveaa;
            if (Player.HasBuff("lichbane"))
                dmg += lichdmg;
            if (R.IsReady())
                dmg += Rdmg(target);

            return (float)dmg;

        }
        public static double Overkillcheck(Obj_AI_Hero target)
        {
            var passivedmg = 10 + (8 * Player.Level) + Player.FlatMagicDamageMod * 0.2 - target.FlatMagicReduction;
            var lichdmg = Player.CalculateDamage(target, DamageType.Magical,
            (Player.BaseAttackDamage * 0.75) + ((Player.BaseAbilityDamage + Player.FlatMagicDamageMod) * 0.5));

            double dmg = 0;
            if (E.IsReady() && target.IsValidTarget(E.Range))
                dmg += Edmg(target);
            if (LuxE != null && target.Position.Distance(LuxE.Position) < 275)
                dmg += Edmg(target);
            if (Q.IsReady() && Q.GetPrediction(target).Hitchance >= HitChance.High && target.IsValidTarget(Q.Range))
                dmg += Qdmg(target);
            if (target.HasBuff("luxilluminatingfraulein") && target.IsValidTarget(Player.GetRealAutoAttackRange()))
                dmg += passivedmg;
            if (Player.HasBuff("lichbane") && target.IsValidTarget(Player.GetRealAutoAttackRange()))
                dmg += lichdmg;
            if (target.IsValidTarget(Player.GetRealAutoAttackRange()))
                dmg += Player.GetAutoAttackDamage(target);
            if (AlliesInRange(target.Position, 600) >= 1)
                dmg += 100 + (3*Player.Level);

            return dmg;
        }

        public static void HarassLogic()
        {
            var target = TargetSelector.GetTarget(Q.Range);
            var qprediction = Q.GetPrediction(target);
            if (target == null || target.IsInvulnerable)
                return;

            //GetCollision
            var collision = Q.GetCollision(ObjectManager.Player.ServerPosition.ToVector2(),
                new List<Vector2> { qprediction.CastPosition.ToVector2() });
            var getcollision = collision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);

            if (Config["harass"]["harrQ"].GetValue<MenuBool>().Value && Player.ManaPercent >= Config["harass"]["harassmana"].GetValue<MenuSlider>().Value)
            {
                if (Q.GetPrediction(target).Hitchance >= PredictionQ() && getcollision <= 1)
                    Q.Cast(target);
            }

            if (Config["harass"]["harrE"].GetValue<MenuBool>().Value && Player.ManaPercent >= Config["harass"]["harassmana"].GetValue<MenuSlider>().Value 
                && Orbwalker.ActiveMode != OrbwalkerMode.Orbwalk)
            {
                if (target.HasBuff("luxilluminatingfraulein") && target.HasBuff("LuxLightBindingMis") &&
                    Player.Distance(target.Position) <= Player.GetRealAutoAttackRange())
                    return;

                if (Exist == 1 && target.Distance(LuxE.Position) <= 275)
                    E.Cast();

                if (Q.GetPrediction(target).Hitchance >= PredictionQ() && getcollision <= 1 && Q.IsReady())
                    return;

                if (Exist == 0 && E.IsReady() && E.GetPrediction(target).Hitchance >= PredictionE())
                    E.Cast(target);

            }
        }
        public static void ComboLogic()
        {
            var target = TargetSelector.GetTarget(Q.Range);
            var qprediction = Q.GetPrediction(target);
            if (target == null || target.IsInvulnerable)
                return;

            //GetCollision
            var collision = Q.GetCollision(ObjectManager.Player.ServerPosition.ToVector2(),
                new List<Vector2> {qprediction.CastPosition.ToVector2()});
            var getcollision = collision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);

            if ((Config["spell"]["UseQ"].GetValue<MenuBool>().Value && Orbwalker.ActiveMode != OrbwalkerMode.Hybrid))
            {
                if (Q.GetPrediction(target).Hitchance >= PredictionQ() && getcollision <= 1)
                    Q.Cast(target);
            }
            //ECAST
            if ((Config["spell"]["UseE"].GetValue<MenuBool>().Value) && Orbwalker.ActiveMode != OrbwalkerMode.Hybrid)
            {
                if (target.HasBuff("luxilluminatingfraulein") && target.HasBuff("LuxLightBindingMis") &&
                    Player.Distance(target.Position) <= Player.GetRealAutoAttackRange())
                    return;

                if (Exist == 1 && target.Distance(LuxE.Position) <= 275)
                    E.Cast();

                if (Q.GetPrediction(target).Hitchance >= PredictionQ() && getcollision <= 1 && Q.IsReady())
                    return;

                if (Exist == 0 && E.IsReady() && E.GetPrediction(target).Hitchance >= PredictionE())
                    E.Cast(target);
            }
        }

        public static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;
            if (Ignite == SpellSlot.Summoner1)
                return (float) Player.GetSpellDamage(target, SpellSlot.Summoner1);
            if (Ignite == SpellSlot.Summoner2)
                return (float) Player.GetSpellDamage(target, SpellSlot.Summoner2);
            return 0;
        }
        public static HitChance PredictionQ()
        {
            var mode = Config["combo"]["hitchanceQ"].GetValue<MenuList<string>>();

            switch (mode.Index)
            {
                case 0:
                    return HitChance.High;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.Low;
                case 3:
                    return HitChance.VeryHigh;
            }
            return HitChance.VeryHigh;
        }
        public static HitChance PredictionE()
        {
            var mode = Config["combo"]["hitchanceE"].GetValue<MenuList<string>>();

            switch (mode.Index)
            {
                case 0:
                    return HitChance.High;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.Low;
                case 3:
                    return HitChance.VeryHigh;
            }
            return HitChance.High;
        }
        public static HitChance PredictionR()
        {
            var mode = Config["combo"]["hitchanceR"].GetValue<MenuList<string>>();

            switch (mode.Index)
            {
                case 0:
                    return HitChance.High;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.Low;
                case 3:
                    return HitChance.VeryHigh;
            }
            return HitChance.High;
        }
    }
}
