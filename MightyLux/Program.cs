﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using LeagueSharp.SDK.Core.UI.IMenu.Skins;
using LeagueSharp.SDK.Core.Utils;
using SharpDX;
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;

using Color = System.Drawing.Color;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;


namespace MightyLux
{
    internal class Program
    {
        public static Font LuxFont;
        public static Spell Q, W, E, R;
        public static Menu Config;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        public static GameObject LuxE;
        private static SpellSlot Ignite;

        private static void Main(string[] args)
        {
            Load.OnLoad += OnLoad;
        }

        private static void OnLoad(object sender, EventArgs e)
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

            var combo = Config.Add(new Menu("combo", "Combo Settings"));
            combo.Add(new MenuSeparator("Combo Menu", "Combo Menu"));
            combo.Add(new MenuBool("UseQ", "Use Q", true));
            combo.Add(new MenuBool("UseE", "Use E", true));
            combo.Add(new MenuBool("UseR", "Use R", true));
            combo.Add(new MenuSeparator("Advanced R Settings", "Advanced R Settings"));
            combo.Add(new MenuBool("UseRF", "Use R Finisher", true));
            combo.Add((new MenuBool("UseRKS", "Use R for Killstealing", true)));

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

            jungle.Add(new MenuList<string>("jungleteam", "[BROKEN/NOT WORKING]", objects: new[] { "Enemy", "Ally" }));

            var drawing = Config.Add(new Menu("draw", "Draw Settings"));
            drawing.Add(new MenuSeparator("Draw Menu", "Draw Menu"));
            drawing.Add(new MenuBool("disable", "Disable all drawings", false));
            drawing.Add(new MenuList<string>("drawmode", "Drawing Mode:", objects: new[] { "Normal", "Custom"}));
            drawing.Add(new MenuBool("disableq", "[Q] draw", true));
            drawing.Add(new MenuBool("disablew", "[W] draw", true));
            drawing.Add(new MenuBool("disablee", "[E] draw", true));
            drawing.Add(new MenuBool("disabler", "[R] draw", true));
            drawing.Add(new MenuSeparator("Color Settings", "Color Settings"));
            drawing.Add(new MenuColor("drawq", "[Q] Range Draw Color", new ColorBGRA(32, 20, 10, 255)));
            drawing.Add(new MenuColor("draww", "[W] Range Draw Color", new ColorBGRA(32, 20, 10, 255)));
            drawing.Add(new MenuColor("drawe", "[E] Range Draw Color", new ColorBGRA(32, 20, 10, 255)));
            drawing.Add(new MenuColor("drawr", "[R] Range Draw Color", new ColorBGRA(32, 20, 10, 255)));
            drawing.Add(new MenuSeparator("Misc Drawings", "Misc Drawings"));
            drawing.Add(new MenuBool("dmg", "Draw Damage Amount", true));
            drawing.Add(new MenuBool("dmg%", "Draw Damage % Amount", true));
            drawing.Add(new MenuBool("orbmode", "Draw Active Orbwalk Mode", true));

            combo.Add(new MenuSeparator("science", "Prediction Settings"));
            combo.Add(new MenuList<string>("hitchanceQ", "[Q] Hitchance", objects: new[] { "High", "Medium", "Low", "VeryHigh" }));
            combo.Add(new MenuList<string>("hitchanceE", "[E] Hitchance", objects: new[] { "High", "Medium", "Low", "VeryHigh" }));
            combo.Add(new MenuList<string>("hitchanceR", "[R] Hitchance", objects: new[] { "High", "Medium", "Low", "VeryHigh" }));

          Config.Add(new MenuButton("resetAll", "Settings", "Reset All Settings")
                    {Action = () =>
                    {
                        Config.RestoreDefault();
                    }
                    });

            //Font


            Config.Attach();
            Game.OnUpdate += OnUpdate;
            GameObject.OnDelete += GameObject_OnDelete;
            GameObject.OnCreate += GameObject_OnCreate;
            Drawing.OnDraw += Ondraw;
            Drawing.OnDraw += DamageDrawing;
        }

