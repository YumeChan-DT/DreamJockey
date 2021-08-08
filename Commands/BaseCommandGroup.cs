using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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

		}
		[Command, RequireUserPermissions(Permissions.Administrator)]
		public async Task JoinAsync(CommandContext ctx, DiscordChannel channel)
		{

		}

		[Command("leave"), RequireVoicePresence]
		public async Task LeaveAsync(CommandContext ctx)
		{

		}
		[Command, RequireUserPermissions(Permissions.Administrator)]
		public async Task LeaveAsync(CommandContext ctx, DiscordChannel channel)
		{

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
