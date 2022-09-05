using lib;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Collections.Concurrent;

using static svc.IPlayer;

namespace svc
{

	public interface IPlayer
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


		Task<PlayerRes> Register( string username, string password );

		Task<PlayerRes> Login( string username, string password );
		Task<PlayerRes> Logout( string username );

	}

	public record PlayerData( string Name, string PasswordHash );

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
			return base.Run( stoppingToken );
		}

		protected override Task<Result> Stop( CancellationToken stoppingToken )
		{
			return base.Stop( stoppingToken );
		}

		public Task<PlayerRes> Register( string username, string password )
		{
			Log.LogInformation( $"Register: {username} Attempting to login" );

			var playerFile = $"{Settings.Value.PlayerDir}/{username}.xml";

			if( File.Exists( playerFile ) )
			{
				Log.LogWarning( $"Register: {username} File {playerFile} not found" );

				return Task.FromResult( PlayerRes.ErrPlayerAlreadyExists );
			}

			return Task.FromResult( PlayerRes.WinPlayerRegistered );
		}

		public Task<PlayerRes> Login( string username, string password )
		{
			Log.LogInformation( $"Login: {username} Attempting to login" );

			var playerFile = $"{Settings.Value.PlayerDir}/{username}.xml";

			if( !File.Exists( playerFile ) )
			{
				Log.LogWarning( $"Login: {username} File {playerFile} not found" );
				return Task.FromResult( PlayerRes.ErrPlayerNotFound );
			}

			lib.XmlFormatter2 xml = new();

			var stream = File.OpenRead(playerFile);

			var data = xml.Deserialize<PlayerData>( stream );


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

}