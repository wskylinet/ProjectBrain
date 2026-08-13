namespace ProjectBrain.Api.Security;

public static class SecurityEventCodes
{
    public const string InvalidCredentials = "InvalidCredentials";
    public const string AccountTemporarilyLocked = "AccountTemporarilyLocked";
    public const string IpRateLimited = "IpRateLimited";

    public const string HttpContextItemKey = "ProjectBrain.SecurityEventCode";
}
