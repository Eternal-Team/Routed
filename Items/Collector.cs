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
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Routed.Items
{
	public class Collector : BaseItem
	{
		public override string Texture => "Terraria/Item_" + ItemID.DD2ElderCrystalStand;

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
			item.createTile = ModContent.TileType<CollectorTile>();
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Collector");
		}
	}

	public class CollectorTE : BaseTE, IHasUI, IItemHandler
	{
		public override Type TileType => typeof(CollectorTile);
		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound { get; }
		public LegacySoundStyle OpenSound { get; }
		public ItemHandler Handler { get; }

		public CollectorTE()
		{
			UUID = Guid.NewGuid();
			Handler = new ItemHandler(18);
		}

		public override void Update()
		{
			for (int i = 0; i < Main.item.Length; i++)
			{
				ref Item item = ref Main.item[i];

				if (!item.active || item.IsAir) continue;

				if (Vector2.DistanceSquared(item.Center, Position.ToWorldCoordinates(40, 32)) < 8 * 16 * 8 * 16)
				{
					Handler.InsertItem(ref item);
				}
			}
		}

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");
			Handler.Load(tag.GetCompound("Items"));
		}

		public override void OnKill()
		{
			Handler.DropItems(new Rectangle(Position.X * 16, Position.Y * 16, 80, 64));
		}

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID,
			["Items"] = Handler.Save()
		};
	}

	public class CollectorTile : BaseTile
	{
		public override string Texture => "Terraria/Tiles_" + TileID.ElderCrystalStand;

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			CollectorTE drawer = Utility.GetTileEntity<CollectorTE>(i, j);
			PanelUI.Instance.CloseUI(drawer);

			Item.NewItem(i * 16, j * 16, 32, 32, ModContent.ItemType<Collector>());
			drawer.Kill(i, j);
		}

		public override bool NewRightClick(int i, int j)
		{
			CollectorTE drawer = Utility.GetTileEntity<CollectorTE>(i, j);
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
			TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidBottom, 0, 0);
			TileObjectData.newTile.Origin = new Point16(0, 3);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16 };
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<CollectorTE>().Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			disableSmartCursor = true;

			ModTranslation name = CreateMapEntryName();
			AddMapEntry(Color.Brown, name);
		}
	}

	public class CollectorUI : BaseUIPanel<CollectorTE>, IItemHandlerUI
	{
		public ItemHandler Handler => Container.Handler;
		public string GetTexture(Item item) => "Terraria/Item_" + ItemID.DD2ElderCrystalStand;

		public CollectorUI(CollectorTE chest) : base(chest)
		{
			Width.Pixels = 16 + (SlotSize + SlotMargin) * 9 - SlotMargin;
			Height.Pixels = 44 + SlotSize * 2 + SlotMargin;

			UITextButton buttonClose = new UITextButton("X")
			{
				Size = new Vector2(20),
				X = { Percent = 100 },
				Padding = Padding.Zero,
				RenderPanel = false
			};
			buttonClose.OnClick += args => PanelUI.Instance.CloseUI(Container);
			Add(buttonClose);

			UIText textLabel = new UIText("Collector")
			{
				Width = { Percent = 100 },
				Height = { Pixels = 20 },
				HorizontalAlignment = HorizontalAlignment.Center
			};
			Add(textLabel);

			UIGrid<UIContainerSlot> gridItems = new UIGrid<UIContainerSlot>(9)
			{
				Width = { Percent = 100 },
				Height = { Percent = 100, Pixels = -28 },
				Y = { Pixels = 28 },
				ItemMargin = SlotMargin
			};
			Add(gridItems);

			for (int i = 0; i < Container.Handler.Slots; i++)
			{
				UIContainerSlot slot = new UIContainerSlot(() => Container.Handler, i)
				{
					Width = { Pixels = SlotSize },
					Height = { Pixels = SlotSize }
				};
				gridItems.Add(slot);
			}
		}
	}
}