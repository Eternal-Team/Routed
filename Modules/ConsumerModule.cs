using BaseLibrary;
using BaseLibrary.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using System;
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
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/ConsumerModule"), position, Color.White);
		}

		public override bool Interact()
		{
			BaseLibrary.BaseLibrary.PanelGUI.UI.HandleUI(this);

			return true;
		}

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");
		}

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID
		};
	}
}