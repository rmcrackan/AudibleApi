using System;
using Newtonsoft.Json;

namespace AudibleApi.Common
{
	/// <summary>
	/// Product (aka: catalog product) and Book.Item (from user's library) are the same except that the library book has much more information
	/// </summary>
	public partial class BookDtoV10 : V10Base<BookDtoV10>
	{
		[JsonProperty("item")]
		public Item Item { get; set; }
	}
}
