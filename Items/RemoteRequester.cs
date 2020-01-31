using BaseLibrary;
using BaseLibrary.Items;
using BaseLibrary.UI;
using ContainerLibrary;
using Routed.Layer;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Items
{
	public class RemoteRequester : BaseItem, IItemHandler, ICraftingStorage, IHasUI
	{
		public override bool CloneNewInstances => false;
		public ItemHandler CraftingHandler => Handler;

		public virtual LegacySoundStyle OpenSound => SoundID.Item1;
		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public virtual LegacySoundStyle CloseSound => SoundID.Item1;

		public ItemHandler Handler { get; set; }
		public ItemHandler ReturnHandler { get; set; }

		public Modules.RemoteRequestModule Module;

		public Queue<(int type, int count)> RequestQueue = new Queue<(int type, int count)>();

		private const int maxTimer = 30;
		private int timer;

		public void RequestItem(int type, int stack)
		{
			RequestQueue.Enqueue((type, stack));
		}

		public override void UpdateInventory(Player player)
		{
			if (Module == null) return;

			if (timer++ < maxTimer) return;
			timer = 0;

			if (RequestQueue.Count > 0)
			{
				(int type, int count) = RequestQueue.Dequeue();

				Module.Network.PullItem(type, count, Module.Parent);
			}

			for (int i = 0; i < ReturnHandler.Slots; i++)
			{
				if (ReturnHandler.Items[i] == null || ReturnHandler.Items[i].IsAir) continue;

				Item item = ReturnHandler.ExtractItem(i, 100);

				if (Module.Network.PushItem(item, Module.Parent)) break;

				ReturnHandler.InsertItem(ref item);
			}
		}

		public RemoteRequester()
		{
			UUID = Guid.NewGuid();

			Handler = new ItemHandler(20);
			Handler.OnContentsChanged += (slot, user) => Recipe.FindRecipes();

			ReturnHandler = new ItemHandler(6);
		}

		public override bool CanRightClick() => true;

		public override ModItem Clone()
		{
			RemoteRequester clone = (RemoteRequester)base.Clone();
			clone.Handler = Handler.Clone();
			clone.UUID = UUID;
			return clone;
		}

		public override ModItem Clone(Item item)
		{
			var clone = Clone();
			clone.SetValue("item", item);
			return clone;
		}

		public override bool ConsumeItem(Player player) => false;

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");
			Handler.Load(tag.GetCompound("Items"));
			ReturnHandler.Load(tag.GetCompound("ReturnItems"));
			Point16 pos = tag.Get<Point16>("Module");

			// bug: runs before the world is loaded
			if (pos != Point16.NegativeOne && Routed.RoutedLayer.TryGetValue(pos, out Duct duct) && duct.Module is Modules.RemoteRequestModule module)
			{
				Module = module;
				module.Requester = this;
			}
		}

		public override void NetRecieve(BinaryReader reader)
		{
			UUID = reader.ReadGUID();
			Handler.Read(reader);
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(UUID);
			Handler.Write(writer);
		}

		public override ModItem NewInstance(Item itemClone)
		{
			ModItem copy = (ModItem)Activator.CreateInstance(GetType());
			copy.SetValue("item", itemClone);
			copy.SetValue("mod", mod);
			copy.SetValue("Name", Name);
			copy.SetValue("DisplayName", DisplayName);
			copy.SetValue("Tooltip", Tooltip);
			return copy;
		}

		public override void RightClick(Player player)
		{
			if (player.whoAmI == Main.LocalPlayer.whoAmI)
			{
				if (Module == null) Main.NewText("Remote Requester needs to be linked first!");
				else PanelUI.Instance.HandleUI(this);
			}
		}

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID,
			["Items"] = Handler.Save(),
			["ReturnItems"] = ReturnHandler.Save(),
			["Module"] = Module?.Position ?? Point16.NegativeOne
		};

		public override void SetDefaults()
		{
			item.width = item.height = 32;
			item.useTime = 5;
			item.useAnimation = 5;
			item.useStyle = 1;
			item.rare = 0;
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool UseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				if (Routed.RoutedLayer.TryGetValue(Player.tileTargetX, Player.tileTargetY, out Duct duct))
				{
					if (duct.Module is Modules.RemoteRequestModule module)
					{
						Module = module;
						module.Requester = this;
					}
				}
			}
			else
			{
				if (player.whoAmI == Main.LocalPlayer.whoAmI)
				{
					if (Module == null) Main.NewText("Remote Requester needs to be linked first!");
					else PanelUI.Instance.HandleUI(this);
				}
			}

			return true;
		}
	}
}