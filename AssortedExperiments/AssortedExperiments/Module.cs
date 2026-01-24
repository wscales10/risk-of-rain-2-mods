using BepInEx.Logging;

namespace AssortedExperiments
{
    public abstract class Module
    {
        public ManualLogSource Logger { get; set; } // TODO: there's probably a better way to set these, but this is the easiest to get up and running

        public Settings Settings { get; set; }

        public abstract void Init();
    }
}