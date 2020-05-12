﻿using System;
using TaleWorlds.CampaignSystem;
using KNTLibrary.Components.Clans;

namespace Revolutions.Components.Base.Clans
{
    [Serializable]
    public class ClanInfo : BaseClanInfo
    {
        public ClanInfo() : base()
        {

        }

        public ClanInfo(Clan clan) : base(clan)
        {

        }

        #region Reference Properties



        #endregion

        #region Virtual Objects

        #region Reference Properties



        #endregion

        #region Reference Properties Inherited



        #endregion

        #region Normal Properties



        #endregion

        #endregion

        #region Normal Properties

        public bool IsRevoltClan { get; set; } = false;

        #endregion
    }
}