using AudibleApi.Common;
using Dinah.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Xml.Linq;

namespace AudibleApi
{
	/*

	SAMPLE USEAGE:

		RecordCreator rc = new(asin);

		rc.AddClip(5000000, 5030000, "clip 1 title", "clip 1 note");
		rc.AddClip(6000000, 6030000, "clip 2 title", "clip 2 note");
		rc.SetLastHeard(7654321);
		rc.AddBookmark(70000000);
		rc.AddBookmark(60000000);
		rc.AddNote(69000000, 69900000, "Standalone note 1");
		rc.AddNote(42000000, 42200000, "Standalone note 2");

		var success = await CreateRecordsAsync(rc);
	*/
	/// <summary>
	/// <para>Create records (e.g. <see cref="RecordType.LastHeard"/>, <see cref="RecordType.Bookmark"/>, <see cref="RecordType.Note"/>, and <see cref="RecordType.Clip"/>) for an audiobook.</para>
	/// <para>Used with <see cref="Api.CreateRecordsAsync(RecordCreator)"/></para>
	/// </summary>
	public class RecordCreator
	{
		private readonly XElement _book;
		public string Asin { get; }
		public int Count => _book.Elements().Count();
		/// <summary>The Xml annotation request post body</summary>
		public XElement Annotation { get; }

		public RecordCreator(string asin)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));
			Asin = asin;
			(Annotation, _book) = CreateAnnotation(asin);
		}

		internal static (XElement annotation, XElement book) CreateAnnotation(string asin)
		{
			var book =
				new XElement("book",
					new XAttribute("key", asin),
					new XAttribute("type", "AUDI"),
					new XAttribute("guid", $"_LATEST_"));

			var annotation =
				new XElement("annotations",
					new XAttribute("version", "1.0"),
					new XAttribute("timestamp", ToXmlDateTime(DateTimeOffset.Now)),
					book);

			return (annotation, book);
		}
		/// <summary>
		/// Creates or modifies the <see cref="RecordType.LastHeard"/> record.
		/// </summary>
		/// <param name="startMs">Timestamp for the last_heard record, in milliseconds from the beginning of the audiobook</param>
		public void SetLastHeard(long startMs)
		{
			validate(startMs);

			_book
				.Elements()
				.FirstOrDefault(e=> e.Name == LastHeard.Name)
				?.Remove();

			_book.Add(
				new XElement(LastHeard.Name,
					new XAttribute("action", "modify"),
					new XAttribute("begin", startMs),
					new XAttribute("timestamp", ToXmlDateTime(DateTimeOffset.Now)))
				);
		}
		/// <summary>
		/// Creates a <see cref="RecordType.Bookmark"/> record
		/// </summary>
		/// <param name="startMs">Timestamp for the bookmark, in milliseconds from the beginning of the audiobook</param>
		public void AddBookmark(long startMs)
		{
			validate(startMs);
			RemoveDuplicate(Bookmark.Name, startMs);

			_book.Add(
				new XElement(Bookmark.Name,
					new XAttribute("action", "create"),
					new XAttribute("begin", startMs),
					new XAttribute("timestamp", ToXmlDateTime(DateTimeOffset.Now)))
				);
		}
		/// <summary>
		/// Creates a <see cref="RecordType.Note"/> record or modifies an existing note with the same start and end times.
		/// </summary>
		/// <param name="startMs">Beginning timestamp for the note, in milliseconds from the beginning of the audiobook</param>
		/// <param name="endMs">Ending timestamp for the note, in milliseconds from the beginning of the audiobook</param>
		/// <param name="note">Note text</param>
		public void AddNote(long startMs, long endMs, string note)
		{
			validate(startMs, endMs);
			ArgumentValidator.EnsureNotNullOrEmpty(note, nameof(note));
			RemoveDuplicate(Note.Name, startMs, endMs);

			_book.Add(
				new XElement(Note.Name,
					new XAttribute("action", "create"),
					new XAttribute("begin", startMs),
					new XAttribute("end", endMs),
					new XAttribute("timestamp", ToXmlDateTime(DateTimeOffset.Now)),
					note)
				);
		}
		/// <summary>
		/// <para>Creates a <see cref="RecordType.Clip"/> record or modifies
		/// an existing clip with the same start and end times.</para>
		/// <para>The server will also create a <see cref="RecordType.Bookmark"/> and <see cref="RecordType.Note"/>
		/// (if a note was set).</para>
		/// <para>When deleting clips, the <see cref="RecordType.Clip"/> must be deleted before the
		/// auto-generated <see cref="RecordType.Bookmark"/> and <see cref="RecordType.Note"/> can be deleted.</para>
		/// </summary>
		/// <param name="startMs"><para>Beginning timestamp for the clip, in milliseconds from the beginning of the audiobook</para></param>
		/// <param name="endMs">
		/// <para>Ending timestamp for the clip, in milliseconds from the beginning of the audiobook</para>
		/// <para>Total clip length (<paramref name="endMs"/> - <paramref name="startMs"/>) may not exceed 40,000 milliseconds.</para>
		/// </param>
		/// <param name="title"><para>Clip title</para></param>
		/// <param name="note"><para>Clip note</para></param>
		public void AddClip(long startMs, long endMs, string title = null, string note = null)
		{
			validate(startMs, endMs);
			//Clips can only be 45 seconds long
			ArgumentValidator.EnsureGreaterThan(startMs + 45_001, nameof(endMs), endMs);

			RemoveDuplicate(Clip.Name, startMs, endMs);

			var clipMeta = new JObject();

			if (!string.IsNullOrWhiteSpace(title))
				clipMeta.Add("title", title);

			if (!string.IsNullOrWhiteSpace(note))
				clipMeta.Add("note", note);

			_book.Add(
				new XElement(Clip.Name,
					new XAttribute("action", "create"),
					new XAttribute("begin", startMs),
					new XAttribute("end", endMs),
					new XAttribute("timestamp", ToXmlDateTime(DateTimeOffset.Now)),
					new XElement("metadata",
						new XCData(clipMeta.ToString(Newtonsoft.Json.Formatting.None))
						)
					)
				);

		}

		private static void validate(long startMs)
			=> ArgumentValidator.EnsureGreaterThan(startMs, nameof(startMs), -1);

		private static void validate(long startMs, long endMs)
		{
			validate(startMs);
			ArgumentValidator.EnsureGreaterThan(endMs + 1, nameof(endMs), startMs);
		}

		private void RemoveDuplicate(string type, long startMs)
			=> _book
				.Elements()
				.FirstOrDefault(e => e.Name == type && e.Attribute("begin")?.Value == startMs.ToString())
				?.Remove();
		
		private void RemoveDuplicate(string type, long startMs, long endMs)
			=> _book
				.Elements()
				.FirstOrDefault(e => e.Name == type && e.Attribute("begin")?.Value == startMs.ToString() && e.Attribute("end")?.Value == endMs.ToString())
				?.Remove();

		internal static string ToXmlDateTime(DateTimeOffset dt)
		{
			//The K formatter inserts a colon, but that's invalid for these requests
			var dtString = dt.ToString("yyyy-MM-ddTHH\\:mm\\:ssK");
			return dtString[..^3] + dtString[^2..];
		}
	}
}
