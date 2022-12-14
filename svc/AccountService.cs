
namespace svc;

using ent;

//using AutoMapper;
//using BCrypt.Net;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using WebApi.Helpers;
using WebApi.Models.Accounts;

public interface IAccountService
{
	AuthenticateResponse Authenticate( AuthenticateRequest model, string ipAddress );
	AuthenticateResponse RefreshToken( string token, string ipAddress );
	void RevokeToken( string token, string ipAddress );
	void Register( RegisterRequest model, string origin );
	void VerifyEmail( string token );
	void ForgotPassword( ForgotPasswordRequest model, string origin );
	void ValidateResetToken( ValidateResetTokenRequest model );
	void ResetPassword( ResetPasswordRequest model );
	IEnumerable<AccountResponse> GetAll();
	AccountResponse GetById( Guid id );
	AccountResponse Create( CreateRequest model );
	AccountResponse Update( Guid id, UpdateRequest model );
	void Delete( Guid id );
}

public class AccountService : IAccountService
{
	//private readonly DataContext _context;
	private readonly auth.IJwtUtils _jwtUtils;
	//private readonly IMapper _mapper;
	private readonly AppSettings _appSettings;
	private readonly IEmailService _emailService;
	private readonly IUser _player;

	public AccountService(
			// PORT DataContext context,
			auth.IJwtUtils jwtUtils,
			// PORTIMapper mapper,
			IUser player,
			IOptions<AppSettings> appSettings,
			IEmailService emailService )
	{
		// PORT_context = context;
		_jwtUtils = jwtUtils;
		// PORT_mapper = mapper;
		_player = player;
		_appSettings = appSettings.Value;
		_emailService = emailService;
	}

	public AuthenticateResponse Authenticate( AuthenticateRequest model, string ipAddress )
	{
		/* PORT
		var account = _context.Accounts.SingleOrDefault(x => x.Email == model.Email);

		// validate
		if( account == null || !account.IsVerified || !BCrypt.Verify( model.Password, account.PasswordHash ) )
			throw new AppException( "Email or password is incorrect" );

		// authentication successful so generate jwt and refresh tokens
		var jwtToken = _jwtUtils.GenerateJwtToken(account);
		var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
		account.RefreshTokens.Add( refreshToken );

		// remove old refresh tokens from account
		removeOldRefreshTokens( account );

		// save changes to db
		_context.Update( account );
		_context.SaveChanges();

		var response = _mapper.Map<AuthenticateResponse>(account);
		response.JwtToken = jwtToken;
		response.RefreshToken = refreshToken.Token;
		return response;
		/*/
		return null;
		//*/
	}

	public AuthenticateResponse RefreshToken( string token, string ipAddress )
	{
		/* PORT
		var account = getAccountByRefreshToken(token);
		var refreshToken = account.RefreshTokens.Single(x => x.Token == token);

		if( refreshToken.IsRevoked )
		{
			// revoke all descendant tokens in case this token has been compromised
			revokeDescendantRefreshTokens( refreshToken, account, ipAddress, $"Attempted reuse of revoked ancestor token: {token}" );
			_context.Update( account );
			_context.SaveChanges();
		}

		if( !refreshToken.IsActive )
			throw new AppException( "Invalid token" );

		// replace old refresh token with a new one (rotate token)
		var newRefreshToken = rotateRefreshToken(refreshToken, ipAddress);
		account.RefreshTokens.Add( newRefreshToken );

		// remove old refresh tokens from account
		removeOldRefreshTokens( account );

		// save changes to db
		_context.Update( account );
		_context.SaveChanges();

		// generate new jwt
		var jwtToken = _jwtUtils.GenerateJwtToken(account);

		// return data in authenticate response object
		var response = _mapper.Map<AuthenticateResponse>(account);
		response.JwtToken = jwtToken;
		response.RefreshToken = newRefreshToken.Token;
		return response;
		/*/
		return null;
		//*/
	}

