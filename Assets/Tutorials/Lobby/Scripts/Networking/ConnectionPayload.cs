using System;

namespace DapperDino.UMT.Lobby.Networking
{
    [Serializable]
    public class ConnectionPayload
    {
        public string clientGUID;
        public int clientScene = -1;
        public string playerName;
    }
}
