using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

using PeerReviewWeb.ActionFilters;
using PeerReviewWeb.Models.FormSchema;
using Newtonsoft.Json;

namespace PeerReviewWeb.Controllers
{
	[DevelopmentOnly]
	public class TestController : Controller
	{
		public IActionResult DemoSchema()
		{
			var entries = new List<AbsFormEntry>();
			entries.Add(new LikertForm("q1", "The submission is clear.", 5));
			entries.Add(new TextForm("q1-exp", "Yes, and...", 3));
			var model = new Schema
			{
				Title = "Demo Form",
				Instructions = "Answer the following questions regarding the material from class.",
				Entries = entries,
			};

			return View(model);
		}

		public IActionResult DemoFilledForm()
		{
			var entries = new List<AbsFormEntry>();
			entries.Add(new LikertForm("q1", "The submission is clear.", 5));
			entries.Add(new TextForm("q1-exp", "Yes, and...", 3));
			var model = new Schema
			{
				Title = "Demo Form",
				Instructions = "Answer the following questions regarding the material from class.",
				Entries = entries,
			};

			var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(
				"{\"q1\":5,\"q1-exp\":\"asdfasdf\"}"
			);

			ViewData["Values"] = values;
			return View(model);
		}
	}
}