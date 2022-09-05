using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace game
{
	public class PlayerController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}
