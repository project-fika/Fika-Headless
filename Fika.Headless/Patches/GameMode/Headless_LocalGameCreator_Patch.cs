using Comfort.Common;
using Diz.Resources;
using EFT;
using EFT.Communications;
using EFT.InputSystem;
using EFT.UI;
using EFT.Utilities;
using Fika.Core.Main.GameMode;
using Fika.Core.Main.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Headless.Classes.GameMode;
using HarmonyLib;
using JsonType;
using SPT.Reflection.Patching;
using SPT.SinglePlayer.Utils.InRaid;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches.GameMode;

internal class Headless_LocalGameCreator_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication)
            .GetMethod(nameof(TarkovApplication.LocalGameCreate));
    }

    [PatchPrefix]
    public static bool Prefix(ref Task __result, TarkovApplication __instance, TimeAndWeatherSettings timeAndWeather,
        RaidSettings ____raidSettings, InputTree ____inputTree, GameDateTime ____localGameDateTime,
        float ____fixedDeltaTime, string ____backendUrl, ClientMetricsEvents metricsEvents,
        ClientMetricsConfig metricsConfig, GameWorld gameWorld, MainMenuShowOperation ____menuOperation,
        CompositeDisposable ____unsubscriber, BundleLock ___BundleLock)
    {
#if DEBUG
        Logger.LogInfo("TarkovApplication_LocalGameCreator_Patch:Prefix");
#endif
        __result = CreateFikaGame(__instance, timeAndWeather, ____raidSettings, ____localGameDateTime,
            ____fixedDeltaTime, ____backendUrl, metricsEvents, gameWorld, ____menuOperation,
            ____unsubscriber, ___BundleLock);
        return false;
    }

    public static async Task CreateFikaGame(TarkovApplication instance, TimeAndWeatherSettings timeAndWeather,
        RaidSettings raidSettings, GameDateTime localGameDateTime, float fixedDeltaTime, string backendUrl, ClientMetricsEvents metricsEvents,
        GameWorld gameWorld, MainMenuShowOperation menuOperation, CompositeDisposable unsubscriber, BundleLock bundleLock)
    {
        var isTransit = FikaBackendUtils.IsTransit;

        if (!isTransit)
        {
            FikaBackendUtils.CachedRaidSettings = raidSettings;
        }
        else if (isTransit && FikaBackendUtils.CachedRaidSettings != null)
        {
            Logger.LogInfo("Applying cached raid settings from previous raid");
            var cachedSettings = FikaBackendUtils.CachedRaidSettings;
            raidSettings.WavesSettings = cachedSettings.WavesSettings;
            raidSettings.BotSettings = cachedSettings.BotSettings;
            raidSettings.MetabolismDisabled = cachedSettings.MetabolismDisabled;
            raidSettings.PlayersSpawnPlace = cachedSettings.PlayersSpawnPlace;
        }

        metricsEvents.SetGamePrepared();

        if (Singleton<NotificationManager>.Instantiated)
        {
            Singleton<NotificationManager>.Instance.Deactivate();
        }

        var session = instance.Session;
        if (session == null)
        {
            throw new NullReferenceException("Backend session was null when initializing game!");
        }

        var profile = session.GetProfileBySide(raidSettings.Side);

        profile.Inventory.Stash = null;
        profile.Inventory.QuestStashItems = null;
        profile.Inventory.DiscardLimits = Singleton<ItemFactory>.Instance.GetDiscardLimits();

#if DEBUG
        Logger.LogInfo("TarkovApplication_LocalGameCreator_Patch:Postfix: Attempt to set Raid Settings");
        Logger.LogInfo($"RaidSettings TransitType: {raidSettings.transitionType}");
#endif

        if (!raidSettings.isInTransition)
        {
            await session.SendRaidSettings(raidSettings);
        }
        LocalRaidSettings localRaidSettings = new()
        {
            location = raidSettings.LocationId,
            timeVariant = raidSettings.SelectedDateTime,
            mode = ELocalMode.PVE_OFFLINE,
            playerSide = raidSettings.Side,
            transitionType = raidSettings.transitionType
        };
        var applicationTraverse = Traverse.Create(instance);
        applicationTraverse.Field<LocalRaidSettings>("_localRaidSettings").Value = localRaidSettings;

        var localSettings = await instance.Session.LocalRaidStarted(localRaidSettings);
        var raidSettingsToUpdate = applicationTraverse.Field<LocalRaidSettings>("_localRaidSettings").Value;
        var escapeTimeLimit = raidSettings.IsScav ? RaidChangesUtil.NewEscapeTimeMinutes : raidSettings.SelectedLocation.EscapeTimeLimit;
        raidSettings.SelectedLocation = localSettings.locationLoot;
        raidSettings.SelectedLocation.EscapeTimeLimit = escapeTimeLimit;
        raidSettingsToUpdate.serverId = localSettings.serverId;
        raidSettingsToUpdate.selectedLocation = localSettings.locationLoot;
        raidSettingsToUpdate.selectedLocation.EscapeTimeLimit = escapeTimeLimit;

        var transitData = FikaBackendUtils.TransitData;
        transitData.transitionType = raidSettings.transitionType;
        raidSettingsToUpdate.transition = FikaBackendUtils.TransitData;

        instance.Matchmaker.UpdateMatchingStatus("Hosting headless game...");
        Singleton<FikaServer>.Instance.LocationReceived = true;

        StartHandler startHandler = new(instance, session.Profile, session.ProfileOfPet, raidSettings.SelectedLocation);

        var raidLimits = GetRaidMinutes(raidSettings.SelectedLocation.EscapeTimeLimit);

        var headlessGame = HeadlessGame.Create(gameWorld, localGameDateTime, raidSettings.SelectedLocation, timeAndWeather,
            raidSettings.WavesSettings, raidSettings.SelectedDateTime, startHandler.HandleStop, fixedDeltaTime, instance.PlayerUpdateQueue, instance.Session,
            raidLimits, localRaidSettings, raidSettings);

        startHandler.HeadlessGame = headlessGame;

        Singleton<AbstractGame>.Create(headlessGame);
        unsubscriber.AddDisposable(headlessGame);
        unsubscriber.AddDisposable(startHandler.ReleaseSingleton);
        metricsEvents.SetGameCreated();
        FikaEventDispatcher.DispatchEvent(new AbstractGameCreatedEvent(headlessGame));

        headlessGame.SetMatchmakerStatus("Headless game created");

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
        menuOperation?.Unsubscribe();
        bundleLock.MaxConcurrentOperations = 1;
        gameWorld.OnGameStarted();

        FikaEventDispatcher.DispatchEvent(new GameWorldStartedEvent(gameWorld));
    }

    private static TimeSpan GetRaidMinutes(int defaultMinutes)
    {
        return TimeSpan.FromSeconds((double)(60 * defaultMinutes));
    }

    private class StartHandler(TarkovApplication tarkovApplication, Profile pmcProfile, Profile scavProfile,
        LocationSettings.Location location)
    {
        private readonly TarkovApplication _tarkovApplication = tarkovApplication;
        private readonly Profile _pmcProfile = pmcProfile;
        private readonly Profile _scavProfile = scavProfile;
        private readonly LocationSettings.Location _location = location;
        public HeadlessGame HeadlessGame;

        public void HandleStop(Result<ExitStatus, TimeSpan, ClientMetrics> result)
        {
            _tarkovApplication.OnGameEnd(_pmcProfile.Id, _scavProfile, _location, result);
        }

        public void ReleaseSingleton()
        {
            Singleton<AbstractGame>.Release(HeadlessGame);
            Singleton<IFikaGame>.Release(HeadlessGame);
        }
    }
}
