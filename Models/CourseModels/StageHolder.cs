using System;

namespace PeerReviewWeb.Models.CourseModels
{
	// TODO: this shouldn't exist and I hate it
	public class StageHolder
	{
		public Guid AssignmentID { get; set; }
		public string UserID { get; set; }

		public int CurrentStage { get; set; }
	}
}