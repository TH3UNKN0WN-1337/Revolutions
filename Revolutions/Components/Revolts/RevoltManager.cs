﻿using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using Helpers;
using KNTLibrary.Helpers;
using Revolutions.Components.Base.Factions;
using Revolutions.Settings;

namespace Revolutions.Components.Revolts
{
    internal class RevoltManager
    {
        #region Singleton

        private RevoltManager() { }

        static RevoltManager()
        {
            Instance = new RevoltManager();
        }

        internal static RevoltManager Instance { get; private set; }

        #endregion

        internal HashSet<Revolt> Revolts = new HashSet<Revolt>();

        internal Revolt GetRevoltByPartyId(string id)
        {
            return this.Revolts.FirstOrDefault(r => r.PartyId == id);
        }

        internal Revolt GetRevoltByParty(PartyBase party)
        {
            return this.GetRevoltByPartyId(party.Id);
        }

        internal Revolt GetRevoltBySettlementId(string id)
        {
            return this.Revolts.FirstOrDefault(r => r.SettlementId == id);
        }

        internal Revolt GetRevoltBySettlement(Settlement settlement)
        {
            return this.GetRevoltBySettlementId(settlement.StringId);
        }

        internal List<Settlement> GetSettlements()
        {
            return this.Revolts.Select(r => r.Settlement).ToList();
        }

        internal List<PartyBase> GetParties()
        {
            return this.Revolts.Select(r => r.Party).ToList();
        }

        internal void IncreaseDailyLoyaltyForSettlement()
        {
            foreach (var info in Managers.Settlement.Infos)
            {
                foreach (var party in info.Settlement.Parties)
                {
                    if (party.IsLordParty && party.Party.Owner.Clan == info.Settlement.OwnerClan)
                    {
                        info.Settlement.Town.Loyalty += RevolutionsSettings.Instance.GeneralPlayerInTownLoyaltyIncrease;

                        if (info.Settlement.OwnerClan.StringId == Hero.MainHero.Clan.StringId)
                        {
                            var textObject = new TextObject("{=PqkwszGz}Seeing you spend time at {SETTLEMENT}, your subjects feel more loyal to you.");
                            textObject.SetTextVariable("SETTLEMENT", info.Settlement.Name.ToString());
                            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
                        }

                        break;
                    }
                }
            }
        }

        internal void CheckRevoltProgress()
        {
            foreach (var settlementInfo in Managers.Settlement.Infos)
            {
                var settlement = settlementInfo.Settlement;

                if (!settlement.IsTown)
                {
                    continue;
                }

                if (settlementInfo.LoyalFactionId == settlementInfo.CurrentFactionId)
                {
                    continue;
                }

                if (settlementInfo.CurrentFactionInfo?.CanRevolt == false || settlementInfo.HasRebellionEvent)
                {
                    settlementInfo.RevoltProgress = 0;
                    continue;
                }

                settlementInfo.RevoltProgress += RevolutionsSettings.Instance.GeneralMinimumObedienceLoyalty - settlement.Town.Loyalty;

                if (settlementInfo.RevoltProgress >= 100 && !settlement.IsUnderSiege)
                {
                    this.StartRebellionEvent(settlement);
                    continue;
                }

                if (settlementInfo.RevoltProgress < 0)
                {
                    settlementInfo.RevoltProgress = 0;
                }
            }
        }

        internal void EndFailedRevolt(Revolt revolt)
        {
            var information = new TextObject("{=dkpS074R}The revolt in {SETTLEMENT} has ended.");
            information.SetTextVariable("SETTLEMENT", revolt.Settlement.Name.ToString());
            InformationManager.DisplayMessage(new InformationMessage(information.ToString(), ColorHelper.Yellow));

            revolt.SettlementInfo.CurrentFactionInfo.CityRevoltionFailed(revolt.Settlement);

            if (revolt.IsMinorFaction)
            {
                var kingdom = revolt.Party.Owner.Clan.Kingdom;
                var mapFaction = kingdom.MapFaction;
                foreach (var faction in Campaign.Current.Factions.Where(go => go.IsAtWarWith(mapFaction)))
                {
                    if (kingdom.MapFaction.IsAtWarWith(faction))
                    {
                        FactionHelper.FinishAllRelatedHostileActionsOfFactionToFaction(faction, mapFaction);
                        FactionHelper.FinishAllRelatedHostileActionsOfFactionToFaction(mapFaction, faction);
                    }
                }

                KillCharacterAction.ApplyByExecution(revolt.Party.Owner, revolt.Settlement.OwnerClan?.Kingdom.Leader ?? revolt.Settlement.OwnerClan.Leader);
                Managers.Kingdom.RemoveKingdom(kingdom);
            }

            if (revolt.Party?.MobileParty != null)
            {
                DestroyPartyAction.Apply(revolt.SettlementInfo.Garrision, revolt.Party.MobileParty);
            }

            this.Revolts.Remove(revolt);
        }

