﻿using DnsClient.Internal;
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
	private readonly PeriodicTimer _timer;

	private Task _cullingLoop;
	private CancellationTokenSource _cullingLoopCts;

	public IdleInstancesCullingHandler(DiscordClient discordClient, ILogger<IdleInstancesCullingHandler> logger)
	{
		_discordClient = discordClient;
		_logger = logger;
		_timer = new(TimeSpan.FromMinutes(5));
	}

	public Task StartAsync(CancellationToken _)
	{
		_cullingLoopCts = new();
		_cullingLoop = Task.Factory.StartNew(() => HandleCullingCycles(_cullingLoopCts.Token));

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
}