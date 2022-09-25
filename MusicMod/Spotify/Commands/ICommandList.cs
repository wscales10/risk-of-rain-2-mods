﻿using System.Collections.Generic;
using Utils;

namespace Spotify.Commands
{
    public interface ICommandList : IEnumerable<Command>, IXmlExportable
    {
    }
}