using LayerLibrary;

namespace Routed.Items
{
	public class BasicDuct : BaseLayerItem
	{
		public override IModLayer Layer => Routed.RoutedLayer;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Basic Duct");
		}
	}
}