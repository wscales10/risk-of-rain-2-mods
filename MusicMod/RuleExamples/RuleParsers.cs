using Minecraft;
using MyRoR2;
using Patterns;
using Rules;
using Spotify.Commands;
using System.Xml.Linq;

namespace RuleExamples
{
    public static class RuleParsers
    {
        // TODO: move elsewhere
        public static RuleParser<RoR2Context, string> RoR2ToString { get; } = new RuleParser<RoR2Context, string>(RoR2PatternParser.Instance, s => s);

        public static RuleParser<string, ICommandList> StringToSpotify { get; } = new RuleParser<string, ICommandList>(PatternParser.Instance, s =>
        {
            if (s == "")
            {
                return null;
            }

            return CommandList.Parse(XElement.Parse(s));
        });

        public static RuleParser<MinecraftContext, string> MinecraftToString { get; } = new RuleParser<MinecraftContext, string>(MinecraftPatternParser.Instance, s => s);
    }
}