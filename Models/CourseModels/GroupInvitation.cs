namespace PeerReviewWeb.Models.CourseModels
{
	public class GroupInvitation
	{
		public int ID { get; set; }
		public ApplicationUser ApplicationUser { get; set; }
		public Group Group { get; set; }

	}
}