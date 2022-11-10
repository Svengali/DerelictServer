using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace svc
{

	public interface IPresence
	{

	}


	public class PresenceSettings : svc.Settings
	{

	}


	public class Presence : svc.Service<Presence, PresenceSettings>, IPresence
	{
		public Presence(ILogger<Presence> logger, IOptions<PresenceSettings> settings)
			:
			base(logger, settings)
		{
		}


		protected override Task<Result> Start(CancellationToken stoppingToken)
		{
			return base.Start(stoppingToken);
		}

		protected override Task<Result> Run(CancellationToken stoppingToken)
		{
			return base.Run(stoppingToken);
		}

		protected override Task<Result> Stop(CancellationToken stoppingToken)
		{
			return base.Stop(stoppingToken);
		}
	}

}