using System;
using System.Linq;
using System.Collections.Generic;
using PeerReviewWeb.Models.JoinTagModels;
using System.ComponentModel.DataAnnotations;

namespace PeerReviewWeb.Models.CourseModels
{
    public class AssignmentStage {
        public string Id { get; set; }

        public Guid AssignmentId { get; set; }
        public Assignment Assignment { get; set;}

        public string Instructions { get; set; }
    }

    public class Assignment
    {
        public static int START_STAGE = 0;

        public Guid ID { get; set; }

        public int Seq { get; set; }
        public ICollection<Group> Groups { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public List<AssignmentStage> Stages { get; set; }

        public DateTime? Opens { get; set; }
        public DateTime? Closes { get; set; }

        public Group GroupFor(ApplicationUser user) {
            return Groups.First(g => g.Members.Any(gt => gt.ApplicationUserId == user.Id));
        }

        public bool IsOpen() {
            var now = DateTime.Now;
            return (Opens == null || now > Opens) && (Closes == null || now < Closes);
        }
    }
}
