using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PeerReviewWeb.Models.FormSchema
{
	public class Schema
	{
		public string Title { get; set; }
		public string Instructions { get; set; }
		[JsonProperty(ItemConverterType = typeof(FormEntryConverter))]
		public IList<AbsFormEntry> Entries { get; set; }
	}
}