using System.Collections.Generic;
using PeerReviewWeb.Models.CourseModels;

namespace PeerReviewWeb.Models.AssignmentViewModels
{
	public class StudentDetailsViewModel
	{
		public Assignment Assignment { get; set; }
		public Group Group { get; set; }
		public AssignmentStage Stage { get; set; }
		public ICollection<ExtendedSubmission> Submissions { get; set; }
	}
}