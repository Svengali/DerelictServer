namespace game;

using Microsoft.AspNetCore.Mvc;

public abstract class CoreController : Controller
{
    // returns the current authenticated account (null if not logged in)
    public ent.Account Account => (ent.Account)HttpContext.Items["Account"];
}