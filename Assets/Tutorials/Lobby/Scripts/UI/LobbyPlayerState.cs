using System;
using Unity.Collections;
using Unity.Netcode;

namespace DapperDino.UMT.Lobby.UI
{
    public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
    {
        public ulong ClientId;
        public FixedString32Bytes PlayerName;
        public bool IsReady;

        public LobbyPlayerState(ulong clientId, FixedString32Bytes playerName, bool isReady)
        {
            ClientId = clientId;
            PlayerName = playerName;
            IsReady = isReady;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref IsReady);
        }

        public bool Equals(LobbyPlayerState other)
        {
            return ClientId == other.ClientId &&
                PlayerName.Equals(other.PlayerName) &&
                IsReady == other.IsReady;
        }
    }
}
