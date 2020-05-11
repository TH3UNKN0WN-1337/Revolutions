﻿using System;
using TaleWorlds.CampaignSystem;

namespace KNTLibrary.Components.Parties
{
    [Serializable]
    public class PartyInfo : IGameComponent<PartyInfo>
    {
        #region IGameComponent<InfoType>

        public bool Equals(PartyInfo other)
        {
            return this.PartyId == other.PartyId;
        }

        public override bool Equals(object other)
        {
            if (other is PartyInfo partyInfo)
            {
                return this.PartyId == partyInfo.PartyId;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.PartyId.GetHashCode();
        }

        #endregion

        public PartyInfo()
        {

        }

        public PartyInfo(PartyBase party)
        {
            this.PartyId = party.Id;
        }

        #region Reference Properties

        public string PartyId { get; set; }

        #endregion

        #region Virtual Objects

        #region Reference Properties

        public PartyBase Party => LibraryManagers.PartyManager.GetGameObject(this.PartyId);

        #endregion



        #endregion

        #region Normal Properties

        public bool IsCustomParty { get; set; } = false;

        #endregion
    }
}