using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RFI.MicroserviceFramework._Cryptography
{
    public static class Rsa
    {
        public static string Sign(string data, string privateKey)
        {
            using var privateKeyRsaProvider = CreateRsaProviderFromPrivateKey(privateKey);
            var signatureBytes = privateKeyRsaProvider.SignData(Encoding.UTF8.GetBytes(data), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signatureBytes);
        }

        public static bool Verify(string data, string sign, string keyPublicString)
        {
            using var publicKeyRsaProvider = CreateRsaProviderFromPublicKey(keyPublicString);
            return publicKeyRsaProvider.VerifyData(Encoding.UTF8.GetBytes(data), Convert.FromBase64String(sign), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }


        public static string Decrypt(string cipherText, string privateKey)
        {
            using var privateKeyRsaProvider = CreateRsaProviderFromPrivateKey(privateKey);
            var decryptedBytes = privateKeyRsaProvider.Decrypt(Convert.FromBase64String(cipherText), RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public static string Encrypt(string text, string keyPublic)
        {
            using var publicKeyRsaProvider = CreateRsaProviderFromPublicKey(keyPublic);
            return Convert.ToBase64String(publicKeyRsaProvider.Encrypt(Encoding.UTF8.GetBytes(text), RSAEncryptionPadding.Pkcs1));
        }


        private static RSA CreateRsaProviderFromPrivateKey(string privateKey)
        {
            var privateKeyBits = Convert.FromBase64String(privateKey);

            var rsa = RSA.Create();
            var rsaKeyInfo = new RSAParameters();

            using(var binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                var twobytes = binr.ReadUInt16();
                if(twobytes == 0x8130) binr.ReadByte();
                else if(twobytes == 0x8230) binr.ReadInt16();
                else throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if(twobytes != 0x0102) throw new Exception("Unexpected version");

                var bt = binr.ReadByte();
                if(bt != 0x00) throw new Exception("Unexpected value read binr.ReadByte()");

                rsaKeyInfo.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                rsaKeyInfo.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                rsaKeyInfo.D = binr.ReadBytes(GetIntegerSize(binr));
                rsaKeyInfo.P = binr.ReadBytes(GetIntegerSize(binr));
                rsaKeyInfo.Q = binr.ReadBytes(GetIntegerSize(binr));
                rsaKeyInfo.DP = binr.ReadBytes(GetIntegerSize(binr));
                rsaKeyInfo.DQ = binr.ReadBytes(GetIntegerSize(binr));
                rsaKeyInfo.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            rsa.ImportParameters(rsaKeyInfo);

            return rsa;
        }

        private static RSA CreateRsaProviderFromPublicKey(string publicKeyString)
        {
            var rsaKeyInfo = new RSAParameters();

            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] seqOid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };

            var x509Key = Convert.FromBase64String(publicKeyString);

            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            using var mem = new MemoryStream(x509Key);
            using var binr = new BinaryReader(mem);
            var twobytes = binr.ReadUInt16();
            if(twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte(); //advance 1 byte
            else if(twobytes == 0x8230) binr.ReadInt16(); //advance 2 bytes
            else return null;

            var seq = binr.ReadBytes(15);
            if(!CompareByteArrays(seq, seqOid)) return null; //make sure Sequence for OID is correct


            twobytes = binr.ReadUInt16();
            if(twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                binr.ReadByte(); //advance 1 byte
            else if(twobytes == 0x8203) binr.ReadInt16(); //advance 2 bytes
            else return null;

            var bt = binr.ReadByte();
            if(bt != 0x00) return null; //expect null byte next


            twobytes = binr.ReadUInt16();
            if(twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte(); //advance 1 byte
            else if(twobytes == 0x8230) binr.ReadInt16(); //advance 2 bytes
            else return null;

            twobytes = binr.ReadUInt16();
            byte lowbyte;
            byte highbyte = 0x00;

            if(twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
            {
                lowbyte = binr.ReadByte(); // read next bytes which is bytes in modulus
            }
            else if(twobytes == 0x8202)
            {
                highbyte = binr.ReadByte(); //advance 2 bytes
                lowbyte = binr.ReadByte();
            }
            else
            {
                return null;
            }

            byte[] modint = { lowbyte, highbyte, 0x00, 0x00 }; //reverse byte order since asn.1 key uses big endian order
            var modsize = BitConverter.ToInt32(modint, 0);

            var firstbyte = binr.PeekChar();
            if(firstbyte == 0x00)
            {
                //if first byte (highest order) of modulus is zero, don't include it
                binr.ReadByte(); //skip this null byte
                modsize -= 1; //reduce modulus buffer size by 1
            }

            var modulus = binr.ReadBytes(modsize); //read the modulus bytes

            if(binr.ReadByte() != 0x02) return null; //expect an Integer for the exponent data

            int expbytes = binr.ReadByte(); // should only need one byte for actual exponent data (for all useful values)
            var exponent = binr.ReadBytes(expbytes);

            // ------- create RSACryptoServiceProvider instance and initialize with public key -----
            var rsa = RSA.Create();
            rsaKeyInfo.Modulus = modulus;
            rsaKeyInfo.Exponent = exponent;

            rsa.ImportParameters(rsaKeyInfo);

            return rsa;
        }


        private static int GetIntegerSize(BinaryReader binr)
        {
            int count;
            var bt = binr.ReadByte();
            if(bt != 0x02) return 0;
            bt = binr.ReadByte();

            if(bt == 0x81)
            {
                count = binr.ReadByte();
            }
            else if(bt == 0x82)
            {
                var highbyte = binr.ReadByte();
                var lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while(binr.ReadByte() == 0x00) count -= 1;
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

        private static bool CompareByteArrays(IReadOnlyCollection<byte> a, IReadOnlyList<byte> b)
        {
            if(a.Count != b.Count) return false;
            var i = 0;
            foreach(var c in a)
            {
                if(c != b[i]) return false;
                i++;
            }

            return true;
        }
    }
}