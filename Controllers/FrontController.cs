using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace game
{
	public class FrontController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}
