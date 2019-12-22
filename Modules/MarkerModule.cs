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

	public class AnyItemsMode : FilterMode
	{
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

	public class MarkerModule : BaseModule, IHasUI
	{
		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound => SoundID.Item1;
		public LegacySoundStyle OpenSound => SoundID.Item1;

		//public enum Modes
		//{
		//	AnyItems,
		//	ItemsAlreadyInInv,
		//	ModBased,
		//	FilteredItems,
		//	TilesAndWalls,
		//	Consumables,
		//	Weapons,
		//	Accessories,
		//	Armor,
		//	Ammo,
		//	Materials // prefer previous categories before this
		//}

		public override int DropItem => ModContent.ItemType<Items.MarkerModule>();

		public FilterMode mode;

		public MarkerModule()
		{
			UUID = Guid.NewGuid();
			mode = new AnyItemsMode(this);
		}

		public override ItemHandler GetHandler()
		{
			if (Utility.TryGetTileEntity(Parent.Position, out ModTileEntity te) && te is IItemHandler handler) return handler.Handler;
			return null;
		}

		public override bool IsItemValid(Item item) => mode.Check(item);

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID,
			["Mode"] = mode.GetType().AssemblyQualifiedName
		};

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");
			mode = (FilterMode)Activator.CreateInstance(Type.GetType(tag.GetString("Mode")));
			mode.Module = this;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/MarkerModule"), position, Color.White);
		}
	}
}