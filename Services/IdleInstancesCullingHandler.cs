using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using YumeChan.DreamJockey.Data;

namespace YumeChan.DreamJockey.Services;

/// <summary>
/// Periodic service that handles disconnecting voice instances from empty channels.
/// </summary>
public sealed class IdleInstancesCullingHandler
{
	private readonly DiscordClient _discordClient;
	private readonly ILogger<IdleInstancesCullingHandler> _logger;
	private readonly IPluginConfig _config;
	private readonly MusicPlayerService _playerService;
	private PeriodicTimer? _timer;

	private Task? _cullingLoop;
	private CancellationTokenSource? _cullingLoopCts;

	public IdleInstancesCullingHandler(DiscordClient discordClient, ILogger<IdleInstancesCullingHandler> logger, IPluginConfig config, MusicPlayerService playerService)
	{
		_discordClient = discordClient;
		_logger = logger;
		_config = config;
		_playerService = playerService;
	}

	/// <summary>
	/// Starts the service.
	/// </summary>
	public Task StartAsync(CancellationToken ct)
	{
		_cullingLoopCts = new();
		_timer = new(TimeSpan.FromMinutes(_config.CullingSpanMinutes ?? 5));
		_cullingLoop = Task.Factory.StartNew(() => HandleCullingCycles(_cullingLoopCts.Token), ct);
		

		_logger.LogInformation($"Started {nameof(IdleInstancesCullingHandler)}");
		return Task.CompletedTask;
	}

	/// <summary>
	/// Stops the service.
	/// </summary>
	public async Task StopAsync(CancellationToken ct)
	{
		_cullingLoopCts?.Cancel();
		
		if (_cullingLoop is not null)
		{
			await _cullingLoop.WaitAsync(TimeSpan.FromSeconds(30), ct);
		}

		_logger.LogInformation($"Stopped {nameof(IdleInstancesCullingHandler)}");
	}

	/// <summary>
	/// Handles the heuristics of culling cycles.
	/// </summary>
	public async Task HandleCullingCycles(CancellationToken ct)
	{
		while (await _timer.WaitForNextTickAsync(ct))
		{
			CullIdleInstances(ct);
		}
	}

	/// <summary>
	/// Culls idle instances on voice channels where the bot is the only user.
	/// </summary>
	public void CullIdleInstances(CancellationToken ct)
	{
		_logger.LogTrace("Startng idle-culling cycle...");

		// Loop through all guilds
		_discordClient.Guilds.Values.AsParallel<DiscordGuild>().AsUnordered().WithCancellation(ct).ForAll(async guild =>
		{
			// Check for empty voice channels
			if (guild.CurrentMember.VoiceState?.Channel is { Users.Count: 1 } channel)
			{
				// If the bot is the only user, disconnect the channel
				await _playerService.DisconnectAsync(guild.Id);
				_logger.LogDebug("Culled idle voice instance from guild {GuildId} (channel {ChannelId})", guild.Id, channel.Id);
			}
		});
	}
}
