using ContainerLibrary;
using IL.Terraria;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Routed.UI;
using System;
using System.Collections.Generic;
using Terraria.UI;

namespace Routed
{
	internal static class Hooking
	{
		internal static void Initialize()
		{
			Recipe.Create += Recipe_Create;
			Recipe.FindRecipes += Recipe_FindRecipes;
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

				cursor.EmitDelegate<Func<Terraria.Recipe, Terraria.Item, int, int>>((self, ingredient, amount) =>
				{
					foreach (UIElement element in BaseLibrary.BaseLibrary.PanelGUI.Elements)
					{
						if (element is RequesterModulePanel panel)
						{
							ItemHandler handler = panel.Container.Handler;

							for (int i = 0; i < handler.Slots; i++)
							{
								if (amount <= 0) return amount;
								Terraria.Item item = handler.GetItemInSlot(i);

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

		// todo: query all items in network
		private static void Recipe_FindRecipes(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdcI4(0), i => i.MatchStloc(9)))
			{
				cursor.Emit(OpCodes.Ldloc, 6);

				cursor.EmitDelegate<Func<Dictionary<int, int>, Dictionary<int, int>>>(availableItems =>
				{
					foreach (UIElement element in BaseLibrary.BaseLibrary.PanelGUI.Elements)
					{
						if (element is RequesterModulePanel panel)
						{
							ItemHandler handler = panel.Container.Handler;

							for (int i = 0; i < handler.Slots; i++)
							{
								Terraria.Item item = handler.GetItemInSlot(i);
								if (item.stack > 0)
								{
									if (availableItems.ContainsKey(item.netID)) availableItems[item.netID] += item.stack;
									else availableItems[item.netID] = item.stack;
								}
							}
						}
					}

					return availableItems;
				});

				cursor.Emit(OpCodes.Stloc, 6);
			}
		}
	}
}