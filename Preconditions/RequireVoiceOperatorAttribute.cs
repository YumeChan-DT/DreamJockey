using DSharpPlus.CommandsNext;
using System.Threading.Tasks;
using YumeChan.PluginBase.Infrastructure;

namespace YumeChan.DreamJockey.Preconditions
{
	public class RequireVoiceOperatorAttribute : PluginCheckBaseAttribute
	{
		public override string ErrorMessage { get; protected set; }

		public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
		{
			if (!ctx.IsVoiceOperator())
			{
				ErrorMessage ??= "Sorry, you must have Voice Operator privileges to use this command.";
				return Task.FromResult(false);
			}

			return Task.FromResult(true);
		}
	}
}
