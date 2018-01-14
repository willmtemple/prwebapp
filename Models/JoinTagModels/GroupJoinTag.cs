using System;
using PeerReviewWeb.Models.CourseModels;

namespace PeerReviewWeb.Models.JoinTagModels
{
    public class GroupJoinTag
    {
        // Why is this a string? TODO: ask Auth people why this is a string.
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public Guid GroupId { get; set; }
        public Group Group { get; set; }
    }
}