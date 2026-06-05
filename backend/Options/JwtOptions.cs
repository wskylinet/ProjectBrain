namespace ProjectBrain.Api.Options;

/// <summary>
/// JWT 相关配置，对应 appsettings.json 中的 "Jwt" 节点。
/// </summary>
public class JwtOptions
{
    public string Issuer { get; set; } = "ProjectBrain";

    public string Audience { get; set; } = "ProjectBrain.Client";

    /// <summary>签名密钥，生产环境务必使用足够长且保密的随机值。</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>令牌有效期（分钟）。</summary>
    public int ExpireMinutes { get; set; } = 480;
}
