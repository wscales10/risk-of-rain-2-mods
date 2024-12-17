namespace UntitledMod
{
    public partial class UntitledMod
    {
        public UntitledMod(Writer writer, Reader reader)
        {
            On.RoR2.ItemCatalog.Init += this.ItemCatalog_Init;
            this.Writer = writer;
            this.Reader = reader;
        }

        internal Writer Writer { get; }

        internal Reader Reader { get; }

        private void ItemCatalog_Init(On.RoR2.ItemCatalog.orig_Init orig)
        {
            orig();
            InventoryManager.Init();
        }
    }
}