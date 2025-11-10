namespace BlazeJump.Tools.Services.Crypto
{
	/// <summary>
	/// Provides browser-based cryptographic operations through JavaScript interop.
	/// </summary>
	public interface IBrowserCrypto
	{
		/// <summary>
		/// Invokes a browser crypto function via JavaScript interop.
		/// </summary>
		/// <param name="functionName">The name of the crypto function to invoke.</param>
		/// <param name="args">The arguments to pass to the function.</param>
		/// <returns>The result from the browser crypto operation.</returns>
		Task<string> InvokeBrowserCrypto(string functionName, params object[] args);
	}
}
