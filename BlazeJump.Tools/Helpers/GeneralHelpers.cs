
using BlazeJump.Tools.Enums;
using Nano.Bech32;

namespace BlazeJump.Tools.Helpers
{
	/// <summary>
	/// General utility helper methods for date/time conversions and Bech32 encoding/decoding.
	/// </summary>
	public static class GeneralHelpers
	{
		/// <summary>
		/// Converts a Unix timestamp to a DateTime object.
		/// </summary>
		/// <param name="unixTimeStamp">The Unix timestamp in seconds.</param>
		/// <returns>A DateTime object representing the timestamp.</returns>
		public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
		{
			DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
			DateTime dateTime = dateTimeOffset.DateTime;
			return dateTime;
		}
		
		/// <summary>
		/// Converts a DateTime object to a Unix timestamp.
		/// </summary>
		/// <param name="dateTime">The DateTime to convert.</param>
		/// <returns>The Unix timestamp in seconds.</returns>
		public static long DateTimeToUnixTimeStamp(DateTime dateTime)
		{
			return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
		}
		
		/// <summary>
		/// Converts a hexadecimal string to a Bech32 encoded string.
		/// </summary>
		/// <param name="hexString">The hexadecimal string to convert.</param>
		/// <param name="bechLabel">The Bech32 prefix to use.</param>
		/// <returns>A Bech32 encoded string.</returns>
		public static string HexToBech32(string hexString, Bech32PrefixEnum bechLabel)
		{
			var bytes = HexStringToByteArray(hexString);
			return Bech32Encoder.Encode(bechLabel.ToString(), bytes);
		}

		/// <summary>
		/// Converts a Bech32 encoded string to a hexadecimal string.
		/// </summary>
		/// <param name="bech32string">The Bech32 string to decode.</param>
		/// <param name="bechLabel">The expected Bech32 prefix.</param>
		/// <returns>A hexadecimal string representation of the decoded data.</returns>
		public static string Bech32ToHex(string bech32string, Bech32PrefixEnum bechLabel)
		{
			if (bech32string == null || !bech32string.Contains(bechLabel.ToString()))
			{
				return bech32string ?? string.Empty;
			}
			Bech32Encoder.Decode(bech32string, out var hrp, out var bytes);
			if (hrp != bechLabel.ToString() || bytes == null) throw new Exception("Invalid npub string");
			return ByteArrayToHexString(bytes);
		}

		/// <summary>
		/// Decodes a Bech32 string and extracts TLV (Type-Length-Value) components.
		/// </summary>
		/// <param name="bech32string">The Bech32 string to decode.</param>
		/// <param name="bechLabel">The expected Bech32 prefix.</param>
		/// <returns>A dictionary mapping TLV types to their hexadecimal values.</returns>
		public static Dictionary<TLVTypeEnum, string> Bech32ToTLVComponents(string bech32string, Bech32PrefixEnum bechLabel)
		{
			if (bech32string == null || !bech32string.Contains(bechLabel.ToString()))
			{
				return new Dictionary<TLVTypeEnum, string>();
			}
			Bech32Encoder.Decode(bech32string, out var hrp, out var bytes);
			if (hrp != bechLabel.ToString() || bytes == null) throw new Exception("Invalid npub string");
			var cursor = 0;
			var tlvDictionary = new Dictionary<TLVTypeEnum, string>();
			while (cursor < bytes.Length)
			{
				try
				{
					TLVTypeEnum type = (TLVTypeEnum)bytes[cursor++];
					int length = bytes[cursor++];
					byte[] value = new byte[length];
					Array.Copy(bytes, cursor, value, 0, length);
					cursor += length;
					tlvDictionary.TryAdd(type, ByteArrayToHexString(value));
				}
				catch (Exception)
				{
					// Ignore malformed TLV entries
				}

			}
			return tlvDictionary;
		}

		private static byte[] HexStringToByteArray(string hex)
		{
			return new byte[hex.Length / 2].Select((val, idx) => Convert.ToByte(hex.Substring(idx * 2, 2), 16)).ToArray();
		}

		private static string ByteArrayToHexString(byte[] bytes)
		{
			return BitConverter.ToString(bytes).Replace("-", "").ToLower();
		}
	}
}
