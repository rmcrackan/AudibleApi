using AudibleApi.Common;
using Dinah.Core.Net.Http;
using System;
using System.Collections.Concurrent;
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
			/// <summary>Holds all GetNextBatch tasks</summary>
			private readonly BlockingCollection<Task<Item[]>> GetItemsTasks = new();
			/// <summary>The downloader loop task that makes successive calls to GetNextBatch</summary>
			private readonly Task GetAllItemsTask;
			/// <summary>The Continuation-Token received from the last call to the Api.</summary>
			private string ContinuationToken;
			/// <summary>Holds all Items from a call to GetNextBatch</summary>
			private Item[] Items;
			/// <summary>Index in <see cref="Items"/> for the enumerator's current position.</summary>
			private int currentIndex = 0;

			public LibraryItemAsyncEnumerator(Api api, string queryString)
			{
				GetAllItemsTask = GetAllItems(api, queryString);
			}

			private async Task GetAllItems(Api api, string queryString)
			{
				do
				{
					var currentGetItemsTask = GetNextBatch(api, queryString);

					GetItemsTasks.Add(currentGetItemsTask);

					//Must await here because the ContinuationToken for the next call
					//to GetNextBatch is returned by the curret call to GetNextBatch
					await currentGetItemsTask;

				} while (!string.IsNullOrEmpty(ContinuationToken));

				GetItemsTasks.CompleteAdding();
			}

			private async Task<Item[]> GetNextBatch(Api api, string queryString)
			{
				var continuationQuery = ContinuationToken is null ? string.Empty : $"continuation_token={ContinuationToken}&";
				var response = await api.getLibraryResponseAsync($"{continuationQuery}{queryString}");

				var page = await response.Content.ReadAsStringAsync();
				LibraryDtoV10 libResult;
				try
				{
					// important! use this convert/deser method
					libResult = LibraryDtoV10.FromJson(page);
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Error converting library for importing use. Full library:\r\n" + page);
					throw;
				}
				if (response.Headers.TryGetValues("Continuation-Token", out var values))
				{
					ContinuationToken = values.First();
				}
				else
					ContinuationToken = null;

				return libResult.Items;
			}

			public Item Current => Items[currentIndex];

			public ValueTask DisposeAsync()
			{
				GetItemsTasks.Dispose();
				return ValueTask.CompletedTask;
			}

			public async ValueTask<bool> MoveNextAsync()
			{
				currentIndex++;
				if (Items is null || currentIndex >= Items.Length)
				{
					if (!GetItemsTasks.TryTake(out var itemsTask, -1))
					{
						await GetAllItemsTask;
						return false;
					}

					Items = await itemsTask;
					currentIndex = 0;
				}

				return true;
			}
		}
	}
}
