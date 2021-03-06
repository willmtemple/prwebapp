using PeerReviewWeb.Data;
using PeerReviewWeb.Models;
using PeerReviewWeb.Models.JoinTagModels;
using System;
using System.Linq;
using System.Collections.Generic;

namespace PeerReviewWeb.Models.CourseModels
{
	public class Group
	{
		public Guid ID { get; set; }
		public Assignment Assignment { get; set; }
		public ICollection<GroupJoinTag> Members { get; set; }

		/**
        The current stage of the group within the assignment.
         */
		public int CurrentStage { get; set; }

		public string GetFormattedMemberList()
		{
			return string.Join(", ", Members.Select(gt => gt.ApplicationUser.Email));
		}

		// Will return null if the assignment is complete.
		public AssignmentStage NextStage()
		{
			if (CurrentStage >= Assignment.Stages.Count)
			{
				return null;
			}
			else
			{
				return Assignment.Stages.OrderBy(s => s.Seq).ElementAt(CurrentStage);
			}
		}
	}
}