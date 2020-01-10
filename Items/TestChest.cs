using BaseLibrary;
using BaseLibrary.Items;
using BaseLibrary.Tiles;
using BaseLibrary.Tiles.TileEntites;
using BaseLibrary.UI;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Routed.Items
{
	public class TestChest : BaseItem
	{
		public override string Texture => "Terraria/Item_48";

		public override void SetDefaults()
		{
			item.width = 16;
			item.height = 16;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			item.createTile = ModContent.TileType<TestChestTile>();
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Test Chest");
		}
	}

	public class TestChestTE : BaseTE, IHasUI, IItemHandler
	{
		public override Type TileType => typeof(TestChestTile);
		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound { get; }
		public LegacySoundStyle OpenSound { get; }
		public ItemHandler Handler { get; }

		public TestChestTE()
		{
			UUID = Guid.NewGuid();
			Handler = new ItemHandler(27);
		}

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");
			Handler.Load(tag.GetCompound("Items"));
		}

		public override void OnKill()
		{
			Handler.DropItems(new Rectangle(Position.X * 16, Position.Y * 16, 32, 32));
		}

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID,
			["Items"] = Handler.Save()
		};
	}

	public class TestChestTile : BaseTile
	{
		public override string Texture => "Terraria/Tiles_21";

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			TestChestTE drawer = Utility.GetTileEntity<TestChestTE>(i, j);
			PanelUI.Instance.CloseUI(drawer);

			Item.NewItem(i * 16, j * 16, 32, 32, ModContent.ItemType<TestChest>());
			drawer.Kill(i, j);
		}

		public override bool NewRightClick(int i, int j)
		{
			TestChestTE drawer = Utility.GetTileEntity<TestChestTE>(i, j);
			if (drawer == null) return false;

			PanelUI.Instance.HandleUI(drawer);

			return true;
		}

		public override void SetDefaults()
		{
			Main.tileSolidTop[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidBottom, 0, 0);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TestChestTE>().Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			disableSmartCursor = true;

			ModTranslation name = CreateMapEntryName();
			AddMapEntry(Color.Brown, name);
		}
	}

	public class TestChestUI : BaseUIPanel<TestChestTE>, IItemHandlerUI
	{
		public ItemHandler Handler => Container.Handler;
		public string GetTexture(Item item) => "Terraria/Item_48";

		public TestChestUI(TestChestTE chest) : base(chest)
		{
			Width.Pixels = 408;
			Height.Pixels = 172;

			UITextButton buttonClose = new UITextButton("X")
			{
				Size = new Vector2(20),
				X = { Percent = 100 },
				Padding = Padding.Zero,
				RenderPanel = false
			};
			buttonClose.OnClick += args => PanelUI.Instance.CloseUI(Container);
			Add(buttonClose);

			UIText textLabel = new UIText("Test Chest")
			{
				Width = { Percent = 100 },
				Height = { Pixels = 20 },
				HorizontalAlignment = HorizontalAlignment.Center
			};
			Add(textLabel);

			UIGrid<UIContainerSlot> gridItems = new UIGrid<UIContainerSlot>(9)
			{
				Width = { Percent = 100 },
				Height = { Pixels = -28, Percent = 100 },
				Y = { Pixels = 28 },
				ListPadding = 4
			};
			Add(gridItems);

			for (int i = 0; i < Container.Handler.Slots; i++)
			{
				UIContainerSlot slot = new UIContainerSlot(() => Container.Handler, i);
				gridItems.Add(slot);
			}
		}
	}
}