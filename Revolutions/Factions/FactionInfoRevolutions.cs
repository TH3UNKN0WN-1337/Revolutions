﻿using System;
using ModLibrary.Factions;
using TaleWorlds.CampaignSystem;

namespace Revolutions.Factions
{
    [Serializable]
    public class FactionInfoRevolutions : FactionInfo
    {
        public FactionInfoRevolutions() : base()
        {

        }

        public FactionInfoRevolutions(IFaction faction) : base(faction)
        {

        }

        public string RevoltedSettlementId { get; set; }

        public bool CanRevolt { get; set; } = false;

        public int DaysSinceLastRevolution { get; set; } = 0;

        public bool SuccessfullyRevolted { get; set; } = false;
    }
}