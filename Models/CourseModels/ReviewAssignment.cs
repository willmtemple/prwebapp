using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeerReviewWeb.Models.CourseModels
{
	public class ReviewAssignment
	{
		public Guid ID { get; set; }
		public Submission Submission { get; set; }
		public ApplicationUser ApplicationUser { get; set; }

		public DateTime TimeStamp { get; set; }

		public bool Complete { get; set; }
	}
}