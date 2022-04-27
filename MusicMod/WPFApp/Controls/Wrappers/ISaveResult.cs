using System;
using System.Collections.Generic;

namespace WPFApp.Controls.Wrappers
{
	public interface ISaveResult
	{
		bool IsSuccess { get; }

		Queue<Action> ReleaseActions { get; }
	}

	public interface ISaveResult<out T> : ISaveResult
	{
		T Value { get; }
	}
}