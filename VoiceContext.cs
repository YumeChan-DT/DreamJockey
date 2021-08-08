using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Linq;

namespace YumeChan.DreamJockey
{
	public class VoiceCommandContext
	{
		public CommandContext Context { get; private init; }
		public DiscordChannel Channel { get; init; }
		public LavalinkExtension Lavalink { get; private init; }
		public LavalinkNodeConnection Node { get; protected init; }

		public VoiceCommandContext(CommandContext ctx, DiscordChannel voiceChannel = null)
		{
			Context = ctx;
			Lavalink = ctx.Client.GetLavalink();
			Node ??= Lavalink.ConnectedNodes?.Values.First() ?? throw new ApplicationException("Lavalink is not connected.");
			Channel ??= voiceChannel ?? ctx.Member.VoiceState?.Channel ?? throw new InvalidOperationException($"No Voice channel has been set for current {nameof(VoiceCommandContext)}.");
		}
	}
}
