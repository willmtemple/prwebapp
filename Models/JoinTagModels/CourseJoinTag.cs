using System;
using System.Collections.Generic;
using PeerReviewWeb.Models.CourseModels;

namespace PeerReviewWeb.Models.JoinTagModels
{
	public class CourseJoinTag
	{
		public const int ROLE_STUDENT = 0;
		public const int ROLE_INSTRUCTOR = 1;

		// Why is this a string? TODO: ask Auth people why this is a string.
		public string ApplicationUserId { get; set; }
		public ApplicationUser ApplicationUser { get; set; }

		public Guid CourseId { get; set; }
		public Course Course { get; set; }

		public int Role { get; set; }
	}
}
