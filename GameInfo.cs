using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LiveSplit.Quake
{
    class GameInfo
    {       
        private static readonly DeepPointer mapAddress = new DeepPointer(0x6FD148, new int[] { });
        private static readonly DeepPointer gameStateAddress = new DeepPointer(0x7020A4, new int[] { });
        private static readonly DeepPointer intermissionTimeAddress = new DeepPointer(0x64F668, new int[] { });
        private static readonly DeepPointer qdqTotalTimeAddress = new DeepPointer(0x6FBFF8, new int[] { 0x2948 });

        private const int MAX_MAP_LENGTH = 5;

        private Process gameProcess;
        
        public string PrevMap { get; private set; }
        public string CurrMap { get; private set; }
        public bool MapChanged { get; private set; }

        public bool IsPaused
        {
            get
            {
                int gameState;
                if (gameStateAddress.Deref(gameProcess, out gameState))
                {
                    return gameState != 0;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool InIntermission
        {
            get
            {
                int intermissionTime;
                if (intermissionTimeAddress.Deref(gameProcess, out intermissionTime))
                {
                    return intermissionTime != 0;
                }
                else
                {
                    return false;
                }
            }
        }

        public float IngameTime
        {
            get
            {
                float ingameTime;
                if (qdqTotalTimeAddress.Deref(gameProcess, out ingameTime))
                {
                    return ingameTime;
                }
                else
                {
                    return 0;
                }
            }
        }


        public GameInfo(Process gameProcess)
        {
            this.gameProcess = gameProcess;
        }

        private void UpdateMap()
        {
            string map;
            mapAddress.Deref(gameProcess, out map, MAX_MAP_LENGTH);
            if (map.Length > 0 && map != CurrMap)
            {
                PrevMap = CurrMap;
                CurrMap = map;
                MapChanged = true;
            }
            else
            {
                MapChanged = false;
            }
        }

        public void Update()
        {
            UpdateMap();
        }
    }

    abstract class GameEvent
    {
        private static Dictionary<string, GameEvent> events = null;
        
        public abstract string Id { get; }

        public static Dictionary<string, GameEvent> GetEvents()
        {
            if (events == null)
            {
                string[] maps = {"start",
                                 "e1m1", "e1m2", "e1m3", "e1m4", "e1m5", "e1m6", "e1m7", "e1m8",
                                 "e2m1", "e2m2", "e2m3", "e2m4", "e2m5", "e2m6", "e2m7",
                                 "e3m1", "e3m2", "e3m3", "e3m4", "e3m5", "e3m6", "e3m7",
                                 "e4m1", "e4m2", "e4m3", "e4m4", "e4m5", "e4m6", "e4m7", "e4m8",
                                 "end"};

                events = new Dictionary<string, GameEvent>();
                foreach (string map in maps)
                {
                    events.Add("loaded_map_" + map, new LoadedMapEvent(map));
                }
            }

            return events;
        }

        public abstract bool HasOccured(GameInfo info);
    }
    
    abstract class MapEvent : GameEvent
    {
        protected readonly string map;

        public MapEvent(string map)
        {
            this.map = map;
        }
    }

    class LoadedMapEvent : MapEvent
    {
        public override string Id { get { return "loaded_map_" + map; } }

        public LoadedMapEvent(string map) : base(map) { }

        public override bool HasOccured(GameInfo info)
        {
            return !info.InIntermission && (info.CurrMap == map);
        }
                
        public override string ToString()
        {
            return "Loaded '" + map + "'";
        }
    }
    
    class EmptyEvent : GameEvent
    {
        public override string Id { get { return "empty"; } }

        public override bool HasOccured(GameInfo info)
        {
            return false;
        }
    }
}
