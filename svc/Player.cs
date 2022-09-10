using ent;

using lib;

using MailKit.Security;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Org.BouncyCastle.Crypto;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;

using static svc.IPlayer;

namespace svc;

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

public record PlayerInfo(
	PlayerData Data,
	DateTime LoggedInAt,
	DateTime LastActivityAt
);

public class PlayerSettings : svc.Settings
{
	public string PlayerDir = "./db/players/";

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
		string playerFile = GetFilenameFromPlayer( email );

		if( File.Exists( playerFile ) )
		{
			/*
			lib.XmlFormatter2 xml = new();

			var stream = File.OpenRead(playerFile);

			var data = xml.Deserialize<PlayerData>( stream );
			/*/
			var stream = File.OpenRead(playerFile);

			string jsonString = File.ReadAllText(playerFile);

			PlayerData? data =
								JsonSerializer.Deserialize<PlayerData>(jsonString);
			//*/




			return Task.FromResult( PlayerRes.WinPlayerRegistered );
		}

		return Task.FromResult( PlayerRes.ErrPlayerNotFound );
	}

	private string GetFilenameFromPlayer( string email )
	{
		return $"{Settings.Value.PlayerDir}/{email}.json";
	}

	public Task<PlayerRes> Register( PlayerData data )
	{
		Log.LogInformation( $"Register: {data.Email} Attempting to login" );

		string playerFile = GetFilenameFromPlayer( data.Email );

		if( File.Exists( playerFile ) )
		{
			Log.LogWarning( $"Register: {data.DiplayName} File {playerFile} not found" );

			return Task.FromResult( PlayerRes.ErrPlayerAlreadyExists );
		}

		/*
		lib.XmlFormatter2 xml = new();

		var stream = File.OpenWrite(playerFile);

		xml.Serialize( stream, data );
		/*/
		using FileStream createStream = File.Create(playerFile);
		JsonSerializer.SerializeAsync( createStream, data );
		createStream.DisposeAsync();

		//*/


		return Task.FromResult( PlayerRes.ErrPlayerAlreadyExists );
	}



	public Task<PlayerRes> Login( string username, string password )
	{
		Log.LogInformation( $"Login: {username} Attempting to login" );

		string playerFile = GetFilenameFromPlayer( username );

		if( !File.Exists( playerFile ) )
		{
			Log.LogWarning( $"Login: {username} File {playerFile} not found" );
			return Task.FromResult( PlayerRes.ErrPlayerNotFound );
		}

		/*
		lib.XmlFormatter2 xml = new();

		var stream = File.OpenRead(playerFile);

		var data = xml.Deserialize<PlayerData>( stream );
		/*/
		var stream = File.OpenRead(playerFile);

		string jsonString = File.ReadAllText(playerFile);

		PlayerData? data =
								JsonSerializer.Deserialize<PlayerData>(jsonString);
		//*/

		var info = new PlayerInfo(data, DateTime.Now, DateTime.Now);

		var added = _players.TryAdd(username, info);

		if( added )
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

	private ConcurrentDictionary<string, PlayerInfo> _players = new();

}
