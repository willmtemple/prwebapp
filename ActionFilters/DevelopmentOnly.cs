using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PeerReviewWeb.ActionFilters
{
	public class DevelopmentOnly : ActionFilterAttribute { /* This only exists to serve the ActionFilter */ }

	public class DevelopmentZoneActionFilter : IActionFilter
	{
		private readonly IHostingEnvironment _e;
		public DevelopmentZoneActionFilter(IHostingEnvironment environment) {
			_e = environment;
		}
		public void OnActionExecuting(ActionExecutingContext context)
		{
			if (_e.IsDevelopment() && HasAttribute(context))
			{
				context.Result = new NotFoundResult();
			}
		}
		public void OnActionExecuted(ActionExecutedContext context) { /* Do nothing */ }

		private  bool HasAttribute(ActionExecutingContext context) {

			var attr = (DevelopmentOnly)context.Controller
				.GetType()
				.GetCustomAttributes(typeof(DevelopmentOnly), false)
				.SingleOrDefault();

			return attr != null;
		}
	}
}