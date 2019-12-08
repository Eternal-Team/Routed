namespace Routed.Items
{
	public class MarkerModule : BaseModuleItem<Modules.MarkerModule>
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Marker Module");
			Tooltip.SetDefault("Marks an inventory as being able to receive items");
		}
	}
}