using DnsClient.Internal;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YumeChan.DreamJockey.Data;

namespace YumeChan.DreamJockey.Services;

public class IdleInstancesCullingHandler
{
	private readonly DiscordClient _discordClient;
	private readonly ILogger<IdleInstancesCullingHandler> _logger;
	private readonly IPluginConfig _config;
	private PeriodicTimer _timer;

	private Task _cullingLoop;
	private CancellationTokenSource _cullingLoopCts;

	public IdleInstancesCullingHandler(DiscordClient discordClient, ILogger<IdleInstancesCullingHandler> logger, IPluginConfig config)
	{
		_discordClient = discordClient;
		_logger = logger;
		_config = config;
	}

	public Task StartAsync(CancellationToken ct)
	{
		_cullingLoopCts = new();
		_timer = new(TimeSpan.FromMinutes(_config.CullingSpanMinutes ?? 5));
		_cullingLoop = Task.Factory.StartNew(() => HandleCullingCycles(_cullingLoopCts.Token), ct);
		

		_logger.LogInformation("Started IdleInstancesCullingHandler.");
		return Task.CompletedTask;
	}

	public async Task StopAsync(CancellationToken ct)
	{
		_cullingLoopCts.Cancel();
		await _cullingLoop.WaitAsync(TimeSpan.FromSeconds(30), ct);
		_logger.LogInformation("Stopped IdleInstancesCullingHandler.");
	}

	public async Task HandleCullingCycles(CancellationToken ct)
	{
		while (await _timer.WaitForNextTickAsync(ct))
		{
			CullIdleInstances(ct);
		}
	}

	public void CullIdleInstances(CancellationToken ct)
	{
		_logger.LogTrace("Startng idle-culling cycle...");

		_discordClient.Guilds.Values.AsParallel().AsUnordered().WithCancellation(ct).ForAll(async guild =>
		{
			if (guild.CurrentMember.VoiceState?.Channel is DiscordChannel channel && channel.Users.Count() is 1)
			{
				await guild.CurrentMember.ModifyAsync(m => m.VoiceChannel = null);
				_logger.LogDebug("Culled idle voice instance from guild {guildId} (channel {channelId}).", guild.Id, channel.Id);
			}
		});
	}
}
