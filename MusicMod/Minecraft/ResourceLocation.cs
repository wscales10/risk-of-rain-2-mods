namespace Minecraft
{
	public class ResourceLocation
	{
		public ResourceLocation(string? @namespace, string? path)
		{
			Namespace = @namespace;
			Path = path;
		}

		public string? Namespace { get; }

		public string? Path { get; }

		public override string ToString() => AsString();

		public string AsString() => $"{Namespace}.{Path}";
	}
}