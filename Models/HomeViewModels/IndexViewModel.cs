using PeerReviewWeb.Models.CourseModels;
using System.Collections.Generic;

namespace PeerReviewWeb.Models.HomeViewModels
{
	public class Notification
	{
		public string Message;
		public string Controller;
		public string Action;
		public string RouteId;
	}
	public class IndexViewModel
	{
		public List<Notification> Notifications;
		public List<Course> ActiveCourses;
	}
}