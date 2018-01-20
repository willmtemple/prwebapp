using System;
using System.Linq;
using System.Collections.Generic;
using PeerReviewWeb.Data;
using PeerReviewWeb.Models.JoinTagModels;
using System.ComponentModel.DataAnnotations;

namespace PeerReviewWeb.Models.CourseModels
{
	public class AssignmentStage
	{
		[Required]
		public string Id { get; set; }

		public Guid AssignmentId { get; set; }
		public Assignment Assignment { get; set; }
		public int Seq { get; set; }
		public string Instructions { get; set; }
		[Required]
		public bool IsPeerReviewed { get; set; } = false;

		public string ReviewSchemaJSON { get; set; }

		public int ReviewsPerStudent { get; set; } = 0;
	}

	public class Assignment
	{
		public static int START_STAGE = 0;

		public Guid ID { get; set; }

		public Course Course { get; set; }

		public int Seq { get; set; }

		public bool IsGroupAssignment { get; set; } = false;
		public ICollection<Group> Groups { get; set; }
		public int MinGroupSize { get; set; } = 1;
		public int MaxGroupSize { get; set; } = 1;

		[Required]
		public string Name { get; set; }
		public string Description { get; set; }
		public IList<AssignmentStage> Stages { get; set; }

		public DateTime? Opens { get; set; } = null;
		public DateTime? Closes { get; set; } = null;

		public Group GroupFor(ApplicationUser user)
		{
			return Groups.FirstOrDefault(g => g.Members.Any(gt => gt.ApplicationUserId == user.Id));
		}

		public bool IsOpen()
		{
			var now = DateTime.Now;
			return (Opens == null || now > Opens) && (Closes == null || now < Closes);
		}

		public bool GroupSatisfied(ApplicationUser user)
		{
			if (!IsGroupAssignment) return true;
			var group = GroupFor(user);

			if (group != null)
			{
				return group.Members.Count >= MinGroupSize
					&& group.Members.Count <= MaxGroupSize;
			}
			return false;
		}

		// ONLY FOR USE WITH SINGLE USER ASSIGNMENTS
		// WILL RETURN FIRST STAGE FOR USERS NOT IN THE COURSE!! SO CHECK
		// #wt -- get rid of this as soon as possible, it's gross
		public AssignmentStage StageForUser(ApplicationUser user, ApplicationDbContext _context)
		{
			var stageHolder = _context.StageHolders
				.SingleOrDefault(sh =>
					sh.AssignmentID == ID
					&& sh.UserID == user.Id);

			var si = 0;

			if (stageHolder != null) si = stageHolder.CurrentStage;

			if (si >= Stages.Count)
			{
				return null;
			}
			else
			{
				return Stages[si];
			}
		}
	}
}
