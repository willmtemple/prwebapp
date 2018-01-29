using PeerReviewWeb.Models.CourseModels;
using System.Collections.Generic;

namespace PeerReviewWeb.Models.HomeViewModels
{
	public class Notification
	{
		public string Class { get; set; } = "alert alert-info";
		public string Message { get; set; }
		public string Controller { get; set; }
		public string Action { get; set; }
		public string RouteId { get; set; }
	}
	public class IndexViewModel
	{
		public List<Notification> Notifications;
		public List<Course> ActiveCourses;
	}
}