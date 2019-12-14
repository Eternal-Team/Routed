using BaseLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using Terraria.ModLoader;

namespace Routed.Modules
{
	public class ProviderModule : BaseModule
	{
		public override int DropItem => ModContent.ItemType<Items.ProviderModule>();

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/ProviderModule"), position, Color.White);
		}
	}
}