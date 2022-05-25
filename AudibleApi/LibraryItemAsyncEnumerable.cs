using AudibleApi.Common;
using Dinah.Core.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AudibleApi
{
	internal class LibraryItemAsyncEnumerable : IAsyncEnumerable<Item>
	{
		private readonly Api Api;
		private readonly LibraryOptions LibraryOptions;
		internal LibraryItemAsyncEnumerable(Api api, LibraryOptions libraryOptions)
		{
			Api = api;
			LibraryOptions = libraryOptions;
			LibraryOptions.PageNumber = null;
			libraryOptions.NumberOfResultPerPage = null;
		}

		public IAsyncEnumerator<Item> GetAsyncEnumerator(CancellationToken cancellationToken = default)
			=> new LibraryItemAsyncEnumerator(Api, LibraryOptions.ToQueryString());

		private class LibraryItemAsyncEnumerator : IAsyncEnumerator<Item>
		{
			private readonly Api Api;
			private readonly string QueryString;
			private string ContinuationToken;

			private IEnumerator<Item> itemEnumerator;
			public LibraryItemAsyncEnumerator(Api api, string queryString)
			{
				Api = api;
				QueryString = queryString;
			}

			private async ValueTask<IEnumerator<Item>> GetNextBatch()
			{
				var continuationQuery = ContinuationToken is null ? string.Empty : $"continuation_token={ContinuationToken}&";
				var response = await Api.getLibraryResponseAsync($"{continuationQuery}{QueryString}");

				var page = await response.Content.ReadAsJObjectAsync();
				var pageStr = page.ToString();
				LibraryDtoV10 libResult;
				try
				{
					// important! use this convert/deser method
					libResult = LibraryDtoV10.FromJson(pageStr);
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Error converting library for importing use. Full library:\r\n" + pageStr);
					throw;
				}
				if (response.Headers.TryGetValues("Continuation-Token", out var values))
				{
					ContinuationToken = values.First();
				}
				else
					ContinuationToken = null;

				return libResult.Items.AsEnumerable().GetEnumerator();
			}

			public Item Current => itemEnumerator.Current;

			public ValueTask DisposeAsync()
			{
				itemEnumerator?.Dispose();
				return ValueTask.CompletedTask;
			}

			public async ValueTask<bool> MoveNextAsync()
			{
				itemEnumerator ??= await GetNextBatch();

				if (itemEnumerator.MoveNext()) return true;

				if (!string.IsNullOrEmpty(ContinuationToken))
				{
					itemEnumerator = await GetNextBatch();
					return itemEnumerator.MoveNext();
				}

				return false;
			}
		}
	}
}
