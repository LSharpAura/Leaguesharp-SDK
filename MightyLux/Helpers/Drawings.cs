using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.Extensions.SharpDX;
using LeagueSharp.SDK.Core.UI;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using SharpDX;
using Color = System.Drawing.Color;

namespace MightyLux.Helpers
{
    public class Drawings : Statics
    {
        public static void DrawEvent()
        {
            Drawing.OnDraw += OnDraw;
            Drawing.OnDraw += DamageIndicator1;
            Drawing.OnDraw += Indicator;
            Drawing.OnDraw += Rdamage;
        }

        private static void Rdamage(EventArgs args)
        {
            var enemies1 = GameObjects.EnemyHeroes.ToList();

            foreach (var enemy in enemies1.Where(e => !e.IsDead && e.IsVisible))
            {
                var pos = enemy.HPBarPosition;
                if (Config["util"]["drawRdmg"].GetValue<MenuBool>().Value && R.IsReady() && R.Level >= 1)
                    Drawing.DrawText(pos.X + 10, pos.Y - 25, Color.Yellow, "[R] Damage: " + Program.Rdmg(enemy).ToString());
            }
        }

        private static void Indicator(EventArgs args)
        {

            var enemies1 = GameObjects.EnemyHeroes.ToList();

            foreach (var enemy in enemies1.Where(enemy => enemy.Team != Player.Team))
            {
                if (enemy.IsVisible && !enemy.IsDead)
                {
                    if (Config["util"]["indicator"].GetValue<MenuBool>().Value)
                    {
                        var pos = Player.Position +
                                  Vector3.Normalize(enemy.Position - Player.Position)*150;
                        var myPos = Drawing.WorldToScreen(pos);
                        pos = Player.Position + Vector3.Normalize(enemy.Position - Player.Position) * 450;
                        var ePos = Drawing.WorldToScreen(pos);

                        var linecolor = Color.LawnGreen;

                        if (enemy.Position.Distance(Player.Position) > 3000)
                        {
                            linecolor = Color.LawnGreen;
                        }
                        else if (enemy.Position.Distance(Player.Position) < 3000) 
                            linecolor = Color.Red;

                            Drawing.DrawLine(myPos.X, myPos.Y, ePos.X, ePos.Y, 2, linecolor);
                        }
                    }
                }
            }
        

       
        private static void DamageIndicator1(EventArgs args)
        {

            var mode = Config["util"]["dmgdrawer"].GetValue<MenuList<string>>();
            switch (mode.Index)
            {
                case 0:

                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                    {
                        if (enemy.IsVisible && !enemy.IsDead)
                        {

                            var combodamage = (MightyLux.Program.ComboDmgFull(enemy));

                            var PercentHPleftAfterCombo = (enemy.Health - combodamage)/enemy.MaxHealth;
                            var PercentHPleft = enemy.Health/enemy.MaxHealth;
                            if (PercentHPleftAfterCombo < 0)
                                PercentHPleftAfterCombo = 0;

                            var hpBarPos = enemy.HPBarPosition;
                            hpBarPos.X += 45;
                            hpBarPos.Y += 18;
                            double comboXPos = hpBarPos.X - 36 + (107*PercentHPleftAfterCombo);
                            double currentHpxPos = hpBarPos.X - 36 + (107*PercentHPleft);
                            var diff = currentHpxPos - comboXPos;
                            for (var i = 0; i < diff; i++)
                            {
                                Drawing.DrawLine(
                                    (float) comboXPos + i, hpBarPos.Y + 2, (float) comboXPos + i,
                                    hpBarPos.Y + 10, 1, Color.FromArgb((int)Config["util"]["dmgcolor"].GetValue<MenuColor>().Color));
                                DamageIndicator.Enabled = false;
                            }

                        }
                    }
                    break;

                case 1:
                {
                    DamageIndicator.DamageToUnit = MightyLux.Program.ComboDmgFull;
                    DamageIndicator.Color = Color.FromArgb((int)Config["util"]["dmgcolor"].GetValue<MenuColor>().Color);
                    DamageIndicator.Enabled = true;
                }
                    break;
            }
        }


        private static void OnDraw(EventArgs args)
        {

            var mode = Config["draw"]["drawmode"].GetValue<MenuList<string>>();

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
        }
    }



