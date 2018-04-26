using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using PeerReviewWeb.Data;
using PeerReviewWeb.Models.ReviewViewModels;
using PeerReviewWeb.Models.FormSchema;
using PeerReviewWeb.Models;
using PeerReviewWeb.Models.CourseModels;
using PeerReviewWeb.Models.JoinTagModels;
using Newtonsoft.Json;

namespace PeerReviewWeb.Controllers
{
	[Authorize]
	public class ReviewController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;

		public ReviewController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			ApplicationDbContext context)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_context = context;
		}
		public async Task<IActionResult> For(Guid id)
		{
			var submission = await _context.Submissions
				.Include(s => s.AssignmentStage)
				.Include(s => s.Files)
				.Where(s => s.AssignmentStage.IsPeerReviewed)
				.SingleOrDefaultAsync(s => s.ID == id);

			if (submission == null) return NotFound();

			var schema = JsonConvert.DeserializeObject<Schema>(
				submission.AssignmentStage.ReviewSchemaJSON
			);

			return View(new ReviewViewModel
			{
				Submission = submission,
				ReviewSchema = schema,
			});
		}

		public async Task<IActionResult> Assigned(Guid id)
		{
			var reviewAssignment = await _context.ReviewAssignments
				.Include(ra => ra.ApplicationUser)
				.Include(ra => ra.Submission)
				.ThenInclude(s => s.AssignmentStage)
				.Include(ra => ra.Submission)
				.ThenInclude(s => s.Files)
				.SingleOrDefaultAsync(ra => ra.ID == id);

			var user = await _userManager.GetUserAsync(User);

			if (reviewAssignment == null ||
				reviewAssignment.ApplicationUser.Id != user.Id)
			{
				return NotFound();
			}

			var schema = JsonConvert.DeserializeObject<Schema>(
				reviewAssignment.Submission.AssignmentStage.ReviewSchemaJSON
			);

			ViewData["ForReviewAssignment"] = id;
			return View(new ReviewViewModel
			{
				Submission = reviewAssignment.Submission,
				ReviewSchema = schema,
			});
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Submit(Guid forSubmission, Guid? forReviewAssignment = null)
		{
			var submission = await _context.Submissions
				.Include(s => s.AssignmentStage)
				.SingleOrDefaultAsync(s => s.ID == forSubmission);

			var user = await _userManager.GetUserAsync(User);

			if (forReviewAssignment != null)
			{
				var rasg = await _context.ReviewAssignments
					.Where(ra => !ra.Complete)
					.SingleOrDefaultAsync(ra => ra.ID == forReviewAssignment);

				if (rasg == null) return NotFound();

				rasg.Complete = true;
				_context.Update(rasg);
			}

			var concreteSchema = JsonConvert.DeserializeObject<Schema>(
				submission.AssignmentStage.ReviewSchemaJSON
			);

			var review = new Review
			{
				ID = Guid.NewGuid(),
				Owner = user,
				SubmittedWithSchemaJSON = submission.AssignmentStage.ReviewSchemaJSON,
				Submission = submission,
				DataJSON = ReviewDataFromFormData(concreteSchema, Request.Form),
				TimeStamp = DateTime.Now
			};

			_context.Reviews.Add(review);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Success));
		}

		public IActionResult Success()
		{
			return View();
		}

		public async Task<IActionResult> View(Guid id)
		{
			var user = await _userManager.GetUserAsync(User);
			var reviewInter = await _context.Reviews
				.Include(r => r.Submission)
				.ThenInclude(s => s.Submitter)
				.Include(r => r.Submission)
				.ThenInclude(s => s.ForGroup)
				.ThenInclude(g => g.Members)
				.Include(r => r.Submission)
				.ThenInclude(s => s.AssignmentStage)
				.ThenInclude(stg => stg.Assignment)
				.ThenInclude(a => a.Course)
				.ThenInclude(c => c.Affiliates)
				.Include(r => r.Submission)
				.ThenInclude(s => s.Files)
				.ToListAsync();
			var review = reviewInter.Where(r =>
					(((r.Submission.ForGroup != null) &&
					 r.Submission.ForGroup.Members.Any(t => t.ApplicationUserId == user.Id)) ||
					(r.Submission.Submitter.Id == user.Id))
					|| user.IsAdmin || r.Submission.AssignmentStage.Assignment.Course.RoleFor(user) == CourseJoinTag.ROLE_INSTRUCTOR)
				.SingleOrDefault(r => r.ID == id);

			var schema = JsonConvert.DeserializeObject<Schema>(
				review.SubmittedWithSchemaJSON
			);

			var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(
				review.DataJSON
			);

			return View(new ReviewViewModel
			{
				ReviewId = id,
				ReviewSchema = schema,
				Values = values,
				Submission = review.Submission,
			});
		}

		public async Task<IActionResult> ForAssignmentJSON(Guid forAssignment)
		{
			var user = await _userManager.GetUserAsync(User);
			var assignment = _context.Assignments
				.Where(asg => asg.ID == forAssignment)
				.SingleOrDefaultAsync();
			var course = await _context.Courses
				.Include(c => c.Assignments)
				.Include(c => c.Affiliates)
				.ThenInclude(cjt => cjt.ApplicationUser)
				.ThenInclude(u => u.Groups)
				.ThenInclude(gjt => gjt.Group)
				.ThenInclude(g => g.Assignment)
				.Where(c => c.Assignments.Any(asg => asg.ID == forAssignment))
				.SingleOrDefaultAsync();
			var reviewAssignments = await _context.ReviewAssignments
				.Include(ra => ra.Submission)
				.ThenInclude(s => s.AssignmentStage)
				.ThenInclude(ags => ags.Assignment)
				.Include(ra => ra.Submission)
				.ThenInclude(s => s.Submitter)
				.Include(ra => ra.ApplicationUser)
				.Where(ra => ra.Submission.AssignmentStage.AssignmentId == forAssignment)
				.ToListAsync();

			if (assignment == null || course == null ||
				!(user.IsAdmin || course.OwnerEmail == user.Email || course.RoleFor(user) == CourseJoinTag.ROLE_INSTRUCTOR))
			{
				return StatusCode(404, new { status = 404, error = "Not Found"});
			}


			return new OkObjectResult(new {
				status = 200,
				Users = course.Affiliates
					.Where(cjt => cjt.Role == CourseJoinTag.ROLE_STUDENT)
					.Select(cjt => new {
						User = cjt.ApplicationUser.Id,
						Email = cjt.ApplicationUser.Email,
						Group = cjt.ApplicationUser.Groups.Where(gjt => gjt.Group.Assignment.ID == forAssignment).SingleOrDefault()?.GroupId
					}).ToList(),
				Reviews = reviewAssignments.Select(ra => new {
					Assignee = ra.ApplicationUser.Id,
					AssignedTo = ra.Submission.Submitter.Id,
				})
			});
		}

		private string ReviewDataFromFormData(Schema schema, IFormCollection formData)
		{
			// My favorite class
			var d = new Dictionary<string, object>();
			CollectFormEntriesFromFormData(schema.Entries, formData, d);
			return JsonConvert.SerializeObject(d);
		}

		private void CollectFormEntriesFromFormData(
				IEnumerable<AbsFormEntry> entries,
				IFormCollection formData,
				Dictionary<string, object> d)
		{
			foreach (AbsFormEntry e in entries)
			{
				object o;
				switch (e.Type)
				{
					case "Likert":
						o = int.Parse(formData[e.Id]);
						break;
					case "Text":
						o = formData[e.Id].FirstOrDefault();
						break;
					case "Section":
						throw new NotImplementedException("Section form handling");
					default:
						throw new ArgumentException($"Unrecognized FormEntry variant {e.Type}.");
				}
				d[e.Id] = o;
			}
		}
	}
}