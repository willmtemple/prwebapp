using System;
using System.Linq;
using System.Collections.Generic;
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

namespace PeerReviewWeb.Controllers
{
    [Authorize]
    public class AssignmentController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AssignmentController(
            ApplicationDbContext context,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager
            )
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create(Guid forCourse)
        {
            var course = await _context.Courses.SingleOrDefaultAsync(c => c.ID == forCourse);
            if (course == null)
            {
                // TODO: Message
                return NotFound();
            }
            ViewData["forCourse"] = course.ID;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Assignment assignment, Guid forCourse)
        {
            if (ModelState.IsValid)
            {
                var course = await _context.Courses
                    .Include(c => c.Affiliates)
                    .Include(c => c.Assignments)
                    .SingleOrDefaultAsync(c => c.ID == forCourse);

                var user = await _userManager.GetUserAsync(User);

                // This is gross and should be abstracted into some kind of permissions manager
                // I.e. user.Can<Course>(Permissions.CREATE)
                // or user.CanCreate<Course>()
                if (course == null ||
                    (course.RoleFor(user) != CourseJoinTag.ROLE_INSTRUCTOR &&
                     course.OwnerEmail != user.Email))
                {
                    return NotFound();
                }

                // All of this is gone once EF Core supports Lazy loading.
                _context.Entry(course).Collection(c => c.Assignments).Load();

                assignment.ID = Guid.NewGuid();
                course.Assignments.Add(assignment);
                assignment.Seq = course.Assignments.Count;
                _context.Update(course);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(CourseController.Details), "Course", new { id = course.ID });
            }

            // TODO: return assignment validation errors
            return View(assignment);
        }
    }
}