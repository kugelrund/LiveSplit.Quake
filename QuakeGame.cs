using System;

namespace LiveSplit.Quake
{
    using ComponentAutosplitter;

    class QuakeGame : Game
    {
        private static readonly Type[] eventTypes = new Type[] { typeof(LoadedMapEvent),
                                                                 typeof(ShubNiggurathDeadEvent) };
        public override Type[] EventTypes => eventTypes;

        public override string Name => "Quake";
        public override string[] ProcessNames => new string[] { "joequake-gl", "NeaQuakeGL" };
        public override bool GameTimeExists => true;
        public override bool LoadRemovalExists => false;

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
        JoeQuake, // joequake_gl.exe v0.15
        NeaQuake  // NeaQuakeGL.exe Version 1
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
        private DeepPointer qdqTotalTimeAddress;

        private GameVersion gameVersion;
        
        private float savedTotalTime = 0;

        public string CurrMap { get; private set; }
        public bool MapChanged { get; private set; }
        public float TotalTime { get; private set; }
        public float MapTime { get; private set; }
        public QuakeState CurrGameState { get; private set; }

        partial void GetVersion()
        {
            switch (gameProcess.ProcessName)
            {
                case "joequake-gl":
                    gameVersion = GameVersion.JoeQuake;
                    break;
                case "NeaQuakeGL":
                    gameVersion = GameVersion.NeaQuake;
                    break;
                default:
                    gameVersion = GameVersion.JoeQuake;
                    break;
            }

            switch (gameVersion)
            {
                case GameVersion.JoeQuake:
                    mapAddress = 0x6FD148;
                    mapTimeAddress = 0x6108F0;
                    gameStateAddress = 0x64F664;
                    qdqTotalTimeAddress = new DeepPointer(0x6FBFF8, 0x2948);
                    break;
                case GameVersion.NeaQuake:
                    mapAddress = 0x26E368;
                    mapTimeAddress = 0x2619EC;
                    gameStateAddress = 0xB6AA84;
                    qdqTotalTimeAddress = new DeepPointer(0x28085C, 0x2948);
                    break;
            }
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
            UpdateMap();

            int currGameState;
            if (gameProcess.ReadValue(baseAddress + gameStateAddress, out currGameState))
            {
                CurrGameState = (QuakeState)currGameState;
            }

            if (CurrGameState != QuakeState.Playing)
            {
                // we are in an intermission, so total time should be available now.
                float qdqTotalTime;
                if (qdqTotalTimeAddress.Deref(gameProcess, out qdqTotalTime) && qdqTotalTime > 0)
                {
                    // set time to the one that qdqstats says + an eventually saved one that isn't included
                    // in the qdqTotalTime because there was a reset
                    TotalTime = qdqTotalTime + savedTotalTime;
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
                    if (MapTime > mapTime)
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
            CurrMap = "";
            CurrGameState = QuakeState.Playing;
            TotalTime = 0;
            savedTotalTime = 0;
        }
    }
}
