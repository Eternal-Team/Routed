using BaseLibrary;
using BaseLibrary.UI;
using ContainerLibrary;
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
	public class RequesterModule : BaseModule, IHasUI, IItemHandler
	{
		public RequesterModule()
		{
			UUID = Guid.NewGuid();

			Handler = new ItemHandler(18);
		}

		public override int DropItem => ModContent.ItemType<Items.RequesterModule>();

		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound => SoundID.Item1;
		public LegacySoundStyle OpenSound => SoundID.Item1;

		public ItemHandler Handler { get; }

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/RequesterModule"), position, Color.White);
		}

		public override ItemHandler GetHandler() => Handler;

		public override bool Interact()
		{
			BaseLibrary.BaseLibrary.PanelGUI.UI.HandleUI(this);

			return true;
		}

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");
			Handler.Load(tag.GetCompound("Items"));
		}

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID,
			["Items"] = Handler.Save()
		};
	}
}