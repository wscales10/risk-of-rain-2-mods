using Newtonsoft.Json;
using Utils;

namespace Minecraft
{
	public class Screen : IScreen
	{
		[JsonProperty]
		public string? FullName { get; private set; }

		[JsonProperty]
		public string? SimpleName { get; private set; }

		string? IScreen.Name => SimpleName;

		public override string ToString() => HelperMethods.GetNullSafeString(((IScreen)this).Name);
	}
}