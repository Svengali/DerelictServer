namespace game;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using WebApi.Models.Accounts;

//using WebApi.Models.Accounts;
//using WebApi.Services;

[auth.Authorize]
public class AccountsController : CoreController
{
	private readonly svc.IAccountService _accountService;

	public AccountsController( svc.IAccountService accountService )
	{
		_accountService = accountService;

	}

	[auth.AllowAnonymous]
	public ActionResult Login()
	{
		var cookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Expires = DateTime.UtcNow.AddDays(7)
		};
		Response.Cookies.Append( "refreshToken", "test_token", cookieOptions );

		return View();
	}

	[auth.AllowAnonymous]
	public ActionResult Signup()
	{
		var cookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Expires = DateTime.UtcNow.AddDays(7)
		};
		Response.Cookies.Append( "refreshToken", "signup", cookieOptions );

		return View();
	}






	[auth.AllowAnonymous]
	[HttpPost]
	public ActionResult<AuthenticateResponse> Authenticate( AuthenticateRequest model )
	{
		var response = _accountService.Authenticate(model, ipAddress());
		setTokenCookie( response.RefreshToken );
		return Ok( response );
	}

	[auth.AllowAnonymous]
	[HttpPost]
	public ActionResult<AuthenticateResponse> RefreshToken()
	{
		var refreshToken = Request.Cookies["refreshToken"];
		var response = _accountService.RefreshToken(refreshToken, ipAddress());
		setTokenCookie( response.RefreshToken );
		return Ok( response );
	}

	[HttpPost]
	public IActionResult RevokeToken( RevokeTokenRequest model )
	{
		// accept token from request body or cookie
		var token = model.Token ?? Request.Cookies["refreshToken"];

		if( string.IsNullOrEmpty( token ) )
			return BadRequest( new { message = "Token is required" } );

		// users can revoke their own tokens and admins can revoke any tokens
		if( !Account.OwnsToken( token ) && Account.Role != ent.Role.Admin )
			return Unauthorized( new { message = "Unauthorized" } );

		_accountService.RevokeToken( token, ipAddress() );
		return Ok( new { message = "Token revoked" } );
	}

	[auth.AllowAnonymous]
	[HttpPost]
	public IActionResult Register( RegisterRequest model )
	{
		_accountService.Register( model, Request.Headers["origin"] );
		return Ok( new { message = "Registration successful, please check your email for verification instructions" } );
	}

	[auth.AllowAnonymous]
	public IActionResult VerifyEmail( VerifyEmailRequest model )
	{
		_accountService.VerifyEmail( model.Token );
		return Ok( new { message = "Verification successful, you can now login" } );
	}

	[auth.AllowAnonymous]
	[HttpPost]
	public IActionResult ForgotPassword( ForgotPasswordRequest model )
	{
		_accountService.ForgotPassword( model, Request.Headers["origin"] );
		return Ok( new { message = "Please check your email for password reset instructions" } );
	}

	[auth.AllowAnonymous]
	[HttpPost]
	public IActionResult ValidateResetToken( ValidateResetTokenRequest model )
	{
		_accountService.ValidateResetToken( model );
		return Ok( new { message = "Token is valid" } );
	}

	[auth.AllowAnonymous]
	[HttpPost]
	public IActionResult ResetPassword( ResetPasswordRequest model )
	{
		_accountService.ResetPassword( model );
		return Ok( new { message = "Password reset successful, you can now login" } );
	}

	[auth.Authorize( ent.Role.Admin )]
	[HttpGet]
	public ActionResult<IEnumerable<AccountResponse>> GetAll()
	{
		var accounts = _accountService.GetAll();
		return Ok( accounts );
	}

	[HttpGet( "{id:int}" )]
	public ActionResult<AccountResponse> GetById( Guid id )
	{
		// users can get their own account and admins can get any account
		if( id != Account.Id && Account.Role != ent.Role.Admin )
			return Unauthorized( new { message = "Unauthorized" } );

		var account = _accountService.GetById(id);
		return Ok( account );
	}

	[auth.Authorize( ent.Role.Admin )]
	[HttpPost]
	public ActionResult<AccountResponse> Create( CreateRequest model )
	{
		var account = _accountService.Create(model);
		return Ok( account );
	}

	[HttpPut( "{id:int}" )]
	public ActionResult<AccountResponse> Update( Guid id, UpdateRequest model )
	{
		// users can update their own account and admins can update any account
		if( id != Account.Id && Account.Role != ent.Role.Admin )
			return Unauthorized( new { message = "Unauthorized" } );

		// only admins can update role
		if( Account.Role != ent.Role.Admin )
			model.Role = null;

		var account = _accountService.Update(id, model);
		return Ok( account );
	}

	[HttpDelete( "{id:int}" )]
	public IActionResult Delete( Guid id )
	{
		// users can delete their own account and admins can delete any account
		if( id != Account.Id && Account.Role != ent.Role.Admin )
			return Unauthorized( new { message = "Unauthorized" } );

		_accountService.Delete( id );
		return Ok( new { message = "Account deleted successfully" } );
	}

	// helper methods

	private void setTokenCookie( string token )
	{
		var cookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Expires = DateTime.UtcNow.AddDays(7)
		};
		Response.Cookies.Append( "refreshToken", token, cookieOptions );
	}

	private string ipAddress()
	{
		if( Request.Headers.ContainsKey( "X-Forwarded-For" ) )
			return Request.Headers["X-Forwarded-For"];
		else
			return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
	}
}