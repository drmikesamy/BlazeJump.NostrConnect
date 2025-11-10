using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace BlazeJump.Helpers
{
	/// <summary>
	/// Extension methods for common operations including cloning, hashing, and padding.
	/// </summary>
	public static class ExtensionMethods
	{
		/// <summary>
		/// Creates a deep clone of an object using JSON serialization.
		/// </summary>
		/// <typeparam name="T">The type of object to clone.</typeparam>
		/// <param name="source">The source object to clone.</param>
		/// <returns>A deep copy of the source object.</returns>
		public static T? Clone<T>(this T source)
		{
			var serialized = JsonConvert.SerializeObject(source);
			return JsonConvert.DeserializeObject<T>(serialized);
		}
		
		/// <summary>
		/// Converts a byte array to a hexadecimal string.
		/// </summary>
		/// <param name="inputBytes">The byte array to convert.</param>
		/// <returns>A lowercase hexadecimal string representation.</returns>
		public static string ToHashString(this byte[] inputBytes)
		{
				return BitConverter.ToString(inputBytes).Replace("-", "").ToLower();
		}
		
		/// <summary>
		/// Computes the SHA256 hash of a string.
		/// </summary>
		/// <param name="inputString">The string to hash.</param>
		/// <returns>A byte array containing the SHA256 hash.</returns>
		public static byte[] SHA256Hash(this string inputString)
		{
			using (SHA256 sha256 = SHA256.Create())
			{
				return sha256.ComputeHash(Encoding.UTF8.GetBytes(inputString));
			}
		}
		
		/// <summary>
		/// Pads a byte array using PKCS7 padding to a multiple of 16 bytes.
		/// </summary>
		/// <param name="data">The data to pad.</param>
		/// <returns>The padded byte array.</returns>
		public static byte[] Pad(this byte[] data)
		{
			// Get the number of bytes needed to pad
			int padLength = 16 - (data.Length % 16);

			// Create a new array with the padded length
			byte[] paddedData = new byte[data.Length + padLength];

			// Copy the original data to the new array
			Array.Copy(data, paddedData, data.Length);

			// Fill the remaining bytes with the pad value
			for (int i = data.Length; i < paddedData.Length; i++)
			{
				paddedData[i] = (byte)padLength;
			}

			// Return the padded array
			return paddedData;
		}

		/// <summary>
		/// Removes PKCS7 padding from a byte array.
		/// </summary>
		/// <param name="data">The padded data.</param>
		/// <returns>The unpadded byte array.</returns>
		public static byte[] Unpad(this byte[] data)
		{
			// Get the pad value from the last byte
			int padLength = data[data.Length - 1];

			// Create a new array with the unpadded length
			byte[] unpaddedData = new byte[data.Length - padLength];

			// Copy the original data without the padding to the new array
			Array.Copy(data, unpaddedData, unpaddedData.Length);

			// Return the unpadded array
			return unpaddedData;
		}
	}
}
