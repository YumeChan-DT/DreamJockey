using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Threading.Tasks;
using YumeChan.DreamJockey.Preconditions;

namespace YumeChan.DreamJockey.Commands
{
	[Group("dreamjockey"), Aliases("dj", "music"), RequirePermissions(Permissions.AccessChannels | Permissions.Speak)]
	public partial class BaseCommandGroup : BaseCommandModule
	{
		[Command("join"), RequireVoicePresence]
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

		[Command("leave"), RequireVoicePresence]
		public async Task LeaveAsync(CommandContext ctx)
		{
			VoiceCommandContext vc = new(ctx);
			LavalinkGuildConnection conn = vc.GetGuildConnection();
			await conn.DisconnectAsync();
			await ctx.RespondAsync($"Left {vc.Channel.Mention}.");
		}
		[Command]
		public async Task LeaveAsync(CommandContext ctx, DiscordChannel channel)
		{
			await ctx.EnsureVoiceOperatorAsync();

			VoiceCommandContext vc = new(ctx, channel);
			LavalinkGuildConnection conn = vc.GetGuildConnection();
			await conn.DisconnectAsync();
			await ctx.RespondAsync($"Left {vc.Channel.Mention}.");
		}

		[Command("play"), RequireVoicePresence]
		public async Task PlayAsync(CommandContext ctx, [RemainingText] string search)
		{

		}
		[Command, RequireVoicePresence]
		public async Task PlayAsync(CommandContext ctx, Uri url)
		{

		}

		[Command("pause"), RequireVoicePresence]
		public async Task PauseAsync(CommandContext ctx)
		{

		}
	}
}
