using ent;

using lib;

using MailKit.Security;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Org.BouncyCastle.Crypto;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

using static svc.IPlayer;



namespace svc;


static public class Data<T>
{
	static public string BaseDir = "bad";

	static public string GetFilename( string key )
	{
		return $"{BaseDir}/{key}.json";
	}
	 
	static public void Save( T data, string key )
	{
		JsonOptions options = new JsonOptions();
		options.SerializerOptions.WriteIndented = true;

		string file = GetFilename( key );

		using FileStream createStream = File.Create( file );

		var jsonStr = JsonSerializer.Serialize( data );

		createStream.WriteAsync( Encoding.UTF8.GetBytes( jsonStr ) );

		//JsonSerializer.SerializeAsync( createStream, data );

		createStream.DisposeAsync();
	}

	static public T? Load( string key )
	{
		string file = GetFilename( key );

		var stream = File.OpenRead( file );

		string jsonString = File.ReadAllText( file );

		T? data = JsonSerializer.Deserialize<T>(jsonString);

		return data;
	}

	static public void Load( string key, out T? data )
	{
		data = Load( key );
	}


}

static public class Data
{

	static public void Save<T>( T data, string key )
	{
		Data<T>.Save( data, key );
	}

	static public void Load<T>( string key, out T? data )
	{
		data = Data<T>.Load( key );
	}

}


public interface IPlayer : IHostedService
{
	public enum PlayerRes
	{
		Invalid,
		ErrPlayerAlreadyExists,
		ErrPlayerNotFound,
		ErrPlayerFoundBadPassword,
		ErrPlayerNotAddedToDB,

		WinPlayerRegistered = 128,
		WinPlayerLoggedIn,
		WinPlayerLoggedOut,
	}


	Task<PlayerRes> Register( PlayerData data );

	Task<PlayerRes> IsRegistered( string username );

	Task<PlayerRes> Login( string username, string password );
	Task<PlayerRes> Logout( string username );

	Task<PlayerData?> Get( string email );
	Task<PlayerRes> Update( PlayerData newPlayerData );
}

public record PlayerData(
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
	PlayerData? Old = null
)
{


	bool IsVerified => Verified.HasValue || PasswordResetAt.HasValue;

	public bool OwnsToken( string token )
	{
		return RefreshTokens?.Find( x => x.Token == token ) != null;
	}
};

public record TokenToPlayer(
	string Token,
	string PlayerId
);

/*
public record PlayerInfo(
	PlayerData Data,
	DateTime LoggedInAt,
	DateTime LastActivityAt
);
*/

public class PlayerSettings : svc.Settings
{
	public string PlayerDir = "./run/db/players/";
	public string TokenDir  = "./run/db/tokens/";

}


public class Player : svc.Service<Player, PlayerSettings>, IPlayer
{
	public Player( ILogger<Player> logger, IOptions<PlayerSettings> settings )
		:
		base( logger, settings )
	{
		lib.Util.checkAndAddDirectory( Settings.Value.PlayerDir );
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


	public Task<PlayerRes> IsRegistered( string email )
	{
		string playerFile = Data<PlayerData>.GetFilename( email );

		if( File.Exists( playerFile ) )
		{
			
			//Data.Load( email, out PlayerData data );

			return Task.FromResult( PlayerRes.WinPlayerRegistered );
		}

		return Task.FromResult( PlayerRes.ErrPlayerNotFound );
	}

	public Task<PlayerRes> Register( PlayerData data )
	{
		Log.LogInformation( $"Register: {data.Email} Attempting to login" );

		string playerFile = Data<PlayerData>.GetFilename( data.Email );

		if( File.Exists( playerFile ) )
		{
			Log.LogWarning( $"Register: {data.DiplayName} File {playerFile} not found" );

			return Task.FromResult( PlayerRes.ErrPlayerAlreadyExists );
		}

		Data.Save( data, data.Email );

		return Task.FromResult( PlayerRes.ErrPlayerAlreadyExists );
	}



	public Task<PlayerRes> Login( string username, string password )
	{
		Log.LogInformation( $"Login: {username} Attempting to login" );

		string playerFile = Data<PlayerData>.GetFilename( username );

		if( !File.Exists( playerFile ) )
		{
			Log.LogWarning( $"Login: {username} File {playerFile} not found" );
			return Task.FromResult( PlayerRes.ErrPlayerNotFound );
		}

		Data.Load( username, out PlayerData? data );


		//var info = new PlayerInfo(data, DateTime.Now, DateTime.Now);

		//var added = _players.TryAdd(username, info);

		//Imm.Up

		if( data != null )
		{
			Log.LogInformation( $"Login: {username} Login Succeeded" );
			return Task.FromResult( PlayerRes.WinPlayerLoggedIn );
		}
		else
		{
			Log.LogWarning( $"Login: {username} Login Failed" );
			return Task.FromResult( PlayerRes.ErrPlayerNotAddedToDB );
		}
	}

	public Task<PlayerRes> Logout( string username )
	{
		return Task.FromResult( PlayerRes.WinPlayerLoggedOut );
	}

	public Task<PlayerData?> Get( string email )
	{
		Data.Load( email, out PlayerData? data );

		//var found = _players.TryGetValue( email, out var playerInfo );

		return Task.FromResult( data );

	}

	public Task<PlayerRes> Update( PlayerData newPlayerData )
	{
		throw new NotImplementedException();
	}

	//private ConcurrentDictionary<string, PlayerInfo> _players = new();

	private ImmutableDictionary<string, DateTime> _login;
	private ImmutableDictionary<string, DateTime> _logout;


}