	public void RevokeToken( string token, string ipAddress )
	{
		/* PORT
		var account = getAccountByRefreshToken(token);
		var refreshToken = account.RefreshTokens.Single(x => x.Token == token);

		if( !refreshToken.IsActive )
			throw new AppException( "Invalid token" );

		// revoke token and save
		revokeRefreshToken( refreshToken, ipAddress, "Revoked without replacement" );
		_context.Update( account );
		_context.SaveChanges();
		/*/
		return;
		//*/
	}

	public async void Register( RegisterRequest model, string origin )
	{
		//* PORT
		// validate

		var playerExists = await _player.IsRegistered( model.Email );

		if( playerExists == IUser.UserRes.WinUserRegistered )
		{
			// send already registered error in email to prevent account enumeration
			sendAlreadyRegisteredEmail( model.Email, origin );
			return;
		}

		var id = Guid.NewGuid();
		var isFirstAccount = false; //_context.Accounts.Count() == 0;
		var role = isFirstAccount ? ent.Role.Admin : ent.Role.User;
		var created = DateTime.UtcNow;
		var verificationToken = generateVerificationToken();

		// hash password
		var passwordHash = BCrypt.Net.BCrypt.HashPassword( model.Password );


		UserData data = new(
			id, 
			model.DisplayName, 
			model.Email, 
			passwordHash, 
			model.AcceptTerms, 
			role, 
			verificationToken, 
			null, 
			"", 
			null, 
			null, 
			DateTime.Now, 
			DateTime.Now,
			ImmutableList<RefreshToken>.Empty
		);

		var playerRegister = await _player.Register( data );

		var tokenToPlayer = new svc.TokenToUser( verificationToken, model.Email );

		svc.Data.Save( tokenToPlayer, verificationToken );

		sendVerificationEmail( data, origin, verificationToken );


		/*
		// map model to new account object
		var account = _mapper.Map<ent.Account>(model);

		// first registered account is an admin
		var isFirstAccount = _context.Accounts.Count() == 0;
		account.Role = isFirstAccount ? ent.Role.Admin : ent.Role.User;
		account.Created = DateTime.UtcNow;
		account.VerificationToken = generateVerificationToken();

		// hash password
		account.PasswordHash = BCrypt.HashPassword( model.Password );

		// save account
		_context.Accounts.Add( account );
		_context.SaveChanges();
		*/

		// send email
		/*/
		return;
		//*/
	}

	public void VerifyEmail( string token )
	{
		svc.Data.Load( token, out TokenToUser tokenToPlayer );


		//* PORT

		svc.Data.Load( tokenToPlayer.UserId, out UserData player );

		//var account = _context.Accounts.SingleOrDefault(x => x.VerificationToken == token);

		if( player == null )
			throw new AppException( "Verification failed" );

		player = player with { Verified = DateTime.UtcNow, VerificationToken = null };

		svc.Data.Save( player, tokenToPlayer.UserId );
		/*/
		return;
		//*/
	}

	public void ForgotPassword( ForgotPasswordRequest model, string origin )
	{
		/* PORT
		var account = _context.Accounts.SingleOrDefault(x => x.Email == model.Email);

		// always return ok response to prevent email enumeration
		if( account == null ) return;

		// create reset token that expires after 1 day
		account.ResetToken = generateResetToken();
		account.ResetTokenExpires = DateTime.UtcNow.AddDays( 1 );

		_context.Accounts.Update( account );
		_context.SaveChanges();

		// send email
		sendPasswordResetEmail( account, origin );
		/*/
		return;
		//*/
	}

	public void ValidateResetToken( ValidateResetTokenRequest model )
	{
		getAccountByResetToken( model.Token );
	}

	public void ResetPassword( ResetPasswordRequest model )
	{
		/* PORT
		var account = getAccountByResetToken(model.Token);

		// update password and remove reset token
		account.PasswordHash = BCrypt.HashPassword( model.Password );
		account.PasswordReset = DateTime.UtcNow;
		account.ResetToken = null;
		account.ResetTokenExpires = null;

		_context.Accounts.Update( account );
		_context.SaveChanges();
		/*/
		return;
		//*/
	}

