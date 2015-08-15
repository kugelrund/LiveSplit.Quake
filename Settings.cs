using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.Quake
{
    partial class Settings : UserControl
    {
        private readonly Dictionary<string, GameEvent> events;

        public bool UpdateGameTime { get; private set; }

        private bool eventsChanged = false;
        public event EventHandler EventsChanged;
        protected virtual void OnChanged(EventArgs e)
        {
            if (EventsChanged != null)
            {
                EventsChanged(this, e);
            }
        }

        public Settings()
        {
            InitializeComponent();

            events = GameEvent.GetEvents();

            foreach (Object item in events.Values)
            {
                lstAvailEvents.Items.Add(item);
            }
            UpdateGameTime = false;
        }

        public GameEvent[] GetEventList()
        {
            int length = lstUsedEvents.Items.Count;
            GameEvent[] gameEvents = new GameEvent[length + 1];
            for (int i = 0; i < length; ++i)
            {
                gameEvents[i] = (lstUsedEvents.Items[i] as GameEvent);
            }
            gameEvents[length] = new EmptyEvent();

            return gameEvents;
        }

        private void btnAddEvent_Click(object sender, EventArgs e)
        {
            lstUsedEvents.BeginUpdate();

            foreach (object item in lstAvailEvents.SelectedItems)
            {
                lstUsedEvents.Items.Add(item);
            }
            
            lstUsedEvents.EndUpdate();
            eventsChanged = true;
        }

        private void btnRemoveEvent_Click(object sender, EventArgs e)
        {
            lstUsedEvents.BeginUpdate();

            while (lstUsedEvents.SelectedIndices.Count > 0)
            {
                lstUsedEvents.Items.RemoveAt(lstUsedEvents.SelectedIndices[0]);
            }

            lstUsedEvents.EndUpdate();
            eventsChanged = true;
        }

        private void btnAllEvents_Click(object sender, EventArgs e)
        {
            lstUsedEvents.BeginUpdate();
            lstAvailEvents.BeginUpdate();
            lstAvailEvents.SelectedIndices.Clear();

            lstUsedEvents.Items.AddRange(lstAvailEvents.Items);

            lstAvailEvents.EndUpdate();
            lstUsedEvents.EndUpdate();
            eventsChanged = true;
        }

        private void btnNoEvents_Click(object sender, EventArgs e)
        {
            lstUsedEvents.BeginUpdate();
            lstUsedEvents.Items.Clear();
            lstUsedEvents.EndUpdate();
        }


        private void btnUp_Click(object sender, EventArgs e)
        {
            foreach (int index in lstUsedEvents.SelectedIndices)
            {
                if (index != 0)
                {
                    object item = lstUsedEvents.Items[index];
                    lstUsedEvents.Items.RemoveAt(index);
                    lstUsedEvents.Items.Insert(index - 1, item);
                    lstUsedEvents.SelectedIndices.Add(index - 1);
                }
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            foreach (int index in lstUsedEvents.SelectedIndices)
            {
                if (index != lstUsedEvents.Items.Count - 1)
                {
                    object item = lstUsedEvents.Items[index];
                    lstUsedEvents.Items.RemoveAt(index);
                    lstUsedEvents.Items.Insert(index + 1, item);
                    lstUsedEvents.SelectedIndices.Add(index + 1);
                }
            }
        }

        private void chkUpdateGameTime_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGameTime = chkUpdateGameTime.Checked;
        }

        private void settings_HandleDestroyed(object sender, EventArgs e)
        {
            if (eventsChanged)
            {
                eventsChanged = false;
                OnChanged(EventArgs.Empty);
            }
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            XmlElement settingsNode = document.CreateElement("settings");

            XmlElement usedEventsNode = document.CreateElement("usedEvents");
            XmlElement eventNode;
            foreach (GameEvent gameEvent in lstUsedEvents.Items)
            {
                eventNode = document.CreateElement("event");
                eventNode.InnerText = gameEvent.Id;
                usedEventsNode.AppendChild(eventNode);
            }
            settingsNode.AppendChild(usedEventsNode);

            XmlElement pauseGameTimeNode = document.CreateElement("updateGameTime");
            pauseGameTimeNode.InnerText = UpdateGameTime.ToString();
            settingsNode.AppendChild(pauseGameTimeNode);

            return settingsNode;
        }

        public void SetSettings(XmlNode settings)
        {
            if (settings["usedEvents"] != null)
            {
                lstUsedEvents.BeginUpdate();
                foreach (XmlNode node in settings["usedEvents"].ChildNodes)
                {
                    if (events.ContainsKey(node.InnerText))
                    {
                        lstUsedEvents.Items.Add(events[node.InnerText]);
                    }
                }
                lstUsedEvents.EndUpdate();

                eventsChanged = false;
                OnChanged(EventArgs.Empty);
            }

            bool updateGameTime;
            if (settings["updateGameTime"] != null && Boolean.TryParse(settings["updateGameTime"].InnerText, out updateGameTime))
            {
                UpdateGameTime = updateGameTime;
                chkUpdateGameTime.Checked = UpdateGameTime;
            }
        }
    }
}
