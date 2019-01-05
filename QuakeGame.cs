using System;

namespace LiveSplit.Quake
{
    using ComponentAutosplitter;

    class QuakeGame : Game
    {
        private static readonly Type[] eventTypes = new Type[] { typeof(StartedRunEvent),
                                                                 typeof(LoadedMapEvent),
                                                                 typeof(ShubNiggurathDeadEvent) };
        public override Type[] EventTypes => eventTypes;

        public override string Name => "Quake";
        public override string[] ProcessNames => new string[] { "joequake-gl", "NeaQuakeGL" };
        public override bool GameTimeExists => true;
        public override bool LoadRemovalExists => false;

        public QuakeGame() : base(new CustomSettingBool[]
            { new CustomSettingBool("Keep IGT going when restarting map", false) })
        { 
        }

        public override GameEvent ReadLegacyEvent(string id)
        {
            // fallback to read old autosplitter settings
            if (id.StartsWith("loaded_map_"))
            {
                return new LoadedMapEvent(id.Replace("loaded_map_", ""));
            }
            else if (id == "empty")
            {
                return new EmptyEvent();
            }
            else
            {
                return new EmptyEvent();
            }
        }
    }

    class StartedRunEvent : MapEvent
    {
        public override string Description => "A new run was started.";

        public StartedRunEvent() : base("start")
        {
        }

        public StartedRunEvent(string map) : base((map.Length == 0) ? "start" : map)
        {
        }

        public override bool HasOccured(GameInfo info)
        {
            return info.CounterChanged && (info.CurrMap == map);
        }

        public override string ToString()
        {
            return "Start of new run on '" + map + "'";
        }
    }

    class LoadedMapEvent : MapEvent
    {
        public override string Description => "A certain map was loaded.";

        public LoadedMapEvent() : base()
        {
        }

        public LoadedMapEvent(string map) : base(map)
        {
        }

        public override bool HasOccured(GameInfo info)
        {
            return info.MapChanged && (info.CurrMap == map);
        }

        public override string ToString()
        {
            return "Map '" + map + "' was loaded";
        }
    }

    class ShubNiggurathDeadEvent : NoAttributeEvent
    {
        public override string Description => "Player defeated Shub-Niggurath.";

        public override bool HasOccured(GameInfo info)
        {
            return info.CurrMap == "end" && info.CurrGameState == QuakeState.IntermissionText;
        }

        public override string ToString()
        {
            return "Shub-Niggurath dead";
        }
    }

    public enum GameVersion
    {
        JoeQuake3798,   // joequake-gl.exe build 3798
        JoeQuake5288,   // joequake-gl.exe build 5288
        NeaQuake        // NeaQuakeGL.exe Version 1
    }

    public enum QuakeState
    {
        Playing = 0, Intermission = 1, IntermissionText = 2
    }
}

namespace LiveSplit.ComponentAutosplitter
{
    using System.Text;
    using ComponentUtil;
    using Quake;

    partial class GameInfo
    {
        private Int32 mapAddress;
        private Int32 mapTimeAddress;
        private Int32 gameStateAddress;
        private Int32 counterAddress;
        private DeepPointer totalTimeAddress;

        private GameVersion gameVersion;
        private bool keepInGameTimeGoing = false;
        
        private float savedTotalTime = 0;
        private int counter = 0;

        public string CurrMap { get; private set; }
        public bool CounterChanged { get; private set; }
        public bool MapChanged { get; private set; }
        public float TotalTime { get; private set; }
        public float MapTime { get; private set; }
        public QuakeState CurrGameState { get; private set; }

        partial void SetCustomSettings(CustomSettingBool[] customSettings)
        {
            keepInGameTimeGoing = customSettings[0].Value;
        }

