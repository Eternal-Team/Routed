using BaseLibrary;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using Terraria;
using Terraria.ModLoader;

namespace Routed.Modules
{
	public class MarkerModule : BaseModule
	{
		public override int DropItem => ModContent.ItemType<Items.MarkerModule>();

		public ItemHandler GetHandler()
		{
			if (Utility.TryGetTileEntity(Parent.Position, out ModTileEntity te) && te is IItemHandler handler) return handler.Handler;
			return null;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)(Parent.Position.X * 16 - Main.screenPosition.X), (int)(Parent.Position.Y * 16 - Main.screenPosition.Y), 16, 16), Color.Pink * 0.5f);
		}
	}
}