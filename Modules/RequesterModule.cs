using BaseLibrary;
using BaseLibrary.UI;
using ContainerLibrary;
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
	public class RequesterModule : BaseModule, IHasUI, IItemHandler
	{
		private const int maxTimer = 30;

		public override int DropItem => ModContent.ItemType<Items.RequesterModule>();

		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound => SoundID.Item1;
		public LegacySoundStyle OpenSound => SoundID.Item1;

		public ItemHandler Handler { get; }
		public ItemHandler ReturnHandler;

		// todo: queue for requests

		private int timer;

		public RequesterModule()
		{
			UUID = Guid.NewGuid();

			Handler = new ItemHandler(20);
			Handler.OnContentsChanged += slot =>
			{
				if (UI != null) Recipe.FindRecipes();
			};

			ReturnHandler = new ItemHandler(6);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/RequesterModule"), position, Color.White);
		}

		public override ItemHandler GetHandler() => Handler;

		public override bool Interact()
		{
			BaseLibrary.BaseLibrary.PanelGUI.UI.HandleUI(this);
			if (Main.netMode != NetmodeID.Server) Recipe.FindRecipes();

			return true;
		}

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");
			Handler.Load(tag.GetCompound("Items"));
			ReturnHandler.Load(tag.GetCompound("ReturnItems"));
		}

		public override void OnRemove()
		{
			Handler.DropItems(new Rectangle(Parent.Position.X * 16, Parent.Position.Y * 16, 16, 16));
		}

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID,
			["Items"] = Handler.Save(),
			["ReturnItems"] = ReturnHandler.Save()
		};

		public override void Update()
		{
			if (UI != null && Vector2.DistanceSquared(Main.LocalPlayer.position, new Vector2(Parent.Position.X, Parent.Position.Y) * 16) > 9216) BaseLibrary.BaseLibrary.PanelGUI.UI.CloseUI(this);

			if (timer++ < maxTimer) return;
			timer = 0;

			for (int i = 0; i < ReturnHandler.Slots; i++)
			{
				if (ReturnHandler.Items[i] == null || ReturnHandler.Items[i].IsAir) continue;

				Item item = ReturnHandler.ExtractItem(i, 100);

				if (Parent.Network.PushItem(item, Parent)) break;

				ReturnHandler.InsertItem(ref item);
			}
		}
	}
}