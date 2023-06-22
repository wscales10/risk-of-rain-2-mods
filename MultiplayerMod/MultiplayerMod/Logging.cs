using System;
using UnityEngine;

namespace MultiplayerMod
{
	internal static class Logging
	{
		public static void Record(object obj)
		{
			Debug.Log($"{DateTime.UtcNow} | {obj}");
		}
	}
}