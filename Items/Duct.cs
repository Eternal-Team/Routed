using LayerLibrary;
using Terraria;
using Terraria.ModLoader;

namespace Routed.Items
{
	public class Duct : BaseLayerItem
	{
		public override IModLayer Layer => ModContent.GetInstance<Routed>().RoutedLayer;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Duct");
		}
	}
}