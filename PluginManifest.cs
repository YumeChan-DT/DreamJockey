using System.Threading.Tasks;
using YumeChan.PluginBase;

namespace YumeChan.DreamJockey
{
	public class PluginManifest : Plugin
	{
		public override string PluginDisplayName => "YumeChan DreamJockey";
		public override bool PluginStealth => false;

		public override Task LoadPlugin()
		{
			return base.LoadPlugin();
		}

		public override Task UnloadPlugin()
		{
			return base.UnloadPlugin();
		}
	}
}
