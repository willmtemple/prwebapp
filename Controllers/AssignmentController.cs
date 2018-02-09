using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using PeerReviewWeb.Data;
using PeerReviewWeb.Models;
using PeerReviewWeb.Models.FormSchema;
using PeerReviewWeb.Models.CourseModels;
using PeerReviewWeb.Models.JoinTagModels;
using PeerReviewWeb.Models.AssignmentViewModels;
using Newtonsoft.Json;

namespace PeerReviewWeb.Controllers
{
	[Authorize]
	public class AssignmentController : Controller
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _context;
		private readonly IConfiguration _config;

		public AssignmentController(
			ApplicationDbContext context,
			SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			IConfiguration config
			)
		{
			_context = context;
			_signInManager = signInManager;
			_userManager = userManager;
			_config = config;
		}

		public async Task<IActionResult> ForCourse(Guid id)
		{
			var course = await _context.Courses
									   .Include(c => c.Assignments)
									   .SingleOrDefaultAsync(c => c.ID == id);
			if (course == null)
			{
				return NotFound();
			}
			ViewData["CourseName"] = course.Name;
			return View(course.Assignments);
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
		[ValidateAntiForgeryToken]
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
				assignment.Course = course;
				course.Assignments.Add(assignment);
				assignment.Seq = course.Assignments.Count;
				_context.Update(course);
				await _context.SaveChangesAsync();

				return RedirectToAction(nameof(CourseController.Details), "Course", new { id = course.ID });
			}

