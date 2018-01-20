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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Submit(Guid forSubmission)
		{
			var submission = await _context.Submissions
				.Include(s => s.AssignmentStage)
				.SingleOrDefaultAsync(s => s.ID == forSubmission);

			var user = await _userManager.GetUserAsync(User);

			var concreteSchema = JsonConvert.DeserializeObject<Schema>(
				submission.AssignmentStage.ReviewSchemaJSON
			);

			var review = new Review
			{
				ID = Guid.NewGuid(),
				Owner = user,
				SubmittedWithSchemaJSON = submission.AssignmentStage.ReviewSchemaJSON,
				Submission = submission,
				DataJSON = ReviewDataFromFormData(concreteSchema, Request.Form)
			};

			_context.Reviews.Add(review);
			await _context.SaveChangesAsync();
			return RedirectToAction("Index", "Home");
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