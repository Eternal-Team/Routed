using LayerLibrary;
using Routed.Layer;

namespace Routed.Items
{
	public abstract class BaseDuct : BaseLayerItem
	{
		public abstract Duct.DuctTier Tier { get; }
	}

	public class BasicDuct : BaseDuct
	{
		public override Duct.DuctTier Tier => Duct.DuctTier.Basic;

		public override IModLayer Layer => Routed.RoutedLayer;
	}

	public class AdvancedDuct : BaseDuct
	{
		public override Duct.DuctTier Tier => Duct.DuctTier.Advanced;

		public override IModLayer Layer => Routed.RoutedLayer;
	}

	public class EliteDuct : BaseDuct
	{
		public override Duct.DuctTier Tier => Duct.DuctTier.Elite;

		public override IModLayer Layer => Routed.RoutedLayer;
	}
}