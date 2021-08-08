using DSharpPlus.CommandsNext;
using System.Threading.Tasks;
using YumeChan.PluginBase.Infrastructure;

namespace YumeChan.DreamJockey.Preconditions
{
	public class RequireVoicePresenceAttribute : PluginCheckBaseAttribute
	{
		public override string ErrorMessage { get; protected set; }

		public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
		{
			if (ctx.Member.VoiceState?.Channel is null)
			{
				ErrorMessage ??= "Sorry, you must be in a voice channel to use this command.";
				return Task.FromResult(false);
			}

			return Task.FromResult(true);
		}
	}
}
