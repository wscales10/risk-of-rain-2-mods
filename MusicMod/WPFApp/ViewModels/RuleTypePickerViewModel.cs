using System;
using System.Collections.ObjectModel;

namespace WPFApp.ViewModels
{
	public class RuleTypePickerViewModel : ViewModelBase
	{
		private (Type, Type)? selectedTypePair;

		public RuleTypePickerViewModel(Action<bool> action)
		{
			OkCommand = new(_ => action(true), this, nameof(SelectedTypePair), obj => obj is not null);
			CancelCommand = new(_ => action(false));
		}

		public ReadOnlyCollection<(Type, Type)> TypePairs { get; } = new(new[]
		{
			(typeof(MyRoR2.Context), typeof(string)),
			(typeof(string), typeof(Spotify.Commands.ICommandList))
		});

		public (Type, Type)? SelectedTypePair
		{
			get => selectedTypePair;
			set => SetProperty(ref selectedTypePair, value);
		}

		public ButtonCommand OkCommand { get; }

		public ButtonCommand CancelCommand { get; }
	}
}