        private static void RCast()
        {
            Ignite = Player.GetSpellSlot("summonerdot");

            foreach (var enemy in
                ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible && ene.Distance(Player.Position) <= R.Range))
            {
                if (Overkillcheck(enemy) > enemy.Health)
                    return;
                if (R.IsReady() && enemy.Health < Rdmg(enemy) && Config["combo"]["UseRKS"].GetValue<MenuBool>().Value && R.GetPrediction(enemy).Hitchance >= PredictionR())
                    R.Cast(enemy);
            }

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
        }
        private static void Junglesteal()
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

        private static void DamageDrawing(EventArgs args)
        {
            var pos1 = Drawing.WorldToScreen(Player.Position);

            if (Config["draw"]["disable"].GetValue<MenuBool>().Value)
                return;

            foreach (var enemy in
                ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
            {
                var percent = ComboDmgFull(enemy);
                var percent2 = ComboDmgFull(enemy) * 100 / enemy.MaxHealth;
                var pos = Drawing.WorldToScreen(enemy.Position);

                if (Config["draw"]["dmg"].GetValue<MenuBool>().Value)
                Drawing.DrawText(pos.X - 40, pos.Y + 20, System.Drawing.Color.Gold, "[" + percent.ToString("#.#") + "]" + " Combo Damage");

                if (percent2 < enemy.HealthPercent && Config["draw"]["dmg%"].GetValue<MenuBool>().Value)
                Drawing.DrawText(pos.X - 40, pos.Y + 35, System.Drawing.Color.LawnGreen, "[" + percent2.ToString("#.#") + "%] " + " Combo Damage");
                if (percent2 > enemy.HealthPercent && Config["draw"]["dmg%"].GetValue<MenuBool>().Value)
                    Drawing.DrawText(pos.X - 40, pos.Y + 35, System.Drawing.Color.LawnGreen, "[KILLABLE]");

            }

            if (!Player.IsDead && Config["draw"]["orbmode"].GetValue<MenuBool>().Value)
            {
                Drawing.DrawText(pos1.X - 75, pos1.Y + 40, Color.Gold, "[Orbwalker Mode]:");
                Drawing.DrawText(pos1.X + 60, pos1.Y + 40, Color.LawnGreen, Orbwalker.ActiveMode.ToString());
            }
            if (Config["jungle"]["toggle"].GetValue<MenuKeyBind>().Active)
                Drawing.DrawText(pos1.X - 75, pos1.Y + 60, Color.LawnGreen, "Junglesteal Enabled");
        }

        private static void Ondraw(EventArgs args)
        {

            var mode = Config["draw"]["drawmode"].GetValue<MenuList<string>>();
            if (Config["draw"]["disable"].GetValue<MenuBool>().Value) return;

            switch (mode.Index)
            {
                case 0:
                {
                    if (Q.Level >= 1 && Config["draw"]["disableq"].GetValue<MenuBool>().Value)
                        Drawing.DrawCircle(GameObjects.Player.Position, Q.Range,
                            Color.FromArgb((int) Config["draw"]["drawq"].GetValue<MenuColor>().Color));
                    if (W.Level >= 1 && Config["draw"]["disablew"].GetValue<MenuBool>().Value)
                        Drawing.DrawCircle(GameObjects.Player.Position, W.Range,
                            Color.FromArgb((int) Config["draw"]["draww"].GetValue<MenuColor>().Color));
                    if (E.Level >= 1 && Config["draw"]["disablee"].GetValue<MenuBool>().Value)
                        Drawing.DrawCircle(GameObjects.Player.Position, E.Range,
                            Color.FromArgb((int) Config["draw"]["drawe"].GetValue<MenuColor>().Color));
                    if (R.Level >= 1 && Config["draw"]["disabler"].GetValue<MenuBool>().Value)
                        Drawing.DrawCircle(GameObjects.Player.Position, R.Range,
                            Color.FromArgb((int) Config["draw"]["drawr"].GetValue<MenuColor>().Color));
                    break;
                }
            }
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            //Lux E has detonated :S
            if (sender.Name.Contains("Lux_Base_E"))

            {
                LuxE = null;
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            //Lux E spell position (detonation check/enemy check)
            if (sender.Name.Contains("Lux_Base_E"))
            {
                LuxE = sender;
            }
        }

        private static void OnUpdate(EventArgs args)
        {

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                    ComboLogic();
                    if (Config["combo"]["UseR"].GetValue<MenuBool>().Value)
                    {
                        RCast();
                    }
                    break;
                case OrbwalkerMode.LastHit:
                    break;
                case OrbwalkerMode.LaneClear:
                    Laneclear();
                    break;
                case OrbwalkerMode.Hybrid:
                    ComboLogic();
                    break;
            }
            if (Config["combo"]["UseR"].GetValue<MenuBool>().Value)
                RCast();
            if (Config["jungle"]["toggle"].GetValue<MenuKeyBind>().Active)
                Junglesteal();
        }

        private static void Laneclear()
        {

         var minions = GameObjects.EnemyMinions.Where(m => m.IsValid && m.Distance(Player) < E.Range).ToList();
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

        }
        private static double Rdmg(Obj_AI_Base target)
        {
            var passivedmg = 10 + (8*Player.Level) + Player.FlatMagicDamageMod*0.2 - target.FlatMagicReduction;
            double rdmg1 = 0;
            if (R.Level == 1)
                rdmg1 += Player.CalculateDamage(target, DamageType.Magical, 300 + 0.75*Player.FlatMagicDamageMod);
            if (R.Level == 2)
                rdmg1 += Player.CalculateDamage(target, DamageType.Magical, 400 + 0.75 * Player.FlatMagicDamageMod);
            if (R.Level == 3)
                rdmg1 += Player.CalculateDamage(target, DamageType.Magical, 500 + 0.75 * Player.FlatMagicDamageMod);
            if (target.HasBuff("luxilluminatingfraulein"))
                rdmg1 += passivedmg;

            return rdmg1;
        }
        private static double Edmg(Obj_AI_Base target)
        {
            return
              Player.CalculateDamage(target, DamageType.Magical,
                    new[] { 60, 105, 150, 195, 240 }[Program.E.Level - 1] + 0.6 * Player.FlatMagicDamageMod);
        }
        private static double Qdmg(Obj_AI_Base target)
        {
            return
              Player.CalculateDamage(target, DamageType.Magical,
                    new[] { 60, 110, 160, 210, 260 }[Program.Q.Level - 1] + 0.7 * Player.FlatMagicDamageMod);
        }

        private static double ComboDmgFull(Obj_AI_Base target)
        {
            var passivedmg = 10 + (8 * Player.Level) + Player.FlatMagicDamageMod * 0.2 - target.FlatMagicReduction;
            var passiveaa = Player.GetAutoAttackDamage(Player) + passivedmg;
            var lichdmg = Player.CalculateDamage(target, DamageType.Magical,
            (Player.BaseAttackDamage * 0.75) + ((Player.BaseAbilityDamage + Player.FlatMagicDamageMod) * 0.5));

            double dmg = Player.GetAutoAttackDamage(target);
            if (E.IsReady())
                dmg += Edmg(target);
            if (LuxE != null && target.Position.Distance(LuxE.Position) < 280)
                dmg += Edmg(target);
            if (Q.IsReady())
                dmg += Qdmg(target);
            if (target.HasBuff("luxilluminatingfraulein") && target.IsValidTarget(Player.GetRealAutoAttackRange()))
                dmg += passiveaa;
            if (Player.HasBuff("lichbane"))
                dmg += lichdmg;
            if (R.IsReady())
                dmg += Rdmg(target);

            return dmg;

        }
        private static double Overkillcheck(Obj_AI_Base target)
        {
            var passivedmg = 10 + (8 * Player.Level) + Player.FlatMagicDamageMod * 0.2 - target.FlatMagicReduction;
            var lichdmg = Player.CalculateDamage(target, DamageType.Magical,
            (Player.BaseAttackDamage * 0.75) + ((Player.BaseAbilityDamage + Player.FlatMagicDamageMod) * 0.5));

            double dmg = 0;
            if (E.IsReady() && target.IsValidTarget(E.Range))
                dmg += Edmg(target);
            if (LuxE != null && target.Position.Distance(LuxE.Position) < 280)
                dmg += Edmg(target);
            if (Q.IsReady() && Q.GetPrediction(target).Hitchance >= HitChance.High && target.IsValidTarget(Q.Range))
                dmg += Qdmg(target);
            if (target.HasBuff("luxilluminatingfraulein") && target.IsValidTarget(Player.GetRealAutoAttackRange()))
                dmg += passivedmg;
            if (Player.HasBuff("lichbane") && target.IsValidTarget(Player.GetRealAutoAttackRange()))
                dmg += lichdmg;
            if (target.IsValidTarget(Player.GetRealAutoAttackRange()))
                dmg += Player.GetAutoAttackDamage(target);

            return dmg;
        }

        private static void ComboLogic()
        {
            var target = TargetSelector.GetTarget(Q.Range);
            var qprediction = Q.GetPrediction(target);
            var eprediciton = E.GetPrediction(target);
            if (target == null || target.IsInvulnerable)
                return;

            //GetCollision
            var collision = Q.GetCollision(ObjectManager.Player.ServerPosition.ToVector2(),
                new List<Vector2> {qprediction.CastPosition.ToVector2()});
            var getcollision = collision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);

            if ((Config["combo"]["UseQ"].GetValue<MenuBool>().Value ||
                 Config["harass"]["harrQ"].GetValue<MenuBool>().Value && Player.ManaPercent >= Config["harass"]["harassmana"].GetValue<MenuSlider>().Value))
            {
                if (Q.GetPrediction(target).Hitchance >= PredictionQ() && getcollision <= 1)
                    Q.Cast(target);
            }

            //ECAST
            if ((Config["combo"]["UseE"].GetValue<MenuBool>().Value ||
                 Config["harass"]["harrE"].GetValue<MenuBool>().Value && Player.ManaPercent >= Config["harass"]["harassmana"].GetValue<MenuSlider>().Value))
            {
                if (LuxE == null && E.IsReady() && E.GetPrediction(target).Hitchance >= PredictionE())
                    E.Cast(target);
                //EDETONATE
                if (target.HasBuff("luxilluminatingfraulein") && target.HasBuff("LuxLightBindingMis") &&
                    Player.Distance(target.Position) <= Player.GetRealAutoAttackRange())
                    return;

                if (LuxE != null && target.Distance(LuxE.Position) <= 280)
                    E.Cast();
            }
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;
            if (Ignite == SpellSlot.Summoner1)
                return (float) Player.GetSpellDamage(target, SpellSlot.Summoner1);
            if (Ignite == SpellSlot.Summoner2)
                return (float) Player.GetSpellDamage(target, SpellSlot.Summoner2);
            return 0;
        }
        private static HitChance PredictionQ()
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
        private static HitChance PredictionE()
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
        private static HitChance PredictionR()
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