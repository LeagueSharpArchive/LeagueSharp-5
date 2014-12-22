﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Rumble
{
    internal class Rumble
    {
        #region Spells

        private static void RumbleCombo()
        {
            var target = SimpleTs.GetTarget(1200f, SimpleTs.DamageType.Magical);
            if (!target.IsValidTarget()) return;

            // ==== [ CHECKS ] ====

            CalculateHeatFunctions(target);

            var hitChance = HitChance.Medium;
            if (Menu.GetMenu() != null && Menu.GetItem(RumbleMenu.MiscHitChance) != null)
            {
                var menuItem = Menu.GetValue<StringList>(RumbleMenu.MiscHitChance);
                Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out hitChance);
            }

            // ==== [  COMBO  ] ====

            /* Q CASTING */
            if (Menu.GetValue<bool>(RumbleMenu.ComboQ) && QSpell.IsReady() && _shouldCastQAndE &&
                target.Distance(PlayerObjAiHero.Position) < 600f && PlayerObjAiHero.IsFacing(target, 600f) &&
                Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250)
            {
                QSpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
            }

            /* E CASTING */
            if (Menu.GetValue<bool>(RumbleMenu.ComboE) && !PlayerObjAiHero.HasBuff("RumbleGrenade", true) &&
                ESpell.IsReady() && _shouldCastQAndE &&
                target.Distance(PlayerObjAiHero.Position) < 850f &&
                Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250)
            {
                ESpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                _lastETick = Environment.TickCount;
            }

            if (Menu.GetValue<bool>(RumbleMenu.ComboE) && PlayerObjAiHero.HasBuff("RumbleGrenade", true) &&
                ESpell.IsReady() && target.Distance(PlayerObjAiHero.Position) < 850f)
            {
                var c0 = Menu.GetValue<bool>(RumbleMenu.MiscEMDelay) &&
                         Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250 &&
                         target.Distance(PlayerObjAiHero.Position) >= 300f;

                var c1 = Menu.GetValue<bool>(RumbleMenu.MiscEMDelay) &&
                         Environment.TickCount - _lastETick > 1000*Menu.GetValue<Slider>(RumbleMenu.MiscEDelay).Value &&
                         target.Distance(PlayerObjAiHero.Position) < 350f;

                var c2 = !Menu.GetValue<bool>(RumbleMenu.MiscEMDelay) &&
                         Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250;

                if (!c0 && !c1 && !c2) return;
                ESpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                _lastETick = Environment.TickCount;
            }

            /* W CASTNG */
            if (Menu.GetValue<bool>(RumbleMenu.ComboW) && WSpell.IsReady() && _shouldCastW &&
                target.Distance(PlayerObjAiHero.Position) < 900f &&
                Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250)
            {
                // TODO
            }

            /* R CASTING */
            if (Menu.GetValue<bool>(RumbleMenu.ComboR))
            {
                var rc0 = (RSpell.IsReady() && ESpell.IsReady() && QSpell.IsReady()) &&
                          PlayerObjAiHero.GetComboDamage(target,
                              new[] {SpellSlot.Q, SpellSlot.E, SpellSlot.E, SpellSlot.R}) >
                          target.Health;

                var rc1 = (RSpell.IsReady() && QSpell.IsReady()) &&
                          PlayerObjAiHero.GetComboDamage(target, new[] {SpellSlot.R, SpellSlot.Q}) > target.Health;

                var rc2 = (RSpell.IsReady() && ESpell.IsReady()) &&
                          PlayerObjAiHero.GetComboDamage(target, new[] {SpellSlot.R, SpellSlot.E, SpellSlot.E}) >
                          target.Health;

                if (!rc0 && !rc1 && !rc2) return;

                // TODO
            }

            if (Menu.GetValue<bool>(RumbleMenu.ComboOverheat) && _shouldOverheat &&
                Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250)
            {
                if (Menu.GetValue<bool>(RumbleMenu.ComboQ) && QSpell.IsReady() &&
                    target.Distance(PlayerObjAiHero.Position) < 600f && PlayerObjAiHero.IsFacing(target, 600f))
                {
                    QSpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                }

                if (Menu.GetValue<bool>(RumbleMenu.ComboE) &&
                    ESpell.IsReady() &&
                    target.Distance(PlayerObjAiHero.Position) < 850f)
                {
                    ESpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                    _lastETick = Environment.TickCount;
                }

                WSpell.Cast(Menu.GetValue<bool>(RumbleMenu.MiscPackets));
            }
        }

        private static void RumbleHarass()
        {
        }

        private static void RumbleFlee()
        {
            var target =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(e => e.IsValidTarget() && e.Distance(PlayerObjAiHero.Direction) < 850f)
                    .OrderBy(e => e.Distance(PlayerObjAiHero.Position))
                    .FirstOrDefault();

            if (ESpell.IsReady())
            {
                var hitChance = HitChance.Medium;
                if (Menu.GetMenu() != null && Menu.GetItem(RumbleMenu.MiscHitChance) != null)
                {
                    var menuItem = Menu.GetValue<StringList>(RumbleMenu.MiscHitChance);
                    Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out hitChance);
                }
                ESpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
            }

            if (WSpell.IsReady())
            {
                WSpell.Cast(Menu.GetValue<bool>(RumbleMenu.MiscPackets));
            }
        }

        private static void RumbleHeatManager()
        {
            if (PlayerObjAiHero.HasBuff("Recall", true))
                return;

            if (Menu.GetValue<bool>(RumbleMenu.HmStayInDanger) && PlayerObjAiHero.CountEnemysInRange(1000) < 2)
            {
                if (WSpell.IsReady() && Menu.GetValue<bool>(RumbleMenu.HmW) && PlayerObjAiHero.Mana < 35 &&
                    Environment.TickCount - _lastSavedTick > 750)
                {
                    _lastSavedTick = Environment.TickCount;
                    WSpell.Cast(Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                }
                else if (QSpell.IsReady() && Menu.GetValue<bool>(RumbleMenu.HmQ))
                {
                    QSpell.Cast(Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                }
            }
            else if (Menu.GetValue<bool>(RumbleMenu.HmStayInDanger))
            {
                if (!WSpell.IsReady() || !Menu.GetValue<bool>(RumbleMenu.HmW) || !(PlayerObjAiHero.Mana < 35) ||
                    !(Environment.TickCount - _lastSavedTick > 750)) return;
                _lastSavedTick = Environment.TickCount;
                WSpell.Cast(Menu.GetValue<bool>(RumbleMenu.HmW));
            }
        }

        private static void KillSteal()
        {
        }

        #endregion

        #region Main

        static Rumble()
        {
            Menu = new RumbleMenu();

            PlayerObjAiHero = ObjectManager.Player;

            RSpell = new Spell(SpellSlot.R, 1700f);
            ESpell = new Spell(SpellSlot.E, 850f);
            WSpell = new Spell(SpellSlot.W);
            QSpell = new Spell(SpellSlot.Q, 600f);
        }

        private static void Main()
        {
            if (!PlayerObjAiHero.ChampionName.Equals("Rumble"))
                return;

            ESpell.SetSkillshot(0.25f, 70, 2000, true, SkillshotType.SkillshotLine);
            RSpell.SetSkillshot(1700, 120, 1400, false, SkillshotType.SkillshotLine);

            CustomEvents.Game.OnGameLoad += args =>
            {
                Game.OnGameUpdate += GameOnOnGameUpdate;
                Drawing.OnDraw += DrawingOnOnDraw;
                Game.PrintChat("WorstPing | Rumble the Mechanized Menace, loaded.");
            };
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            switch (Menu.GetOrbwalker().ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    RumbleCombo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    RumbleLaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    RumbleHarass();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    RumbleLastHit();
                    break;
                case Orbwalking.OrbwalkingMode.Flee:
                    RumbleFlee();
                    break;
            }
            KillSteal();
            RumbleHeatManager();
        }

        #endregion

        #region Farming

        private static void RumbleLaneClear()
        {
        }

        private static void RumbleLastHit()
        {
        }

        #endregion

        #region Vars

        private static readonly Spell QSpell;
        private static readonly Spell WSpell;
        private static readonly Spell ESpell;
        private static readonly Spell RSpell;

        private static readonly Obj_AI_Hero PlayerObjAiHero;
        private static readonly RumbleMenu Menu;

        private static double _lastSavedTick;
        private static double _lastETick;

        private static bool _shouldOverheat;
        private static bool _shouldCastQAndE;
        private static bool _shouldCastW;

        #endregion

        #region Functions

        private static bool ShouldOverheat(Obj_AI_Base targetObjAiBase)
        {
            return ((PlayerObjAiHero.Mana > 80 && !QSpell.IsReady() && !ESpell.IsReady() &&
                     targetObjAiBase.Distance(PlayerObjAiHero.Position) < 350f) &&
                    ((PlayerObjAiHero.GetAutoAttackDamage(targetObjAiBase, true))*3D > targetObjAiBase.Health));
        }

        private static void CalculateHeatFunctions(Obj_AI_Base targetObjAiBase)
        {
            if (!targetObjAiBase.IsValidTarget()) return;

            if (PlayerObjAiHero.Mana > 80 && ShouldOverheat(targetObjAiBase))
            {
                _shouldOverheat = true;
            }
            else if (PlayerObjAiHero.Mana > 60)
            {
                _shouldCastQAndE = true;
            }
            else if (PlayerObjAiHero.Mana > 0)
            {
                _shouldCastQAndE = true;
                _shouldCastW = true;
            }
        }

        #endregion
    }
}