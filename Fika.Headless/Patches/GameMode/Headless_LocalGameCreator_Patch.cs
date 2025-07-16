using Comfort.Common;
using EFT;
using EFT.InputSystem;
using EFT.UI;
using EFT.UI.Matchmaker;
using Fika.Core.Main.GameMode;
using Fika.Core.Main.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Patching;
using Fika.Headless.Classes.GameMode;
using HarmonyLib;
using JsonType;
using SPT.SinglePlayer.Utils.InRaid;
using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Fika.Headless.Patches.GameMode
{
    internal class Headless_LocalGameCreator_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TarkovApplication).GetMethod(nameof(TarkovApplication.method_49));
        }

        [PatchPrefix]
        public static bool Prefix(ref Task __result, TarkovApplication __instance, TimeAndWeatherSettings timeAndWeather, MatchmakerTimeHasCome.TimeHasComeScreenClass timeHasComeScreenController,
            RaidSettings ____raidSettings, InputTree ____inputTree, GameDateTime ____localGameDateTime, float ____fixedDeltaTime, string ____backendUrl, MetricsEventsClass metricsEvents,
            MetricsConfigClass metricsConfig, GameWorld gameWorld, MainMenuControllerClass ____menuOperation, CompositeDisposableClass ___compositeDisposableClass, BundleLockClass ___BundleLock)
        {
#if DEBUG
            Logger.LogInfo("TarkovApplication_LocalGameCreator_Patch:Prefix");
#endif
            __result = CreateFikaGame(__instance, timeAndWeather, timeHasComeScreenController, ____raidSettings,
                ____inputTree, ____localGameDateTime, ____fixedDeltaTime, ____backendUrl,
                metricsEvents, metricsConfig, gameWorld, ____menuOperation, ___compositeDisposableClass, ___BundleLock);
            return false;
        }

        public static async Task CreateFikaGame(TarkovApplication instance, TimeAndWeatherSettings timeAndWeather, MatchmakerTimeHasCome.TimeHasComeScreenClass timeHasComeScreenController,
            RaidSettings raidSettings, InputTree inputTree, GameDateTime localGameDateTime, float fixedDeltaTime, string backendUrl, MetricsEventsClass metricsEvents, MetricsConfigClass metricsConfig,
            GameWorld gameWorld, MainMenuControllerClass ___mainMenuController, CompositeDisposableClass compositeDisposableClass, BundleLockClass bundleLock)
        {
            bool isTransit = FikaBackendUtils.IsTransit;

            if (!isTransit)
            {
                FikaBackendUtils.CachedRaidSettings = raidSettings;
            }
            else if (isTransit && FikaBackendUtils.CachedRaidSettings != null)
            {
                Logger.LogInfo("Applying cached raid settings from previous raid");
                RaidSettings cachedSettings = FikaBackendUtils.CachedRaidSettings;
                raidSettings.WavesSettings = cachedSettings.WavesSettings;
                raidSettings.BotSettings = cachedSettings.BotSettings;
                raidSettings.MetabolismDisabled = cachedSettings.MetabolismDisabled;
                raidSettings.PlayersSpawnPlace = cachedSettings.PlayersSpawnPlace;
            }

            metricsEvents.SetGamePrepared();

            if (Singleton<NotificationManagerClass>.Instantiated)
            {
                Singleton<NotificationManagerClass>.Instance.Deactivate();
            }

            ISession session = instance.Session;
            if (session == null)
            {
                throw new NullReferenceException("Backend session was null when initializing game!");
            }

            Profile profile = session.GetProfileBySide(raidSettings.Side);

            profile.Inventory.Stash = null;
            profile.Inventory.QuestStashItems = null;
            profile.Inventory.DiscardLimits = Singleton<ItemFactoryClass>.Instance.GetDiscardLimits();

#if DEBUG
            Logger.LogInfo("TarkovApplication_LocalGameCreator_Patch:Postfix: Attempt to set Raid Settings");
#endif

            await session.SendRaidSettings(raidSettings);
            LocalRaidSettings localRaidSettings = new()
            {
                location = raidSettings.LocationId,
                timeVariant = raidSettings.SelectedDateTime,
                mode = ELocalMode.PVE_OFFLINE,
                playerSide = raidSettings.Side,
                transitionType = FikaBackendUtils.TransitData.visitedLocations.Length > 0 ? ELocationTransition.Common : ELocationTransition.None
            };
            Traverse applicationTraverse = Traverse.Create(instance);
            applicationTraverse.Field<LocalRaidSettings>("localRaidSettings_0").Value = localRaidSettings;

            LocalSettings localSettings = await instance.Session.LocalRaidStarted(localRaidSettings);
            LocalRaidSettings raidSettingsToUpdate = applicationTraverse.Field<LocalRaidSettings>("localRaidSettings_0").Value;
            int escapeTimeLimit = raidSettings.IsScav ? RaidChangesUtil.NewEscapeTimeMinutes : raidSettings.SelectedLocation.EscapeTimeLimit;
            raidSettings.SelectedLocation = localSettings.locationLoot;
            raidSettings.SelectedLocation.EscapeTimeLimit = escapeTimeLimit;
            raidSettingsToUpdate.serverId = localSettings.serverId;
            raidSettingsToUpdate.selectedLocation = localSettings.locationLoot;
            raidSettingsToUpdate.selectedLocation.EscapeTimeLimit = escapeTimeLimit;
            raidSettingsToUpdate.transition = FikaBackendUtils.TransitData;

            ProfileInsuranceClass profileInsurance = localSettings.profileInsurance;
            if ((profileInsurance?.insuredItems) != null)
            {
                profile.InsuredItems = localSettings.profileInsurance.insuredItems;
            }

            instance.MatchmakerPlayerControllerClass.UpdateMatchingStatus("Creating coop game...");

            StartHandler startHandler = new(instance, session.Profile, session.ProfileOfPet, raidSettings.SelectedLocation);

            TimeSpan raidLimits = GetRaidMinutes(raidSettings.SelectedLocation.EscapeTimeLimit);

            HeadlessGame headlessGame = HeadlessGame.Create(gameWorld, localGameDateTime, raidSettings.SelectedLocation, timeAndWeather,
                raidSettings.WavesSettings, raidSettings.SelectedDateTime, startHandler.HandleStop, fixedDeltaTime, instance.PlayerUpdateQueue, instance.Session,
                raidLimits, localRaidSettings, raidSettings);

            startHandler.HeadlessGame = headlessGame;

            Singleton<AbstractGame>.Create(headlessGame);
            compositeDisposableClass.AddDisposable(headlessGame);
            compositeDisposableClass.AddDisposable(startHandler.ReleaseSingleton);
            metricsEvents.SetGameCreated();
            FikaEventDispatcher.DispatchEvent(new AbstractGameCreatedEvent(headlessGame));

            headlessGame.SetMatchmakerStatus("Coop game created");

            try
            {
                await headlessGame.Init(raidSettings.BotSettings, backendUrl);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw;
            }
            GameObject.DestroyImmediate(MonoBehaviourSingleton<MenuUI>.Instance.gameObject);
            ___mainMenuController?.Unsubscribe();
            bundleLock.MaxConcurrentOperations = 1;
            gameWorld.OnGameStarted();

            FikaEventDispatcher.DispatchEvent(new GameWorldStartedEvent(gameWorld));
        }

        private static TimeSpan GetRaidMinutes(int defaultMinutes)
        {
            return TimeSpan.FromSeconds((double)(60 * defaultMinutes));
        }

        private class StartHandler(TarkovApplication tarkovApplication, Profile pmcProfile, Profile scavProfile,
            LocationSettingsClass.Location location)
        {
            private readonly TarkovApplication _tarkovApplication = tarkovApplication;
            private readonly Profile _pmcProfile = pmcProfile;
            private readonly Profile _scavProfile = scavProfile;
            private readonly LocationSettingsClass.Location _location = location;
            public HeadlessGame HeadlessGame;

            public void HandleStop(Result<ExitStatus, TimeSpan, MetricsClass> result)
            {
                _tarkovApplication.method_52(_pmcProfile.Id, _scavProfile, _location, result);
            }

            public void ReleaseSingleton()
            {
                Singleton<AbstractGame>.Release(HeadlessGame);
                Singleton<IFikaGame>.Release(HeadlessGame);
            }
        }
    }
}
