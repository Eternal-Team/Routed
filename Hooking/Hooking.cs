using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Routed.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Routed
{
	internal static class Hooking
	{
		private static Dictionary<int, int> existing;
		private static List<int> indexes;
		private static Texture2D textureTick;

		internal static void Initialize()
		{
			textureTick = ModContent.GetTexture("Routed/Textures/UI/Tick");

			existing = new Dictionary<int, int>();
			indexes = new List<int>();

			IL.Terraria.Recipe.Create += Recipe_Create;
			IL.Terraria.Recipe.FindRecipes += Recipe_FindRecipes;
			IL.Terraria.Main.DrawInventory += Main_DrawInventory;
		}

		private static void Main_DrawInventory(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);
			
			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchCall<ItemSlot>("Draw"), i => i.MatchStsfld<Main>("inventoryBack"), i => i.MatchLdloc(136)))
			{
				cursor.Index += 3;

				cursor.Emit(OpCodes.Ldloc, 136);
				cursor.Emit(OpCodes.Ldloc, 137);
				cursor.Emit(OpCodes.Ldloc, 138);
				cursor.EmitDelegate<Action<int, int, int>>((index, x, y) =>
				{
					//Recipe recipe = Main.recipe[Main.availableRecipe[index]];
					if (indexes.Contains(Main.availableRecipe[index]))
					{
						Main.spriteBatch.Draw(textureTick, new Vector2(x + 6, y + 6), Color.Red);
					}
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
					//Recipe recipe = Main.recipe[Main.availableRecipe[index]];
					if (indexes.Contains(Main.availableRecipe[index]))
					{
						Main.spriteBatch.Draw(textureTick, new Vector2(x + 6, y + 6), Color.Red);
					}
				});
			}
		}

		private static void Recipe_Create(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			//if (cursor.TryGotoNext(i => i.MatchLdloc(3), i => i.MatchBrfalse(out _)))
			//{
			//	cursor.Index++;
			//	cursor.Remove();
			//	cursor.Emit(OpCodes.Brfalse, labelAlchemy);
			//}

			//if (cursor.TryGotoNext(i => i.MatchLdarg(0), i => i.MatchLdfld<Recipe>("alchemy"), i => i.MatchBrfalse(out _)))
			//{
			//	cursor.MarkLabel(labelAlchemy);

			//	cursor.Emit(OpCodes.Ldarg, 0);
			//	cursor.Emit<Recipe>(OpCodes.Ldfld, "alchemy");
			//	cursor.Emit(OpCodes.Ldloc, 2);

			//	cursor.EmitDelegate<Func<bool, int, int>>((alchemy, amount) =>
			//	{
			//		if (alchemy && AlchemyApplyChance())
			//		{
			//			int reduction = 0;
			//			for (int j = 0; j < amount; j++)
			//			{
			//				if (Main.rand.Next(AlchemyConsumeChance()) == 0)
			//					reduction++;
			//			}

			//			amount -= reduction;
			//		}

			//		return amount;
			//	});

			//	cursor.Emit(OpCodes.Stloc, 2);
			//	cursor.Emit(OpCodes.Br, labelCheckAmount);
			//}

			//if (cursor.TryGotoNext(i => i.MatchLdloc(2), i => i.MatchLdcI4(0), i => i.MatchBle(out _)))
			//	cursor.MarkLabel(labelCheckAmount);


			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdloc(0), i => i.MatchLdcI4(1), i => i.MatchAdd()))
			{
				cursor.Emit(OpCodes.Ldarg, 0);
				cursor.Emit(OpCodes.Ldloc, 1);
				cursor.Emit(OpCodes.Ldloc, 2);

				cursor.EmitDelegate<Func<Recipe, Item, int, int>>((self, ingredient, amount) =>
				{
					foreach (UIElement element in BaseLibrary.BaseLibrary.PanelGUI.Elements)
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
					existing = availableItems.ToDictionary(pair => pair.Key, pair => pair.Value);
					indexes.Clear();

					foreach (UIElement element in BaseLibrary.BaseLibrary.PanelGUI.Elements)
					{
						if (element is RequesterModulePanel panel)
						{
							// todo: cache
							foreach (Item item in panel.Container.Parent.Network.MarkerModules.SelectMany(module => module.GetHandler()?.Items))
							{
								if (!item.IsAir)
								{
									if (availableItems.ContainsKey(item.netID)) availableItems[item.netID] += item.stack;
									else
									{
										availableItems[item.netID] = item.stack;
									}
								}
							}
						}
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
					if (recipe.requiredItem.Any(item => !item.IsAir && (!existing.ContainsKey(item.netID) || existing[item.netID] < item.stack)))
					{
						indexes.Add(index);
						Main.NewText($"Recipe for {recipe.createItem.HoverName} requiures items from network");
					}
				});
			}
		}
	}
}