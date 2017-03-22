using System;
using System.Collections.Generic;
using System.Text;

namespace BrawlerServer.Server
{
    public interface ICommandHandler
    {
        void Init(Packet packet);
    }
}
