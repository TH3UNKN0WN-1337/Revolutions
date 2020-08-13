﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Revolutions.Library.Components.Settlements;
using Revolutions.Library.Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Revolutions.Library.Components.Banners
{
    public class BaseBannerManager
    {
        #region Singleton

        static BaseBannerManager()
        {
            Instance = new BaseBannerManager();
        }

        public static BaseBannerManager Instance { get; private set; }

        #endregion

        public HashSet<BaseBannerInfo> Infos { get; set; } = new HashSet<BaseBannerInfo>();


        public void InitializeInfos()
        {
            var directoryPath = BasePath.Name + "Modules/Revolutions/ModuleData/Banners";

            foreach (var file in Directory.GetFiles(directoryPath, "*.xml"))
            {
                var loadedInfos = FileHelper.Load<List<BaseBannerInfo>>(directoryPath, Path.GetFileNameWithoutExtension(file));
                foreach (var info in loadedInfos)
                {
                    Infos.Add(info);
                }

                foreach (var subDirectoryPath in Directory.GetDirectories(directoryPath))
                {
                    foreach (var subDirectoryFile in Directory.GetFiles(subDirectoryPath, "*.xml"))
                    {
                        var loadedSubDirectoryInfos = FileHelper.Load<List<BaseBannerInfo>>(directoryPath, Path.GetFileNameWithoutExtension(subDirectoryFile));
                        foreach (var info in loadedSubDirectoryInfos)
                        {
                            Infos.Add(info);
                        }
                    }
                }
            }
        }

        public void CleanupDuplicatedInfos()
        {
            Infos.Reverse();
            Infos = Infos.GroupBy(i => i.Id)
                .Select(i => i.First())
                .ToHashSet();
            Infos.Reverse();
        }

        public BaseBannerInfo GetBaseBanner(BaseSettlementInfo settlementInfo)
        {
            var availableBannerInfos = new List<BaseBannerInfo>();
            BaseBannerInfo bannerInfo = null;

            foreach (var info in Infos.Where(i => !i.Used))
            {
                if (info.Settlement == settlementInfo.Settlement.Name.ToString() && info.Culture == settlementInfo.Settlement.Culture.StringId)
                {
                    availableBannerInfos.Add(info);
                    break;
                }

                if (info.Culture == settlementInfo.Settlement.Culture.StringId)
                {
                    availableBannerInfos.Add(info);
                    break;
                }

                if (info.Faction == settlementInfo.CurrentFaction.StringId)
                {
                    availableBannerInfos.Add(info);
                    break;
                }
            }

            if (availableBannerInfos.Count > 0)
            {
                bannerInfo = availableBannerInfos.GetRandomElement();
            }

            if (bannerInfo != null)
            {
                bannerInfo.Used = true;
            }

            return bannerInfo;
        }

        public BaseBannerInfo GetBaseBanner(Settlement settlement)
        {
            var settlementInfo = BaseManagers.Settlement.Get(settlement);
            return settlementInfo == null ? null : GetBaseBanner(settlementInfo);
        }
    }
}