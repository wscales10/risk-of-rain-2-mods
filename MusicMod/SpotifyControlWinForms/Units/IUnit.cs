namespace SpotifyControlWinForms.Units
{
	public interface IUnit
	{
		bool IsEnabled { get; }
		string Name { get; }
		bool CanToggleIsEnabled { get; }

		event PropertyChangedEventHandler<bool>? IsEnabledChanged;
		event PropertyChangingEventHandler<bool>? IsEnabledChanging;
		event Func<UnitBase, bool>? CanToggleIsEnabledEvent;
		event Action? IsEnabledTogglabilityUpdated;

		void SetIsEnabled(object source, bool value);
		void UpdateIsEnabledTogglability();
	}
}