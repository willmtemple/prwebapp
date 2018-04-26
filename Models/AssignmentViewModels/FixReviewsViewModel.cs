using System.Collections.Generic;
using PeerReviewWeb.Models.CourseModels;

namespace PeerReviewWeb.Models.AssignmentViewModels {
    public class FixReviewsViewModel {
        public IList<(ApplicationUser, int)> ReviewsPerStudent { get; set; }
        public IList<(Submission, int)> ReviewsPerSubmission { get; set; }
    }
}