	public IEnumerable<AccountResponse> GetAll()
	{
		/*
		var accounts = _context.Accounts;
		return _mapper.Map<IList<AccountResponse>>( accounts );
		/*/
		return null;
		//*/
	}

	public AccountResponse GetById( Guid id )
	{
		/* PORT
		var account = getAccount(id);
		return _mapper.Map<AccountResponse>( account );
		/*/
		return null;
		//*/
	}

	public AccountResponse Create( CreateRequest model )
	{
		/* PORT
		// validate
		if( _context.Accounts.Any( x => x.Email == model.Email ) )
			throw new AppException( $"Email '{model.Email}' is already registered" );

		// map model to new account object
		var account = _mapper.Map<Account>(model);
		account.Created = DateTime.UtcNow;
		account.Verified = DateTime.UtcNow;

		// hash password
		account.PasswordHash = BCrypt.HashPassword( model.Password );

		// save account
		_context.Accounts.Add( account );
		_context.SaveChanges();

		return _mapper.Map<AccountResponse>( account );
		/*/
		return null;
		//*/
	}

	public AccountResponse Update( Guid id, UpdateRequest model )
	{
		/* PORT
		var account = getAccount(id);

		// validate
		if( account.Email != model.Email && _context.Accounts.Any( x => x.Email == model.Email ) )
			throw new AppException( $"Email '{model.Email}' is already registered" );

		// hash password if it was entered
		if( !string.IsNullOrEmpty( model.Password ) )
			account.PasswordHash = BCrypt.HashPassword( model.Password );

		// copy model to account and save
		_mapper.Map( model, account );
		account.Updated = DateTime.UtcNow;
		_context.Accounts.Update( account );
		_context.SaveChanges();

		return _mapper.Map<AccountResponse>( account );
		/*/
		return null;
		//*/
	}

	public void Delete( Guid id )
	{
		/* PORT
		var account = getAccount(id);
		_context.Accounts.Remove( account );
		_context.SaveChanges();
		/*/
		return;
		//*/
	}

	// helper methods

	private ent.Account getAccount( Guid id )
	{
		/* PORT
		var account = _context.Accounts.Find(id);
		if( account == null ) throw new KeyNotFoundException( "Account not found" );
		return account;
		/*/
		return null;
		//*/
	}

	private ent.Account getAccountByRefreshToken( string token )
	{
		/* PORT
		var account = _context.Accounts.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
		if( account == null ) throw new AppException( "Invalid token" );
		return account;
		/*/
		return null;
		//*/
	}

	private ent.Account getAccountByResetToken( string token )
	{
		/* PORT
		var account = _context.Accounts.SingleOrDefault(x =>
						x.ResetToken == token && x.ResetTokenExpires > DateTime.UtcNow);
		if( account == null ) throw new AppException( "Invalid token" );
		return account;
		/*/
		return null;
		//*/
	}

	private string generateJwtToken( ent.Account account )
	{
		/* PORT
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[] { new Claim("id", account.Id.ToString()) }),
			Expires = DateTime.UtcNow.AddMinutes(15),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};
		var token = tokenHandler.CreateToken(tokenDescriptor);
		return tokenHandler.WriteToken( token );
		/*/
		return null;
		//*/
	}

	private string generateResetToken()
	{
		/* PORT
		// token is a cryptographically strong random sequence of values
		var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

		// ensure token is unique by checking against db
		var tokenIsUnique = !_context.Accounts.Any(x => x.ResetToken == token);
		if( !tokenIsUnique )
			return generateResetToken();

		return token;
		/*/
		return null;
		//*/
	}

	private string generateVerificationToken()
	{
		/* PORT
		// token is a cryptographically strong random sequence of values
		//var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

		// ensure token is unique by checking against db
		var tokenIsUnique = !_context.Accounts.Any(x => x.VerificationToken == token);
		if( !tokenIsUnique )
			return generateVerificationToken();

		return token;
		/*/
		var token = Guid.NewGuid();
		return token.ToString();
		//*/
	}

