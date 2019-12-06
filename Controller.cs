using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using Terraria;
using Terraria.ModLoader.IO;

namespace Routed
{
	public class Controller : BaseComponent
	{
		public override TagCompound Save() => new TagCompound
		{
			["bna"] = "dot"
		};

		public override void Load(TagCompound tag)
		{
			string s = tag.GetString("bna");
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)(Parent.Position.X * 16 - Main.screenPosition.X), (int)(Parent.Position.Y * 16 - Main.screenPosition.Y), 16, 16), Color.Red);
		}
	}
}