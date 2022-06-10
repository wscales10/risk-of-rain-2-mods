using Spotify.Commands.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Utils.Reflection;

namespace Spotify.Commands.Mutable
{
#warning Command property set accessors are public to allow building, but no readonly versions defined

    public abstract class Command : ICommand<Command>
    {
        private static readonly Type[] types = new Type[] { typeof(PauseCommand), typeof(PlayCommand), typeof(ResumeCommand), typeof(SeekToCommand), typeof(SetPlaybackOptionsCommand), typeof(StopCommand), typeof(TransferCommand), typeof(PlayOnceCommand), typeof(LoopCommand) };

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

        public CommandList Then(params Command[] commands) => new CommandList(new[] { this }.Concat(commands));

        public XElement ToXml()
        {
            var element = new XElement(GetType().Name);
            AddDetail(element);
            element.Name = GetType().Name;
            return element;
        }

        public override string ToString() => GetType().Name.Replace(nameof(Command), string.Empty);

        IEnumerable<Command> ICommand<Command>.Then(params Command[] commands) => Then(commands);

        public abstract IReadOnlyCommand ToReadOnly();

        protected virtual void AddDetail(XElement element)
        { }
    }
}