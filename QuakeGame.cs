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
        public override string[] ProcessNames => new string[] { "joequake-gl" };
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
        private static readonly Int32 mapAddress = 0x6FD148;
        private static readonly Int32 mapTimeAddress = 0x6108F0;
        private static readonly Int32 gameStateAddress = 0x64F664;
        private static readonly DeepPointer qdqMapTimeAddress = new DeepPointer(0x6FBFF8, 0x335C);

        public string CurrMap { get; private set; }
        public bool MapChanged { get; private set; }
        public float IntermissionTime { get; private set; }
        public float MapTime { get; private set; }
        public QuakeState CurrGameState { get; private set; }

        private bool mapTimeUpdateDone = false;

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
                if (!mapTimeUpdateDone)
                {
                    float qdqMapTime;
                    if (qdqMapTimeAddress.Deref(gameProcess, out qdqMapTime))
                    {
                        if (qdqMapTime > 0)
                        {
                            IntermissionTime += qdqMapTime;
                            MapTime = 0;
                        }
                        else
                        {
                            IntermissionTime += MapTime;
                            MapTime = 0;
                        }
                    }

                    mapTimeUpdateDone = true;
                }

                GameTime = IntermissionTime;
            }
            else
            {
                mapTimeUpdateDone = false;

                float mapTime;
                if (gameProcess.ReadValue(baseAddress + mapTimeAddress, out mapTime))
                {
                    if (MapTime > mapTime)
                    {
                        IntermissionTime += MapTime - mapTime;
                    }

                    if (MapTime != 0 || mapTime < 3)  // hack to not update when map time hasn't been reset yet
                    {
                        MapTime = mapTime;
                    }
                }

                if (CurrMap == "start")
                {
                    MapTime = 0;
                }

                GameTime = MapTime + IntermissionTime;
            }
        }

        partial void ResetInfo()
        {
            CurrMap = "";
            CurrGameState = QuakeState.Playing;
            IntermissionTime = 0;
        }
    }
}
