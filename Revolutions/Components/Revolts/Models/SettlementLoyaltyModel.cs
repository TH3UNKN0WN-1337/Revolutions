﻿using Revolutions.Components.Revolts.Localization;
using Revolutions.Components.Settlements;
using Revolutions.Settings;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;

namespace Revolutions.Components.Revolts.Models
{
    public class SettlementLoyaltyModel : DefaultSettlementLoyaltyModel
    {
        public SettlementLoyaltyModel()
        {
        }

        public override float CalculateLoyaltyChange(Town town, StatExplainer statExplainer = null)
        {
            if (!town.Settlement.IsFortification)
            {
                return base.CalculateLoyaltyChange(town, statExplainer);
            }

            var explainedNumber = new ExplainedNumber(0, statExplainer, null);
            var settlementInfo = Managers.Settlement.Get(town.Settlement);

            if (settlementInfo.CurrentFaction.Leader == Hero.MainHero)
            {
                explainedNumber.Add(RevolutionsSettings.Instance.RevoltsGeneralPlayerBaseLoyalty, new TextObject(GameTexts.RevoltsLoyaltyCalculationBannerlordSettlement));
            }

            this.NotablesChange(settlementInfo, ref explainedNumber);
            this.ImperialChange(settlementInfo, ref explainedNumber);
            this.OverextensionChange(settlementInfo, ref explainedNumber);
            this.LuckyNationChange(settlementInfo, ref explainedNumber);

            return explainedNumber.ResultNumber + base.CalculateLoyaltyChange(town, statExplainer);
        }

        private void NotablesChange(SettlementInfo settlementInfo, ref ExplainedNumber explainedNumber)
        {
            var notablesLoyaltyChange = 0f;

            var settlement = settlementInfo.Settlement;
            foreach (var notable in settlement.Notables.Where(notable => notable.SupporterOf != null))
            {
                if (settlement.OwnerClan.MapFaction.StringId == notable.SupporterOf.MapFaction.StringId)
                {
                    notablesLoyaltyChange += 1;
                }
                else
                {
                    notablesLoyaltyChange -= 1;
                }
            }

            var textObject = new TextObject(GameTexts.RevoltsLoyaltyCalculationNotables);
            textObject.SetTextVariable("SETTLEMENT", settlement.Name);
            explainedNumber.Add(notablesLoyaltyChange, textObject);

            foreach (var village in settlement.BoundVillages.Select(village => village.Settlement))
            {
                notablesLoyaltyChange = 0;

                foreach (var noteable in village.Notables.Where(notable => notable.SupporterOf != null))
                {
                    if (village.OwnerClan.MapFaction.StringId == noteable.SupporterOf.MapFaction.StringId)
                    {
                        notablesLoyaltyChange += 1;
                    }
                    else
                    {
                        notablesLoyaltyChange -= 1;
                    }
                }

                textObject = new TextObject(GameTexts.RevoltsLoyaltyCalculationNotables);
                textObject.SetTextVariable("SETTLEMENT", settlement.Name);
                explainedNumber.Add(notablesLoyaltyChange, textObject);
            }
        }

        private void OverextensionChange(SettlementInfo settlementInfo, ref ExplainedNumber explainedNumber)
        {
            if (!RevolutionsSettings.Instance.RevoltsOverextensionMechanics
                || !RevolutionsSettings.Instance.RevoltsOverextensionAffectsPlayer && settlementInfo.CurrentFaction.Leader == Hero.MainHero
                || settlementInfo.CurrentFaction.StringId == settlementInfo.LoyalFaction.StringId)
            {
                return;
            }

            var loyalSettlements = settlementInfo.CurrentFaction.Settlements.Where(s => s.IsFortification && settlementInfo.CurrentFactionId == Managers.Settlement.Get(s).LoyalFaction.StringId).Count();
            var illoyalSettlements = settlementInfo.CurrentFaction.Settlements.Where(s => s.IsFortification && settlementInfo.CurrentFactionId != Managers.Settlement.Get(s).LoyalFaction.StringId).Count();
            var overextension = loyalSettlements - illoyalSettlements;
            var calculatedOverextension = overextension < 0 ? overextension * RevolutionsSettings.Instance.RevoltsOverextensionMultiplier : 1;
            explainedNumber.Add(calculatedOverextension, new TextObject(GameTexts.RevoltsLoyaltyCalculationOverextension));
        }

        private void ImperialChange(SettlementInfo settlementInfo, ref ExplainedNumber explainedNumber)
        {
            if (!RevolutionsSettings.Instance.RevoltsImperialLoyaltyMechanic)
            {
                return;
            }

            if (settlementInfo.IsOfImperialCulture)
            {
                if (settlementInfo.IsCurrentFactionOfImperialCulture)
                {
                    explainedNumber.Add(3, new TextObject(GameTexts.RevoltsLoyaltyCalculationImperialLoyalty));
                }
                else
                {
                    explainedNumber.Add(-1, new TextObject(GameTexts.RevoltsLoyaltyCalculationForeignRule));
                }
            }
            else
            {
                if (settlementInfo.IsCurrentFactionOfImperialCulture)
                {
                    explainedNumber.Add(-1, new TextObject(GameTexts.RevoltsLoyaltyCalculationImperialAversion));
                }

                if (settlementInfo.LoyalFaction.StringId != settlementInfo.CurrentFactionId)
                {
                    explainedNumber.Add(1, new TextObject(GameTexts.RevoltsLoyaltyCalculationForeignRule));
                }
            }
        }

        private void LuckyNationChange(SettlementInfo settlementInfo, ref ExplainedNumber explainedNumber)
        {
            if (settlementInfo.CurrentFaction.IsKingdomFaction)
            {
                if (Managers.Kingdom.Get(settlementInfo.Settlement.OwnerClan.Kingdom)?.LuckyNation == true)
                {
                    explainedNumber.Add(5, new TextObject(GameTexts.RevoltsLoyaltyCalculationLuckyNation));
                    return;
                }
            }
        }
    }
}