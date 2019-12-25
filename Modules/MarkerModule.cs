using BaseLibrary;
using BaseLibrary.UI;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Items;
using Routed.Layer;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Modules
{
	public abstract class FilterMode
	{
		public BaseModule Module;

		protected FilterMode()
		{
		}

		public FilterMode(BaseModule module)
		{
			Module = module;
		}

		public abstract bool Check(Item item);
	}

	// basically default route
	public class AnyItemsMode : FilterMode
	{
		public AnyItemsMode()
		{
		}

		public AnyItemsMode(BaseModule module)
		{
			Module = module;
		}

		public override bool Check(Item item) => true;
	}

	public class InInvMode : FilterMode
	{
		public override bool Check(Item item) => Module.GetHandler().Contains(item.type);
	}

	public class ModBasedMode : FilterMode
	{
		public Mod mod;

		public ModBasedMode(BaseModule module, Mod mod)
		{
			Module = module;
			this.mod = mod;
		}

		public override bool Check(Item item)
		{
			if (item.modItem == null) return false;
			return item.modItem.mod == mod;
		}
	}

	public class BuildingMode : FilterMode
	{
		public override bool Check(Item item) => item.createTile >= 0 || item.createWall >= 0;
	}

	public class ConsumablesMode : FilterMode
	{
		public override bool Check(Item item) => item.consumable;

		//(item.potion && item.healLife > 0 ||
		//item.healMana > 0 && !item.potion ||
		//item.buffType > 0 && !item.summon && item.buffType != BuffID.Rudolph) && item.type != ItemID.NebulaPickup1 && item.type != ItemID.NebulaPickup2 && item.type != ItemID.NebulaPickup3;
	}

	public class WeaponsMode : FilterMode
	{
		public override bool Check(Item item)
		{
			return item.damage > 0 && (!item.notAmmo || item.useStyle > 0) && (item.type < 71 || item.type > 74);
			//return (item.melee || item.ranged || item.magic || item.summon || item.thrown) && item.damage > 0;
		}
	}

	public class ToolsMode : FilterMode
	{
		public override bool Check(Item item) => item.pick > 0 || item.hammer > 0 || item.axe > 0 || item.fishingPole > 0;
	}

	public class AccessoriesMode : FilterMode
	{
		public override bool Check(Item item) => item.accessory;
	}

	public class ArmorMode : FilterMode
	{
		public override bool Check(Item item) => item.headSlot > 0 || item.bodySlot > 0 || item.legSlot > 0;
	}

	public class AmmoMode : FilterMode
	{
		public override bool Check(Item item) => item.ammo > 0;
	}

	public class MaterialsMode : FilterMode
	{
		public override bool Check(Item item) => item.material;
	}

	public class FilteredItemsMode : FilterMode
	{
		private List<int> whitelist;

		public FilteredItemsMode(BaseModule module, List<int> whitelist)
		{
			Module = module;
			this.whitelist = whitelist;
		}

		public override bool Check(Item item) => whitelist.Contains(item.type);
	}

	public class MarkerModule : BaseModule, IHasUI
	{
		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound => SoundID.Item1;
		public LegacySoundStyle OpenSound => SoundID.Item1;

		public override int DropItem => ModContent.ItemType<Items.MarkerModule>();

		public FilterMode Mode;

		public MarkerModule()
		{
			UUID = Guid.NewGuid();
			Mode = new AnyItemsMode(this);
		}

		public override ItemHandler GetHandler()
		{
			if (Utility.TryGetTileEntity(Parent.Position, out ModTileEntity te) && te is IItemHandler handler) return handler.Handler;
			return null;
		}

		public override bool IsItemValid(Item item) => Mode.Check(item);

		public override void OnPlace(BaseModuleItem item)
		{
			if (item is Items.MarkerModule a) Mode = a.Mode;
		}

		public override bool Interact()
		{
			BaseLibrary.BaseLibrary.PanelGUI.UI.HandleUI(this);

			return true;
		}

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID,
			["Mode"] = Mode.GetType().AssemblyQualifiedName
		};

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");
			Mode = (FilterMode)Activator.CreateInstance(Type.GetType(tag.GetString("Mode")));
			Mode.Module = this;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/MarkerModule"), position, Color.White);
		}
	}
}