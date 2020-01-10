using BaseLibrary;
using BaseLibrary.UI;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Items;
using Routed.Layer;
using Routed.Modules.FilterModes;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Modules
{
	public class MarkerModule : BaseModule, IHasUI
	{
		public override int DropItem => ModContent.GetInstance<Routed>().ItemType("MarkerModule" + Mode.GetType().Name.Replace("Mode", ""));

		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound => SoundID.Item1;
		public LegacySoundStyle OpenSound => SoundID.Item1;
		public FilterMode Mode;

		public int priority = int.MinValue;

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

		public override bool IsItemValid(Item item) => Mode.Check(item);

		public override bool Interact()
		{
			if (Mode is FilteredItemsMode || Mode is ModBasedMode) PanelUI.Instance.HandleUI(this);

			return true;
		}

		public override void OnPlace(BaseModuleItem item)
		{
			if (item is Items.MarkerModule module)
			{
				Type t = Routed.markerModules[module.Mode];
				Mode = (FilterMode)Activator.CreateInstance(t);
				Mode.Module = this;

				GetPriority();
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

				GetPriority();
			}
			else Mode = new AnyItemsMode();
		}

		private void GetPriority()
		{
			switch (Mode)
			{
				case AnyItemsMode _:
					priority = -1000;
					break;
				case MaterialsMode _:
					priority = -900;
					break;
				case ConsumablesMode _:
					priority = -800;
					break;
				case BuildingMode _:
					priority = -700;
					break;
				case ModBasedMode _:
					priority = -600;
					break;
				case WeaponsMode _:
				case ToolsMode _:
				case AccessoriesMode _:
				case ArmorMode _:
				case AmmoMode _:
					priority = -600;
					break;
				case InInventoryMode _:
					priority = 900;
					break;
				case FilteredItemsMode _:
					priority = 1000;
					break;
				default:
					priority = -10000;
					break;
			}
		}
	}
}