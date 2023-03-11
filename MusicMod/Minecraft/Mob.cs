namespace Minecraft
{
	public class Mob
	{
		internal Mob(string id) => Id = id;

		public string Id { get; }

		public override string ToString() => Id;
	}
}