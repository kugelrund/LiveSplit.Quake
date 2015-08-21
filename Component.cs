using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace LiveSplit.Quake
{
    class Component : IComponent
    {
        private Settings settings = new Settings();
        private TimerModel model = null;

        private Process gameProcess = null;
        private GameInfo info = null;
        private GameEvent[] eventList = null;

        #region trivial attribute implementations
        public IDictionary<string, Action> ContextMenuControls { get; protected set; }
        public float HorizontalWidth { get { return 0; } }
        public float VerticalHeight { get { return 0; } }
        public float MinimumHeight { get { return 0; } }
        public float MinimumWidth { get { return 0; } }
        public float PaddingBottom { get { return 0; } }
        public float PaddingLeft { get { return 0; } }
        public float PaddingRight { get { return 0; } }
        public float PaddingTop { get { return 0; } }
        #endregion
        
        public string ComponentName { get { return "Quake Auto Splitter"; } }

        public Component(LiveSplitState state)
        {
            model = new TimerModel() { CurrentState = state };
            eventList = settings.GetEventList();
            state.OnReset += State_OnReset;
            settings.EventsChanged += settings_EventsChanged;
        }

        private void State_OnReset(object sender, TimerPhase value)
        {
            info.Reset();
        }

        public void Update(UI.IInvalidator invalidator, Model.LiveSplitState state, float width, float height, UI.LayoutMode mode)
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
        
        public System.Windows.Forms.Control GetSettingsControl(UI.LayoutMode mode)
        {
            return settings;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            return settings.GetSettings(document);
        }

        public void SetSettings(XmlNode settings)
        {
            this.settings.SetSettings(settings);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                settings.EventsChanged -= settings_EventsChanged;
                settings.Dispose();
            }

            disposed = true;
        }

        #region trivial method implementations
        public void DrawHorizontal(System.Drawing.Graphics g, Model.LiveSplitState state, float height, System.Drawing.Region clipRegion) { }
        public void DrawVertical(System.Drawing.Graphics g, Model.LiveSplitState state, float width, System.Drawing.Region clipRegion) { }
        public void RenameComparison(string oldName, string newName) { }
        #endregion
    }
}
