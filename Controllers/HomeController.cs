using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PeerReviewWeb.Data;
using PeerReviewWeb.Models;
using PeerReviewWeb.Models.CourseModels;
using PeerReviewWeb.Models.HomeViewModels;
using PeerReviewWeb.Services;

namespace PeerReviewWeb.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{

		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IEmailSender _emailSender;
		private readonly ILogger _logger;
		private readonly ApplicationDbContext _context;

		public HomeController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			IEmailSender emailSender,
			ILogger<AccountController> logger,
			ApplicationDbContext context)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_logger = logger;
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);

			_context.Entry(user).Collection(u => u.Courses).Load();

			var invites = _context.GroupInvitations
				.Include(gi => gi.Group)
					.ThenInclude(g => g.Members)
						.ThenInclude(gjt => gjt.ApplicationUser)
				.Where(gi => gi.ApplicationUser == user);

			var reviewAssignments = _context.ReviewAssignments
				.Include(ra => ra.ApplicationUser)
				.Where(ra => ra.ApplicationUser.Id == user.Id)
				.Where(ra => !ra.Complete);

			await invites.LoadAsync();

			var inviteNotes = invites.Select(gi => new Notification
			{
				Message = $"You've been invited to join a group with {gi.Group.GetFormattedMemberList()}.",
				Controller = "Assignment",
				Action = "JoinGroup",
				RouteId = gi.ID.ToString(),
			});

			var reviewNotes = reviewAssignments.Select(ra => new Notification
			{
				Class = "alert alert-warning",
				Message = "You have a new Peer Review to complete.",
				Controller = "Review",
				Action = "Assigned",
				RouteId = ra.ID.ToString(),
			});

			List<Notification> notifications = await inviteNotes
				.Concat(reviewNotes)
				.ToListAsync();

			List<Course> courses = user.Courses?.Select(cjt =>
			{
				_context.Entry(cjt).Reference(cjt_i => cjt_i.Course).Load();
				_context.Entry(cjt.Course).Collection(c => c.Assignments).Load();
				return cjt.Course;
			}).ToList() ?? new List<Course>();

			var model = new IndexViewModel
			{
				Notifications = notifications,
				ActiveCourses = courses,
			};
			return View(model);
		}

		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
