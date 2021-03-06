using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PeerReviewWeb.Data;
using PeerReviewWeb.Models;
using PeerReviewWeb.Models.CourseModels;
using PeerReviewWeb.Models.JoinTagModels;
using PeerReviewWeb.Services;

namespace PeerReviewWeb.Controllers
{
	[Authorize]
	public class CourseController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IEmailSender _emailSender;
		private readonly ILogger _logger;

		public CourseController(
			ApplicationDbContext context,
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			IEmailSender emailSender,
			ILogger<AccountController> logger)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_logger = logger;
			_context = context;
		}

		// GET: Course
		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);

			var courses = _context.Courses;

			if (!user.IsAdmin)
			{
				return View(await courses.Where(c => c.OwnerEmail == user.Email).ToListAsync());
			}

			return View(await courses.ToListAsync());
		}

		// GET: Course/Details/5
		public async Task<IActionResult> Details(Guid? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var user = await _userManager.GetUserAsync(User);

			var course = await _context.Courses
				.Include(c => c.Assignments)
				.Include(c => c.Affiliates)
				.ThenInclude(cjt => cjt.ApplicationUser)
				.SingleOrDefaultAsync(m => m.ID == id);
			if (course == null
				|| ((course.OwnerEmail != user.Email)
					&& (course.RoleFor(user) != CourseJoinTag.ROLE_INSTRUCTOR)))
			{
				return NotFound();
			}

			return View(course);
		}

		// GET: Course/Create
		public async Task<IActionResult> Create()
		{
			ViewData["EmailPrefill"] = (await _userManager.GetUserAsync(User))?.Email ?? "";
			return View();
		}

		// POST: Course/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Course course)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.GetUserAsync(User);

				if (!user.IsAdmin)
				{
					// TODO: Return an actual error page to the user.
					return Forbid();
				}

				var owner = await _userManager.FindByEmailAsync(course.OwnerEmail);

				if (owner == null)
				{
					// TODO: Return a validation error to the user.
					return NotFound();
				}

				course.ID = Guid.NewGuid();
				if (course.RequireEnrollmentKey)
				{
					course.EnrollmentKey = course.EnrollmentKey.HashUTF8ToBase64();
				}
				_context.Add(course);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Details), new { id = course.ID });
			}

			// TODO: Return validation errors.
			return View(course);
		}

		// GET: Course/Edit/5
		public async Task<IActionResult> Edit(Guid? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var course = await _context.Courses
				.Include(c => c.Affiliates)
				.ThenInclude(cjt => cjt.ApplicationUser)
				.SingleOrDefaultAsync(m => m.ID == id);
			if (course == null)
			{
				return NotFound();
			}

			ViewData["EmailPrefill"] = course.OwnerEmail;
			return View(course);
		}

		// POST: Course/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(Guid id, Course course, string newStudents, string newInstructors)
		{
			newStudents = newStudents ?? "";
			newInstructors = newInstructors ?? "";

			var _user = await _userManager.GetUserAsync(User);
			if (id != course.ID || (course.OwnerEmail != _user.Email && !_user.IsAdmin))
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var owner = await _userManager.FindByEmailAsync(course.OwnerEmail);

					if (owner == null)
					{
						// TODO: Return a validation error to the user.
						return NotFound();
					}

					_context.Update(course);
					_context.Entry(course).Collection(c => c.Affiliates).Load();

					DropUsersWithRole(course, CourseJoinTag.ROLE_STUDENT);
					DropUsersWithRole(course, CourseJoinTag.ROLE_INSTRUCTOR);
					course.Affiliates = null;
					_context.Update(course);
					await _context.SaveChangesAsync();

					//TODO: What if we're removing someone who was in groups?
					foreach (string e in newStudents.Split(","))
					{
						var u = await _userManager.FindByEmailAsync(e);
						if (u != null) JoinCourse(u, course, CourseJoinTag.ROLE_STUDENT);
					}

					foreach (string e in newInstructors.Split(","))
					{
						var u = await _userManager.FindByEmailAsync(e);
						if (u != null) JoinCourse(u, course, CourseJoinTag.ROLE_INSTRUCTOR);
					}

					if (course.RequireEnrollmentKey && course.EnrollmentKey != null)
					{
						course.EnrollmentKey = course.EnrollmentKey.HashUTF8ToBase64();
					}

					_context.Update(course);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!CourseExists(course.ID))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Details), new { id = course.ID });
			}
			return View(course);
		}

		// GET: Course/Delete/5
		public async Task<IActionResult> Delete(Guid? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var _user = await _userManager.GetUserAsync(User);

			var course = await _context.Courses
				.SingleOrDefaultAsync(m => m.ID == id);
			if (course == null || (course.OwnerEmail != _user.Id && !_user.IsAdmin))
			{
				return NotFound();
			}

			return View(course);
		}

		// POST: Course/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(Guid id)
		{
			var _user = await _userManager.GetUserAsync(User);
			var course = await _context.Courses
				.Include(c => c.Affiliates)
				.SingleOrDefaultAsync(m => m.ID == id);

			if (course.OwnerEmail != _user.Email && !_user.IsAdmin)
			{
				return NotFound();
			}
			_context.Courses.Remove(course);
			DropUsersWithRole(course, CourseJoinTag.ROLE_STUDENT);
			DropUsersWithRole(course, CourseJoinTag.ROLE_INSTRUCTOR);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Join(Guid id)
		{
			var course = await _context.Courses
				.SingleOrDefaultAsync(c => c.ID == id);

			if (course == null) return NotFound();

			return View(course);
		}

		[HttpPost, ActionName("Join")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> JoinConfirmed(Guid id, string enrollmentKey)
		{
			var user = await _userManager.GetUserAsync(User);
			var course = await _context.Courses
				.Include(c => c.Affiliates)
				.SingleOrDefaultAsync(c => c.ID == id);

			if (course == null) return NotFound();

			if (course.RequireEnrollmentKey)
			{
				if (enrollmentKey == null ||
					enrollmentKey.HashUTF8ToBase64() != course.EnrollmentKey)
				{
					ViewData["Error"] = "The enrollment key was incorrect.";
					return View(course);
				}
			}

			JoinCourse(user, course, CourseJoinTag.ROLE_STUDENT);
			_context.Update(course);
			await _context.SaveChangesAsync();
			return RedirectToAction("Index", "Home");
		}

		private bool CourseExists(Guid id)
		{
			return _context.Courses.Any(e => e.ID == id);
		}

		private void JoinCourse(ApplicationUser user, Course course, int affiliation)
		{
			var cjt = new CourseJoinTag();
			cjt.ApplicationUser = user;
			cjt.Course = course;
			cjt.Role = affiliation;

			if (course.Affiliates == null)
			{
				course.Affiliates = new List<CourseJoinTag>();
			}
			course.Affiliates.Add(cjt);
		}

		private void DropUsersWithRole(Course course, int role)
		{
			foreach (CourseJoinTag t in course.Affiliates.Where(cjt => cjt.Role == role))
			{
				_context.Remove(t);
			}
		}
	}
}
