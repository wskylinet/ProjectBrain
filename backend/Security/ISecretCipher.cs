namespace ProjectBrain.Api.Security;

public interface ISecretCipher
{
    string Encrypt(string plaintext);

    string Decrypt(string encrypted);
}
