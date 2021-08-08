using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace YumeChan.DreamJockey.Preconditions
{
	public static class ChecksExtensions
	{
		public static bool IsVoiceOperator(this CommandContext ctx) => ctx.Member.Permissions.HasPermission(Permissions.Administrator);

		public static async Task EnsureVoiceOperatorAsync(this CommandContext ctx)
		{
			RequireVoiceOperatorAttribute attribute = new();

			if (!await attribute.ExecuteCheckAsync(ctx, false))
			{
				throw new ChecksFailedException(ctx.Command, ctx, new[] { attribute });
			}
		}
	}
}
