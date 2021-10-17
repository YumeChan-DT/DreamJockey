using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YumeChan.DreamJockey.Data;

namespace YumeChan.DreamJockey;

public static class Utilities
{
	public static IGlobalConfig PopulateConfig(this IGlobalConfig config)
	{
		config.CullingSpanMinutes ??= 5;

		return config;
	}
}
