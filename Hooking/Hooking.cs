using BaseLibrary;
using BaseLibrary.UI;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Routed.Layer;
using Routed.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Routed
{
	internal static class Hooking
	{
		private static Dictionary<int, int> existing;
		private static List<int> indexes;
		private static Texture2D textureTick;
		internal static RoutedNetwork Network;

		internal static void Initialize()
		{
			textureTick = ModContent.GetTexture("Routed/Textures/UI/Tick");

			existing = new Dictionary<int, int>();
			indexes = new List<int>();

			IL.Terraria.Recipe.Create += Recipe_Create;
			IL.Terraria.Recipe.FindRecipes += Recipe_FindRecipes;
			IL.Terraria.Main.DrawInventory += Main_DrawInventory;
			On.Terraria.IngameOptions.Draw += DrawBG;
			IL.Terraria.IngameOptions.Draw += DrawButton;

			IL.Terraria.Main.DoDraw += DrawLayer;
		}

		private static void DrawLayer(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			if (cursor.TryGotoNext(i => i.MatchCall<Main>("DrawWoF")))
			{
				cursor.EmitDelegate<Action>(() =>
				{
					Routed.RoutedLayer.FilterVisible();
					Routed.RoutedLayer.Visible = Routed.RoutedLayer.Visible.OrderBy(pair => pair.Value.Tier).ToDictionary(pair => pair.Key, pair => pair.Value);

					SpriteBatchState state = Utility.End(Main.spriteBatch);
					Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Utility.DefaultSamplerState, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

					if (Mode == ViewMode.PartiallyVisible)
					{
						Routed.RoutedLayer.DrawDucts(Main.spriteBatch);
						Routed.RoutedLayer.DrawItems(Main.spriteBatch);
					}

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(state);
				});
			}
		}

		private static Rectangle toggleRectangle = new Rectangle(16, 16, 32, 32);

		public enum ViewMode
		{
			AlwaysVisible,
			PartiallyVisible,
			Hidden
		}

		public static ViewMode Mode = ViewMode.AlwaysVisible;

		private static void DrawButton(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdloc(25), i => i.MatchStsfld(typeof(UILinkPointNavigator.Shortcuts), "INGAMEOPTIONS_BUTTONS_RIGHT")))
			{
				cursor.Index += 2;

				cursor.Emit(OpCodes.Ldarg, 1);

				cursor.EmitDelegate<Action<SpriteBatch>>(spriteBatch =>
				{
					spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/UI/Mode"), toggleRectangle, new Rectangle(0, 32 * (int)Mode, 32, 32), Color.White);

					if (toggleRectangle.Contains(Main.mouseX, Main.mouseY))
					{
						if (Main.mouseLeft && Main.mouseLeftRelease) Mode = Mode.NextEnum();

						Main.blockMouse = true;
						Main.instance.MouseTextHackZoom("Current view: " + Mode);
					}
				});
			}
		}

		private static void DrawBG(On.Terraria.IngameOptions.orig_Draw orig, Main mainInstance, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Main.magicPixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * 0.5f);

			orig(mainInstance, spriteBatch);
		}

		private static void RequestItems(ILCursor cursor)
		{
			ILLabel label = cursor.DefineLabel();

			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchCallvirt<Player>("IsStackingItems"), i => i.MatchBrtrue(out _), i => i.MatchLdsfld<Main>("mouseLeftRelease"), i => i.MatchBrfalse(out _)))
			{
				cursor.Index += 6;

				cursor.Emit(OpCodes.Ldloc, 136);
				cursor.EmitDelegate<Func<int, bool>>(index =>
				{
					if (indexes.Contains(Main.availableRecipe[index]))
					{
						if (Network == null) return false;

						Recipe recipe = Main.recipe[Main.availableRecipe[index]];

						foreach (Item item in recipe.requiredItem)
						{
							if (!item.IsAir)
							{
								RequesterModulePanel ui = PanelUI.Instance.Children.First(panel => panel is RequesterModulePanel) as RequesterModulePanel;
								ui?.Container.RequestItem(item.type, item.stack);
							}
						}

						return false;
					}

					return true;
				});
				cursor.Emit(OpCodes.Brfalse, label);
			}

			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdloc(136), i => i.MatchStsfld<Main>("focusRecipe")))
			{
				cursor.Index -= 4;

				cursor.MarkLabel(label);
			}
		}

		private static void DrawTicks(ILCursor cursor)
		{
			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchCall<ItemSlot>("Draw"), i => i.MatchStsfld<Main>("inventoryBack"), i => i.MatchLdloc(136)))
			{
				cursor.Index += 3;

				cursor.Emit(OpCodes.Ldloc, 136);
				cursor.Emit(OpCodes.Ldloc, 137);
				cursor.Emit(OpCodes.Ldloc, 138);
				cursor.EmitDelegate<Action<int, int, int>>((index, x, y) =>
				{
					if (indexes.Contains(Main.availableRecipe[index])) Main.spriteBatch.Draw(textureTick, new Vector2(x + 6, y + 6), Color.Red);
				});
			}

			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdloc(13), i => i.MatchCall<ItemSlot>("Draw"), i => i.MatchStsfld<Main>("inventoryBack"), i => i.MatchLdloc(163)))
			{
				cursor.Index += 3;

				cursor.Emit(OpCodes.Ldloc, 167);
				cursor.Emit(OpCodes.Ldloc, 168);
				cursor.Emit(OpCodes.Ldloc, 169);
				cursor.EmitDelegate<Action<int, int, int>>((index, x, y) =>
				{
					if (indexes.Contains(Main.availableRecipe[index])) Main.spriteBatch.Draw(textureTick, new Vector2(x + 6, y + 6), Color.Red);
				});
			}
		}

		private static void Main_DrawInventory(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			RequestItems(cursor);

			DrawTicks(cursor);
		}

		private static void Recipe_Create(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdloc(0), i => i.MatchLdcI4(1), i => i.MatchAdd()))
			{
				cursor.Emit(OpCodes.Ldarg, 0);
				cursor.Emit(OpCodes.Ldloc, 1);
				cursor.Emit(OpCodes.Ldloc, 2);

				cursor.EmitDelegate<Func<Recipe, Item, int, int>>((self, ingredient, amount) =>
				{
					foreach (BaseElement element in PanelUI.Instance.Children)
					{
						if (element is RequesterModulePanel panel)
						{
							ItemHandler handler = panel.Container.Handler;

							for (int i = 0; i < handler.Slots; i++)
							{
								if (amount <= 0) return amount;
								Item item = handler.GetItemInSlot(i);

								if (item.IsTheSameAs(ingredient) || self.useWood(item.type, ingredient.type) || self.useSand(item.type, ingredient.type) || self.useIronBar(item.type, ingredient.type) || self.usePressurePlate(item.type, ingredient.type) || self.useFragment(item.type, ingredient.type) || self.AcceptedByItemGroups(item.type, ingredient.type))
								{
									int count = Math.Min(amount, item.stack);
									amount -= count;
									handler.ExtractItem(i, count);
								}
							}
						}
					}

					return amount;
				});

				cursor.Emit(OpCodes.Stloc, 2);
			}
		}

		private static void Recipe_FindRecipes(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdcI4(0), i => i.MatchStloc(9)))
			{
				cursor.Emit(OpCodes.Ldloc, 6);

				cursor.EmitDelegate<Func<Dictionary<int, int>, Dictionary<int, int>>>(availableItems =>
				{
					foreach (BaseElement element in PanelUI.Instance.Children)
					{
						if (element is RequesterModulePanel panel)
						{
							ItemHandler handler = panel.Container.Handler;
							for (int i = 0; i < handler.Slots; i++)
							{
								Item item = handler.GetItemInSlot(i);
								if (item.IsAir) continue;

								if (availableItems.ContainsKey(item.netID)) availableItems[item.netID] += item.stack;
								else availableItems[item.netID] = item.stack;
							}
						}
					}

					existing = availableItems.ToDictionary(pair => pair.Key, pair => pair.Value);
					indexes.Clear();

					if (Network == null) return availableItems;

					foreach (var pair in Network.ItemCache)
					{
						if (availableItems.ContainsKey(pair.Key)) availableItems[pair.Key] += pair.Value;
						else availableItems[pair.Key] = pair.Value;
					}

					return availableItems;
				});

				cursor.Emit(OpCodes.Stloc, 6);
			}

			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchAdd(), i => i.MatchStsfld<Main>("numAvailableRecipes")))
			{
				cursor.Index += 2;

				cursor.Emit(OpCodes.Ldloc, 9);
				cursor.EmitDelegate<Action<int>>(index =>
				{
					Recipe recipe = Main.recipe[index];
					if (recipe.requiredItem.Any(item => !item.IsAir && (!existing.ContainsKey(item.netID) || existing[item.netID] < item.stack))) indexes.Add(index);
				});
			}
		}
	}
}