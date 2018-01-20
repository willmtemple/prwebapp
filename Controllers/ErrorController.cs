using Microsoft.AspNetCore.Mvc;

namespace PeerReviewWeb.Controllers
{
	public class ErrorController : Controller
	{
		[Route("Error/404")]
		public IActionResult ErrorNotFound()
		{
			return View();
		}
	}
}