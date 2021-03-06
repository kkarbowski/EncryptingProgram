﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Decryption
    {
        public static byte[] Decrypt(byte[] encryptedData, byte[] key, byte[] iv, CipherMode aesType)
        {
            byte[] decryptedData;
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;
                    aesAlg.Mode = aesType;
                    aesAlg.Padding = PaddingMode.Zeros;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (var msDecrypt = new MemoryStream(encryptedData))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            decryptedData = new byte[encryptedData.Length];
                            csDecrypt.Read(decryptedData, 0, decryptedData.Length);
                        }
                    }
                }
                return decryptedData;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
