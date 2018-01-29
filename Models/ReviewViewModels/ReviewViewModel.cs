using System;
using System.Collections.Generic;
using PeerReviewWeb.Models.FormSchema;
using PeerReviewWeb.Models.CourseModels;

namespace PeerReviewWeb.Models.ReviewViewModels
{
	public class ReviewViewModel
	{
		public Submission Submission { get; set; }
		public Schema ReviewSchema { get; set; }
		public Dictionary<string, object> Values { get; set; }
		public Guid? ReviewId { get; set; }
	}
}