        internal void EndSucceededRevolut(Revolt revolt)
        {
            var information = new TextObject("{=dkpS074R}The revolt in {SETTLEMENT} has ended.");
            information.SetTextVariable("SETTLEMENT", revolt.Settlement.Name.ToString());
            InformationManager.DisplayMessage(new InformationMessage(information.ToString(), ColorHelper.Yellow));

            revolt.SettlementInfo.CurrentFactionInfo.CityRevoltionSucceeded(revolt.Settlement);

            if (RevolutionsSettings.Instance.RevoltsImperialLoyaltyMechanic && revolt.SettlementInfo.IsCurrentFactionOfImperialCulture && !revolt.SettlementInfo.IsLoyalFactionOfImperialCulture)
            {
                revolt.Settlement.OwnerClan.AddRenown(-RevolutionsSettings.Instance.RevoltsImperialRenownLoss);
            }

            if (RevolutionsSettings.Instance.RevoltsMinorFactionsMechanic && revolt.IsMinorFaction)
            {
                ChangeOwnerOfSettlementAction.ApplyBySiege(revolt.Party.LeaderHero, revolt.Party.LeaderHero, revolt.Settlement);
                revolt.Party.LeaderHero.Clan.AddRenown(RevolutionsSettings.Instance.RevoltsMinorFactionsRenownGainOnWin);

                var companion = Managers.Character.CreateRandomLeader(revolt.Party.LeaderHero.Clan, revolt.SettlementInfo);
                Managers.Character.GetInfo(companion.CharacterObject);

                var companionClan = Managers.Clan.CreateClan(companion, companion.Name, companion.Name);
                Managers.Clan.GetInfo(companionClan).IsRevoltClan = true;

                var mobileParty = Managers.Party.CreateMobileParty(companion, revolt.Settlement.GatePosition, revolt.Settlement, true, true);
                var amountOfEliteTroops = (RevolutionsSettings.Instance.RevoltsGeneralBaseArmy + (int)(revolt.Settlement.Prosperity * RevolutionsSettings.Instance.RevoltsGeneralArmyProsperityMulitplier)) / 2;
                mobileParty.MemberRoster.Add(Managers.Party.GenerateEliteTroopRoster(mobileParty.LeaderHero, amountOfEliteTroops));

                ChangeKingdomAction.ApplyByJoinToKingdom(companion.Clan, revolt.Party.LeaderHero.Clan.Kingdom, true);

                revolt.Party.MobileParty.Ai.SetDoNotMakeNewDecisions(false);
                mobileParty.Ai.SetDoNotMakeNewDecisions(false);

                SetPartyAiAction.GetActionForPatrollingAroundSettlement(mobileParty, revolt.Settlement);

                this.Revolts.Remove(revolt);
            }
        }

        internal void StartRebellionEvent(Settlement settlement)
        {
            var information = new TextObject("{=dRoS0maD}{SETTLEMENT} is revolting!");
            information.SetTextVariable("SETTLEMENT", settlement.Name.ToString());
            InformationManager.DisplayMessage(new InformationMessage(information.ToString(), ColorHelper.Yellow));

            var settlementInfo = Managers.Settlement.GetInfo(settlement);
            var atWarWithLoyalFaction = settlementInfo.CurrentFaction.IsAtWarWith(settlementInfo.LoyalFaction);

            Hero leader;

            if (atWarWithLoyalFaction)
            {
                leader = Managers.Faction.GetLordWithLeastFiefs(settlementInfo.LoyalFaction).HeroObject;
            }
            else
            {
                leader = Managers.Character.CreateRandomLeader(settlement.OwnerClan, settlementInfo);
                Managers.Character.GetInfo(leader.CharacterObject).IsRevoltKingdomLeader = true;

                Managers.Clan.CreateClan(leader, leader.Name, leader.Name);
                Managers.Clan.GetInfo(leader.Clan).IsRevoltClan = true;

                Kingdom kingdom;
                var bannerInfo = Managers.Banner.GetRevolutionsBannerBySettlementInfo(settlementInfo);
                if (bannerInfo != null)
                {
                    var banner = new Banner(bannerInfo.BannerId);
                    bannerInfo.Used = true;
                    kingdom = Managers.Kingdom.CreateKingdom(leader, new TextObject($"Kingdom of {settlement.Name}"), new TextObject($"Kingdom of {settlement.Name}"), banner);
                }
                else
                {
                    kingdom = Managers.Kingdom.CreateKingdom(leader, new TextObject($"Kingdom of {settlement.Name}"), new TextObject($"Kingdom of {settlement.Name}"));
                }

                Managers.Kingdom.GetInfo(kingdom).IsRevoltKingdom = true;
            }

            var mobileParty = Managers.Party.CreateMobileParty(leader, settlement.GatePosition, settlement, !atWarWithLoyalFaction, true);

            var amountOfBasicTroops = RevolutionsSettings.Instance.RevoltsGeneralBaseArmy + (int)(settlement.Prosperity * RevolutionsSettings.Instance.RevoltsGeneralArmyProsperityMulitplier);
            mobileParty.MemberRoster.Add(Managers.Party.GenerateBasicTroopRoster(leader, amountOfBasicTroops, withTier4: false));

            if (settlement.MilitaParty != null && settlement.MilitaParty.CurrentSettlement == settlement && settlement.MilitaParty.MapEvent == null)
            {
                foreach (var troopRosterElement in settlement.MilitaParty.MemberRoster)
                {
                    mobileParty.AddElementToMemberRoster(troopRosterElement.Character, troopRosterElement.Number, false);
                }

                settlement.MilitaParty.RemoveParty();
            }

            if (!atWarWithLoyalFaction)
            {
                mobileParty.ChangePartyLeader(mobileParty.Party.Owner.CharacterObject, false);
            }

            this.Revolts.Add(new Revolt(mobileParty.Party.Id, settlement, !atWarWithLoyalFaction));

            settlementInfo.HasRebellionEvent = true;

            FactionManager.DeclareWar(leader.MapFaction, settlement.MapFaction);
            Campaign.Current.FactionManager.RegisterCampaignWar(leader.MapFaction, settlement.MapFaction);

            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(leader, settlement.OwnerClan.Leader, -20, false);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(leader, settlement.OwnerClan.Kingdom.Leader, -20, false);

            mobileParty.Ai.SetDoNotMakeNewDecisions(true);
            SetPartyAiAction.GetActionForBesiegingSettlement(mobileParty, settlement);
        }
    }
}