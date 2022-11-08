using ent;

using lib;

using MailKit.Security;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Org.BouncyCastle.Crypto;

using System.Collections.Concurrent;
using System.Collections.Immutable;

using static svc.IUser;



namespace svc;


public interface IUser : IHostedService
{
	public enum UserRes
	{
		Invalid,
		ErrUserAlreadyExists,
		ErrUserNotFound,
		ErrUserFoundBadPassword,
		ErrUserNotAddedToDB,

		WinUserRegistered = 128,
		WinUserLoggedIn,
		WinUserLoggedOut,
	}


	Task<UserRes> Register( UserData data );

	Task<UserRes> IsRegistered( string username );

	Task<UserRes> Login( string username, string password );
	Task<UserRes> Logout( string username );

	Task<UserData?> Get( string email );
	Task<UserRes> Update( UserData newUserData );
}

public record UserData(
	Guid Id,
	string DiplayName,
	string Email,
	string PasswordHash,
	bool AcceptTerms,
	ent.Role Role,
	string VerificationToken,
	DateTime? Verified,
	string ResetToken,
	DateTime? ResetTokenExpiresAt,
	DateTime? PasswordResetAt,
	DateTime CreatedAt,
	DateTime UpdatedAt,
	ImmutableList<RefreshToken> RefreshTokens,
	UserData? Old = null
)
{


	bool IsVerified => Verified.HasValue || PasswordResetAt.HasValue;

	public bool OwnsToken( string token )
	{
		return RefreshTokens?.Find( x => x.Token == token ) != null;
	}
};

public record TokenToUser(
	string Token,
	string UserId
);

/*
public record UserInfo(
	UserData Data,
	DateTime LoggedInAt,
	DateTime LastActivityAt
);
*/

public class UserSettings : svc.Settings
{
	public string UserDir = "./run/db/users/";
	public string TokenDir  = "./run/db/tokens/";

	public string UserBackupDir = "./run/db_backup/users/";
	public string TokenBackupDir  = "./run/db_backup/tokens/";
}


public class User : svc.Service<User, UserSettings>, IUser
{
	public User( ILogger<User> logger, IOptions<UserSettings> settings )
		:
		base( logger, settings )
	{
		lib.Util.checkAndAddDirectory( Settings.Value.UserDir );
	}


	protected override Task<Result> Start( CancellationToken stoppingToken )
	{
		return base.Start( stoppingToken );
	}

	protected override Task<Result> Run( CancellationToken stoppingToken )
	{
		// Currently unused
		while( true )
		{

		}

		return base.Run( stoppingToken );
	}

	protected override Task<Result> Stop( CancellationToken stoppingToken )
	{
		return base.Stop( stoppingToken );
	}


	public Task<UserRes> IsRegistered( string email )
	{
		string userFile = Data<UserData>.GetFilename( email );

		if( File.Exists( userFile ) )
		{
			
			//Data.Load( email, out UserData data );

			return Task.FromResult( UserRes.WinUserRegistered );
		}

		return Task.FromResult( UserRes.ErrUserNotFound );
	}

	public Task<UserRes> Register( UserData data )
	{
		Log.LogInformation( $"Register: {data.Email} Attempting to login" );

		string userFile = Data<UserData>.GetFilename( data.Email );

		if( File.Exists( userFile ) )
		{
			Log.LogWarning( $"Register: {data.DiplayName} File {userFile} not found" );

			return Task.FromResult( UserRes.ErrUserAlreadyExists );
		}

		Data.Save( data );

		return Task.FromResult( UserRes.ErrUserAlreadyExists );
	}



	public Task<UserRes> Login( string username, string password )
	{
		Log.LogInformation( $"Login: {username} Attempting to login" );

		string userFile = Data<UserData>.GetFilename( username );

		if( !File.Exists( userFile ) )
		{
			Log.LogWarning( $"Login: {username} File {userFile} not found" );
			return Task.FromResult( UserRes.ErrUserNotFound );
		}

		Data.Load( username, out UserData? data );


		//var info = new UserInfo(data, DateTime.Now, DateTime.Now);

		//var added = _users.TryAdd(username, info);

		//Imm.Up

		if( data != null )
		{
			Log.LogInformation( $"Login: {username} Login Succeeded" );
			return Task.FromResult( UserRes.WinUserLoggedIn );
		}
		else
		{
			Log.LogWarning( $"Login: {username} Login Failed" );
			return Task.FromResult( UserRes.ErrUserNotAddedToDB );
		}
	}

	public Task<UserRes> Logout( string username )
	{
		return Task.FromResult( UserRes.WinUserLoggedOut );
	}

	public Task<UserData?> Get( string email )
	{
		Data.Load( email, out UserData? data );

		//var found = _users.TryGetValue( email, out var userInfo );

		return Task.FromResult( data );

	}

	public Task<UserRes> Update( UserData newUserData )
	{
		throw new NotImplementedException();
	}

	//private ConcurrentDictionary<string, UserInfo> _users = new();

	private ImmutableDictionary<string, DateTime> _login;
	private ImmutableDictionary<string, DateTime> _logout;


}
