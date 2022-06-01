using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using YumeChan.DreamJockey.Infrastructure;
using YumeChan.DreamJockey.Infrastructure.Preconditions;
using YumeChan.DreamJockey.Services;


namespace YumeChan.DreamJockey.Commands;

#pragma warning disable CA1822 // Mark members as static

[Group("dreamjockey"), Aliases("dj", "music"), Description("Provides Music-oriented commands for voice channels.")]
[RequirePermissions(Permissions.AccessChannels | Permissions.Speak)]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public partial class BaseCommandGroup : BaseCommandModule
{
	private readonly MusicPlayerService _playerService;

	public BaseCommandGroup(MusicPlayerService playerService)
	{
		_playerService = playerService;
	}

	/// <summary>
	///	Instructs the Bot to join the calling user's voice channel.
	/// </summary>
	[Command("join"), Description("Instructs the Bot to join a voice channel.")]
	public async Task JoinAsync(CommandContext ctx)
	{
		await ctx.EnsureUserVoicePresenceAsync();
		
		VoiceCommandContext vc = new(ctx);
		await ctx.RespondAsync((await _playerService.ConnectAsync(vc)).Status switch
			{
				OperationStatus.Success => $"Joined {vc.Channel.Mention}.",
				_                       => $"Failed to join {vc.Channel.Mention}."
			}
		);
	}
	
	/// <summary>
	///	Instructs the Bot to join the specified voice channel.
	/// </summary>
	/// 
	/// <remarks>
	///	This command is subject to the <see cref="RequireVoiceOperatorAttribute" /> precondition.
	/// </remarks>
	[Command]
	public async Task JoinAsync(CommandContext ctx, DiscordChannel channel)
	{
		await ctx.EnsureVoiceOperatorAsync();

		VoiceCommandContext vc = new(ctx, channel);
		await ctx.RespondAsync((await _playerService.ConnectAsync(vc)).Status switch
			{
				OperationStatus.Success => $"Joined {vc.Channel.Mention}.",
				_                       => $"Failed to join {vc.Channel.Mention}."
			}
		);
	}

	/// <summary>
	/// Instructs the Bot to leave the calling user's voice channel.
	/// </summary>
	[Command("leave"), Description("Instructs the Bot to leave its current voice channel.")]
	public async Task LeaveAsync(CommandContext ctx)
	{
		await ctx.EnsureUserVoicePresenceAsync();
		
		VoiceCommandContext vc = new(ctx);
		await ctx.RespondAsync((await _playerService.DisconnectAsync(vc)).Status switch
			{
				OperationStatus.Success => $"Left {vc.Channel.Mention}.",
				_                       => "Sorry, I'm currently not in any voice channel."
			}
		);
	}

	/// <summary>
	/// Instructs the Bot to leave the specified voice channel.
	/// </summary>
	[Command]
	public async Task LeaveAsync(CommandContext ctx, DiscordChannel channel)
	{
		await ctx.EnsureVoiceOperatorAsync();
		VoiceCommandContext vc = new(ctx, channel);

		await ctx.RespondAsync((await _playerService.DisconnectAsync(vc)).Status switch
			{
				OperationStatus.Success => $"Left {vc.Channel.Mention}.",
				_                       => "Sorry, I'm currently not in any voice channel."
			}
		);
	}

	/// <summary>
	/// Instructs the bot to play a track found by a specified search query.
	/// </summary>
	[Command("play"), RequireUserVoicePresence, Description("Joins a user's voice channel and plays music.")]
	public async Task PlayAsync(CommandContext ctx,
		[RemainingText, Description("Search query to seek & load track from.")]
		string search)
	{
		OperationResult<IEnumerable<LavalinkTrack>> result = await _playerService.PlayAsync(new(ctx), search);
		await ctx.RespondAsync(result.Message!);
	}
	
	/// <summary>
	/// Instructs the bot to play a track found by a specified URI.
	/// </summary>
	[Command, RequireUserVoicePresence, Priority(10)]
	public async Task PlayAsync(CommandContext ctx,
		[Description("URL to load track from.")]
		Uri url)
	{
		OperationResult<LavalinkTrack> result = await _playerService.PlayAsync(new(ctx), url);
		await ctx.RespondAsync(result.Message!);
	}

	/// <summary>
	/// Instructs the bot to stop playing the current track.
	/// </summary>
	[Command("stop"), RequireUserVoicePresence, Description("Stops the currently played track.")]
	public async Task StopAsync(CommandContext ctx)
	{
		OperationResult result = await _playerService.StopAsync(new(ctx));
		await ctx.RespondAsync(result.Message ?? "Unknown error.");
	}

	/// <summary>
	/// Instructs the bot to pause the currently playing track.
	/// </summary>
	[Command("pause"), RequireUserVoicePresence, Description("Puts the currently playing track on hold.")]
	public async Task PauseAsync(CommandContext ctx)
	{
		OperationResult result = await _playerService.PauseAsync(new(ctx));
		await ctx.RespondAsync(result.Message ?? "Unknown error.");
	}

	/// <summary>
	/// Instructs the bot to resume the currently paused track.
	/// </summary>
	[Command("resume"), RequireUserVoicePresence, Description("Resumes a currently paused track.")]
	public async Task ResumeAsync(CommandContext ctx)
	{
		OperationResult result = await _playerService.ResumeAsync(new(ctx));
		await ctx.RespondAsync(result.Message ?? "Unknown error.");
	}
	
	/// <summary>
	/// Queues a track found by a specified search query.
	/// </summary>
	[Command("queue"), RequireUserVoicePresence, Description("Queues a track to be played after the current track.")]
	public async Task QueueAsync(CommandContext ctx,
		[RemainingText, Description("Search query to seek & load track from.")]
		string search)
	{
		OperationResult<IEnumerable<LavalinkTrack>> result = await _playerService.QueueAsync(new(ctx), search);
		await ctx.RespondAsync(result.Message!);
	}
	
	/// <summary>
	/// Queues a track found by a specified URI.
	/// </summary>
	[Command, RequireUserVoicePresence, Priority(10)]
	public async Task QueueAsync(CommandContext ctx,
		[Description("URL to load track from.")]
		Uri url)
	{
		OperationResult<LavalinkTrack> result = await _playerService.QueueAsync(new(ctx), url);
		await ctx.RespondAsync(result.Message!);
	}
	
	/*
	 * I swear, GitHub Copilot is a work of art.
	 * Thank you dear MS pals! <3
	 * 
	 *	- Sakura Akeno Isayeki
	 */
	
	/// <summary>
	/// Instructs the bot to skip the currently playing track.
	/// </summary>
	[Command("skip"), RequireUserVoicePresence, Description("Skips the currently playing track.")]
	public async Task SkipAsync(CommandContext ctx)
	{
		OperationResult result = await _playerService.SkipAsync(new(ctx));
		await ctx.RespondAsync(result.Message ?? "Unknown error.");
	}
	
	/// <summary>
	/// Clears the queue.
	/// </summary>
	[Command("clear"), RequireUserVoicePresence, Description("Clears the music queue.")]
	public async Task ClearAsync(CommandContext ctx)
	{
		OperationResult result = _playerService.ClearQueue(new(ctx));
		await ctx.RespondAsync(result.Message ?? "Unknown error.");
	}
}