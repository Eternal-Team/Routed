using BaseLibrary;
using BaseLibrary.UI;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using Routed.UI;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

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

		public Queue<(int type, int count)> RequestQueue = new Queue<(int type, int count)>();
		public ItemHandler ReturnHandler;

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
			if (Main.netMode != NetmodeID.Server)
			{
				if (UI != null) PanelUI.Instance.CloseUI(this);
				else
				{
					for (int i = 0; i < PanelUI.Instance.Elements.Count; i++)
					{
						UIElement element = PanelUI.Instance.Elements[i];
						if (element is RequesterModulePanel panel) PanelUI.Instance.CloseUI(panel.Container);
					}

					PanelUI.Instance.HandleUI(this);
				}
			}

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

		public void RequestItem(int type, int stack)
		{
			RequestQueue.Enqueue((type, stack));
		}

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID,
			["Items"] = Handler.Save(),
			["ReturnItems"] = ReturnHandler.Save()
		};

		public override void Update()
		{
			if (UI != null && Vector2.DistanceSquared(Main.LocalPlayer.position, new Vector2(Parent.Position.X, Parent.Position.Y) * 16) > 9216) PanelUI.Instance.CloseUI(this);

			if (timer++ < maxTimer) return;
			timer = 0;

			if (RequestQueue.Count > 0)
			{
				(int type, int count) = RequestQueue.Dequeue();

				Parent.Network.PullItem(type, count, Parent);
			}

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