﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Yasuo.Evade;

namespace Yasuo
{
    public class YasuoGame
    {
        private static int _sweepingBladeDelay;

        public static void OnGameUpdate(EventArgs args)
        {
            Yasuo.EvadeDetectedSkillshots.RemoveAll(skillshot => !skillshot.IsActive());
            Evader();

            switch (Yasuo.Menu.GetOrbwalker().ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    if (Yasuo.Menu.GetItemValue<KeyBind>(YasuoMenu.FleeKeyLoc).Active)
                    {
                        Flee();
                    }
                    break;
            }

            Killsteal();

            if (Yasuo.Menu.GetOrbwalker().ActiveMode == Orbwalking.OrbwalkingMode.None)
            {
                Auto();
            }
        }

        private static void Combo()
        {
            var packets = Yasuo.Menu.GetItemValue<bool>(YasuoMenu.MiscPacketsLoc);

            var target = TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Physical);

            // => Sweeping Blade (E)
            if (Yasuo.Menu.GetItemValue<StringList>(YasuoMenu.ComboGapcloserModeLoc).SelectedIndex == 0 &&
                Yasuo.Menu.GetItemValue<bool>(YasuoMenu.ComboELoc) && YasuoSpells.E.IsReady())
            {
                // => In AA range, keep dashing
                if (target != null && Yasuo.Player.Distance(target.Position) < Yasuo.Player.GetAutoAttackRange())
                {
                    if (Yasuo.Menu.GetItemValue<bool>(YasuoMenu.ComboGapcloserEModeLoc))
                    {
                        var dashData = Yasuo.Player.GetDashData(Game.CursorPos, target);
                        var dashPoint = (Vector3) dashData[0];
                        var dashAi = (Obj_AI_Base) dashData[1];

                        if (dashPoint.IsValid() && dashAi.IsValidTarget())
                        {
                            if (Environment.TickCount - _sweepingBladeDelay > 300)
                            {
                                if (dashPoint.Distance(Game.CursorPos) < Yasuo.Player.Distance(Game.CursorPos) - 100 &&
                                    dashPoint.Distance(Game.CursorPos) < 250)
                                {
                                    YasuoSpells.E.Cast(dashAi, packets);
                                    _sweepingBladeDelay = Environment.TickCount;
                                }
                            }
                        }
                    }
                }
                else if (target != null && Yasuo.Player.Distance(target.Position) > Yasuo.Player.GetAutoAttackRange())
                {
                    // target, but out of AA
                    var dashData = Yasuo.Player.GetDashData(Game.CursorPos, target);
                    var dashPoint = (Vector3) dashData[0];
                    var dashAi = (Obj_AI_Base) dashData[1];

                    if (dashPoint.IsValid() && dashAi.IsValidTarget())
                    {
                        if (Environment.TickCount - _sweepingBladeDelay > 300)
                        {
                            if (dashPoint.Distance(Game.CursorPos) < Yasuo.Player.Distance(Game.CursorPos) - 100 &&
                                dashPoint.Distance(Game.CursorPos) < 250)
                            {
                                YasuoSpells.E.Cast(dashAi, packets);
                                _sweepingBladeDelay = Environment.TickCount;
                            }
                        }
                    }
                }
                else if (target == null)
                {
                    // no target, just dash
                    var dashData = Yasuo.Player.GetDashData(Game.CursorPos);
                    var dashPoint = (Vector3) dashData[0];
                    var dashAi = (Obj_AI_Base) dashData[1];

                    if (dashPoint.IsValid() && dashAi.IsValidTarget())
                    {
                        if (Environment.TickCount - _sweepingBladeDelay > 300)
                        {
                            if (dashPoint.Distance(Game.CursorPos) < Yasuo.Player.Distance(Game.CursorPos) - 100 &&
                                dashPoint.Distance(Game.CursorPos) < 250)
                            {
                                YasuoSpells.E.Cast(dashAi, packets);
                                _sweepingBladeDelay = Environment.TickCount;
                            }
                        }
                    }
                }
            }

