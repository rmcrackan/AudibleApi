using AudibleApi.Common;
using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AudibleApi
{
	public partial class Api
	{
		private const string FIONA_DOMAIN = "ht" + "tps://cde-ta-g7g.amazon.com";
		private const string FIONA_SIDECAR_PATH = "/FionaCDEServiceEngine/sidecar";

		public async Task<bool> CreateRecordsAsync(string asin, AnnotationBuilder annotationBuilder)
		{
			const string requestUri = FIONA_SIDECAR_PATH + $"?type=AUDI";

			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));
			ArgumentValidator.EnsureNotNull(annotationBuilder, nameof(annotationBuilder));
			if (annotationBuilder.Count == 0) return false;

			try
			{
				var client = Sharer.GetSharedHttpClient(FIONA_DOMAIN);
				var response = await AdHocAuthenticatedXmlPostAsync(requestUri, client, annotationBuilder.GetAnnotation(asin));

				if (!response.IsSuccessStatusCode)
					Serilog.Log.Information(
						"Record creation failed for {asin}. {Response} {@DebugInfo}",
						asin,
						await response.Content.ReadAsStringAsync(),
						annotationBuilder.GetAnnotation(asin));
				
				return response.IsSuccessStatusCode;
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Error encountered while trying to create records for {asin}.", asin);
				throw;
			}
		}

		public Task<bool> DeleteRecordAsync(string asin, IRecord records)
			=> DeleteRecordsAsync(asin, new[] { records });

		/// <summary>
		/// Delete audiobook records
		/// <para>Note: When deleting clips, the <see cref="RecordType.Clip"/> must be deleted before the
		/// auto-generated <see cref="RecordType.Bookmark"/> and <see cref="RecordType.Note"/> can be deleted.</para>
		/// </summary>
		public async Task<bool> DeleteRecordsAsync(string asin, IEnumerable<IRecord> records)
		{
			const string requestUri = FIONA_SIDECAR_PATH + $"?type=AUDI";

			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));
			if (!records.Any()) return false;

			try
			{
				static XElement createDeleteAction(IRecord record)
				{
					var deleteAction =
						new XElement(record.GetName(),
							new XAttribute("action", "delete"),
							new XAttribute("begin", (long)record.Start.TotalMilliseconds),
							new XAttribute("timestamp", AnnotationBuilder.ToXmlDateTime(DateTimeOffset.Now)));

					if (record is IRangeAnnotation range)
						deleteAction.Add(new XAttribute("end", (long)range.End.TotalMilliseconds));

					return deleteAction;
				}

				var (annotation, book) = AnnotationBuilder.CreateAnnotation(asin);
				book.Add(records.Select(r => createDeleteAction(r)));

				var client = Sharer.GetSharedHttpClient(FIONA_DOMAIN);
				var response = await AdHocAuthenticatedXmlPostAsync(requestUri, client, annotation);

				if (!response.IsSuccessStatusCode)
					Serilog.Log.Information(
						"Record deletion failed for {asin}. {Response} {@DebugInfo}",
						asin,
						await response.Content.ReadAsStringAsync(),
						annotation);

				return response.IsSuccessStatusCode;
			}
			catch(Exception ex)
			{
				Serilog.Log.Error(ex, "Error encountered while trying to delete records for {asin}.", asin);
				throw;
			}
		}

		public async Task<List<IRecord>> GetRecordsAsync(string asin)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));

			var requestUri = FIONA_SIDECAR_PATH + $"?type=AUDI&key={asin}";

			try
			{
				var client = Sharer.GetSharedHttpClient(FIONA_DOMAIN);
				var response = await AdHocAuthenticatedGetAsync(requestUri, client);
				var responseContent = await response.Content.ReadAsStringAsync();

				//Response is 404 if book has no records
				return response.IsSuccessStatusCode ? RecordDto.FromJson(responseContent)?.Payload?.Records ?? new() : new();
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Error encountered while trying to get records for {asin}.", asin);
				throw;
			}
		}
	}
}
