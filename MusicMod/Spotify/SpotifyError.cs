using System;

namespace Spotify
{
	public struct SpotifyError : IEquatable<SpotifyError>
	{
		public SpotifyError(Type exceptionType, ErrorType? subtype = null) : this()
		{
			ExceptionType = exceptionType;
			Subtype = subtype;
		}

		public Type ExceptionType { get; }

		public ErrorType? Subtype { get; }

		public static bool operator ==(SpotifyError err1, SpotifyError err2) => err1.Equals(err2);

		public static bool operator !=(SpotifyError err1, SpotifyError err2) => !err1.Equals(err2);

		public override bool Equals(object obj) => obj is SpotifyError err && Equals(err);

		public bool Equals(SpotifyError err) => ExceptionType == err.ExceptionType && Subtype == err.Subtype;

		public override int GetHashCode() => ExceptionType.GetHashCode();

		public SpotifyError With(ErrorType? subtype)
		{
			if (Subtype is null)
			{
				return new SpotifyError(ExceptionType, subtype);
			}
			else
			{
				throw new InvalidOperationException();
			}
		}
	}
}