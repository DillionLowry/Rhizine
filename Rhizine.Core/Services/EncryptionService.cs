using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Rhizine.Core.Services
{
    public class EncryptionService
    {
        private readonly string _keyFilePath = "path_to_key_file";

        private byte[] GetEncryptionKey()
        {
            // Attempt to read the protected key from a file
            if (File.Exists(_keyFilePath))
            {
                var protectedKey = File.ReadAllBytes(_keyFilePath);
                return SecureKeyStorage.Unprotect(protectedKey);
            }

            // No existing key, create a new one, protect and store it
            var key = GenerateRandomKey();
            var protectedKeyToSave = SecureKeyStorage.Protect(key);
            File.WriteAllBytes(_keyFilePath, protectedKeyToSave);
            return key;
        }

        private byte[] GenerateRandomKey()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var randomKey = new byte[32]; // 256 bits for AES
                rng.GetBytes(randomKey);
                return randomKey;
            }
        }
    }
}
