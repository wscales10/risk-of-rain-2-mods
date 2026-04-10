namespace PactOfPunishment.RebirthPlus
{
    public class LevelInfo : ILevelInfo
    {
        internal int index;

        public LevelInfo(params PickupInfo[] options)
        {
            this.Options = options;
        }

        public PickupInfo[] Options { get; }

        int ILevelInfo.Index => this.index;
    }
}