            if (target == null)
            {
                return;
            }

            // => Steel Tempest (Q)
            if (Yasuo.Menu.GetItemValue<bool>(YasuoMenu.ComboQLoc) && Yasuo.Player.GetYasuoQState().IsReady())
            {
                var prediction = Prediction.GetPrediction(
                    target, Yasuo.Player.GetYasuoQState().Delay, 0f, Yasuo.Player.GetYasuoQState().Speed);
                if (!Yasuo.Player.HasWhirlwind() &&
                    Yasuo.Player.Distance(target.Position) < Yasuo.Player.GetYasuoQState().Range)
                {
                    if (Yasuo.Player.IsDashing() && Yasuo.Player.Distance(target.Position) < 375f)
                    {
                        Yasuo.Player.GetYasuoQState().Cast(prediction.CastPosition, packets);
                    }
                    else if (!Yasuo.Player.IsDashing())
                    {
                        Yasuo.Player.GetYasuoQState().Cast(prediction.CastPosition, packets);
                    }
                }
                else
                {
                    if (Yasuo.Player.HasWhirlwind() &&
                        Yasuo.Player.Distance(target.Position) < Yasuo.Player.GetYasuoQState().Range)
                    {
                        if (Yasuo.Player.IsDashing() && Yasuo.Player.Distance(target.Position) < 375f)
                        {
                            Yasuo.Player.GetYasuoQState().Cast(prediction.CastPosition, packets);
                        }
                        else if (!Yasuo.Player.IsDashing())
                        {
                            Yasuo.Player.GetYasuoQState().Cast(prediction.CastPosition, packets);
                        }
                    }
                }
            }

            // => Sweeping Blade (E)
            if (Yasuo.Menu.GetItemValue<StringList>(YasuoMenu.ComboGapcloserModeLoc).SelectedIndex == 1 &&
                Yasuo.Menu.GetItemValue<bool>(YasuoMenu.ComboELoc) && YasuoSpells.E.IsReady())
            {
                // => In AA range, keep dashing
                if (Yasuo.Player.Distance(target.Position) < Yasuo.Player.GetAutoAttackRange())
                {
                    if (Yasuo.Menu.GetItemValue<bool>(YasuoMenu.ComboGapcloserEModeLoc))
                    {
                        var dashData = Yasuo.Player.GetDashData(target.Position);
                        var dashPoint = (Vector3) dashData[0];
                        var dashAi = (Obj_AI_Base) dashData[1];

                        if (dashPoint.IsValid() && dashAi.IsValidTarget())
                        {
                            if (Environment.TickCount - _sweepingBladeDelay > 300)
                            {
                                if (dashPoint.Distance(target.Position) < Yasuo.Player.Distance(target.Position) - 100 &&
                                    dashPoint.Distance(target.Position) < 250)
                                {
                                    YasuoSpells.E.Cast(dashAi, packets);
                                    _sweepingBladeDelay = Environment.TickCount;
                                }
                            }
                        }
                    }
                }
                else if (Yasuo.Player.Distance(target.Position) > Yasuo.Player.GetAutoAttackRange())
                {
                    // target, but out of AA
                    var dashData = Yasuo.Player.GetDashData(target.Position);
                    var dashPoint = (Vector3) dashData[0];
                    var dashAi = (Obj_AI_Base) dashData[1];

                    if (dashPoint.IsValid() && dashAi.IsValidTarget())
                    {
                        if (Environment.TickCount - _sweepingBladeDelay > 300)
                        {
                            if (dashPoint.Distance(target.Position) < Yasuo.Player.Distance(target.Position) - 100 &&
                                dashPoint.Distance(target.Position) < 250)
                            {
                                YasuoSpells.E.Cast(dashAi, packets);
                                _sweepingBladeDelay = Environment.TickCount;
                            }
                        }
                    }
                }
            }

