using Utils;

namespace MyRoR2
{
    public class MyScene
    {
        public MyScene(string name) => Name = name;

        public string Name { get; }

        public override string ToString() => HelperMethods.GetNullSafeString(Name);
    }
}