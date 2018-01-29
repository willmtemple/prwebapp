using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PeerReviewWeb.Models.JoinTagModels;

namespace PeerReviewWeb.Models.CourseModels
{
	public class Course
	{
		public Guid ID { get; set; }

		[Required]
		public string Name { get; set; }
		public string Description { get; set; }
		public string OwnerEmail { get; set; }
		public IList<Assignment> Assignments { get; set; }
		public ICollection<CourseJoinTag> Affiliates { get; set; }

		[Required]
		public bool RequireEnrollmentKey { get; set; }

		public string EnrollmentKey { get; set; } = null;

		public IEnumerable<ApplicationUser> GetInstructors()
		{
			return Affiliates.Where(c => c.Role == CourseJoinTag.ROLE_INSTRUCTOR).Select(cjt => cjt.ApplicationUser);
		}

		public IEnumerable<ApplicationUser> GetStudents()
		{
			return Affiliates.Where(c => c.Role == CourseJoinTag.ROLE_STUDENT).Select(cjt => cjt.ApplicationUser);
		}

		// Returns -1 on failure, or the role of the user.
		public int RoleFor(ApplicationUser user)
		{
			if (user == null) return -1;
			var tag = Affiliates.Where(a => a.ApplicationUserId == user.Id).FirstOrDefault();

			if (tag == null) return -1;

			return tag.Role;
		}

		public IEnumerable<Assignment> GetActiveAssignments()
		{
			return Assignments.Where(a => a.IsOpen());
		}
	}
}
