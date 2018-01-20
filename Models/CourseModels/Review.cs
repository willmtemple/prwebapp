using System;

namespace PeerReviewWeb.Models.CourseModels
{
	public class Review
	{
		public Guid ID { get; set; }
		public ApplicationUser Owner { get; set; }
		public Submission Submission { get; set; }
		public DateTime TimeStamp { get; set; }
		// In case it changes
		public string SubmittedWithSchemaJSON { get; set; }
		public string DataJSON { get; set; }
	}
}