using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace svc
{

	public interface IEdge
	{

	}


	public class EdgeSettings : svc.Settings
	{

	}


	public class Edge : svc.Service<Edge, EdgeSettings>, IEdge
	{
		public Edge(ILogger<Edge> logger, IOptions<EdgeSettings> settings)
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