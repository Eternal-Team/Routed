using BaseLibrary;
using BaseLibrary.Input;
using BaseLibrary.Input.Mouse;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using Routed.Modules.FilterModes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Terraria;
using Terraria.ModLoader;

namespace Routed
{
	public class Routed : Mod
	{
		public static Dictionary<string, Type> markerModules;

		public static RoutedLayer RoutedLayer;

		public void EmitDynamicItems()
		{
			const MethodAttributes PropertyOverrideAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;

			AppDomain domain = Thread.GetDomain();
			AssemblyBuilder assemblyBuilder = domain.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyBuilder.GetName().Name, false);

			foreach (Type type in Code.GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(FilterMode))))
			{
				string mode = type.Name.Replace("Mode", "");
				string name = "MarkerModule" + mode;
				TypeBuilder builder = moduleBuilder.DefineType(name, TypeAttributes.Public, typeof(Items.MarkerModule));

				MethodBuilder methodGetMode = builder.DefineMethod("get_Mode", PropertyOverrideAttributes, typeof(string), Type.EmptyTypes);

				ILGenerator ilGetMode = methodGetMode.GetILGenerator();
				ilGetMode.Emit(OpCodes.Ldstr, mode);
				ilGetMode.Emit(OpCodes.Ret);
				builder.DefineMethodOverride(methodGetMode, typeof(Items.MarkerModule).GetMethod("get_Mode", Utility.defaultFlags));

				Type dynamicType = builder.CreateType();
				AddItem(name, (ModItem)Activator.CreateInstance(dynamicType));
				markerModules.Add(mode, type);
			}
		}

		internal static Texture2D textureLootAll;
		internal static Texture2D textureDepositAll;
		internal static Texture2D textureQuickStack;

		public override void Load()
		{
			RoutedLayer = new RoutedLayer();
			markerModules = new Dictionary<string, Type>();

			Duct.Initialize();
			Hooking.Initialize();

			if (!Main.dedServ)
			{
				textureLootAll = ModContent.GetTexture("BaseLibrary/Textures/UI/LootAll");
				textureDepositAll = ModContent.GetTexture("BaseLibrary/Textures/UI/DepositAll");
				textureQuickStack = ModContent.GetTexture("BaseLibrary/Textures/UI/QuickStack");
			}

			EmitDynamicItems();
		}

		public override void Unload() => this.UnloadNullableTypes();

		public override void PostSetupContent()
		{
			if (!Main.dedServ) BaseLibrary.BaseLibrary.Layers.PushLayer(new RLayer());
		}
	}

	internal class RLayer : BaseLibrary.Layer
	{
		// bug: click and up still get passed
		public override void OnMouseDown(MouseButtonEventArgs args)
		{
			if (args.Button == MouseButton.Right && Routed.RoutedLayer.Interact()) args.Handled = true;
		}
	}
}