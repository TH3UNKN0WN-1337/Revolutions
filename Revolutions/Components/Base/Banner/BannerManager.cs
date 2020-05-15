﻿using System.Collections.Generic;
using System.Linq;
using KNTLibrary.Components.Banners;
using Revolutions.Components.Base.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Revolutions.Components.Base.Banner
{
    internal class BannerManager : BaseBannerManager
    {
        #region Singleton

        static BannerManager()
        {
            BannerManager.Instance = new BannerManager();
        }

        public static BannerManager Instance { get; private set; }

        #endregion

        public BaseBannerInfo GetRevolutionsBannerBySettlementInfo(SettlementInfo settlementInfo)
        {
            List<BaseBannerInfo> availableBannerInfos = new List<BaseBannerInfo>();
            BaseBannerInfo bannerInfo = null;

            foreach (var info in this.Infos)
            {
                if (info.Used)
                {
                    continue;
                }

                if (info.Settlement == settlementInfo.Settlement.Name.ToString() && info.Culture == settlementInfo.Settlement.Culture.StringId)
                {
                    availableBannerInfos.Add(info);
                    break;
                }
                
                if (info.Faction == settlementInfo.LoyalFaction.StringId)
                {
                    availableBannerInfos.Add(info);
                    break;
                }
                
                if (info.Culture == settlementInfo.Settlement.Culture.StringId)
                {
                    availableBannerInfos.Add(info);
                    break;
                }
            }

            if (availableBannerInfos.Count() > 0)
            {
                return availableBannerInfos.GetRandomElement();
            }
            
            return bannerInfo;
        }

        public BaseBannerInfo GetRevolutionsBannerBySettlement(Settlement settlement)
        {
            var settlementInfo = Managers.Settlement.GetInfo(settlement);
            if (settlementInfo == null)
            {
                return null;
            }

            return this.GetRevolutionsBannerBySettlementInfo(settlementInfo);
        }
    }
}