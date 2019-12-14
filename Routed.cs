using BaseLibrary;
using BaseLibrary.Input;
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

			if (!Main.dedServ)
			{
				MouseEvents.ButtonPressed += args => args.Button == MouseButton.Right && RoutedLayer.Interact();
			}
		}

		public override void Unload() => this.UnloadNullableTypes();
	}
}