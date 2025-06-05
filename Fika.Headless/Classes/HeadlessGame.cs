using Comfort.Common;
using Dissonance.Networking.Client;
using EFT;
using EFT.Bots;
using EFT.Interactive;
using EFT.UI;
using EFT.Weather;
using Fika.Core.Coop.GameMode;
using JsonType;
using System;
using System.Collections.Generic;
using static BackendConfigSettingsClass;

namespace Fika.Headless.Classes
{
    public class HeadlessGame : AbstractGame, IFikaGame, IClientHearingTable
    {
        public override string LocationObjectId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override GameUI GameUi
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string ProfileId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<int> ExtractedPlayers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ExitStatus ExitStatus
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string ExitLocation
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ESeason Season
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public SeasonsSettingsClass SeasonsSettings
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ISession BackendSession { get; set; }

        public BaseGameController GameController { get; set; }
        public GameDateTime GameDateTime { get; private set; }
        public GameWorld GameWorld { get; private set; }

        private LocalRaidSettings _localRaidSettings;
        private Callback<ExitStatus, TimeSpan, MetricsClass> _exitCallback;
        private LocationSettingsClass.Location _location;
        private EDateTime _tarkovDateTime;
        private DateTime _dateTime;
        private readonly Dictionary<string, DateTime> _factoryTimes = new()
        {
            {
                "factory4_day",
                new DateTime(2016, 8, 4, 15, 28, 0, DateTimeKind.Utc)
            },
            {
                "factory4_night",
                new DateTime(2016, 8, 4, 3, 28, 0, DateTimeKind.Utc)
            }
        };

        public static HeadlessGame Create(GameWorld gameWorld, GameDateTime backendDateTime,
            LocationSettingsClass.Location location, TimeAndWeatherSettings timeAndWeather, WavesSettings wavesSettings,
            EDateTime dateTime, Callback<ExitStatus, TimeSpan, MetricsClass> callback, float fixedDeltaTime,
            EUpdateQueue updateQueue, ISession backEndSession, TimeSpan sessionTime, LocalRaidSettings localRaidSettings,
            RaidSettings raidSettings)
        {
            HeadlessGame game = Create<HeadlessGame>(updateQueue, sessionTime);
            game.GameWorld = gameWorld;

            float num = 1.5f;
            foreach (WildSpawnWave wildSpawnWave in location.waves)
            {
                wildSpawnWave.slots_min = (int)((float)wildSpawnWave.slots_min * num);
                wildSpawnWave.slots_max = (int)((float)wildSpawnWave.slots_max * num);
            }

            game.BackendSession = backEndSession;
            game._exitCallback = callback;
            game._location = location;
            game._tarkovDateTime = dateTime;
            game.FixedDeltaTime = fixedDeltaTime;
            game.HandleLocationData(location, wavesSettings.BotAmount);
            if (!Singleton<BotEventHandler>.Instantiated)
            {
                Singleton<BotEventHandler>.Create(new BotEventHandler());
            }

            game.GameDateTime = backendDateTime;
            game._localRaidSettings = localRaidSettings;
            game.DoWeatherThings(timeAndWeather.IsRandomTime, timeAndWeather.IsRandomWeather);
            WorldInteractiveObject.InteractionShouldBeConfirmed = false;

            return game;

        }

        private void HandleLocationData(LocationSettingsClass.Location location, EBotAmount botAmount)
        {
            location.OldSpawn = location.OfflineOldSpawn;
            location.NewSpawn = location.OfflineNewSpawn;
            float num = 1f;
            switch (botAmount)
            {
                case EBotAmount.NoBots:
                case EBotAmount.Low:
                    num = (Singleton<BackendConfigSettingsClass>.Instance != null) ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_LOW : LocalBotSettingsProviderClass.Core.WAVE_COEF_LOW;
                    break;
                case EBotAmount.Medium:
                    num = (Singleton<BackendConfigSettingsClass>.Instance != null) ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_MID : LocalBotSettingsProviderClass.Core.WAVE_COEF_MID;
                    break;
                case EBotAmount.High:
                    num = (Singleton<BackendConfigSettingsClass>.Instance != null) ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_HIGH : LocalBotSettingsProviderClass.Core.WAVE_COEF_HIGH;
                    break;
                case EBotAmount.Horde:
                    num = (Singleton<BackendConfigSettingsClass>.Instance != null) ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_HORDE : LocalBotSettingsProviderClass.Core.WAVE_COEF_HORDE;
                    break;
            }

            location.BotMax = (int)((float)location.BotMax * num);
        }

        private void DoWeatherThings(bool isRandomTime, bool isRandomWeather)
        {
            Random random = new();
            if (isRandomTime)
            {
                _dateTime = new DateTime(2016, 4, 30, random.Next(1, 24), random.Next(1, 59), 0, DateTimeKind.Utc);
            }
            else if (!_factoryTimes.TryGetValue(_location.Id, out _dateTime))
            {
                _dateTime = (_tarkovDateTime == EDateTime.CURR) ? GameDateTime.Calculate() : GameDateTime.Calculate().AddHours(12.0);
            }
            GameDateTime = new GameDateTime(GameDateTime.DateTime_0, _dateTime, GameDateTime.TimeFactor, GameDateTime.Boolean_0);
            GameWorld.GameDateTime = GameDateTime;
            if (WeatherController.Instance != null || MonoBehaviourSingleton<TODSkySimple>.Instance != null)
            {
                GClass4.Instance.CurrentTime.GameDateTime = GameDateTime;
                WeatherClass[] randomTestWeatherNodes = WeatherClass.GetRandomTestWeatherNodes(600, 12);
                if (!isRandomWeather)
                {
                    long time = randomTestWeatherNodes[0].Time;
                    randomTestWeatherNodes[0] = BackendSession.Weather;
                    randomTestWeatherNodes[0].Time = time;
                }
                if (WeatherController.Instance != null)
                {
                    WeatherController.Instance.method_0(randomTestWeatherNodes);
                }
            }
        }

        public bool IsHeard()
        {
            throw new NotImplementedException();
        }

        public void ReportAbuse()
        {
            throw new NotImplementedException();
        }

        public void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0)
        {
            throw new NotImplementedException();
        }
    }
}
