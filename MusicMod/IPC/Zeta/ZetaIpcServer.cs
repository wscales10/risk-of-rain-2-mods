﻿using System;
using IpcServer = ZetaIpc.Runtime.Server.IpcServer;

namespace IPC.Zeta
{
	internal class ZetaIpcServer : IReceiver
	{
		private readonly IpcServer ipcServer = new IpcServer();

		public ZetaIpcServer()
		{
			ipcServer.ReceivedRequest += IpcServer_ReceivedRequest;
		}

		public event Func<string, string> ReceivedRequest;

		public int? Port
		{
			get
			{
				var port = ipcServer.Port;
				return port == 0 ? null : (int?)port;
			}
		}

		public void Start(int port) => ipcServer.Start(port);

		private void IpcServer_ReceivedRequest(object sender, ZetaIpc.Runtime.Server.ReceivedRequestEventArgs e)
		{
			e.Response = ReceivedRequest?.Invoke(e.Request);
			e.Handled = true;
		}
	}
}