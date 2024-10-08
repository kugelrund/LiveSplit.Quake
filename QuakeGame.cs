﻿using System;

namespace LiveSplit.Quake
{
    using ComponentAutosplitter;

    class QuakeGame : Game
    {
        private static readonly Type[] eventTypes = new Type[] { typeof(MapChangedEvent),
                                                                 typeof(LoadedMapEvent),
                                                                 typeof(StartedRunEvent),
                                                                 typeof(ShubNiggurathDeadEvent) };
        public override Type[] EventTypes => eventTypes;

        public override string Name => "Quake";
        public override string[] ProcessNames => new string[] { "joequake-gl", "quake3", "NeaQuakeGL" };
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

    class MapChangedEvent : NoAttributeEvent
    {
        public override string Description => "A different map was loaded.";

        public override bool HasOccured(GameInfo info)
        {
            return info.MapChanged;
        }

        public override string ToString()
        {
            return "Map has changed";
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
        // order this by release date to allow checking backwards compatibility
        // with inequality comparisons on this enum
        JoeQuake3798,   // joequake-gl.exe build 3798
        NeaQuake,       // NeaQuakeGL.exe Version 1
        JoeQuake5288,   // joequake-gl.exe build 5288
        JoeQuake6300,   // joequake-gl.exe build 6300 (version 0.16.2)
        JoeQuake6320,   // joequake-gl.exe build 6320 (version 0.16.2)
        JoeQuake6454,   // joequake-gl.exe build 6454 (version 0.16.2)
        JoeQuake6689,   // joequake-gl.exe build 6689 (version 0.17.0)
        JoeQuake6746,   // joequake-gl.exe build 6746 (version 0.17.1)
        JoeQuake6866,   // joequake-gl.exe build 6866 (version 0.17.1)
        JoeQuake6949,   // joequake-gl.exe build 6949 (version 0.17.2)
        JoeQuake7049,   // joequake-gl.exe build 7049 (version 0.17.3)
        JoeQuake7319,   // joequake-gl.exe build 7319 (version 0.17.4)
        JoeQuake7492,   // joequake-gl.exe build 7492 (version 0.17.5)
        JoeQuake7652,   // joequake-gl.exe build 7652 (version 0.17.6)
        JoeQuake7733,   // joequake-gl.exe build 7733 (version 0.17.7)
        JoeQuake8039,   // joequake-gl.exe build 8039 (version 0.17.8)
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
        private Int32 totalTimeAddress = 0;
        private DeepPointer oldTotalTimePointer = null;

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
                case "quake3":
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
                        else if (mainModule.ModuleMemorySize == 9019392)
                        {
                            gameVersion = GameVersion.JoeQuake6300;
                        }
                        else if (mainModule.ModuleMemorySize == 9027584)
                        {
                            gameVersion = GameVersion.JoeQuake6320;
                        }
                        else if (mainModule.ModuleMemorySize == 18841600)
                        {
                            gameVersion = GameVersion.JoeQuake6454;
                        }
                        else if (mainModule.ModuleMemorySize == 35803136)
                        {
                            gameVersion = GameVersion.JoeQuake6689;
                        }
                        else if (mainModule.ModuleMemorySize == 35889152)
                        {
                            gameVersion = GameVersion.JoeQuake6746;
                        }
                        else if (mainModule.ModuleMemorySize == 98803712)
                        {
                            gameVersion = GameVersion.JoeQuake6866;
                        }
                        else if (mainModule.ModuleMemorySize == 98828288)
                        {
                            gameVersion = GameVersion.JoeQuake6949;
                        }
                        else if (mainModule.ModuleMemorySize == 99049472)
                        {
                            gameVersion = GameVersion.JoeQuake7049;
                        }
                        else if (mainModule.ModuleMemorySize == 99389440)
                        {
                            gameVersion = GameVersion.JoeQuake7319;
                        }
                        else if (mainModule.ModuleMemorySize == 99557376)
                        {
                            gameVersion = GameVersion.JoeQuake7492;
                        }
                        else if (mainModule.ModuleMemorySize == 75218944)
                        {
                            gameVersion = GameVersion.JoeQuake7652;
                        }
                        else if (mainModule.ModuleMemorySize == 75194368)
                        {
                            gameVersion = GameVersion.JoeQuake7733;
                        }
                        else if (mainModule.ModuleMemorySize == 75632640)
                        {
                            gameVersion = GameVersion.JoeQuake8039;
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
                case GameVersion.JoeQuake6300:
                    gameNameAddress = 0x33D300;
                    break;
                case GameVersion.JoeQuake6320:
                    gameNameAddress = 0x33E360;
                    break;
                case GameVersion.JoeQuake6454:
                    gameNameAddress = 0x345DA0;
                    break;
                case GameVersion.JoeQuake6689:
                    gameNameAddress = 0x5D8480;
                    break;
                case GameVersion.JoeQuake6746:
                    gameNameAddress = 0x5E2620;
                    break;
                case GameVersion.JoeQuake6866:
                    gameNameAddress = 0x41E2756;
                    break;
                case GameVersion.JoeQuake6949:
                    gameNameAddress = 0x41E8676;
                    break;
                case GameVersion.JoeQuake7049:
                    gameNameAddress = 0x41EDA56;
                    break;
                case GameVersion.JoeQuake7319:
                    gameNameAddress = 0x4235C96;
                    break;
                case GameVersion.JoeQuake7492:
                    gameNameAddress = 0x425CDA0;
                    break;
                case GameVersion.JoeQuake7652:
                    gameNameAddress = 0x42233C0;
                    break;
                case GameVersion.JoeQuake7733:
                    gameNameAddress = 0x425BC60;
                    break;
                case GameVersion.JoeQuake8039:
                    gameNameAddress = 0x4262B60;
                    break;
                case GameVersion.NeaQuake:
                    gameNameAddress = 0x12F101;
                    break;
            }
            StringBuilder gameNameBuilder = new StringBuilder(32);
            gameProcess.ReadString(baseAddress + gameNameAddress, gameNameBuilder);
            string gameName = gameNameBuilder.ToString();
            Int32 oldTotalTimePointerOffset;
            switch (gameName)
            {
                case "hipnotic":
                    oldTotalTimePointerOffset = 0x3278;
                    break;
                case "rogue":
                    oldTotalTimePointerOffset = 0x527C;
                    break;
                case "digs01":
                    oldTotalTimePointerOffset = 0x2958;
                    break;
                default:
                    oldTotalTimePointerOffset = 0x2948;
                    break;
            }

            switch (gameVersion)
            {
                case GameVersion.JoeQuake3798:
                    mapAddress = 0x6FD148;
                    mapTimeAddress = 0x6108F0;
                    gameStateAddress = 0x64F664;
                    counterAddress = 0x622294;
                    oldTotalTimePointer = new DeepPointer(0x6FBFF8, oldTotalTimePointerOffset);
                    break;
                case GameVersion.NeaQuake:
                    mapAddress = 0x26E368;
                    mapTimeAddress = 0x2619EC;
                    gameStateAddress = 0xB6AA84;
                    counterAddress = 0x12DEA8;
                    oldTotalTimePointer = new DeepPointer(0x28085C, oldTotalTimePointerOffset);
                    break;
                case GameVersion.JoeQuake5288:
                    mapAddress = 0x3F6008;
                    mapTimeAddress = 0x2FEF74;
                    gameStateAddress = 0x34CC18;
                    counterAddress = 0x13A248;
                    oldTotalTimePointer = new DeepPointer(0x3F4EA8, oldTotalTimePointerOffset);
                    break;
                case GameVersion.JoeQuake6300:
                    mapAddress = 0x4005C8;
                    mapTimeAddress = 0x305FF0;
                    gameStateAddress = 0x3F520C;
                    counterAddress = 0x1412A8;
                    oldTotalTimePointer = new DeepPointer(0x3FF468, oldTotalTimePointerOffset);
                    break;
                case GameVersion.JoeQuake6320:
                    mapAddress = 0x401628;
                    mapTimeAddress = 0x307050;
                    gameStateAddress = 0x3F626C;
                    counterAddress = 0x142308;
                    oldTotalTimePointer = new DeepPointer(0x4004C8, oldTotalTimePointerOffset);
                    break;
                case GameVersion.JoeQuake6454:
                    mapAddress = 0xD441A8;
                    mapTimeAddress = 0xD244D8;
                    gameStateAddress = 0xD244AC;
                    counterAddress = 0x142348;
                    oldTotalTimePointer = new DeepPointer(0xD43048, oldTotalTimePointerOffset);
                    break;
                case GameVersion.JoeQuake6689:
                    mapAddress = 0x782A48;
                    mapTimeAddress = 0x745658;
                    gameStateAddress = 0x74562C;
                    counterAddress = 0x14B484;
                    oldTotalTimePointer = new DeepPointer(0x77C970, oldTotalTimePointerOffset);
                    break;
                case GameVersion.JoeQuake6746:
                    mapAddress = 0x78CBE8;
                    mapTimeAddress = 0x74F7F8;
                    gameStateAddress = 0x74F7CC;
                    counterAddress = 0x1514CC;
                    totalTimeAddress = 0x754A48;
                    break;
                case GameVersion.JoeQuake6866:
                    mapAddress = 0x438CCC8;
                    mapTimeAddress = 0x434F7D8;
                    gameStateAddress = 0x434F7AC;
                    counterAddress = 0x1514CC;
                    totalTimeAddress = 0x4354A28;
                    break;
                case GameVersion.JoeQuake6949:
                    mapAddress = 0x4392D28;
                    mapTimeAddress = 0x4355838;
                    gameStateAddress = 0x435580C;
                    counterAddress = 0x1534E0;
                    totalTimeAddress = 0x435AA88;
                    break;
                case GameVersion.JoeQuake7049:
                    mapAddress = 0x4398108;
                    mapTimeAddress = 0x435ABF8;
                    gameStateAddress = 0x435ABCC;
                    counterAddress = 0x158500;
                    totalTimeAddress = 0x435FE48;
                    break;
                case GameVersion.JoeQuake7319:
                    mapAddress = 0x43E8C68;
                    mapTimeAddress = 0x43AB6F8;
                    gameStateAddress = 0x43AB6CC;
                    counterAddress = 0x15F738;
                    totalTimeAddress = 0x43B09A8;
                    break;
                case GameVersion.JoeQuake7492:
                    mapAddress = 0x440F8E8;
                    mapTimeAddress = 0x43D2378;
                    gameStateAddress = 0x43D234C;
                    counterAddress = 0x17F844;
                    totalTimeAddress = 0x43D7628;
                    break;
                case GameVersion.JoeQuake7652:
                    mapAddress = 0x43DDC28;
                    mapTimeAddress = 0x43DDC10;
                    gameStateAddress = 0x43A066C;
                    counterAddress = 0x17704C;
                    totalTimeAddress = 0x43A5948;
                    break;
                case GameVersion.JoeQuake7733:
                    mapAddress = 0x4418A88;
                    mapTimeAddress = 0x4418A70;
                    gameStateAddress = 0x43D90AC;
                    counterAddress = 0x17CFFC;
                    totalTimeAddress = 0x43DE388;
                    break;
                case GameVersion.JoeQuake8039:
                    mapAddress = 0x4417228;
                    mapTimeAddress = 0x4417210;
                    gameStateAddress = 0x43D76AC;
                    counterAddress = 0x17EFFC;
                    totalTimeAddress = 0x43DCB28;
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

        private bool ReadTotalTime(out float currentTotalTime)
        {
            if (oldTotalTimePointer != null)
            {
                // old method with a pointer path to something in the specific
                // qdqstats progs, different depending on the specific mod
                return oldTotalTimePointer.Deref(gameProcess, out currentTotalTime);
            }
            else if (totalTimeAddress != 0)
            {
                // new method with new JoeQuake that directly provides total
                // time in static memory. That one is also in double precision.
                if (gameProcess.ReadValue(baseAddress + totalTimeAddress,
                                          out double currentTotalTimeDouble))
                {
                    currentTotalTime = (float)currentTotalTimeDouble;
                    return true;
                }
            }
            currentTotalTime = 0.0f;
            return false;
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
                if (ReadTotalTime(out currentTotalTime) && currentTotalTime > 0)
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
                bool successReadMapTime;
                if (gameVersion >= GameVersion.JoeQuake6454)
                {
                    // Historically we were using a float value for map time. Unfortunately that one turned out to not
                    // work with -no24bit, and the only really appropriate alternative values are double. So we read
                    // double from now on but the float stuff sticks around for backwards compatibility.
                    successReadMapTime = gameProcess.ReadValue(baseAddress + mapTimeAddress, out double mapTimeDouble);
                    mapTime = (float)mapTimeDouble;
                }
                else
                {
                    successReadMapTime = gameProcess.ReadValue(baseAddress + mapTimeAddress, out mapTime);
                }

                if (successReadMapTime)
                {
                    const float resetTolerance = 0.001f;
                    if (keepInGameTimeGoing && MapTime > mapTime + resetTolerance)
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
