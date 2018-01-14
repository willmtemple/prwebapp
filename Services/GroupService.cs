using System;
using System.Collections.Generic;
using System.Linq;
using PeerReviewWeb.Data;
using PeerReviewWeb.Models.CourseModels;
using PeerReviewWeb.Models.JoinTagModels;

namespace PeerReviewWeb.Services {
    public class GroupService {
        public static Group Merge(ApplicationDbContext ctx, Group g1, Group g2) {
            var users = g2.Members.Select(gt => gt.ApplicationUser);

            g1.Members = g1.Members.Concat(users.Select(u => {
                u.Groups = u.Groups.Where(gt => gt.GroupId != g1.ID).ToList();
                return new GroupJoinTag { GroupId = g1.ID, ApplicationUserId = u.Id };
            })).ToList();

            return g1;
        }
    }
}