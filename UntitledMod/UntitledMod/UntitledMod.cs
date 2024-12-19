namespace UntitledMod
{
    public partial class UntitledMod
    {
        public UntitledMod(WriterHooks writer, ReaderHooks reader)
        {
            On.RoR2.ItemCatalog.Init += this.ItemCatalog_Init;
            this.Writer = writer;
            this.Reader = reader;
        }

        internal WriterHooks Writer { get; }

        internal ReaderHooks Reader { get; }

        private void ItemCatalog_Init(On.RoR2.ItemCatalog.orig_Init orig)
        {
            orig();
            InventoryManager.Init();
        }
    }
}