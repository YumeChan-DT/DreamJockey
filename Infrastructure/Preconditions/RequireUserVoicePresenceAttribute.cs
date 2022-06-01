using DSharpPlus.CommandsNext;
using YumeChan.PluginBase.Infrastructure;

namespace YumeChan.DreamJockey.Infrastructure.Preconditions;

/// <summary>
/// Checks if the calling user is present in a voice channel.
/// </summary>
public class RequireUserVoicePresenceAttribute : PluginCheckBaseAttribute
{
	public override string? ErrorMessage { get; protected set; } = "Sorry, you must be in a voice channel to use this command.";

	/// <summary>
	/// Executes the check, checking if the user is in a voice channel.
	/// </summary>
	/// <remarks>
	///	This check will always pass, if evaluated from a help command.
	/// </remarks>
	public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(help || ctx.Member?.VoiceState?.Channel is not null);
}