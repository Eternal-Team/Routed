namespace Routed.Items
{
	public abstract class MarkerModule : BaseModuleItem<Modules.MarkerModule>
	{
		public abstract string Mode { get; }

		public override string Texture => "Routed/Textures/Modules/MarkerModule";

		public override void SetStaticDefaults()
		{
			string name = "";
			for (int i = 0; i < Mode.Length; i++)
			{
				name += Mode[i];
				if (i + 1 < Mode.Length && char.IsUpper(Mode[i + 1])) name += " ";
			}

			DisplayName.SetDefault($"{name} Marker Module");
		}
	}
}