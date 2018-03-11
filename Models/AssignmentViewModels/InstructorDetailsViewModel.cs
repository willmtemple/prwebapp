using System.Collections.Generic;
using PeerReviewWeb.Models.CourseModels;

namespace PeerReviewWeb.Models.AssignmentViewModels
{
	public class InstructorDetailsViewModel
	{
		public Assignment Assignment { get; set; }
		public ICollection<ExtendedSubmission> Submissions { get; set; }
		public ICollection<ReviewAssignment> IncompleteReviews { get; set; }
	}
}