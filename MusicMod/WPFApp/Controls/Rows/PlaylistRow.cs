using Spotify;
using System.Windows;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Rows
{
	internal class PlaylistRow : Row<Playlist, PlaylistRow>
	{
		private readonly NavigationContext navigationContext;

		public PlaylistRow(NavigationContext navigationContext) : base(true)
		{
			this.navigationContext = navigationContext;
			ButtonOutputUi = new(
				navigationContext,
				RefreshOutputUi,
				this,
				new(
					() => Output?.Name ?? "Untitled playlist",
					new DependencyInformation(
						ButtonOutputUi.OutputViewModel, nameof(PlaylistViewModel.Name))));
		}

		protected ButtonOutputUi<Playlist> ButtonOutputUi { get; }

		protected override Playlist CloneOutput() => Output.DeepClone();

		protected override PlaylistRow deepClone() => new(navigationContext);

		protected override UIElement MakeOutputUi() => ButtonOutputUi.MakeOutputUi();
	}
}