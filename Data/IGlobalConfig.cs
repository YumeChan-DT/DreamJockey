using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YumeChan.DreamJockey.Data
{
	public interface IGlobalConfig
	{
		public int? CullingSpanMinutes { get; set; }

	}
}
