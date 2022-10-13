using System;
using System.Collections.Generic;
using Utils;

namespace Spotify
{
	public class AuthorisationClient : Runner
	{
		private readonly IPC.Client ipcClient = new IPC.Client(5007);

		public AuthorisationClient()
		{
			ipcClient.HandleConnResponse += IpcClient_HandleConnResponse;
			ipcClient.ReceivedRequest += IpcClient_ReceivedRequest;
		}

		public event Action<string> OnNewAccessToken;

		public PreferencesLite Preferences { get; } = new PreferencesLite();

		public void RequestConfigurationPage()
		{
			ipcClient.SendToServer("conf");
		}

		protected override void Start()
		{
			if (!ipcClient.TryStart())
			{
				throw new InvalidOperationException();
			}
		}

		private IEnumerable<string> IpcClient_ReceivedRequest(IEnumerable<string> arg)
		{
			foreach (var request in arg)
			{
				switch (request.Substring(0, 4))
				{
					case "toke":
						OnNewAccessToken?.Invoke(request.Substring(7));
						break;

					case "devi":
						Preferences.DefaultDeviceString = request.Substring(7);
						break;

					default:
						throw new NotSupportedException();
				}
			}

			yield break;
		}

		private void IpcClient_HandleConnResponse(IEnumerable<string> obj)
		{
			_ = IpcClient_ReceivedRequest(obj);
		}
	}
}