			ViewData["Error"] = ModelState.ValidationState.ToString();
			ViewData["forCourse"] = forCourse;
			// TODO: return assignment validation errors
			return View(assignment);
		}

		public async Task<IActionResult> Details(Guid id)
		{
			var asg = await _context.Assignments
				.Include(a => a.Stages)
				.Include(a => a.Course)
				.ThenInclude(c => c.Affiliates)
				.SingleOrDefaultAsync(c => c.ID == id);

			if (asg == null) return NotFound();

			var user = await _userManager.GetUserAsync(User);

			switch (asg.Course.RoleFor(user))
			{
				case CourseJoinTag.ROLE_STUDENT:
					return RedirectToAction(nameof(StudentDetails), new { id = id });
				case CourseJoinTag.ROLE_INSTRUCTOR:
					return RedirectToAction(nameof(InstructorDetails), new { id = id });
				default:
					if (user.IsAdmin)
					{
						return RedirectToAction(nameof(InstructorDetails), new { id = id });
					}
					else
					{
						return NotFound();
					}
			}
		}

		public async Task<IActionResult> StudentDetails(Guid id)
		{
			var asg = await _context.Assignments
				.Include(a => a.Stages)
				.Include(a => a.Course)
					.ThenInclude(c => c.Affiliates)
				.Include(a => a.Groups)
					.ThenInclude(g => g.Members)
						.ThenInclude(gjt => gjt.ApplicationUser)
				.SingleOrDefaultAsync(a => a.ID == id);

			if (asg == null) return NotFound();

			var user = await _userManager.GetUserAsync(User);

			if (!(asg.Course.RoleFor(user) == CourseJoinTag.ROLE_STUDENT))
			{
				return NotFound();
			}

			if (asg.GroupSatisfied(user))
			{
				var group = asg.GroupFor(user);
				var subs = _context.Submissions
						.Include(s => s.AssignmentStage)
						.ThenInclude(stage => stage.Assignment)
						.Include(s => s.Submitter)
						.Include(s => s.Files)
						.OrderByDescending(s => s.AssignmentStage.Seq)
						.Where(s =>
							(s.Submitter.Id == user.Id && s.AssignmentStage.Assignment.ID == asg.ID)
							|| (group != null && s.ForGroup.ID == group.ID));
				var xsubs = subs.Select(s => new ExtendedSubmission
				{
					Submission = s,
					Reviews = _context.Reviews
						.Where(r => r.Submission.ID == s.ID)
						.OrderBy(r => r.TimeStamp)
						.ToList(),
				});
				var vm = new StudentDetailsViewModel
				{
					Group = group,
					Assignment = asg,
					// TODO: StageFprUser is disgusting
					Stage = (group == null) ? asg.StageForUser(user, _context) : group.NextStage(),
					Submissions = await xsubs.ToListAsync(),
				};

				return View(vm);
			}

			return RedirectToAction(nameof(GroupSetup), new
			{
				forAssignment = id
			});
		}

		public async Task<IActionResult> GroupSetup(Guid forAssignment, string error = null)
		{
			var asg = await _context.Assignments
				.Include(a => a.Course)
				.ThenInclude(c => c.Affiliates)
				.Include(a => a.Groups)
				.ThenInclude(g => g.Members)
				.SingleOrDefaultAsync(a => a.ID == forAssignment);

			if (asg == null) return NotFound();

			var user = await _userManager.GetUserAsync(User);

			if (!(asg.Course.RoleFor(user) == CourseJoinTag.ROLE_STUDENT))
			{
				return NotFound();
			}

			if (error != null)
			{
				ViewData["Error"] = error;
			}

			return View(new GroupSetupViewModel
			{
				Group = asg.GroupFor(user),
				Assignment = asg,
			});
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> InviteToGroup(string userEmail, Guid forAssignment)
		{
			var asg = await _context.Assignments
				.Include(a => a.Course)
				.ThenInclude(c => c.Affiliates)
				.Include(a => a.Groups)
				.ThenInclude(grp => grp.Members)
				.SingleOrDefaultAsync(a => a.ID == forAssignment);

			if (asg == null) return NotFound();

			var user = await _userManager.GetUserAsync(User);

			if (!(asg.Course.RoleFor(user) == CourseJoinTag.ROLE_STUDENT))
			{
				return NotFound();
			}

			var otherUser = await _userManager.FindByEmailAsync(userEmail);

			if (otherUser == null || asg.Course.RoleFor(otherUser) != CourseJoinTag.ROLE_STUDENT)
			{
				return RedirectToAction(nameof(GroupSetup), new
				{
					forAssignment = forAssignment,
					error = "That user does not exist or is not a member of this course."
				});
			}
			else if (asg.GroupFor(otherUser) != null)
			{
				return RedirectToAction(nameof(GroupSetup), new
				{
					forAssignment = forAssignment,
					error = "That user is already in a group!"
				});
			}

			Group g = asg.GroupFor(user);

			if (g == null)
			{
				// User does not already have a group, so create one
				var members = new List<GroupJoinTag>();

				g = new Group
				{
					ID = Guid.NewGuid(),
					Assignment = asg,
					Members = members,
					CurrentStage = Assignment.START_STAGE,
				};

				members.Add(new GroupJoinTag
				{
					Group = g,
					ApplicationUser = user,
				});

				asg.Groups.Add(g);

				_context.Update(asg);
			}
			if (user.Id != otherUser.Id)
			{
				var invite = new GroupInvitation
				{
					Group = g,
					ApplicationUser = otherUser,
				};

				_context.GroupInvitations.Add(invite);
			}

			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(GroupSetup), new { forAssignment = forAssignment });
		}

		public async Task<IActionResult> JoinGroup(int ID)
		{
			var groupInvitation = await _context.GroupInvitations
				.Include(gi => gi.ApplicationUser)
				.Include(gi => gi.Group)
					.ThenInclude(g => g.Assignment)
					.ThenInclude(a => a.Course)
				.Include(gi => gi.Group)
					.ThenInclude(g => g.Members)
					.ThenInclude(gjt => gjt.ApplicationUser)
				.SingleOrDefaultAsync(gi => gi.ID == ID);

			var user = await _userManager.GetUserAsync(User);

			if (user != groupInvitation.ApplicationUser)
			{
				return NotFound();
			}

			return View(groupInvitation);
		}

		[HttpPost, ActionName("JoinGroup")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> JoinGroupConfirmed(int ID)
		{
			var groupInvitation = await _context.GroupInvitations
				.Include(gi => gi.ApplicationUser)
				.Include(gi => gi.Group)
					.ThenInclude(g => g.Assignment)
					.ThenInclude(a => a.Course)
				.Include(gi => gi.Group)
					.ThenInclude(g => g.Members)
					.ThenInclude(gjt => gjt.ApplicationUser)
				.SingleOrDefaultAsync(gi => gi.ID == ID);

			var user = await _userManager.GetUserAsync(User);

			if (user != groupInvitation.ApplicationUser)
			{
				return NotFound();
			}

			groupInvitation.Group.Members.Add(new GroupJoinTag
			{
				ApplicationUser = user,
				Group = groupInvitation.Group,
			});

			_context.Update(groupInvitation.Group);

			foreach (GroupInvitation gi in _context.GroupInvitations
				.Where(gi => gi.ApplicationUser == user))
			{
				_context.Remove(gi);
			}

			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(HomeController.Index), "Home");
		}

		public async Task<IActionResult> InstructorDetails(Guid id)
		{
			var asg = await _context.Assignments
				.Include(a => a.Stages)
				.Include(a => a.Course)
				.ThenInclude(c => c.Affiliates)
				.SingleOrDefaultAsync(a => a.ID == id);

			if (asg == null) return NotFound();

			var user = await _userManager.GetUserAsync(User);

			if (!(asg.Course.RoleFor(user) == CourseJoinTag.ROLE_INSTRUCTOR)
				&& !(asg.Course.OwnerEmail == user.Email)
				&& !(user.IsAdmin))
			{
				return NotFound();
			}

			var subs = _context.Submissions
				.Include(s => s.AssignmentStage)
				.ThenInclude(stg => stg.Assignment)
				.Include(s => s.ForGroup)
				.ThenInclude(g => g.Members)
				.ThenInclude(t => t.ApplicationUser)
				.Include(s => s.Files)
				.Include(s => s.Submitter)
				.OrderByDescending(s => s.AssignmentStage.Seq)
				.Where(s => s.AssignmentStage.AssignmentId == asg.ID);

			var incompleteReviews = _context.ReviewAssignments
				.Include(ra => ra.ApplicationUser)
				.Include(ra => ra.Submission)
				.ThenInclude(s => s.AssignmentStage)
				.OrderByDescending(ra => ra.TimeStamp)
				.Where(ra => !ra.Complete && ra.Submission.AssignmentStage.AssignmentId == asg.ID);

			var esubs = subs.Select(s => new ExtendedSubmission
			{
				Submission = s,
				Reviews = _context.Reviews
					.Where(r => r.Submission.ID == s.ID)
					.ToList()
			});

			var vm = new InstructorDetailsViewModel
			{
				Assignment = asg,
				Submissions = await esubs.ToListAsync(),
				IncompleteReviews = await incompleteReviews.ToListAsync()
			};

			return View(vm);
		}

		public async Task<IActionResult> CreateStage(Guid forAssignment)
		{
			var asg = _context.Assignments.SingleOrDefaultAsync(a => a.ID == forAssignment);

			if ((await asg) == null)
			{
				return NotFound();
			}

			ViewData["forAssignment"] = forAssignment;
			return View();
		}

		[HttpPost, ActionName("CreateStage")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateStagePost(AssignmentStage stage, Guid forAssignment)
		{
			if (ModelState.IsValid)
			{
				var assignment = await _context.Assignments
					.Include(a => a.Course)
					.ThenInclude(c => c.Affiliates)
					.Include(a => a.Stages)
					.SingleOrDefaultAsync(a => a.ID == forAssignment);

				var user = await _userManager.GetUserAsync(User);

				if (assignment == null || (
						assignment.Course.RoleFor(user) != CourseJoinTag.ROLE_INSTRUCTOR &&
						assignment.Course.OwnerEmail != user.Email &&
						!user.IsAdmin))
				{
					return NotFound();
				}

				if (assignment.Stages.Any(s => s.Id == stage.Id))
				{
					ViewData["Error"] = "An Assignment with that ID already exists.";
					ViewData["forAssignment"] = forAssignment;
					return View(stage);
				}

				if (stage.ReviewSchemaJSON != null)
				{
					try
					{
						JsonConvert.DeserializeObject<Schema>(
							stage.ReviewSchemaJSON
						);
					}
					catch (JsonReaderException ex)
					{
						ViewData["Error"] = $"JSON Validation error: {ex}";
						ViewData["forAssignment"] = forAssignment;
						return View(stage);
					}
				}

				stage.Assignment = assignment;
				stage.Seq = assignment.Stages.Count + 1;
				_context.Add(stage);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Details), new { id = forAssignment });
			}
			else
			{
				ViewData["Error"] = ModelState.ValidationState.ToString();
				ViewData["forAssignment"] = forAssignment;
				return View(stage);
			}
		}

		public async Task<IActionResult> Submit(Guid id, string stage)
		{
			var asg = await _context.Assignments
				.Include(a => a.Stages)
				.SingleOrDefaultAsync(a => a.ID == id);

			var s = asg?.Stages.FirstOrDefault(stg => stg.Id == stage);

			ViewData["AssignmentID"] = asg.ID;
			ViewData["StageID"] = s.Id;

			return View();
		}

		[HttpPost, ActionName("Submit")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SubmitPost(Submission sub, Guid id, string stage, IEnumerable<IFormFile> uploadFiles)
		{
			if (ModelState.IsValid)
			{
				var asg = await _context.Assignments
					.Include(a => a.Stages)
					.Include(a => a.Course)
					.Include(a => a.Groups)
						.ThenInclude(g => g.Members)
							.ThenInclude(gjt => gjt.ApplicationUser)
					.SingleOrDefaultAsync(a => a.ID == id);
				var astage = asg.Stages.SingleOrDefault(stg => stg.Id == stage);

				var user = await _userManager.GetUserAsync(User);

				var group = asg.GroupFor(user);

				if (group != null) _context.Entry(group).Collection(g => g.Members).Load();

				var presub = await _context.Submissions
					.Include(s => s.AssignmentStage)
					.Include(s => s.Submitter)
					.Include(s => s.ForGroup)
					.SingleOrDefaultAsync(s =>
						s.AssignmentStage == astage
						&& (s.Submitter.Id == user.Id || (group != null && s.ForGroup.ID == group.ID)));

				if (astage == null || presub != null)
				{
					return NotFound();
				}

				var filerefs = new List<FileRef>();

				foreach (IFormFile f in uploadFiles)
				{
					var fileref = new FileRef
					{
						ID = Guid.NewGuid(),
						Owner = user,
						Name = f.FileName,
						ContentType = f.ContentType,
					};

					using (var stream = fileref.Open(_config["Storage:BasePath"], FileMode.Create))
					{
						await f.CopyToAsync(stream);
					}

					await _context.FileRefs.AddAsync(fileref);

					filerefs.Add(fileref);
				}

				sub.Submitter = user;
				sub.ForGroup = group;
				// TODO: change to allow students to modify their submissions
				sub.Confirmed = true;
				sub.AssignmentStage = astage;
				sub.ID = Guid.NewGuid();
				sub.Files = filerefs;
				sub.TimeStamp = DateTime.Now;

				if (group == null)
				{
					var _stageHolder = await _context.StageHolders
						.SingleOrDefaultAsync(sh =>
							sh.AssignmentID == asg.ID &&
							sh.UserID == user.Id);
					if (_stageHolder != null)
					{
						_stageHolder.CurrentStage += 1;
						_context.Update(_stageHolder);
					}
					else
					{
						var newSH = new StageHolder
						{
							AssignmentID = asg.ID,
							UserID = user.Id,
							CurrentStage = Assignment.START_STAGE + 1,
						};
						_context.Add(newSH);
					}
				}
				else
				{
					group.CurrentStage += 1;
					_context.Update(group);
				}

				await _context.AddAsync(sub);

				// Feels wrong to await this. It should be a continuation of some kind that
				// doesn't need to be performed on this thread. Possibly Save Changes and
				// start an assignment pass
				if (astage.IsPeerReviewed)
				{
					await ReviewPass(sub, astage.ReviewsPerStudent);
				}

				await _context.SaveChangesAsync();

				return RedirectToAction(nameof(StudentDetails), new { id = asg.ID });
			}
			else
			{
				ViewData["Error"] = string.Join("; ", ModelState.Values.Select(v => v.Errors));
				ViewData["AssignmentID"] = id;
				ViewData["StageID"] = stage;
				return View(sub);
			}
		}

		// This procedure is a tragedy, but I hope that much of it may be
		// removed if this controller is restructured
		private async Task ReviewPass(Submission sub, int mandatoryReviews)
		{
			var groupSize = sub.ForGroup?.Members.Count ?? 1;
			var reviewsPerSub = mandatoryReviews;
			if (groupSize > 1) reviewsPerSub *= sub.AssignmentStage.Assignment.MinGroupSize;
			var priors = _context.Submissions
				.Include(s => s.AssignmentStage)
				.Include(s => s.ForGroup)
				.ThenInclude(g => g.Members)
				.ThenInclude(gjt => gjt.ApplicationUser)
				.Include(s => s.Submitter)
				.Where(s =>
					s.AssignmentStage.Id == sub.AssignmentStage.Id &&
					s.AssignmentStage.AssignmentId == sub.AssignmentStage.AssignmentId);
			var reviewAssignments = _context.ReviewAssignments
				.Include(ra => ra.ApplicationUser)
				.Include(ra => ra.Submission)
				.ThenInclude(s => s.AssignmentStage)
				.Where(ra =>
					ra.Submission.AssignmentStage.Id == sub.AssignmentStage.Id &&
					ra.Submission.AssignmentStage.AssignmentId == sub.AssignmentStage.AssignmentId);

			var underReviewedPriors = priors
				.Where(s => !(reviewAssignments
								.Where(ra => ra.Submission.ID == s.ID)
								.Count() > reviewsPerSub));

			var underReviewingSubmitters = new List<ApplicationUser>();

			// The distinction between group/individual assignments must be destroyed before
			// it infects all the code.
			foreach (Submission s in priors)
			{
				if (s.ForGroup == null)
				{
					if (reviewAssignments
							.Where(a => a.ApplicationUser.Id == s.Submitter.Id)
							.Count()
						< mandatoryReviews)
					{
						underReviewingSubmitters.Add(s.Submitter);
					}
				}
				else
				{
					var firstUnderReviewer = s.ForGroup.Members
						.Select(t => t.ApplicationUser)
						.Where(u => reviewAssignments
										.Where(ra => ra.ApplicationUser.Id == u.Id)
										.Count()
									< mandatoryReviews)
						.FirstOrDefault();

					if (firstUnderReviewer != null)
					{
						underReviewingSubmitters.Add(firstUnderReviewer);
					}
				}
			}

			// Assign new submitters to old submissions

			ApplicationUser[] needsAssignment = new ApplicationUser[groupSize];
			if (sub.ForGroup == null)
			{
				needsAssignment[0] = sub.Submitter;
			}
			else
			{
				int j = 0;
				foreach (GroupJoinTag gjt in sub.ForGroup.Members)
				{
					await _context.Entry(gjt).Reference(t => t.ApplicationUser).LoadAsync();
					needsAssignment[j] = gjt.ApplicationUser;
					j += 1;
				}
			}

			int i = 0;
			foreach (Submission s in underReviewedPriors.Take(groupSize))
			{
				ReviewAssignment newRa = new ReviewAssignment
				{
					ID = Guid.NewGuid(),
					Complete = false,
					ApplicationUser = needsAssignment[i],
					Submission = s,
					TimeStamp = DateTime.Now
				};
				await _context.AddAsync(newRa);
				i += 1;
			}

			// Assign old submitters to new submission

			foreach (ApplicationUser u in underReviewingSubmitters.Take(reviewsPerSub))
			{
				ReviewAssignment newRa = new ReviewAssignment
				{
					ID = Guid.NewGuid(),
					Complete = false,
					ApplicationUser = u,
					Submission = sub,
					TimeStamp = DateTime.Now,
				};
				await _context.AddAsync(newRa);
			}
		}
	}
}