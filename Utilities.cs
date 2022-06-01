using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using YumeChan.DreamJockey.Data;

namespace YumeChan.DreamJockey;

public static class Utilities
{
	public static IPluginConfig PopulateConfig(this IPluginConfig config)
	{
		config.CullingSpanMinutes ??= 5;

		return config;
	}
	
	[Pure]
	public static VoiceCommandContext GetVoiceContext(this CommandContext ctx) => new(ctx);
}
