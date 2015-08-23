using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace LiveSplit.Quake
{
    class Component : LogicComponent
    {
        private Settings settings = new Settings();
        private TimerModel model = null;

        private Process gameProcess = null;
        private GameInfo info = null;
        private GameEvent[] eventList = null;
        
        public override string ComponentName => "Quake Auto Splitter";

        public Component(LiveSplitState state)
        {
            model = new TimerModel() { CurrentState = state };
            model.CurrentState.OnStart += State_OnStart;
            model.CurrentState.OnReset += State_OnReset;

            eventList = settings.GetEventList();
            settings.EventsChanged += settings_EventsChanged;
        }

        private void State_OnStart(object sender, EventArgs e)
        {
            model.InitializeGameTime();
        }

        private void State_OnReset(object sender, TimerPhase value)
        {
            info.Reset();
        }

        public override void Update(UI.IInvalidator invalidator, Model.LiveSplitState state, float width, float height, UI.LayoutMode mode)
        {
            if (gameProcess != null && !gameProcess.HasExited)
            {
                state.IsGameTimePaused = true;
                info.Update();

                if (info.InIntermission)
                {
                    state.SetGameTime(TimeSpan.FromSeconds(info.IntermissionTime));
                }

                if (eventList[state.CurrentSplitIndex + 1].HasOccured(info))
                {
                    if (state.CurrentPhase == TimerPhase.NotRunning)
                    {
                        state.SetGameTime(TimeSpan.Zero);
                        model.Start();
                    }
                    else
                    {
                        model.Split();
                    }
                }

                if (settings.UpdateGameTime)
                {
                    state.SetGameTime(TimeSpan.FromSeconds(info.IntermissionTime + info.MapTime));
                }
            }
            else
            {
                gameProcess = Process.GetProcessesByName("joequake-gl").FirstOrDefault();
                if (gameProcess != null)
                {
                    info = new GameInfo(gameProcess);
                }
            }
        }

        private void settings_EventsChanged(object sender, EventArgs e)
        {
            eventList = settings.GetEventList();
        }
        
        public override System.Windows.Forms.Control GetSettingsControl(UI.LayoutMode mode)
        {
            return settings;
        }

        public override XmlNode GetSettings(XmlDocument document)
        {
            return settings.GetSettings(document);
        }

        public override void SetSettings(XmlNode settings)
        {
            this.settings.SetSettings(settings);
        }

        public override void Dispose()
        {
            model.CurrentState.OnStart -= State_OnStart;
            model.CurrentState.OnReset -= State_OnReset;
            settings.EventsChanged -= settings_EventsChanged;
            settings.Dispose();
        }
    }
}
