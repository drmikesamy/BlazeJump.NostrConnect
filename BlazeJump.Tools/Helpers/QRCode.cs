using QRCoder;

namespace BlazeJump.Tools.Helpers
{
	/// <summary>
	/// Helper class for generating QR codes.
	/// </summary>
	public static class QRCode
	{
		/// <summary>
		/// Generates a QR code image from input data as a Base64 encoded string.
		/// </summary>
		/// <param name="inputData">The data to encode in the QR code.</param>
		/// <returns>A Base64 encoded string representing the QR code bitmap, or null if generation fails.</returns>
		public static string? GenerateQRCode(string inputData)
		{
			QRCodeGenerator qrGenerator = new QRCodeGenerator();
			QRCodeData qrCodeData = qrGenerator.CreateQrCode(inputData, QRCodeGenerator.ECCLevel.Q);
			BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
			byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);
			return Convert.ToBase64String(qrCodeAsBitmapByteArr);
		}
	}
}
