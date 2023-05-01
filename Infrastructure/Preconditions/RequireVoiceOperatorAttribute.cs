using DSharpPlus.CommandsNext;
using YumeChan.PluginBase.Infrastructure;

namespace YumeChan.DreamJockey.Infrastructure.Preconditions;

/// <summary>
/// Checks if the user has voice operator privileges.
/// </summary>
public sealed class RequireVoiceOperatorAttribute : PluginCheckBaseAttribute
{
	public override string ErrorMessage { get; protected set; } = "Sorry, you must have Voice Operator privileges to use this command.";

	public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.IsVoiceOperator());
}