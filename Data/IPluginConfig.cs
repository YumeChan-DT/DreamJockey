using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YumeChan.DreamJockey.Data;

public interface IPluginConfig
{
	public int? CullingSpanMinutes { get; set; }
}
