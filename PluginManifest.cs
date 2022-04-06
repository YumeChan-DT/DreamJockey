using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using YumeChan.DreamJockey.Data;
using YumeChan.DreamJockey.Services;
using YumeChan.PluginBase;
using YumeChan.PluginBase.Tools;

namespace YumeChan.DreamJockey
{
	public class PluginManifest : Plugin
	{
		internal const string GlobalConfigFilename = "global-config.json";

		private readonly IdleInstancesCullingHandler _cullingHandler;

		public override string DisplayName => "YumeChan DreamJockey";
		public override bool StealthMode => false;

		public PluginManifest(IdleInstancesCullingHandler cullingHandler)
		{
			_cullingHandler = cullingHandler;
		}

		public override async Task LoadAsync()
		{
			CancellationToken ct = CancellationToken.None;
			await _cullingHandler.StartAsync(ct);

			await base.LoadAsync();
		}

		public override async Task UnloadAsync()
		{
			CancellationToken ct = CancellationToken.None;
			await _cullingHandler.StopAsync(ct);

			await base.UnloadAsync();
		}
	}

	public class Dependencies : DependencyInjectionHandler
	{
		public override IServiceCollection ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(services => 
				services.GetRequiredService<IInterfaceConfigProvider<IPluginConfig>>()
					.InitConfig(PluginManifest.GlobalConfigFilename)
					.PopulateConfig());
			
			services.AddSingleton<IdleInstancesCullingHandler>();

			return services;
		}
	}
}
