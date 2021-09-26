using System.Threading.Tasks;
using YumeChan.PluginBase;

namespace YumeChan.DreamJockey
{
	public class PluginManifest : Plugin
	{
		public override string DisplayName => "YumeChan DreamJockey";
		public override bool StealthMode => false;

		public override Task LoadAsync()
		{
			return base.LoadAsync();
		}

		public override Task UnloadAsync()
		{
			return base.UnloadAsync();
		}
	}
}
