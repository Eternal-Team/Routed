using BaseLibrary;
using BaseLibrary.UI;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Items;
using Routed.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Modules
{
	#region Modes
	public abstract class FilterMode
	{
		public BaseModule Module;

		public abstract bool Check(Item item);

		public virtual void Load(TagCompound tag)
		{
		}

		public virtual TagCompound Save() => new TagCompound();
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
		public override bool Check(Item item) => item.consumable;

		//(item.potion && item.healLife > 0 ||
		//item.healMana > 0 && !item.potion ||
		//item.buffType > 0 && !item.summon && item.buffType != BuffID.Rudolph) && item.type != ItemID.NebulaPickup1 && item.type != ItemID.NebulaPickup2 && item.type != ItemID.NebulaPickup3;
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
	#endregion

	public class MarkerModule : BaseModule, IHasUI
	{
		public override int DropItem => ModContent.GetInstance<Routed>().ItemType("MarkerModule" + Mode.GetType().Name.Replace("Mode", ""));

		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound => SoundID.Item1;
		public LegacySoundStyle OpenSound => SoundID.Item1;
		public FilterMode Mode;

		public MarkerModule()
		{
			UUID = Guid.NewGuid();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/MarkerModule"), position, Color.White);
		}

		public override ItemHandler GetHandler()
		{
			if (Utility.TryGetTileEntity(Parent.Position, out ModTileEntity te) && te is IItemHandler handler) return handler.Handler;
			return null;
		}

		public override bool Interact()
		{
			if (Mode is FilteredItemsMode || Mode is ModBasedMode) PanelUI.Instance.HandleUI(this);

			return true;
		}

		public override bool IsItemValid(Item item) => Mode.Check(item);

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");

			TagCompound mode = tag.GetCompound("Mode");

			Type type = Type.GetType(mode.GetString("Name"));
			if (type != null)
			{
				Mode = (FilterMode)Activator.CreateInstance(type);
				Mode.Module = this;
				Mode.Load(mode.GetCompound("Data"));
			}
		}

		public override void OnPlace(BaseModuleItem item)
		{
			if (item is Items.MarkerModule module)
			{
				Type t = Routed.markerModules[module.Mode];
				Mode = (FilterMode)Activator.CreateInstance(t);
				Mode.Module = this;
			}
		}

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID,
			["Mode"] = new TagCompound
			{
				["Name"] = Mode.GetType().AssemblyQualifiedName,
				["Data"] = Mode.Save()
			}
		};
	}
}