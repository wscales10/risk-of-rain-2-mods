using MyRoR2;
using Patterns;
using RuleExamples;
using Rules;
using Rules.RuleTypes.Mutable;
using Spotify;
using Spotify.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Utils;
using Utils.Reflection;
using Utils.Reflection.Properties;
using PropertyInfo = Patterns.Patterns.PropertyInfo;

namespace WPFApp
{
    internal static class Info
    {
        private static readonly ReadOnlyDictionary<(Type, Type), IRuleParser> ruleParsers = new(new Dictionary<(Type, Type), IRuleParser> {
            {(typeof(RoR2Context), typeof(string)), RuleParsers.RoR2ToString },
            {(typeof(string), typeof(ICommandList)), RuleParsers.StringToSpotify },
        });

        private static readonly Dictionary<Type, Type> interfaceToTypeDictionary = new();

        static Info()
        {
            RegisterTypePair<ICommandList, CommandList>();
        }

        public static ObservableCollection<Playlist> Playlists { get; } = new();

        public static Cache<Type, ReadOnlyCollection<PropertyInfo>> ContextProperties { get; } = new(t => new(new[] { new PropertyInfo("this", t) }.Concat(GetProperties(t)).ToList()));

        public static ReadOnlyCollection<Type> SupportedRuleTypes { get; } = new(new Type[] { typeof(StaticSwitchRule<,>), typeof(ArrayRule<,>), typeof(IfRule<,>), typeof(Bucket<,>) });

        public static ReadOnlyCollection<Type> SupportedCommandTypes { get; } = new(new Type[] { typeof(PlayCommand), typeof(StopCommand), typeof(TransferCommand), typeof(SeekToCommand), typeof(SetPlaybackOptionsCommand), typeof(LoopCommand), typeof(PlayOnceCommand), typeof(SkipCommand) });

        public static Regex InvalidFileNameCharsRegex { get; } = new($"[{Regex.Escape(string.Concat(Path.GetInvalidFileNameChars()))}]+");

        public static PatternParser PatternParser { get; private set; } = PatternParser.Instance;

        public static ReadOnlyCollection<(Type, Type)> TypePairs => ruleParsers.Keys.ToReadOnlyCollection();

        public static Type GetConcreteType<T>()
        {
            if (typeof(T).IsInterface || typeof(T).IsAbstract)
            {
                return interfaceToTypeDictionary[typeof(T)];
            }
            else
            {
                return typeof(T);
            }
        }

        public static T Instantiate<T>()
        {
            return (T)GetConcreteType<T>().ConstructDefault();
        }

        public static RuleParser<TContext, TOut> GetRuleParser<TContext, TOut>() => (RuleParser<TContext, TOut>)GetRuleParser(typeof(TContext), typeof(TOut));

        public static IRuleParser GetRuleParser(Type tContext, Type tOut) => ruleParsers[(tContext, tOut)];

        public static IEnumerable<PropertyInfo> GetProperties<T>() => GetProperties(typeof(T));

        public static IEnumerable<PropertyInfo> GetProperties(Type type) => type.GetPublicProperties().Where(pi => pi.GetMethod is not null).Select(p => new PropertyInfo(p.Name, p.PropertyType));

        // TODO: make this implementation safer and more elegant
        internal static void SetPatternParser<TContext>()
        {
            if (typeof(TContext) == typeof(RoR2Context))
            {
                PatternParser = RoR2PatternParser.Instance;
            }
            else
            {
                PatternParser = PatternParser.Instance;
            }
        }

        private static void RegisterTypePair<TAbstract, TConcrete>() where TConcrete : TAbstract, new()
        {
            interfaceToTypeDictionary.Add(typeof(TAbstract), typeof(TConcrete));
        }
    }
}