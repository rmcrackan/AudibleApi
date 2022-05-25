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
	internal class ItemAsyncEnumerable : IAsyncEnumerable<Item>
	{
		private readonly Api Api;
		private readonly LibraryOptions LibraryOptions;
		internal ItemAsyncEnumerable(Api api, LibraryOptions libraryOptions)
		{
			Api = api;
			LibraryOptions = libraryOptions;
			if (!LibraryOptions.PurchasedAfter.HasValue || LibraryOptions.PurchasedAfter.Value < new DateTime(1970, 1, 1))
				LibraryOptions.PurchasedAfter = new DateTime(1970, 1, 1);
		}

		public IAsyncEnumerator<Item> GetAsyncEnumerator(CancellationToken cancellationToken = default)
			=> new LibraryItemAsyncEnumerator(Api, LibraryOptions);

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

			public LibraryItemAsyncEnumerator(Api api, LibraryOptions libraryOptions)
			{
				GetAllItemsTask = GetAllItems(api, libraryOptions);
			}

			private async Task GetAllItems(Api api, LibraryOptions libraryOptions)
			{
				do
				{
					try
					{
						// max 1000 however with higher numbers it stops returning 'provided_review' and 'reviews' groups.
						// Sometimes this threshold is as high as 900, sometimes as low as 400.
						// I've never had problems at 300. Another user had nearly constant problems at 300.
						libraryOptions.NumberOfResultPerPage = 250;
						var currentGetItemsTask = GetNextBatch(api, libraryOptions.ToQueryString());
						GetItemsTasks.Add(currentGetItemsTask);

						//Must await here because the ContinuationToken for the next call
						//to GetNextBatch is returned by the curret call to GetNextBatch
						await currentGetItemsTask;
					}
					catch (Exception ex) when (ex is TaskCanceledException || ex is TimeoutException)
					{
						// if it times out with 250, try 50. This takes about 5 seconds longer for a library of 1,100
						//
						// For each batch size, I ran 3 tests. Results in milliseconds
						//   1000    19389 , 17760 , 19256
						//    500    19099 , 19905 , 18553
						//    250    20288 , 19163 , 19605
						//    100    22156 , 22058 , 22438
						//     50    25017 , 24292 , 24491
						//     25    28627 , 30006 , 31201
						//     10    45006 , 46717 , 44924
						libraryOptions.NumberOfResultPerPage = 50;
						var currentGetItemsTask = GetNextBatch(api, libraryOptions.ToQueryString());
						GetItemsTasks.Add(currentGetItemsTask);

						//Must await here because the ContinuationToken for the next call
						//to GetNextBatch is returned by the curret call to GetNextBatch
						await currentGetItemsTask;
					}
					catch (Exception)
					{
						throw;
					}


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
					ContinuationToken = values.First();
				else
					ContinuationToken = null;

				return libResult.Items;
			}

			public Item Current => Items[currentIndex];

			public async ValueTask DisposeAsync()
			{
				await GetAllItemsTask;
				GetItemsTasks.Dispose();
			}

			public async ValueTask<bool> MoveNextAsync()
			{
				currentIndex++;
				if (Items is null || currentIndex >= Items.Length)
				{
					try
					{
						if (!GetItemsTasks.TryTake(out var itemsTask, -1))
							return false;

						Items = await itemsTask;
					}
					catch (Exception ex) when (ex is TaskCanceledException || ex is TimeoutException)
					{
						if (!GetItemsTasks.TryTake(out var itemsTask, -1))
							return false;

						Items = await itemsTask;
					}
					catch
					{
						return false;
					}
					currentIndex = 0;
				}
				return true;
			}
		}
	}
}