            // => Last Breath (R)
            if (Yasuo.Menu.GetItemValue<bool>(YasuoMenu.ComboRLoc) && YasuoSpells.R.IsReady())
            {
                var enemies =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            e =>
                                e.IsValidTarget() &&
                                e.IsKnockedUp(Yasuo.Menu.GetItemValue<bool>(YasuoMenu.ComboRKnockTypeLoc)))
                        .ToList();
                var f0 = (Yasuo.Menu.GetItemValue<Slider>(YasuoMenu.ComboRDelayLoc).Value) + 0.0;
                const double f1 = 100.0;
                var fraction = f0 / f1;
                var shouldUseR =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Any(
                            e =>
                                e.IsValidTarget() &&
                                e.KnockedUpTimeLeft(Yasuo.Menu.GetItemValue<bool>(YasuoMenu.ComboRKnockTypeLoc)) > 0 &&
                                e.KnockedUpTimeLeft(Yasuo.Menu.GetItemValue<bool>(YasuoMenu.ComboRKnockTypeLoc)) <=
                                fraction ||
                                target.KnockedUpTimeLeft(Yasuo.Menu.GetItemValue<bool>(YasuoMenu.ComboRKnockTypeLoc)) >
                                0 &&
                                target.KnockedUpTimeLeft(Yasuo.Menu.GetItemValue<bool>(YasuoMenu.ComboRKnockTypeLoc)) <=
                                fraction);

                if ((enemies.Count >= Yasuo.Menu.GetItemValue<Slider>(YasuoMenu.ComboREnemiesLoc).Value ||
                     Yasuo.Menu.GetItemValue<bool>(YasuoMenu.ComboRTargetLoc)) && shouldUseR)
                {
                    var health = enemies.Sum(enemy => enemy.Health / enemy.MaxHealth * 100) / enemies.Count;
                    var health2 = target.Health / target.MaxHealth * 100;
                    if (health <= Yasuo.Menu.GetItemValue<Slider>(YasuoMenu.ComboREnemiesPercentLoc).Value)
                    {
                        YasuoSpells.R.Cast(
                            enemies.OrderBy(m => m.Distance(Yasuo.Player.Position)).LastOrDefault(), packets);
                    }
                    else if (health2 <= Yasuo.Menu.GetItemValue<Slider>(YasuoMenu.ComboREnemyPercentLoc).Value)
                    {
                        YasuoSpells.R.Cast(target, packets);
                    }
                }
            }

