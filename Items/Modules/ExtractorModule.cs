namespace Routed.Items
{
	public abstract class ExtractorModule : BaseModuleItem<Modules.ExtractorModule>
	{
		public override string Texture => "Routed/Textures/Modules/ExtractorModule";

		public abstract int ItemsPerExtraction { get; }
		public abstract int ExtractionSpeed { get; }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Extractor Module");
			Tooltip.SetDefault($"Pushes items into the network at a rate of {ItemsPerExtraction}/{ExtractionSpeed / 60f:N1}s");
		}
	}

	public class BasicExtractorModule : ExtractorModule
	{
		public override int ItemsPerExtraction => 10;

		public override int ExtractionSpeed => 30;
	}

	public class AdvancedExtractorModule : ExtractorModule
	{
		public override int ItemsPerExtraction => 100;

		public override int ExtractionSpeed => 20;
	}

	public class EliteExtractorModule : ExtractorModule
	{
		public override int ItemsPerExtraction => 1000;

		public override int ExtractionSpeed => 10;
	}
}