        partial void GetVersion()
        {
            switch (gameProcess.ProcessName)
            {
                case "joequake-gl":
                    {
                        ProcessModuleWow64Safe mainModule = gameProcess.MainModuleWow64Safe();
                        if (!mainModule.ModuleName.EndsWith(".exe"))
                        {
                            // kind of a workaround for MainModuleWow64Safe maybe not returning
                            // the correct module
                            throw new ArgumentException("Process not initialised yet!");
                        }

                        if (mainModule.ModuleMemorySize == 16248832)
                        {
                            gameVersion = GameVersion.JoeQuake3798;
                        }
                        else if (mainModule.ModuleMemorySize == 8974336)
                        {
                            gameVersion = GameVersion.JoeQuake5288;
                        }
                        break;
                    }
                case "NeaQuakeGL":
                    gameVersion = GameVersion.NeaQuake;
                    break;
                default:
                    gameVersion = GameVersion.JoeQuake3798;
                    break;
            }

            System.Threading.Thread.Sleep(200);  // a bit stupid but just to make sure
                                                 // game name is set already
            Int32 gameNameAddress = 0;
            switch (gameVersion)
            {
                case GameVersion.JoeQuake3798:
                    gameNameAddress = 0x61E23D;
                    break;
                case GameVersion.JoeQuake5288:
                    gameNameAddress = 0x13B759;
                    break;
                case GameVersion.NeaQuake:
                    gameNameAddress = 0x12F101;
                    break;
            }
            StringBuilder gameNameBuilder = new StringBuilder(32);
            gameProcess.ReadString(baseAddress + gameNameAddress, gameNameBuilder);
            string gameName = gameNameBuilder.ToString();
            Int32 totalTimeAddressOffset;
            switch (gameName)
            {
                case "hipnotic":
                    totalTimeAddressOffset = 0x3278;
                    break;
                case "rogue":
                    totalTimeAddressOffset = 0x527C;
                    break;
                default:
                    totalTimeAddressOffset = 0x2948;
                    break;
            }

            switch (gameVersion)
            {
                case GameVersion.JoeQuake3798:
                    mapAddress = 0x6FD148;
                    mapTimeAddress = 0x6108F0;
                    gameStateAddress = 0x64F664;
                    counterAddress = 0x622294;
                    totalTimeAddress = new DeepPointer(0x6FBFF8, totalTimeAddressOffset);
                    break;
                case GameVersion.JoeQuake5288:
                    mapAddress = 0x3F6008;
                    mapTimeAddress = 0x2FEF74;
                    gameStateAddress = 0x34CC18;
                    counterAddress = 0x13A248;
                    totalTimeAddress = new DeepPointer(0x3F4EA8, totalTimeAddressOffset);
                    break;
                case GameVersion.NeaQuake:
                    mapAddress = 0x26E368;
                    mapTimeAddress = 0x2619EC;
                    gameStateAddress = 0xB6AA84;
                    counterAddress = 0x12DEA8;
                    totalTimeAddress = new DeepPointer(0x28085C, totalTimeAddressOffset);
                    break;
            }
        }

        private void UpdateCounter()
        {
            int newCounter;
            if (gameProcess.ReadValue(baseAddress + counterAddress, out newCounter))
            {
                if (counter != newCounter)
                {
                    counter = newCounter;
                    CounterChanged = true;
                    return;
                }
            }

            CounterChanged = false;
        }

        private void UpdateMap()
        {
            StringBuilder mapBuilder = new StringBuilder(32);
            gameProcess.ReadString(baseAddress + mapAddress, mapBuilder);
            string map = mapBuilder.ToString();

            if (map != null && map != CurrMap)
            {
                CurrMap = map;
                MapChanged = true;
            }
            else
            {
                MapChanged = false;
            }
        }

        partial void UpdateInfo()
        {
            UpdateCounter();
            UpdateMap();

            int currGameState;
            if (gameProcess.ReadValue(baseAddress + gameStateAddress, out currGameState))
            {
                CurrGameState = (QuakeState)currGameState;
            }

            if (CurrGameState != QuakeState.Playing)
            {
                // we are in an intermission, so total time should be available now.
                float currentTotalTime;
                if (totalTimeAddress.Deref(gameProcess, out currentTotalTime) && currentTotalTime > 0)
                {
                    // set time to the one that qdqstats says + an eventually saved one that isn't included
                    // in the qdqTotalTime because there was a reset
                    TotalTime = currentTotalTime + savedTotalTime;
                }

                MapTime = 0;
                GameTime = TotalTime;
            }
            else
            {
                //
                // in game, so don't have to deal with intermission stuff.
                //

                float mapTime;
                if (gameProcess.ReadValue(baseAddress + mapTimeAddress, out mapTime))
                {
                    if (keepInGameTimeGoing && MapTime > mapTime)
                    {
                        // new map time is smaller than old map time? This means that there was a reset, so
                        // qdqstats' total time will be reset. Therefore update savedTotalTime
                        TotalTime += MapTime - mapTime;
                        savedTotalTime = TotalTime;
                    }

                    if (MapTime != 0 || mapTime < 3)
                    {
                        // hack to not update when map time hasn't been reset yet
                        MapTime = mapTime;
                    }
                }

                if (CurrMap == "start")
                {
                    // timing according to rules doesnt include the time spent on the start map.
                    // literally cheating if you ask me...
                    MapTime = 0;
                }

                GameTime = MapTime + TotalTime;
            }
        }

        partial void ResetInfo()
        {
            MapChanged = false;
            CounterChanged = false;
            CurrGameState = QuakeState.Playing;
            MapTime = 0;
            TotalTime = 0;
            savedTotalTime = 0;
        }
    }
}
