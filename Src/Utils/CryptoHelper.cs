namespace SinaMN75U.Utils;

public static class SimpleCrypto
{
    private static readonly byte[] Key = Convert.FromBase64String("dGhpcy1pcy1hLXRlc3Qta2V5LWZvci1kZXZlbG9wbWVudC1vbmx5LTI1Ng==");
    
    public static string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return plaintext;
        
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] nonce = new byte[12];
        byte[] ciphertext = new byte[plaintextBytes.Length];
        byte[] tag = new byte[16];
        
        RandomNumberGenerator.Fill(nonce);
        
        using AesGcm aes = new(Key, 16);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
        
        byte[] result = new byte[nonce.Length + ciphertext.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length + ciphertext.Length, tag.Length);
        
        return Convert.ToBase64String(result);
    }
    
    public static string Decrypt(string ciphertextBase64)
    {
        if (string.IsNullOrEmpty(ciphertextBase64)) return ciphertextBase64;
        
        byte[] fullData = Convert.FromBase64String(ciphertextBase64);
        
        byte[] nonce = new byte[12];
        byte[] tag = new byte[16];
        byte[] ciphertext = new byte[fullData.Length - 28];
        
        Buffer.BlockCopy(fullData, 0, nonce, 0, 12);
        Buffer.BlockCopy(fullData, 12, ciphertext, 0, ciphertext.Length);
        Buffer.BlockCopy(fullData, 12 + ciphertext.Length, tag, 0, 16);
        
        byte[] plaintextBytes = new byte[ciphertext.Length];
        
        using AesGcm aes = new(Key, 16);
        aes.Decrypt(nonce, ciphertext, tag, plaintextBytes);
        
        return Encoding.UTF8.GetString(plaintextBytes);
    }
}