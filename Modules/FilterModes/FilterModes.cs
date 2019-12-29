using ContainerLibrary;
using Routed.Layer;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Modules.FilterModes
{
	public abstract class FilterMode
	{
		public BaseModule Module;

		public abstract bool Check(Item item);

		public virtual TagCompound Save() => new TagCompound();

		public virtual void Load(TagCompound tag)
		{
		}
	}

	public class AnyItemsMode : FilterMode
	{
		public override bool Check(Item item) => true;
	}

	public class InInventoryMode : FilterMode
	{
		public override bool Check(Item item) => Module.GetHandler().Contains(item.type);
	}

	public class ModBasedMode : FilterMode
	{
		public Mod mod;

		public override bool Check(Item item)
		{
			if (item.modItem == null) return false;
			return item.modItem.mod == mod;
		}

		public override void Load(TagCompound tag)
		{
			if (tag.ContainsKey("Mod")) mod = ModLoader.GetMod(tag.GetString("Mod"));
		}

		public override TagCompound Save()
		{
			if (mod == null) return new TagCompound();
			return new TagCompound
			{
				["Mod"] = mod.Name
			};
		}
	}

	public class BuildingMode : FilterMode
	{
		public override bool Check(Item item) => item.createTile >= 0 || item.createWall >= 0;
	}

	public class ConsumablesMode : FilterMode
	{
		public override bool Check(Item item) => item.potion && item.healLife > 0 || item.healMana > 0 && !item.potion || item.buffType > 0 && !item.summon && item.buffType != BuffID.Rudolph;
	}

	public class WeaponsMode : FilterMode
	{
		public override bool Check(Item item) => item.damage > 0 && (!item.notAmmo || item.useStyle > 0) && (item.type < 71 || item.type > 74);
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
		public List<int> whitelist = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

		public override bool Check(Item item) => whitelist.Contains(item.type);

		public override void Load(TagCompound tag)
		{
			whitelist = tag.GetList<int>("Whitelist").ToList();
		}

		public override TagCompound Save() => new TagCompound
		{
			["Whitelist"] = whitelist
		};
	}
}