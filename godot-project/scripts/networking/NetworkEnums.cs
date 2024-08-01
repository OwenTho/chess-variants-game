
public static class SendingTransmissions
{
    public const int Create = 1;
    public const int Join = 2;
}

public static class ReceivingTransmissions
{
    public const int LobbyCreated = 1;
    public const int LobbyPort = 2;
    public const int Failed = 4;
    public const int InvalidCode = 8;
}