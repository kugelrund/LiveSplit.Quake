using System;
using System.Reflection;
using LiveSplit.ComponentAutosplitter;
using LiveSplit.Model;
using LiveSplit.UI.Components;

namespace LiveSplit.Quake
{
    class Factory : IComponentFactory
    {
        private QuakeGame game = new QuakeGame();

        public string ComponentName => game.Name + "Auto Splitter";
        public string Description => "Automates splitting for " + game.Name + " and supports game time.";
        public ComponentCategory Category => ComponentCategory.Control;

        public string UpdateName => ComponentName;
        public string UpdateURL => "https://raw.githubusercontent.com/kugelrund/LiveSplit.Quake/master/";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string XMLURL => UpdateURL + "Components/update.LiveSplit.Quake.xml";

        public IComponent Create(LiveSplitState state)
        {
            return new Component(game, state);
        }
    }
}
