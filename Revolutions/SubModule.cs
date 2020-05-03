﻿using System;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Library;
using ModLibrary;
using Revolutions.CampaignBehaviors;
using Revolutions.Models;

namespace Revolutions
{
    public class SubModule : MBSubModuleBase
    {
        private DataStorage _dataStorage;

        internal static string ModuleDataPath => Path.Combine(BasePath.Name, "Modules", "Revolutions", "ModuleData");

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            try
            {
                Module.CurrentModule.GlobalTextManager.LoadGameTexts(Path.Combine(SubModule.ModuleDataPath, "global_strings.xml"));
                InformationManager.DisplayMessage(new InformationMessage("Revolutions: Loaded Mod.", ColorManager.Green));
            }
            catch (Exception exception)
            {
                var exceptionMessage = "Revolutions: Failed to load! ";
                InformationManager.DisplayMessage(new InformationMessage(exceptionMessage + exception?.ToString(), ColorManager.Red));
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);

            if (!(game.GameType is Campaign))
            {
                return;
            }

            this.InitializeMod(gameStarter as CampaignGameStarter);
        }

        private void InitializeMod(CampaignGameStarter campaignGameStarter)
        {
            try
            {
                this._dataStorage = new DataStorage();
                this.AddBehaviours(campaignGameStarter);
            }
            catch (Exception exception)
            {
                var exceptionMessage = "Revolutions: Failed to initialize! ";
                InformationManager.DisplayMessage(new InformationMessage(exceptionMessage + exception?.ToString(), ColorManager.Red));
            }
        }

        private void AddBehaviours(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddBehavior(new RevolutionBehavior(ref this._dataStorage));
            campaignGameStarter.AddBehavior(new RevolutionDailyBehavior(ref this._dataStorage));
            campaignGameStarter.AddBehavior(new GuiHandlersBehavior());
            campaignGameStarter.AddBehavior(new CleanupBehavior());

            campaignGameStarter.AddModel(new LoyaltyModel(ref this._dataStorage));
        }
    }
}