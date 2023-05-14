namespace SpotifyControlWinForms
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();
			SpotifyControl spotifyControl = new();
			Form2 form = new(spotifyControl);
			spotifyControl.Init();
			Application.Run(form);
		}
	}
}