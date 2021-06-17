namespace DapperDino.UMT.PlayerNames
{
    public struct PlayerData
    {
        public string PlayerName { get; private set; }

        public PlayerData(string playerName)
        {
            PlayerName = playerName;
        }
    }
}
