using System.Security.Cryptography;
using System.Text;

namespace ProjectBrain.Api.Security;

public sealed class AesSecretCipher : ISecretCipher
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private static readonly byte[] AssociatedData =
        Encoding.UTF8.GetBytes("ProjectBrain.ConnectionPassword.v1");

    private readonly byte[] _key;

    public AesSecretCipher(IConfiguration configuration)
    {
        var keyText = configuration["Encryption:MasterKey"]
            ?? throw new InvalidOperationException("缺少 Encryption:MasterKey 配置");

        try
        {
            _key = Convert.FromBase64String(keyText);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Encryption:MasterKey 必须是 Base64 字符串", ex);
        }

        if (_key.Length != 32)
        {
            throw new InvalidOperationException("Encryption:MasterKey 解码后必须是 32 字节");
        }
    }

    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return string.Empty;

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag, AssociatedData);

        return string.Join('.', "v1", Convert.ToBase64String(nonce),
            Convert.ToBase64String(tag), Convert.ToBase64String(ciphertext));
    }

    public string Decrypt(string encrypted)
    {
        if (string.IsNullOrEmpty(encrypted)) return string.Empty;

        var parts = encrypted.Split('.');
        if (parts.Length != 4 || parts[0] != "v1")
        {
            throw new CryptographicException("连接密码密文格式无效");
        }

        var nonce = Convert.FromBase64String(parts[1]);
        var tag = Convert.FromBase64String(parts[2]);
        var ciphertext = Convert.FromBase64String(parts[3]);
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext, AssociatedData);
        return Encoding.UTF8.GetString(plaintext);
    }
}
