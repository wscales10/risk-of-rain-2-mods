namespace Spotify.Authorisation
{
	internal enum FlowState
	{
		Error = -1,
		None = 0,
		Login = 1,
		TokenRequest = 2,
		TokenGranted = 3,
		Dormant = 4,
		Refreshing = 5,
		TokenRefreshed = 6
	}

	internal enum ErrorState
	{
		None = 0,
		Connection = 1,
		UserDenied = 2,
		ApiDenied = 3,
		StateMismatch = 4
	}
}
