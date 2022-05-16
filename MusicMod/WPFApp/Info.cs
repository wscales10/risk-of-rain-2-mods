using MyRoR2;
using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Mutable;
using Spotify;
using Spotify.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WPFApp
{
    internal static class Info
    {
        public static ObservableCollection<Playlist> Playlists { get; } = new();

        public static ReadOnlyCollection<PropertyInfo> ContextProperties { get; } = new(GetProperties<Context>().ToList());

        public static ReadOnlyCollection<Type> SupportedRuleTypes { get; } = new(new Type[] { typeof(StaticSwitchRule), typeof(ArrayRule), typeof(IfRule), typeof(Bucket) });

        public static ReadOnlyCollection<Type> SupportedCommandTypes { get; } = new(new Type[] { typeof(PlayCommand), typeof(StopCommand), typeof(TransferCommand), typeof(SeekToCommand), typeof(SetPlaybackOptionsCommand), typeof(LoopCommand), typeof(PlayOnceCommand) });

        public static Regex InvalidFileNameCharsRegex { get; } = new($"[{Regex.Escape(string.Concat(Path.GetInvalidFileNameChars()))}]+");

        public static PatternParser PatternParser { get; } = RoR2PatternParser.Instance;

        public static IEnumerable<PropertyInfo> GetProperties<T>() => GetProperties(typeof(T));

        public static IEnumerable<PropertyInfo> GetProperties(Type type) => type.GetProperties().Select(p => new PropertyInfo(p.Name, p.PropertyType));
    }
}