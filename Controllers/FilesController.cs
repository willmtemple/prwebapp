using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using PeerReviewWeb.Data;

namespace PeerReviewWeb.Controllers
{
	[Authorize]
	public class FilesController : Controller
	{

		private readonly ApplicationDbContext _context;
		private readonly IConfiguration _config;

		public FilesController(
			ApplicationDbContext context,
			IConfiguration config)
		{
			_context = context;
			_config = config;
		}

		// TODO: Access control on files
		public async Task<IActionResult> Get(Guid id)
		{
			var fr = await _context.FileRefs
				.SingleOrDefaultAsync(f => f.ID == id);

			if (fr == null)
			{
				return NotFound();
			}

			var content = fr.Open(_config["Storage:BasePath"], FileMode.Open);
			Response.Headers.Add("Content-Disposition", $"attachment; filename={fr.Name}");
			return File(content, fr.ContentType);
		}
	}
}