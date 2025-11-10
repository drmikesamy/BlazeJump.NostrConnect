namespace BlazeJump.Tools.Models
{
	/// <summary>
	/// Represents a menu item with title, description, and navigation link information.
	/// </summary>
	public class MenuItem
	{
		/// <summary>
		/// Gets or sets the menu item title.
		/// </summary>
		public string Title { get; set; } = "Item Title";
		
		/// <summary>
		/// Gets or sets the menu item description.
		/// </summary>
		public string Description { get; set; } = "Item Description";
		
		/// <summary>
		/// Gets or sets the external link URI.
		/// </summary>
		public string LinkUri { get; set; } = "#";
		
		/// <summary>
		/// Gets or sets the internal route path.
		/// </summary>
		public string LinkRoute { get; set; } = "";
	}
}