using System;
using System.IO;
using System.Security.Cryptography;
using RFI.MicroserviceFramework._Helpers;

// ReSharper disable All

namespace RFI.MicroserviceFramework._Cryptography
{
    public class HTripleDesParameters
    {
        public string Key { get; set; }
        public string Vector { get; set; }
    }

    public static class TripleDes
    {
        public static string Encrypt(string plainText, out string desParameters)
        {
            using var tripleDesCryptoServiceProvider = new TripleDESCryptoServiceProvider();
            using var encryptor = tripleDesCryptoServiceProvider.CreateEncryptor(tripleDesCryptoServiceProvider.Key, tripleDesCryptoServiceProvider.IV);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            using var streamWriter = new StreamWriter(cryptoStream);
            streamWriter.Write(plainText);
            streamWriter.Close();
            desParameters = new HTripleDesParameters { Key = Convert.ToBase64String(tripleDesCryptoServiceProvider.Key), Vector = Convert.ToBase64String(tripleDesCryptoServiceProvider.IV) }.JsonSerialize();
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public static string Decrypt(string cipherText, string desDecrypted)
        {
            var desParameters = desDecrypted.JsonDeserialize<HTripleDesParameters>();
            if(desParameters.Key.IsNull() || desParameters.Vector.IsNull()) throw new Exception("des parameters error");

            using var tripleDesCryptoServiceProvider = new TripleDESCryptoServiceProvider();
            using var decryptor = tripleDesCryptoServiceProvider.CreateDecryptor(Convert.FromBase64String(desParameters.Key), Convert.FromBase64String(desParameters.Vector));
            using var memoryStream = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }
    }
}