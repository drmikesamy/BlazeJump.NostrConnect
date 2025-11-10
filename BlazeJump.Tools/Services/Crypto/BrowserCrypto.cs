using Microsoft.JSInterop;

namespace BlazeJump.Tools.Services.Crypto
{
	/// <summary>
	/// Implements browser-based cryptographic operations through JavaScript interop.
	/// </summary>
	public class BrowserCrypto : IBrowserCrypto
	{
		private readonly IJSRuntime _jsRuntime;

		/// <summary>
		/// Initializes a new instance of the <see cref="BrowserCrypto"/> class.
		/// </summary>
		/// <param name="jsRuntime">The JavaScript runtime for interop.</param>
		public BrowserCrypto(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;
		}

		/// <summary>
		/// Invokes a browser crypto function via JavaScript interop.
		/// </summary>
		/// <param name="functionName">The name of the crypto function to invoke.</param>
		/// <param name="args">The arguments to pass to the function.</param>
		/// <returns>The result from the browser crypto operation.</returns>
		public async Task<string> InvokeBrowserCrypto(string functionName, params object[] args)
		{
			return await _jsRuntime.InvokeAsync<string>(functionName, args);
		}
	}
}
