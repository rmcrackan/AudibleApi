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
			private const int MAX_PARALLEL_REQUESTS = 10;
			/// <summary>Holds all GetNextBatch tasks</summary>
			private readonly BlockingCollection<Task<Item[]>> GetItemsTasks = new();
			/// <summary>The downloader loop task that makes successive calls to GetNextBatch</summary>
			private readonly Task GetAllItemsTask;
			/// <summary>The Continuation-Token received from the last call to the Api.</summary>
			private string ContinuationToken;
			/// <summary>The state token passed to future library calls to only receive items that have changed.</summary>
			private string StateToken;
			/// <summary>Total number of items in the library.</summary>
			private int TotalCount;
			/// <summary>Holds all Items from a call to GetNextBatch</summary>
			private Item[] Items;
			/// <summary>Index in <see cref="Items"/> for the enumerator's current position.</summary>
			private int currentIndex = 0;

			public LibraryItemAsyncEnumerator(Api api, LibraryOptions libraryOptions)
			{
				GetAllItemsTask = GetAllItems(api, libraryOptions);
			}

			/// <summary>This is the producer thread that retrieves all library items asap and
			/// stores their tasks in GetItemsTasks</summary>
			private async Task GetAllItems(Api api, LibraryOptions libraryOptions)
			{
				bool getRemainingByPage = false;
				do
				{
					try
					{
						// max 1000 however with higher numbers it stops returning 'provided_review' and 'reviews' groups.
						// Sometimes this threshold is as high as 900, sometimes as low as 400.
						// I've never had problems at 300. Another user had nearly constant problems at 300.
						libraryOptions.NumberOfResultPerPage = 5;
						var currentGetItemsTask = GetBatchContinuing(api, libraryOptions.ToQueryString());
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
						var currentGetItemsTask = GetBatchContinuing(api, libraryOptions.ToQueryString());
						GetItemsTasks.Add(currentGetItemsTask);

						//Must await here because the ContinuationToken for the next call
						//to GetNextBatch is returned by the curret call to GetNextBatch
						await currentGetItemsTask;
					}
					catch (Exception)
					{
						throw;
					}

					if (TotalCount < 10000)
					{
						//Get remaining books by page number in parallel
						getRemainingByPage = true;
						break;
					}


				} while (!string.IsNullOrEmpty(ContinuationToken));

				if (getRemainingByPage)
				{
					//Because LibraryOptions is a class, setting the PageNumber property in a parallel context
					//may lead to the value being changed before it can be converted to a query string.
					var queryString = libraryOptions.ToQueryString();
					int numPages = (int)Math.Ceiling((double)TotalCount / libraryOptions.NumberOfResultPerPage.Value);

					Parallel.For(1, numPages + 1,
						new ParallelOptions { MaxDegreeOfParallelism = MAX_PARALLEL_REQUESTS },
						async (pageNumber) =>
					{
						var currentGetItemsTask = GetBatchPage(api, queryString, pageNumber);
						GetItemsTasks.Add(currentGetItemsTask);
						var items = await currentGetItemsTask;
						Serilog.Log.Logger.Information($"Page {pageNumber}: {items.Length} results");
					});
				}

				GetItemsTasks.CompleteAdding();
			}

			private async Task<Item[]> GetBatchPage(Api api, string queryString, int pageNumber)
			{
				var response = await api.getLibraryResponseAsync($"{queryString}&page={pageNumber}");

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

				return libResult.Items;
			}

			private async Task<Item[]> GetBatchContinuing(Api api, string queryString)
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

				if (StateToken is null && response.Headers.TryGetValues("State-Token", out var values))
					StateToken = values.First();

				if (TotalCount == 0 && response.Headers.TryGetValues("Total-Count", out values))
					TotalCount = int.Parse(values.First());

				if (response.Headers.TryGetValues("Continuation-Token", out  values))
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
				//The enumerator is run on the consumer thread and blocks until new data is
				//available or until all items have been retrieved. Producer signals that all
				//items have been retrieved by calling GetItemsTasks.CompleteAdding() which
				//causes GetItemsTasks.TryTake() to return false when GetItemsTasks is empty.

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
