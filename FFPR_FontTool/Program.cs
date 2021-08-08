using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FFPR_FontTool
{
    class Program
    {
        const string password = "TKX73OHHK1qMonoICbpVT0hIDGe7SkW0";
        const string saltString = "71Ba2p0ULBGaE6oJ7TjCqwsls1jBKmRL";

        public static bool Decrypt(string inFile, string outFile)
        {
            try
            {
                byte[] cipherText = File.ReadAllBytes(inFile);
                byte[] salt = Encoding.UTF8.GetBytes(saltString);

                RijndaelManaged r = new RijndaelManaged();
                Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, salt);
                if (bytes != null)
                {
                    bytes.IterationCount = 10;
                    r.BlockSize = 256;
                    r.KeySize = 256;
                    r.Mode = CipherMode.CBC;

                    r.Key = bytes.GetBytes(32);
                    r.IV = bytes.GetBytes(32);
                    r.Padding = PaddingMode.Zeros;

                    MemoryStream decryptionStreamBacking = new MemoryStream();
                    CryptoStream decrypt = new CryptoStream(decryptionStreamBacking, r.CreateDecryptor(), CryptoStreamMode.Write);
                    decrypt.Write(cipherText, 0, cipherText.Length);
                    decrypt.Flush();

                    decryptionStreamBacking.Seek(0, SeekOrigin.Begin);

                    using (FileStream fs = new FileStream(outFile, FileMode.OpenOrCreate))
                    {
                        decryptionStreamBacking.CopyTo(fs);
                        fs.Flush();
                    }
                    decrypt.Close();
                }

            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static bool Encrypt(string inFile, string outFile)
        {
            try
            {
                List<byte> cipherText = new List<byte>(File.ReadAllBytes(inFile));

                // Pad with 32 bytes
                cipherText.AddRange(new byte[] {
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0 });

                byte[] salt = Encoding.UTF8.GetBytes(saltString);

                RijndaelManaged r = new RijndaelManaged();
                Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, salt);
                if (bytes != null)
                {
                    bytes.IterationCount = 10;
                    r.BlockSize = 256;
                    r.KeySize = 256;
                    r.Mode = CipherMode.CBC;

                    r.Key = bytes.GetBytes(32);
                    r.IV = bytes.GetBytes(32);
                    r.Padding = PaddingMode.Zeros;

                    MemoryStream encryptionStreamBacking = new MemoryStream();
                    CryptoStream encrypt = new CryptoStream(encryptionStreamBacking, r.CreateEncryptor(), CryptoStreamMode.Write);
                    encrypt.Write(cipherText.ToArray(), 0, cipherText.Count);
                    encrypt.Flush();

                    encryptionStreamBacking.Seek(0, SeekOrigin.Begin);

                    using (FileStream fs = new FileStream(outFile, FileMode.OpenOrCreate))
                    {
                        encryptionStreamBacking.CopyTo(fs);
                        fs.Flush();
                    }
                    encrypt.Close();
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        static void ShowUsage()
        {
            Console.WriteLine("Usage: FFPR_Tool.exe [-D|-E] <input file> <output file>");
            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            bool success = false;
            string inFile;
            string outFile;

            if (args.Length == 1 && args[0].ToLower() == "-h")
            {
                ShowUsage();
                return;
            }
            if (args.Length == 3)
            {
                inFile = @"" + args[1].Trim('\"');
                outFile = @"" + args[2].Trim('\"');

                switch (args[0].ToLower())
                {
                    case "-d":
                        success = Decrypt(inFile, outFile);
                        break;
                    case "-e":
                        success = Encrypt(inFile, outFile);
                        break;
                    default:
                        ShowUsage();
                        return;
                }
            }
            else
            {

                Console.Write("encrypt/decrypt? (E/D): ");
                var key = Console.ReadKey();

                Console.Write("Input file: ");
                inFile = @"" + Console.ReadLine().Trim('\"');

                Console.Write("Output file: ");
                outFile = @"" + Console.ReadLine().Trim('\"');

                Console.WriteLine();
                if (key.Key == ConsoleKey.E)
                {
                    success = Encrypt(inFile, outFile);
                }
                else if (key.Key == ConsoleKey.D)
                {
                    success = Decrypt(inFile, outFile);
                }
                else
                {
                    Console.WriteLine("Unrecognized response.");
                    Console.ReadKey();
                    return;
                }
            }

            if (success)
            {
                Console.WriteLine("Success.");
            }
            else
            {
                Console.WriteLine("Failed to process file.");
                Console.ReadKey();
            }

            return;
        }
    }
}
