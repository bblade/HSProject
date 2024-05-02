using System.Security.Cryptography;

namespace HSProject.Api.Authentication;

public static class PasswordGenerator {
    private const string ValidChars =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_-+=<>?";

    public static string Generate(int length) {
        using var rng = RandomNumberGenerator.Create();

        byte[] data = new byte[length];
        rng.GetBytes(data);
        string password = "";
        for (int i = 0; i < length; i++)
        {
            password += ValidChars[data[i] % ValidChars.Length];
        }
        return password;
    }
}
