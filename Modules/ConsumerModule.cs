using BaseLibrary.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Modules
{
	public class ConsumerModule : BaseModule, IHasUI
	{
		public override int DropItem => ModContent.ItemType<Items.ConsumerModule>();

		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound => SoundID.Item1;
		public LegacySoundStyle OpenSound => SoundID.Item1;

		public ConsumerModule()
		{
			UUID = Guid.NewGuid();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)(Parent.Position.X * 16 - Main.screenPosition.X), (int)(Parent.Position.Y * 16 - Main.screenPosition.Y), 16, 16), Color.Orange * 0.5f);
		}
		
		public override bool Interact()
		{
			BaseLibrary.BaseLibrary.PanelGUI.UI.HandleUI(this);

			return true;
		}

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID
		};

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");
		}
	}
}