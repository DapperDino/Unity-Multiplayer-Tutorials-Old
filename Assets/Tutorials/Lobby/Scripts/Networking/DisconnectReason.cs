namespace DapperDino.UMT.Lobby.Networking
{
    public class DisconnectReason
    {
        public ConnectStatus Reason { get; private set; } = ConnectStatus.Undefined;

        public void SetDisconnectReason(ConnectStatus reason)
        {
            Reason = reason;
        }

        public void Clear()
        {
            Reason = ConnectStatus.Undefined;
        }

        public bool HasTransitionReason => Reason != ConnectStatus.Undefined;
    }
}
