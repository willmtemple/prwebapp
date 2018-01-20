using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PeerReviewWeb.Models;

namespace PeerReviewWeb.Models.CourseModels
{
	public class Submission
	{
		public Guid ID { get; set; }
		public AssignmentStage AssignmentStage { get; set; }
		public ApplicationUser Submitter { get; set; }
		public Group ForGroup { get; set; }
		[Required]
		public string SubmissionText { get; set; }
		public ICollection<FileRef> Files { get; set; }
		public bool Confirmed { get; set; } = false;
		public DateTime TimeStamp { get; set; }
	}
}