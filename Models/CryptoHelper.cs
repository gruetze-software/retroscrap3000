using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using RetroScrap3000.Models;

namespace RetroScrap3000.Helpers;

public static class CryptoHelper
{
    // Der Key muss 32 Zeichen (256-bit) und der IV 16 Zeichen (128-bit) lang sein.
    private static readonly byte[] Key = SHA256.HashData(Encoding.UTF8.GetBytes("R3tr0Scrap3000_Grütze709176_Key!")); 
    private static readonly byte[] IV = { 0x42, 0x64, 0x12, 0x09, 0x22, 0x11, 0x00, 0x07, 0x55, 0x44, 0x33, 0x22, 0x11, 0x00, 0xFF, 0xEE }; // 16 Bytes fest

    public static string Encrypt(string clearText)
    {
        if (string.IsNullOrEmpty(clearText)) 
            return string.Empty;

        try
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter sw = new(cs))
                {
                    sw.Write(clearText);
                }
            }

            return Convert.ToBase64String(ms.ToArray());
        }
        catch ( Exception ex)
        {
            Trace.WriteLine($"Exception Encrypt: {Tools.GetExcMsg(ex)}");
            return string.Empty;
        }
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        try
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream ms = new(Convert.FromBase64String(cipherText));
            using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new(cs);
            
            return sr.ReadToEnd();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Exception Decrypt: {Tools.GetExcMsg(ex)}");
            return string.Empty;
        }
    }
}