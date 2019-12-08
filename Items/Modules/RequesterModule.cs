using Terraria;

namespace Routed.Items
{
	public class RequesterModule : BaseModuleItem<Modules.RequesterModule>
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Requester Module");
			Tooltip.SetDefault("Allows the player to reqeust items from the network");
		}
	}
}