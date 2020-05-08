﻿using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace ModLibrary.Components.Parties
{
    public class PartyManager<InfoType> /*: IManager<InfoType, PartyBase>*/ where InfoType : PartyInfo, new()
    {
        #region Singleton

        private PartyManager() { }

        static PartyManager()
        {
            PartyManager<InfoType>.Instance = new PartyManager<InfoType>();
        }

        public static PartyManager<InfoType> Instance { get; private set; }

        #endregion

        #region IManager

        public HashSet<InfoType> Infos { get; set; } = new HashSet<InfoType>();

        public void InitializeInfos()
        {
            if (this.Infos.Count() == Campaign.Current.Parties.Count())
            {
                return;
            }

            foreach (var gameObject in Campaign.Current.Parties)
            {
                this.GetInfo(gameObject);
            }
        }

        public InfoType GetInfo(PartyBase gameObject)
        {
            var info = this.Infos.SingleOrDefault(i => i.PartyId == gameObject.Id);
            if (info != null)
            {
                return info;
            }

            info = (InfoType)Activator.CreateInstance(typeof(InfoType), gameObject);
            this.Infos.Add(info);

            return info;
        }

        public InfoType GetInfo(string id)
        {
            var gameObject = this.GetGameObject(id);
            if (gameObject == null)
            {
                return null;
            }

            return this.GetInfo(gameObject);
        }

        public void RemoveInfo(string id)
        {
            this.Infos.RemoveWhere(i => i.PartyId == id);
        }

        public PartyBase GetGameObject(string id)
        {
            return Campaign.Current.Parties.SingleOrDefault(go => go.Id == id);
        }

        public PartyBase GetGameObject(InfoType info)
        {
            return this.GetGameObject(info.PartyId);
        }

        public void UpdateInfos(bool onlyRemoving = false)
        {
            if (this.Infos.Count == Campaign.Current.Parties.Count)
            {
                return;
            }

            this.Infos.RemoveWhere(i => !Campaign.Current.Parties.Any(go => go.Id == i.PartyId));

            if (onlyRemoving)
            {
                return;
            }

            foreach (var gameObject in Campaign.Current.Parties)
            {
                this.GetInfo(gameObject);
            }
        }

        #endregion

        public MobileParty CreateMobileParty(Hero leader, Vec2 spawnPosition, Settlement homeSettlement, bool addLeaderToRoster, bool addInitialFood = true)
        {
            MobileParty mobileParty = MBObjectManager.Instance.CreateObject<MobileParty>(leader.CharacterObject.Name.ToString() + "_" + leader.Id);
            mobileParty.Initialize();

            TroopRoster memberRoster = new TroopRoster
            {
                IsPrisonRoster = false
            };
            TroopRoster prisonerRoster = new TroopRoster
            {
                IsPrisonRoster = true
            };

            mobileParty.Party.Owner = leader;
            mobileParty.SetAsMainParty();

            if(addLeaderToRoster)
            {
                mobileParty.MemberRoster.AddToCounts(leader.CharacterObject, 1, false, 0, 0, true, -1);
            }

            mobileParty.InitializeMobileParty(new TextObject(leader.CharacterObject.GetName().ToString(), null), memberRoster, prisonerRoster, spawnPosition, 0.0f, 0.0f);

            if (addInitialFood)
            {
                ItemObject foodItem = Campaign.Current.Items.First(item => item.IsFood);
                mobileParty.ItemRoster.AddToCounts(foodItem, 150);
            }

            mobileParty.HomeSettlement = homeSettlement;
            mobileParty.Quartermaster = leader;

            this.GetInfo(mobileParty.StringId);
            return mobileParty;
        }

        public TroopRoster GenerateBasicTroopRoster(Hero leader, int amount, bool withTier1 = true, bool withTier2 = true, bool withTier3 = true, bool withTier4 = true)
        {
            TroopRoster basicUnits = new TroopRoster();

            basicUnits.AddToCounts(leader.Culture.BasicTroop, amount);

            foreach (CharacterObject tier1 in leader.Culture.BasicTroop.UpgradeTargets)
            {
                if (withTier1)
                {
                    basicUnits.AddToCounts(tier1, amount / 2);
                }

                foreach (CharacterObject tier2 in tier1.UpgradeTargets)
                {
                    if (withTier2)
                    {
                        basicUnits.AddToCounts(tier2, amount / 2);
                    }

                    foreach (CharacterObject tier3 in tier2.UpgradeTargets)
                    {
                        if (withTier3)
                        {
                            basicUnits.AddToCounts(tier3, amount / 4);
                        }

                        foreach (CharacterObject tier4 in tier3.UpgradeTargets)
                        {
                            if (withTier4)
                            {
                                basicUnits.AddToCounts(tier4, amount / 8);
                            }
                        }
                    }
                }
            }

            return basicUnits;
        }

        public TroopRoster GenerateEliteTroopRoster(Hero leader, int amount, bool withTier1 = true, bool withTier2 = true, bool withTier3 = true, bool withTier4 = true)
        {
            TroopRoster eliteUnits = new TroopRoster();

            eliteUnits.AddToCounts(leader.Culture.EliteBasicTroop, amount);

            foreach (CharacterObject tier1 in leader.Culture.EliteBasicTroop.UpgradeTargets)
            {
                if (withTier1)
                {
                    eliteUnits.AddToCounts(tier1, amount / 2);
                }

                foreach (CharacterObject tier2 in tier1.UpgradeTargets)
                {
                    if (withTier2)
                    {
                        eliteUnits.AddToCounts(tier2, amount / 2);
                    }

                    foreach (CharacterObject tier3 in tier2.UpgradeTargets)
                    {
                        if (withTier3)
                        {
                            eliteUnits.AddToCounts(tier3, amount / 4);
                        }

                        foreach (CharacterObject tier4 in tier3.UpgradeTargets)
                        {
                            if (withTier4)
                            {
                                eliteUnits.AddToCounts(tier4, amount / 8);
                            }
                        }
                    }
                }
            }

            return eliteUnits;
        }
    }
}