using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NetStalkerAvalonia.Core.Helpers
{
	public class EncryptionHelper
	{
		public static byte[]? StringEncrypt(string stringToEncrypt, byte[] key = null, byte[] iv = null)
		{
			byte[]? encryptedText = null;
			try
			{
				using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(stringToEncrypt)))
				using (var outputStream = new MemoryStream())
				using (var aes = Aes.Create())
				{
					aes.Padding = PaddingMode.ISO10126;
					aes.KeySize = 256;

					using (var cryptoStream = new CryptoStream(outputStream,
							   aes.CreateEncryptor(key, iv),
							   CryptoStreamMode.Write))
					{
						var buffer = new byte[1024];
						var read = inputStream.Read(buffer, 0, buffer.Length);
						while (read > 0)
						{
							cryptoStream.Write(buffer, 0, read);
							read = inputStream.Read(buffer, 0, buffer.Length);
						}

						cryptoStream.FlushFinalBlock();
						encryptedText = outputStream.ToArray();
					}
				}
			}
			catch
			{
			}

			return encryptedText;
		}

		public static byte[]? StringDecrypt(byte[] bytesToDecrypt, byte[]? key = null, byte[]? iv = null)
		{
			byte[]? finalBytes = null;

			try
			{
				using (var input = new MemoryStream(bytesToDecrypt))
				using (var output = new MemoryStream())
				using (var aes = Aes.Create())
				{
					aes.Padding = PaddingMode.ISO10126;
					aes.KeySize = 256;

					using (var cryptoStream =
						   new CryptoStream(input, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
					{
						var buffer = new byte[1024];
						var read = cryptoStream.Read(buffer, 0, buffer.Length);
						while (read > 0)
						{
							output.Write(buffer, 0, read);
							read = cryptoStream.Read(buffer, 0, buffer.Length);
						}

						cryptoStream.Flush();
						finalBytes = output.ToArray();
					}
				}
			}
			catch
			{
			}

			return finalBytes;
		}
	}
}
