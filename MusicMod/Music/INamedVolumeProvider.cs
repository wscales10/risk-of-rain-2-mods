namespace Music
{
    public interface INamedVolumeProvider : IVolumeProvider
    {
        string Name { get; }
    }
}