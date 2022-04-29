using System.Collections.Generic;

namespace Utils
{
	public interface ITreeItem
	{
		IEnumerable<(string, ITreeItem)> Children { get; }
	}

	public interface ITreeItem<T> where T : ITreeItem<T>
	{
		IEnumerable<(string, T)> Children { get; }
	}
}