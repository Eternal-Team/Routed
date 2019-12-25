using BaseLibrary;
using ContainerLibrary;
using LayerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Routed.Modules
{
	public class ExtractorModule : BaseModule
	{
		public override int DropItem => ModContent.ItemType<Items.ExtractorModule>();

		public override ItemHandler GetHandler()
		{
			if (Utility.TryGetTileEntity(Parent.Position, out ModTileEntity te) && te is IItemHandler handler) return handler.Handler;
			return null;
		}

		private int timer;
		private const int maxTimer = 30;

		public override void Update()
		{
			if (timer++ < maxTimer) return;
			timer = 0;

			ItemHandler handler = GetHandler();
			if (handler == null) return;

			// todo: add conditions to extraction (leave x in, take specific items, etc.)

			for (int i = 0; i < handler.Slots; i++)
			{
				if (handler.Items[i] == null || handler.Items[i].IsAir) continue;

				Item item = handler.ExtractItem(i, 10);
				MarkerModule module = Parent.Network.MarkerModules.FirstOrDefault(markerModule => markerModule.GetHandler() != null && markerModule.IsItemValid(item));
				if (module != null)
				{
					Parent.Network.NetworkItems.Add(new NetworkItem(item, Pathfinding.FindPath(Parent.Network.Tiles, Parent.Position, module.Parent.Position)));
					break;
				}

				handler.InsertItem(ref item);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/ExtractorModule"), position, Color.White);
		}
	}
}