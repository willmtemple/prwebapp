using System.Collections.Generic;
using PeerReviewWeb.Models.CourseModels;

namespace PeerReviewWeb.Models.AssignmentViewModels
{
	public class ExtendedSubmission
	{
		public Submission Submission { get; set; }
		public ICollection<Review> Reviews { get; set; }
	}
}