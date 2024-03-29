﻿using System;
using System.Xml.Linq;

namespace Spotify.Commands
{
    public class LoopCommand : PlayCommandBase<LoopCommand>
    {
        public LoopCommand(SpotifyItemType type, string id) : base(type, id)
        {
        }

        public LoopCommand(ISpotifyItem item) : base(item)
        {
        }

        public LoopCommand()
        {
        }

        internal LoopCommand(XElement element) : base(element)
        {
        }

        private LoopCommand(ISpotifyItem item, TimeSpan? at) : base(item, at)
        {
        }

        public override LoopCommand AtMilliseconds(int ms) => new LoopCommand(Item, TimeSpan.FromMilliseconds(ms));

        public override LoopCommand AtSeconds(int s) => new LoopCommand(Item, TimeSpan.FromSeconds(s));
    }
}