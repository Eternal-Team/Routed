namespace Routed.Items
{
	public class ExtractorModule : BaseModuleItem<Modules.ExtractorModule>
	{
		public override string Texture => "Routed/Textures/Modules/ExtractorModule";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Extractor Module");
			Tooltip.SetDefault("Pushes items into the network");
		}
	}
}