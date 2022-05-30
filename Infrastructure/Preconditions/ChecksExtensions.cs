using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;

namespace YumeChan.DreamJockey.Infrastructure.Preconditions;

public static class ChecksExtensions
{
	/// <summary>
	/// Checks if the calling user is a voice operator.
	/// </summary>
	/// <returns>
	///	<c>true</c> if the calling user is a voice operator; otherwise, <c>false</c>.
	/// </returns>
	public static bool IsVoiceOperator(this CommandContext ctx) => ctx.Member?.Permissions.HasPermission(Permissions.Administrator) ?? false;

	/// <summary>
	///	Ensures the calling user is a voice operator, throwing an exception if they're not.
	/// </summary>
	/// <exception cref="ChecksFailedException">
	///	Exception thrown when the calling user is not a voice operator.
	/// </exception>
	public static async Task EnsureVoiceOperatorAsync(this CommandContext ctx)
	{
		RequireVoiceOperatorAttribute attribute = new();

		if (!await attribute.ExecuteCheckAsync(ctx, false))
		{
			throw new ChecksFailedException(ctx.Command!, ctx, new[] { attribute });
		}
	}
	
	/// <summary>
	/// Ensures the calling user is present in a voice channel, throwing an exception if they're not.
	/// </summary>
	/// <exception cref="ChecksFailedException">
	/// Exception thrown when the calling user is not in a voice channel.
	/// </exception>
	public static async Task EnsureUserVoicePresenceAsync(this CommandContext ctx)
	{
		RequireUserVoicePresenceAttribute attribute = new();

		if (!await attribute.ExecuteCheckAsync(ctx, false))
		{
			throw new ChecksFailedException(ctx.Command!, ctx, new[] { attribute });
		}
	}
}