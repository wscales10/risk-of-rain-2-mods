using System.Collections.ObjectModel;
using Utils;

namespace SpotifyControlWinForms.Units
{
	public class UnitUpdateInfo<TOut>
	{
		public UnitUpdateInfo(TOut output, IEnumerable<string> changedPropertyNames)
		{
			Output = output;
			ChangedPropertyNames = changedPropertyNames.ToReadOnlyCollection();
		}

		public TOut Output { get; }

		public ReadOnlyCollection<string> ChangedPropertyNames { get; }
	}
}