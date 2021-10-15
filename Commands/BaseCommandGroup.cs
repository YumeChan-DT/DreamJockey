using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using YumeChan.DreamJockey.Preconditions;

namespace YumeChan.DreamJockey.Commands
{
#pragma warning disable CA1822 // Mark members as static

	[Group("dreamjockey"), Aliases("dj", "music"), Description("Provides Music-oriented commands for voice channels.")]
	[RequirePermissions(Permissions.AccessChannels | Permissions.Speak)]
	public partial class BaseCommandGroup : BaseCommandModule
	{
		[Command("join"), RequireVoicePresence, Description("Allows the Bot to join the calling user's voice channel.")]

		public async Task JoinAsync(CommandContext ctx)
		{
			VoiceCommandContext vc = new(ctx);
			await vc.Node.ConnectAsync(vc.Channel);
			await ctx.RespondAsync($"Joined {vc.Channel.Mention}.");
		}
		[Command]
		public async Task JoinAsync(CommandContext ctx, DiscordChannel channel)
		{
			await ctx.EnsureVoiceOperatorAsync();

			VoiceCommandContext vc = new(ctx, channel);
			await vc.Node.ConnectAsync(vc.Channel);
			await ctx.RespondAsync($"Joined {vc.Channel.Mention}.");
		}

		[Command("leave"), RequireVoicePresence, Description("Instructs the Bot to leave its current voice channel.")]
		public async Task LeaveAsync(CommandContext ctx)
		{
			VoiceCommandContext vc = new(ctx);

			if (vc.GetGuildConnection() is LavalinkGuildConnection conn)
			{
				await conn.DisconnectAsync();
				await ctx.RespondAsync($"Left {vc.Channel.Mention}.");
			}
			else
			{
				await ctx.RespondAsync("Sorry, I'm currently not in any voice channel.");
			}
		}
		[Command]
		public async Task LeaveAsync(CommandContext ctx, DiscordChannel channel)
		{
			await ctx.EnsureVoiceOperatorAsync();
			VoiceCommandContext vc = new(ctx, channel);

			if (vc.GetGuildConnection() is LavalinkGuildConnection conn)
			{
				await conn.DisconnectAsync();
				await ctx.RespondAsync($"Left {vc.Channel.Mention}.");
			}
			else
			{
				await ctx.RespondAsync("Sorry, I'm currently not in any voice channel.");
			}
		}

		[Command("play"), RequireVoicePresence, Description("Joins a user's voice channel and plays music.")]
		public async Task PlayAsync(CommandContext ctx,
			[RemainingText, Description("Search query to seek & load track from.")] string search)
		{
			VoiceCommandContext vc = new(ctx);
			LavalinkGuildConnection conn = await vc.GetOrCreateGuildConnectionAsync();
			LavalinkLoadResult loadResult = await vc.Node.Rest.GetTracksAsync(search);

			if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
			{
				await ctx.RespondAsync($"Failed to find track(s) for query `{search}`.");
				return;
			}

			LavalinkTrack track = loadResult.Tracks.First();
			await conn.PlayAsync(track);
			await ctx.RespondAsync($"Now playing `{track.Title}`.");
		}
		[Command, RequireVoicePresence, Priority(10)]
		public async Task PlayAsync(CommandContext ctx,
			[Description("URL to load track from.")] Uri url)
		{
			VoiceCommandContext vc = new(ctx);
			LavalinkGuildConnection conn = await vc.GetOrCreateGuildConnectionAsync();
			LavalinkLoadResult loadResult = await vc.Node.Rest.GetTracksAsync(url);

			if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
			{
				await ctx.RespondAsync($"Failed to find track(s) for link `{url.AbsoluteUri}`.");
				return;
			}

			LavalinkTrack track = loadResult.Tracks.First();
			await conn.PlayAsync(track);
			await ctx.RespondAsync($"Now playing `{track.Title}`.");
		}

		[Command("stop"), RequireVoicePresence, Description("Stops the currently played track.")]
		public async Task StopAsync(CommandContext ctx)
		{
			VoiceCommandContext vc = new(ctx);
			LavalinkGuildConnection conn = await vc.GetOrCreateGuildConnectionAsync();

			await conn.StopAsync();
			await ctx.RespondAsync("Player stopped.");
		}

		[Command("pause"), RequireVoicePresence, Description("Puts the current playing track on hold.")]
		public async Task PauseAsync(CommandContext ctx)
		{
			await ctx.EnsureVoiceOperatorAsync();
			VoiceCommandContext vc = new(ctx);

			if (vc.GetGuildConnection() is LavalinkGuildConnection conn)
			{
				if (conn.CurrentState.CurrentTrack is LavalinkTrack track)
				{
					await conn.PauseAsync();
					await ctx.RespondAsync($"Paused `{track.Title}`.");
				}
				else
				{
					await ctx.RespondAsync("Sorry, there is nothing to pause.");
				}
			}
			else
			{
				await ctx.RespondAsync("Sorry, I'm currently not in any voice channel.");
			}
		}

		[Command("resume"), RequireVoicePresence, Description("resumes a currently paused track.")]
		public async Task ResumeAsync(CommandContext ctx)
		{
			await ctx.EnsureVoiceOperatorAsync();
			VoiceCommandContext vc = new(ctx);

			if (vc.GetGuildConnection() is LavalinkGuildConnection conn)
			{
				if (conn.CurrentState.CurrentTrack is LavalinkTrack track)
				{
					await conn.ResumeAsync();
					await ctx.RespondAsync($"Resumed `{track.Title}`.");
				}
				else
				{
					await ctx.RespondAsync("Sorry, there is nothing to resume.");
				}
			}
			else
			{
				await ctx.RespondAsync("Sorry, I'm currently not in any voice channel.");
			}
		}
	}
}
