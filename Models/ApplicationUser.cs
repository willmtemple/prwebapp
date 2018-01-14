using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PeerReviewWeb.Models.CourseModels;
using PeerReviewWeb.Models.JoinTagModels;

namespace PeerReviewWeb.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public bool IsAdmin { get; set; }
        public ICollection<CourseJoinTag> Courses { get; set; }

        public ICollection<GroupJoinTag> Groups { get; set; }
    }
}
