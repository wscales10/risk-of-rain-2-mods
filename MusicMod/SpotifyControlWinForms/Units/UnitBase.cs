namespace SpotifyControlWinForms.Units
{
	public abstract class UnitBase
	{
		private bool isEnabled;

		protected UnitBase(string name) => Name = name;

		public event PropertyChangedEventHandler<bool>? IsEnabledChanged;

		public event PropertyChangingEventHandler<bool>? IsEnabledChanging;

		public event Func<UnitBase, bool>? CanToggleIsEnabledEvent;

		public event Action? IsEnabledTogglabilityUpdated;

		public bool IsEnabled => isEnabled;

		public string Name { get; }

		public bool CanToggleIsEnabled => CanToggleIsEnabledEvent?.Invoke(this) ?? true;

		public void UpdateIsEnabledTogglability() => IsEnabledTogglabilityUpdated?.Invoke();

		public void SetIsEnabled(object source, bool value)
		{
			if (isEnabled != value && (IsEnabledChanging?.Invoke(value) ?? true))
			{
				isEnabled = value;
				IsEnabledChanged?.Invoke(source, value);
			}
		}

		internal abstract void SetRule(string? location);
	}
}