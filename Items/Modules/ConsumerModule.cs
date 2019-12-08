using Terraria;

namespace Routed.Items
{
	public class ConsumerModule : BaseModuleItem<Modules.ConsumerModule>
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Consumer Module");
			Tooltip.SetDefault("Pulls items from the network");
		}
	}
}