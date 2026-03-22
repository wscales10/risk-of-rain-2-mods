using System.Collections;

namespace PactOfPunishment.Waves.Stage3
{
    public interface IFistsStrategy
    {
        IEnumerator PlaceFists(PlaceFistsArgs args);
    }
}