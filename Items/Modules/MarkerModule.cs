using Routed.Modules;

namespace Routed.Items
{
	public abstract class MarkerModule : BaseModuleItem<Modules.MarkerModule>
	{
		public FilterMode Mode;

		public override string Texture => "Routed/Textures/Modules/MarkerModule";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Marker Module");
			Tooltip.SetDefault("Marks an inventory as being able to receive items");
		}
	}

	public class MMAI : MarkerModule
	{
		public MMAI()
		{
			Mode = new AnyItemsMode();
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Tooltip.SetDefault("All items mode");
		}
	}
}