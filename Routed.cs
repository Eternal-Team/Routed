using Routed.Layer;
using Terraria;
using Terraria.ModLoader;

namespace Routed
{
	public class Routed : Mod
	{
		public RoutedLayer RoutedLayer;

		public override void Load()
		{
			RoutedLayer = new RoutedLayer();
		}
	}
}