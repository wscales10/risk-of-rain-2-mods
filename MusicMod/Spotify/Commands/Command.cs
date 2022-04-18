﻿using MathNet.Symbolics;
using System;
using System.Linq;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands
{
#warning Command property set accessors are public to allow building, but no readonly versions defined

	public abstract class Command
	{
		private static readonly Type[] types = new Type[] { typeof(PauseCommand), typeof(PlayCommand), typeof(ResumeCommand), typeof(SeekToCommand), typeof(SetPlaybackOptionsCommand), typeof(StopCommand), typeof(TransferCommand) };

		protected Command()
		{ }

		public static Command FromXml(XElement element)
		{
			var type = types.First(x => x.Name == element.Name);
			var constructor = type.GetAnyConstructor(typeof(XElement));
			var args = constructor is null ? new object[] { } : new[] { element };
			constructor = constructor ?? type.GetAnyConstructor();
			return (Command)constructor.Invoke(args);
		}

		public CommandList Then(params Command[] commands)
		{
			return new CommandList(new[] { this }.Concat(commands));
		}

		public XElement ToXml()
		{
			var element = new XElement(GetType().Name);
			AddDetail(element);
			element.Name = GetType().Name;
			return element;
		}

		public override string ToString() => GetType().Name.Replace(nameof(Command), string.Empty);

		protected virtual void AddDetail(XElement element)
		{ }
	}
}