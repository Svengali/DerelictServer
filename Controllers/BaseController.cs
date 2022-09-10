namespace game;

using Microsoft.AspNetCore.Mvc;

public abstract class CoreController : Controller
{
    // returns the current authenticated account (null if not logged in)
    public svc.PlayerData Account => (svc.PlayerData)HttpContext.Items["Account"];
}

