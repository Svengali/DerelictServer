namespace game;

using Microsoft.AspNetCore.Mvc;

[Controller]
public abstract class BaseController : ControllerBase
{
    // returns the current authenticated account (null if not logged in)
    public ent.Account Account => (ent.Account)HttpContext.Items["Account"];
}