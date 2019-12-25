using BaseLibrary;
using BaseLibrary.Input;
using Routed.Layer;
using Routed.Modules;
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
		public RoutedLayer RoutedLayer;
		public static Dictionary<string, Type> markerModules;

		public override void Load()
		{
			RoutedLayer = new RoutedLayer();
			markerModules = new Dictionary<string, Type>();

			if (!Main.dedServ) MouseEvents.ButtonPressed += args => args.Button == MouseButton.Right && RoutedLayer.Interact();

			EmitDynamicItems();
		}

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

		public override void Unload() => this.UnloadNullableTypes();
	}
}