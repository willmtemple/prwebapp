using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PeerReviewWeb.Models;
using PeerReviewWeb.Models.CourseModels;
using PeerReviewWeb.Models.JoinTagModels;

namespace PeerReviewWeb.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public virtual DbSet<Assignment> Assignments { get; set; }
		public virtual DbSet<Course> Courses { get; set; }
		public virtual DbSet<GroupInvitation> GroupInvitations { get; set; }
		public virtual DbSet<Submission> Submissions { get; set; }
		public virtual DbSet<StageHolder> StageHolders { get; set; }
		public virtual DbSet<FileRef> FileRefs { get; set; }
		public virtual DbSet<Review> Reviews { get; set; }
		public virtual DbSet<ReviewAssignment> ReviewAssignments { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			// Customize the ASP.NET Identity model and override the defaults if needed.
			// For example, you can rename the ASP.NET Identity table names and more.
			// Add your customizations after calling base.OnModelCreating(builder);

			// All of this garbage can be removed once EF Core has many-to-many relationships
			#region crap
			builder.Entity<CourseJoinTag>()
				   .HasKey(t => new { t.CourseId, t.ApplicationUserId });

			builder.Entity<CourseJoinTag>()
				   .HasOne(ct => ct.Course)
				   .WithMany(c => c.Affiliates)
				   .HasForeignKey(ct => ct.CourseId);

			builder.Entity<CourseJoinTag>()
				   .HasOne(ct => ct.ApplicationUser)
				   .WithMany(u => u.Courses)
				   .HasForeignKey(ct => ct.ApplicationUserId);

			builder.Entity<GroupJoinTag>()
				   .HasKey(t => new { t.GroupId, t.ApplicationUserId });

			builder.Entity<GroupJoinTag>()
				   .HasOne(gt => gt.Group)
				   .WithMany(g => g.Members)
				   .HasForeignKey(gt => gt.GroupId);

			builder.Entity<GroupJoinTag>()
				   .HasOne(gt => gt.ApplicationUser)
				   .WithMany(u => u.Groups)
				   .HasForeignKey(gt => gt.ApplicationUserId);

			// Compound key for stage name x assignment, so assignments can have stages with the same name.
			builder.Entity<AssignmentStage>()
				   .HasKey(s => new { s.AssignmentId, s.Id });

			// Not sure if this is necessary or not... it should be the default binding but since I
			// specified the key, I decided to make the key Assignment by AssignmentId an explicit
			// property of the Stage
			builder.Entity<AssignmentStage>()
				   .HasOne(st => st.Assignment)
				   .WithMany(a => a.Stages)
				   .HasForeignKey(st => st.AssignmentId);
			#endregion

			builder.Entity<Group>()
				   .Property(g => g.CurrentStage)
				   .HasDefaultValue(Assignment.START_STAGE);

			builder.Entity<Assignment>()
				   .Property(a => a.Opens)
				   .ValueGeneratedNever();

			builder.Entity<Assignment>()
				   .Property(a => a.Closes)
				   .ValueGeneratedNever();

			builder.Entity<StageHolder>()
					.HasKey(sh => new { sh.AssignmentID, sh.UserID });
		}
	}
}
