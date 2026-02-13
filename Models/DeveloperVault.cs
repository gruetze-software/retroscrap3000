using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using RetroScrap3000.Models;

namespace RetroScrap3000.Helpers;

public sealed class DeveloperVault
{
	private const char Separator = '|';
    private const string _file = "retroscrap.bin";
    private const string CryptoKeyHex = "71377235643368326B396D3667317434663878306A3370356332763962317A36";
    private const string CryptoIvHex = "613663396531663468376A306B336D35";

    private static byte[] HexToBytes(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return Array.Empty<byte>();
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return bytes;
    }

    public bool TryLoad(out string? devId, out string? devPwd)
    {
        devId = null;
        devPwd = null;
        // Wir suchen erst im Installationsverzeichnis (Default-Keys)
        string filepath = Path.Combine(AppContext.BaseDirectory, "Config", _file);
    
        // Falls nicht da, schauen wir im User-Verzeichnis (falls der User eigene Keys hat)
        if (!File.Exists(filepath))
        {
            filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RetroScrap3000", "Config", _file);
        }
        
        if (!File.Exists(filepath)) return false;

        try
        {
            var blob = File.ReadAllBytes(filepath);
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = HexToBytes(CryptoKeyHex);
            aesAlg.IV = HexToBytes(CryptoIvHex);

            using ICryptoTransform decryptor = aesAlg.CreateDecryptor();
            using MemoryStream msDecrypt = new MemoryStream(blob);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt);
            
            string combinedString = srDecrypt.ReadToEnd();
            string[] parts = combinedString.Split(Separator);

            if (parts.Length == 2)
            {
                devId = parts[0];
                devPwd = parts[1];
                return true;
            }
            return false;
        }
        catch (Exception ex) 
        {   
            Trace.WriteLine($"Exception DeveloperVault Load: {Tools.GetExcMsg(ex)}");
            return false; 
        }
    }
}