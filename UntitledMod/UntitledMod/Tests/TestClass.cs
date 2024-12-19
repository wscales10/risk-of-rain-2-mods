using Moq.AutoMock;

namespace UntitledMod.Tests
{
    internal class TestClass
    {
        public ICustomLogger Logger { get; set; }

        protected virtual AutoMocker GetMocker()
        {
            var mocker = new AutoMocker();
            mocker.Use(this.Logger);
            return mocker;
        }
    }
}