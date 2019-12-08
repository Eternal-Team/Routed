using LayerLibrary;
using Terraria.ModLoader;

namespace Routed.Items
{
	public class BasicDuct : BaseLayerItem
	{
		public override IModLayer Layer => ModContent.GetInstance<Routed>().RoutedLayer;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Basic Duct");
		}
	}
}