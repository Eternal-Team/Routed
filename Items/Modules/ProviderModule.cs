﻿namespace Routed.Items
{
	public class ProviderModule : BaseModuleItem<Modules.ProviderModule>
	{
		public override string Texture => "Routed/Textures/Modules/ProviderModule";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Provider Module");
			Tooltip.SetDefault("Marks an inventory as being able to provide its items to the network");
		}
	}
}