	private ent.RefreshToken rotateRefreshToken( ent.RefreshToken refreshToken, string ipAddress )
	{
		//* PORT
		var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
		revokeRefreshToken( refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token );
		return newRefreshToken;
	}

	private void removeOldRefreshTokens( ent.Account account )
	{
		/* PORT
		account.RefreshTokens.RemoveAll( x =>
				!x.IsActive &&
				x.Created.AddDays( _appSettings.RefreshTokenTTL ) <= DateTime.UtcNow );
		//*/
	}

	private void revokeDescendantRefreshTokens( ent.RefreshToken refreshToken, ent.Account account, string ipAddress, string reason )
	{
		/* PORT
		// recursively traverse the refresh token chain and ensure all descendants are revoked
		if( !string.IsNullOrEmpty( refreshToken.ReplacedByToken ) )
		{
			var childToken = account.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
			if( childToken.IsActive )
				revokeRefreshToken( childToken, ipAddress, reason );
			else
				revokeDescendantRefreshTokens( childToken, account, ipAddress, reason );
		}
		/*/
		return;
		//*/
	}

	private void revokeRefreshToken( ent.RefreshToken token, string ipAddress, string? reason = null, string? replacedByToken = null )
	{
		//* PORT
		token.Revoked = DateTime.UtcNow;
		token.RevokedByIp = ipAddress;
		token.ReasonRevoked = reason ?? "no reasons passed into revokeRefreshToken";
		token.ReplacedByToken = replacedByToken ?? "UNKNOWN";
		/*/
		return;
		//*/
	}

	private void sendVerificationEmail( UserData account, string origin, string verificationToken )
	{
		//* PORT
		string message;
		if( !string.IsNullOrEmpty( origin ) )
		{
			// origin exists if request sent from browser single page app (e.g. Angular or React)
			// so send link to verify via single page app
			var verifyUrl = $"{origin}/Accounts/VerifyEmail?token={verificationToken}";
			message = $@"<p>Please click the below link to verify your email address:</p>
                            <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
		}
		else
		{
			// origin missing if request sent directly to api (e.g. from Postman)
			// so send instructions to verify directly with api
			message = $@"<p>Please use the below token to verify your email address with the <code>/accounts/verify-email</code> api route:</p>
                            <p><code>{verificationToken}</code></p>";
		}

		_emailService.Send(
				to: account.Email,
				subject: "Sign-up Verification API - Verify Email",
				html: $@"<h4>Verify Email</h4>
                        <p>Thanks for registering!</p>
                        {message}"
		);
		/*/
		return;
		//*/
	}

	private void sendAlreadyRegisteredEmail( string email, string origin )
	{
		//* PORT
		string message;
		if( !string.IsNullOrEmpty( origin ) )
			message = $@"<p>If you don't know your password please visit the <a href=""{origin}/account/forgot-password"">forgot password</a> page.</p>";
		else
			message = "<p>If you don't know your password you can reset it via the <code>/accounts/forgot-password</code> api route.</p>";

		_emailService.Send(
				to: email,
				subject: "Sign-up Verification API - Email Already Registered",
				html: $@"<h4>Email Already Registered</h4>
                        <p>Your email <strong>{email}</strong> is already registered.</p>
                        {message}"
		);
		/*/
		return;
		//*/
	}

	private void sendPasswordResetEmail( UserData account, string origin )
	{
		//* PORT
		string message;
		if( !string.IsNullOrEmpty( origin ) )
		{
			var resetUrl = $"{origin}/account/reset-password?token={account.ResetToken}";
			message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 day:</p>
                            <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
		}
		else
		{
			message = $@"<p>Please use the below token to reset your password with the <code>/accounts/reset-password</code> api route:</p>
                            <p><code>{account.ResetToken}</code></p>";
		}

		_emailService.Send(
				to: account.Email,
				subject: "Sign-up Verification API - Reset Password",
				html: $@"<h4>Reset Password Email</h4>
                        {message}"
		);
		/*/
		return;
		//*/
	}
}