            // => Items
            if (Yasuo.Menu.GetItemValue<bool>(YasuoMenu.ComboItemsLoc))
            {
                if (YasuoSpells.RavenousHydra.GetItem().IsOwned())
                {
                    var hydra = YasuoSpells.RavenousHydra.GetItem();
                    var range = YasuoSpells.RavenousHydra.Range;
                    if (Yasuo.Player.Distance(target.Position) < range)
                    {
                        hydra.Cast();
                    }
                }
                else if (YasuoSpells.Tiamat.GetItem().IsOwned())
                {
                    var tiamat = YasuoSpells.Tiamat.GetItem();
                    var range = YasuoSpells.Tiamat.Range;
                    if (Yasuo.Player.Distance(target.Position) < range)
                    {
                        tiamat.Cast();
                    }
                }

                if (YasuoSpells.BladeoftheRuinedKing.GetItem().IsOwned())
                {
                    var botrk = YasuoSpells.BladeoftheRuinedKing.GetItem();
                    var range = YasuoSpells.BladeoftheRuinedKing.Range;
                    if (Yasuo.Player.Distance(target.Position) < range)
                    {
                        botrk.Cast(target);
                    }
                }
                else if (YasuoSpells.BilgewaterCutlass.GetItem().IsOwned())
                {
                    var cutlass = YasuoSpells.BilgewaterCutlass.GetItem();
                    var range = YasuoSpells.BilgewaterCutlass.Range;
                    if (Yasuo.Player.Distance(target.Position) < range)
                    {
                        cutlass.Cast(target);
                    }
                }
            }
        }

        private static void Harass()
        {
            // TODO
        }

        private static void LaneClear()
        {
            if (LastHit())
            {
                return;
            }

            var packets = Yasuo.Menu.GetItemValue<bool>(YasuoMenu.MiscPacketsLoc);
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        m =>
                            m.IsValidTarget() &&
                            m.Distance(Yasuo.Player.Position) <= Yasuo.Player.BoundingRadius + 1200f);
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();

            if (!objAiMinions.Any())
            {
                return;
            }

            var ownHydraOrTiamat = YasuoSpells.Tiamat.GetItem().IsOwned() ||
                                   YasuoSpells.RavenousHydra.GetItem().IsOwned();

            if (ownHydraOrTiamat && Yasuo.Menu.GetItemValue<bool>(YasuoMenu.FarmingLaneClearItemsLoc))
            {
                var itemRange = (YasuoSpells.Tiamat.GetItem().IsOwned())
                    ? YasuoSpells.Tiamat.Range
                    : (YasuoSpells.RavenousHydra.GetItem().IsOwned() ? YasuoSpells.RavenousHydra.Range : 0f);


                if (objAiMinions.Count(m => m.Distance(Yasuo.Player.Position) <= itemRange) >=
                    Yasuo.Menu.GetItemValue<Slider>(YasuoMenu.FarmingLaneClearMinItemsLoc).Value)
                {
                    if (YasuoSpells.Tiamat.GetItem().IsOwned())
                    {
                        YasuoSpells.Tiamat.GetItem().Cast();
                    }
                    else if (YasuoSpells.RavenousHydra.GetItem().IsOwned())
                    {
                        YasuoSpells.RavenousHydra.GetItem().Cast();
                    }
                }
            }

            var closestMinion = objAiMinions.OrderBy(m => m.Distance(Yasuo.Player.Position)).FirstOrDefault();
            if (closestMinion != null &&
                closestMinion.Distance(Yasuo.Player.Position) <= Yasuo.Player.GetYasuoQState().Range)
            {
                if (Yasuo.Player.HasWhirlwind() && Yasuo.Menu.GetItemValue<bool>(YasuoMenu.FarmingLaneClearQWindLoc))
                {
                    if (Yasuo.Player.IsDashing() && Yasuo.Player.Distance(closestMinion.Position) < 375f)
                    {
                        Yasuo.Player.GetYasuoQState().CastIfHitchanceEquals(closestMinion, HitChance.High, packets);
                    }
                    else if (!Yasuo.Player.IsDashing())
                    {
                        Yasuo.Player.GetYasuoQState().CastIfHitchanceEquals(closestMinion, HitChance.High, packets);
                    }
                }
                else if (!Yasuo.Player.HasWhirlwind() && Yasuo.Menu.GetItemValue<bool>(YasuoMenu.FarmingLaneClearQLoc))
                {
                    if (Yasuo.Player.IsDashing() && Yasuo.Player.Distance(closestMinion.Position) < 375f)
                    {
                        Yasuo.Player.GetYasuoQState().CastIfHitchanceEquals(closestMinion, HitChance.High, packets);
                    }
                    else if (!Yasuo.Player.IsDashing())
                    {
                        Yasuo.Player.GetYasuoQState().CastIfHitchanceEquals(closestMinion, HitChance.High, packets);
                    }
                }
            }
        }

        private static bool LastHit()
        {
            var packets = Yasuo.Menu.GetItemValue<bool>(YasuoMenu.MiscPacketsLoc);
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        m =>
                            m.IsValidTarget() &&
                            m.Distance(Yasuo.Player.Position) <= Yasuo.Player.BoundingRadius + 1200f);
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();

            if (!objAiMinions.Any())
            {
                return false;
            }

            var siege =
                objAiMinions.Where(m => m.BaseSkinName.Contains("Siege")).OrderBy(m => m.Health).FirstOrDefault();
            var stacksPassive = Yasuo.Player.Buffs.FirstOrDefault(b => b.DisplayName.Equals("YasuoDashScalar"));
            var stacks = 1 + 0.25 * ((stacksPassive != null) ? stacksPassive.Count : 0);

            // Siege minion priority
            if (siege != null)
            {
                // => Sweeping Blade (E)
                if (YasuoSpells.E.IsReady() && siege.Health < Yasuo.Player.GetSweepingBladeDamage(siege, stacks) &&
                    Yasuo.Player.Distance(siege) <= YasuoSpells.E.Range &&
                    Yasuo.Menu.GetItemValue<bool>(YasuoMenu.FarmingLastHitELoc))
                {
                    if (!Yasuo.Player.GetDashingEnd(siege).To3D().UnderTurret(true))
                    {
                        YasuoSpells.E.Cast(siege);
                        return true;
                    }
                }
                if (siege.Health < Yasuo.Player.GetSteelTempestDamage(siege) &&
                    siege.Health > Yasuo.Player.GetSweepingBladeDamage(siege, stacks) &&
                    Yasuo.Player.Distance(siege) <= Yasuo.Player.GetYasuoQState().Range &&
                    Yasuo.Menu.GetItemValue<bool>(YasuoMenu.FarmingLastHitQLoc))
                {
                    // => Steel Tempest (Q)
                    if (Yasuo.Player.IsDashing() && Yasuo.Player.Distance(siege.Position) < 375f)
                    {
                        Yasuo.Player.GetYasuoQState().CastIfHitchanceEquals(siege, HitChance.High, packets);
                    }
                    else if (!Yasuo.Player.IsDashing())
                    {
                        Yasuo.Player.GetYasuoQState().CastIfHitchanceEquals(siege, HitChance.High, packets);
                    }
                    return true;
                }
                if (siege.Health <
                    Yasuo.Player.GetSteelTempestDamage(siege) + Yasuo.Player.GetSweepingBladeDamage(siege, stacks) &&
                    siege.Health > Yasuo.Player.GetSweepingBladeDamage(siege, stacks) &&
                    Yasuo.Player.Distance(siege) <= YasuoSpells.E.Range &&
                    Yasuo.Menu.GetItemValue<bool>(YasuoMenu.FarmingLastHitELoc) &&
                    Yasuo.Menu.GetItemValue<bool>(YasuoMenu.FarmingLastHitQLoc))
                {
                    // => Steel Tempest (Q) + Sweeping Blade (E)
                    if (!Yasuo.Player.GetDashingEnd(siege).To3D().UnderTurret(true))
                    {
                        YasuoSpells.E.Cast(siege);
                        Yasuo.Player.GetYasuoQState().CastIfHitchanceEquals(siege, HitChance.High, packets);
                        return true;
                    }
                }
            }

            var lowEMinion =
                objAiMinions.Where(
                    m =>
                        Yasuo.Player.Distance(m.Position) <= YasuoSpells.E.Range &&
                        m.Health < Yasuo.Player.GetSweepingBladeDamage(m, stacks))
                    .OrderBy(m => m.Health)
                    .FirstOrDefault();
            // => Sweeping Blade (E)
            if (lowEMinion != null && YasuoSpells.E.IsReady() &&
                Yasuo.Menu.GetItemValue<bool>(YasuoMenu.FarmingLastHitELoc))
            {
                if (!Yasuo.Player.GetDashingEnd(lowEMinion).To3D().UnderTurret(true))
                {
                    YasuoSpells.E.Cast(lowEMinion);
                    return true;
                }
            }

            var lowQMinion =
                objAiMinions.Where(
                    m =>
                        Yasuo.Player.Distance(m.Position) <= Yasuo.Player.GetYasuoQState().Range &&
                        m.Health < Yasuo.Player.GetSteelTempestDamage(m) &&
                        m.Health > Yasuo.Player.GetSweepingBladeDamage(m, stacks))
                    .OrderBy(m => m.Health)
                    .FirstOrDefault();
            // => Steel Tempest (Q)
            if (lowQMinion != null && Yasuo.Player.GetYasuoQState().IsReady() &&
                Yasuo.Menu.GetItemValue<bool>(YasuoMenu.FarmingLastHitQLoc))
            {
                if (Yasuo.Player.IsDashing() && Yasuo.Player.Distance(lowQMinion.Position) < 375f)
                {
                    Yasuo.Player.GetYasuoQState().CastIfHitchanceEquals(lowQMinion, HitChance.High, packets);
                }
                else if (!Yasuo.Player.IsDashing())
                {
                    Yasuo.Player.GetYasuoQState().CastIfHitchanceEquals(lowQMinion, HitChance.High, packets);
                }
                return true;
            }

            return false;
        }

        private static void Flee()
        {
            var dashData = Yasuo.Player.GetDashData(
                Game.CursorPos, null, Yasuo.Menu.GetItemValue<bool>(YasuoMenu.FleeTowersLoc));
            var dashPoint = (Vector3) dashData[0];
            var dashAi = (Obj_AI_Base) dashData[1];

            if (dashAi.IsValidTarget())
            {
                if (dashPoint.Distance(Game.CursorPos) < Yasuo.Player.Distance(Game.CursorPos) - 100)
                {
                    YasuoSpells.E.Cast(dashAi, Yasuo.Menu.GetItemValue<bool>(YasuoMenu.MiscPacketsLoc));
                }
            }

            Yasuo.Player.MoveTo(Game.CursorPos, Yasuo.Player.BoundingRadius * 2f);
        }

        private static void Killsteal()
        {
            var target =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(e => e.IsValidTarget() && e.Distance(Yasuo.Player.Position) < 1200f)
                    .OrderBy(e => e.Health)
                    .FirstOrDefault();

            if (target == null)
            {
                return;
            }

            var packets = Yasuo.Menu.GetItemValue<bool>(YasuoMenu.MiscPacketsLoc);
            var firstOrDefault = Yasuo.Player.Buffs.FirstOrDefault(b => b.DisplayName.Equals("YasuoDashScalar"));
            var eStacks = 0;
            if (firstOrDefault != null)
            {
                eStacks = firstOrDefault.Count;
            }
            var stacks = 1 + 0.25 * eStacks;

            if (target.Health < Yasuo.Player.GetSweepingBladeDamage(target, stacks))
            {
                YasuoSpells.E.Cast(target, packets);
            }
            else if (target.Health < Yasuo.Player.GetSteelTempestDamage(target))
            {
                var prediction = Prediction.GetPrediction(
                    target, Yasuo.Player.GetYasuoQState().Delay, 0f, Yasuo.Player.GetYasuoQState().Speed);
                if (Yasuo.Player.IsDashing() && Yasuo.Player.Distance(target.Position) < 375f)
                {
                    Yasuo.Player.GetYasuoQState().Cast(prediction.CastPosition, packets);
                }
                else if (!Yasuo.Player.IsDashing())
                {
                    Yasuo.Player.GetYasuoQState().Cast(prediction.CastPosition, packets);
                }
            }
        }

        private static void Auto()
        {
            var packets = Yasuo.Menu.GetItemValue<bool>(YasuoMenu.MiscPacketsLoc);

            // => Q
            if (Yasuo.Player.GetYasuoQState().IsReady() && Yasuo.Menu.GetItemValue<bool>(YasuoMenu.AutoQLoc))
            {
                var enemy =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            e =>
                                e.IsValidTarget() &&
                                e.Distance(Yasuo.Player.Position) < Yasuo.Player.GetYasuoQState().Range)
                        .OrderBy(e => e.Distance(Yasuo.Player.Position))
                        .FirstOrDefault();

                if (enemy != null && !enemy.UnderTurret())
                {
                    var prediction = Prediction.GetPrediction(
                        enemy, Yasuo.Player.GetYasuoQState().Delay, 0f, Yasuo.Player.GetYasuoQState().Speed);
                    if (Yasuo.Player.IsDashing() && Yasuo.Player.Distance(enemy.Position) < 375f)
                    {
                        Yasuo.Player.GetYasuoQState().Cast(prediction.CastPosition, packets);
                    }
                    else if (!Yasuo.Player.IsDashing())
                    {
                        Yasuo.Player.GetYasuoQState().Cast(prediction.CastPosition, packets);
                    }
                }
            }
        }

        private static void Evader()
        {
            foreach (var skillshot in Yasuo.EvadeDetectedSkillshots)
            {
                var flag = false;
                if (YasuoSpells.E.IsReady())
                {
                    if (Yasuo.Menu.GetItemValue<bool>(YasuoMenu.EvadeUseLoc))
                    {
                        flag = Evade(skillshot);
                    }
                }

                if (flag)
                {
                    continue;
                }

                if (YasuoSpells.W.IsReady())
                {
                    if (Yasuo.Menu.GetItemValue<bool>(YasuoMenu.AutoWindWallUseLoc))
                    {
                        Windwall(skillshot);
                    }
                }
            }
        }

        private static bool Evade(Skillshot skillshot)
        {
            if (YasuoSpells.E.IsReady())
            {
                // => If anything, execute the windwall.
                var flag = true;

                foreach (var ss in Yasuo.MenuDashesList.Where(ss => ss.SpellName == skillshot.SpellData.SpellName))
                {
                    flag =
                        Yasuo.Menu.GetItemValue<bool>(
                            ((ss.IsWindwall) ? YasuoMenu.AutoWindWallLoc : YasuoMenu.EvadeLoc) + "." + ss.ChampionName +
                            "." + ss.Slot);
                    break;
                }

                if (flag)
                {
                    // => Will skillshot collide with Yasuo?
                    if (Yasuo.Player.Position.To2D().ProjectOn(skillshot.Start, skillshot.End).IsOnSegment)
                    {
                        var minions =
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(m => m.IsValidTarget() && Yasuo.Player.IsDashable(m));
                        var safeMinions =
                            minions.Where(
                                m =>
                                    !Yasuo.Player.GetDashingEnd(m).ProjectOn(skillshot.Start, skillshot.End).IsOnSegment);

                        var objAiMinions = safeMinions as Obj_AI_Minion[] ?? safeMinions.ToArray();
                        if (objAiMinions.Any())
                        {
                            var minion = objAiMinions.OrderBy(m => m.Distance(Yasuo.Player.Position)).FirstOrDefault();
                            if (minion != null)
                            {
                                YasuoSpells.E.Cast(
                                    objAiMinions.OrderBy(m => m.Distance(Yasuo.Player.Position)).FirstOrDefault(),
                                    Yasuo.Menu.GetItemValue<bool>(YasuoMenu.MiscPacketsLoc));

                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static void Windwall(Skillshot skillshot)
        {
            if (YasuoSpells.W.IsReady() && skillshot.SpellData.Type != SkillShotType.SkillshotCircle ||
                skillshot.SpellData.Type != SkillShotType.SkillshotRing)
            {
                var intersects = Yasuo.Player.Position.To2D().ProjectOn(skillshot.Start, skillshot.End).IsOnSegment;
                if (!intersects)
                {
                    return;
                }
                var isAboutToHitRange = Yasuo.Menu.GetItemValue<Slider>(YasuoMenu.AutoWindWallDelayLoc).Value;

                // => If anything, execute the windwall.
                var flag = true;

                foreach (var ss in Yasuo.MenuWallsList.Where(ss => ss.SpellName == skillshot.SpellData.SpellName))
                {
                    flag =
                        Yasuo.Menu.GetItemValue<bool>(
                            ((ss.IsWindwall) ? YasuoMenu.AutoWindWallLoc : YasuoMenu.EvadeLoc) + "." + ss.ChampionName +
                            "." + ss.Slot);
                    break;
                }

                if (flag)
                {
                    if (skillshot.Unit.IsValidTarget() && skillshot.IsAboutToHit(isAboutToHitRange, Yasuo.Player))
                    {
                        YasuoSpells.W.Cast(skillshot.Unit.Position);
                    }
                    else if (skillshot.IsAboutToHit(isAboutToHitRange, Yasuo.Player))
                    {
                        YasuoSpells.W.Cast(
                            (Yasuo.Player.ServerPosition + skillshot.MissilePosition.To3D()).Normalized());
                    }
                }
            }
        }
    }
}