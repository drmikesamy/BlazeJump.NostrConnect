using QRCoder;

namespace BlazeJump.Tools.Helpers
{
	/// <summary>
	/// Helper class for generating QR codes.
	/// </summary>
	public static class QRCodeHelpers
	{
		/// <summary>
		/// Generates a QR code image from input data as a Base64 encoded data URL.
		/// </summary>
		/// <param name="inputData">The data to encode in the QR code.</param>
		/// <returns>A data URL string representing the QR code PNG image, or null if generation fails.</returns>
		public static string? GenerateQRCode(string inputData)
		{
			QRCodeGenerator qrGenerator = new QRCodeGenerator();
			QRCodeData qrCodeData = qrGenerator.CreateQrCode(inputData, QRCodeGenerator.ECCLevel.Q);
			PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
			byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);
			string base64 = Convert.ToBase64String(qrCodeAsPngByteArr);
			return $"data:image/png;base64,{base64}";
		}
	}
}
