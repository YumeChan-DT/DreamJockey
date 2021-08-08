using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YumeChan.DreamJockey.Commands
{
	[Group("dreamjockey"), Aliases("dj", "music")]
	public partial class BaseCommandGroup : BaseCommandModule
	{
		[Command("join")]
		public async Task JoinAsync(CommandContext ctx, DiscordChannel channel)
		{

		}

		[Command("leave")]
		public async Task LeaveAsync(CommandContext ctx, DiscordChannel channel)
		{

		}

		[Command("play")]
		public async Task PlayAsync(CommandContext ctx, [RemainingText] string search)
		{

		}
		[Command]
		public async Task PlayAsync(CommandContext ctx, Uri url)
		{

		}

		[Command("pause")]
		public async Task PauseAsync(CommandContext ctx)
		{

		}
	}
}
