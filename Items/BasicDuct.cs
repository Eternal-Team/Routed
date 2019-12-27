using LayerLibrary;

namespace Routed.Items
{
	public class BasicDuct : BaseLayerItem
	{
		public override IModLayer Layer => Routed.RoutedLayer;
	}
}