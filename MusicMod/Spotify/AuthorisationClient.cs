using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.Coroutines;
using Utils.Runners;

namespace Spotify
{
	public class AuthorisationClient : CoroutineRunner
	{
		private readonly IPC.Client ipcClient = new IPC.Client(5007);

		public AuthorisationClient()
		{
			ipcClient.HandleConnResponse += HandleResponse;
			ipcClient.ReceivedRequest += IpcClient_ReceivedRequest;
		}

		public PreferencesLite Preferences { get; } = new PreferencesLite();

		public void RequestConfigurationPage()
		{
			ipcClient.SendToServer(new IPC.Message("conf"));
		}

		public void RequestNewAccessToken()
		{
			var response = ipcClient.SendToServerAwaitResponse(new IPC.Message("toke"));
			HandleResponse(response.Messages);
		}

		protected override IEnumerable<ProgressUpdate> Start(Reference reference)
		{
			var run = ipcClient.TryStart.CreateRun();

			foreach (var progressUpdate in run.GetProgressUpdates())
			{
				yield return progressUpdate;
			}

			if (run.Result.Success)
			{
				reference.Complete();
			}
			else
			{
				reference.Fail();
			}
		}

		private IEnumerable<IPC.Message> IpcClient_ReceivedRequest(IEnumerable<IPC.Message> arg)
		{
			this.Log("Request Start");

			foreach (var request in arg)
			{
				this.Log($"Request Line: {request}");
				switch (request.Key)
				{
					case "toke":
						Preferences.AccessToken = request.Value;
						break;

					case "devi":
						Preferences.DefaultDeviceString = request.Value;
						break;

					default:
						throw new NotSupportedException();
				}
			}

			this.Log("Request End");

			yield break;
		}

		private void HandleResponse(IEnumerable<IPC.Message> obj)
		{
			_ = IpcClient_ReceivedRequest(obj).LastOrDefault();
		}
	}
}