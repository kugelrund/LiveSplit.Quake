using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Reflection;

namespace LiveSplit.Quake
{
    public class Factory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "Quake Auto Splitter"; }
        }
        public ComponentCategory Category
        {
            get { return ComponentCategory.Control; }
        }
        public string Description
        {
            get { return "Automates splitting for Quake and allows to remove loadtimes."; }
        }
        public IComponent Create(LiveSplitState state)
        {
            return new Component(state);
        }
        public string UpdateName
        {
            get { return ComponentName; }
        }
        public string UpdateURL
        {
            get { return "https://raw.githubusercontent.com/kugelrund/LiveSplit.Quake/master/"; }
        }
        public Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }
        public string XMLURL
        {
            get { return UpdateURL + "Components/update.LiveSplit.Quake.xml"; }
        }
    }
}