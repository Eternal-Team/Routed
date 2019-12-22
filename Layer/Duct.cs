﻿using BaseLibrary;
using LayerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Layer
{
	public class Duct : ModLayerElement<Duct>
	{
		public override string Texture => "";

		public override int DropItem => ModContent.ItemType<BasicDuct>();

		#region Static
		private static Vector2 Origin = new Vector2(14);

		private static Texture2D textureNormal;
		private static Texture2D textureDiagonal;
		private static Texture2D textureAll;
		private static Texture2D textureIntersection;
		private static Texture2D textureStraightV;
		private static Texture2D textureStraightH;

		private const string TextureLocation = "Routed/Textures/Duct/";

		static Duct()
		{
			textureNormal = ModContent.GetTexture(TextureLocation + "Normal");
			textureDiagonal = ModContent.GetTexture(TextureLocation + "Diagonal");
			textureAll = ModContent.GetTexture(TextureLocation + "All");
			textureIntersection = ModContent.GetTexture(TextureLocation + "Intersection");
			textureStraightV = ModContent.GetTexture(TextureLocation + "StraightV");
			textureStraightH = ModContent.GetTexture(TextureLocation + "StraightH");
		}
		#endregion

		private ushort frame;
		public RoutedNetwork Network;
		public BaseModule Module;

		public override void OnPlace()
		{
			List<RoutedNetwork> networks = GetNeighbors().Select(duct => duct.Network).Distinct().ToList();
			RoutedNetwork network = new RoutedNetwork
			{
				Tiles = networks.SelectMany(routedNetwork => routedNetwork.Tiles).Concat(this).ToList(),
				NetworkItems = networks.SelectMany(routedNetwork => routedNetwork.NetworkItems).ToList()
			};

			foreach (Duct duct in network.Tiles)
			{
				RoutedNetwork.Networks.Remove(duct.Network);
				duct.Network = network;
			}
		}

		public override void OnRemove()
		{
			if (Network.Tiles.Count == 1) RoutedNetwork.Networks.Remove(Network);
			// maybe add a special case for end-of-line tiles
			else
			{
				List<Point16> visited = new List<Point16>();
				List<List<Point16>> doot = new List<List<Point16>>();

				int numNetworks = 0;

				foreach (Duct duct in GetNeighbors())
				{
					if (visited.Contains(duct.Position)) continue;

					numNetworks++;
					visited.Add(duct.Position);

					List<Point16> p = new List<Point16> { Position };
					GetNeighborsRecursive(duct, p);
					visited.AddRange(p);

					p.Remove(Position);
					doot.Add(p);
				}

				if (numNetworks <= 1) Network.Tiles.Remove(this);
				else
				{
					for (int i = 0; i < doot.Count; i++)
					{
						RoutedNetwork network = new RoutedNetwork
						{
							Tiles = doot[i].Select(position => Layer[position]).ToList(),
							NetworkItems = doot[i].Select(position => Layer[position].Network).Distinct().SelectMany(routedNetwork => routedNetwork.NetworkItems).ToList()
						};
						foreach (Duct duct in network.Tiles)
						{
							duct.Network.NetworkItems.Clear();
							RoutedNetwork.Networks.Remove(duct.Network);
							duct.Network = network;
						}
					}
				}
			}
		}

		public void GetNeighborsRecursive(Duct duct, List<Point16> points)
		{
			foreach (Duct neighbor in duct.GetNeighbors())
			{
				if (!points.Contains(neighbor.Position))
				{
					points.Add(neighbor.Position);
					GetNeighborsRecursive(neighbor, points);
				}
			}
		}

		public IEnumerable<Duct> GetVisualNeighbors()
		{
			if (Layer.ContainsKey(Position.X + 1, Position.Y)) yield return Layer[Position.X + 1, Position.Y];
			if (Layer.ContainsKey(Position.X - 1, Position.Y)) yield return Layer[Position.X - 1, Position.Y];
			if (Layer.ContainsKey(Position.X, Position.Y + 1)) yield return Layer[Position.X, Position.Y + 1];
			if (Layer.ContainsKey(Position.X, Position.Y - 1)) yield return Layer[Position.X, Position.Y - 1];
			if (Layer.ContainsKey(Position.X + 1, Position.Y + 1)) yield return Layer[Position.X + 1, Position.Y + 1];
			if (Layer.ContainsKey(Position.X - 1, Position.Y - 1)) yield return Layer[Position.X - 1, Position.Y - 1];
			if (Layer.ContainsKey(Position.X - 1, Position.Y + 1)) yield return Layer[Position.X - 1, Position.Y + 1];
			if (Layer.ContainsKey(Position.X + 1, Position.Y - 1)) yield return Layer[Position.X + 1, Position.Y - 1];
		}

		public override void UpdateFrame()
		{
			frame = 0;

			if (Layer.ContainsKey(Position.X - 1, Position.Y)) frame |= 1;
			if (Layer.ContainsKey(Position.X - 1, Position.Y - 1)) frame |= 2;
			if (Layer.ContainsKey(Position.X, Position.Y - 1)) frame |= 4;
			if (Layer.ContainsKey(Position.X + 1, Position.Y - 1)) frame |= 8;
			if (Layer.ContainsKey(Position.X + 1, Position.Y)) frame |= 16;
			if (Layer.ContainsKey(Position.X + 1, Position.Y + 1)) frame |= 32;
			if (Layer.ContainsKey(Position.X, Position.Y + 1)) frame |= 64;
			if (Layer.ContainsKey(Position.X - 1, Position.Y + 1)) frame |= 128;
			if (Layer.ContainsKey(Position.X - 1, Position.Y)) frame |= 256;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Position.ToScreenCoordinates(false) + new Vector2(8);
			Color color = Lighting.GetColor(Position.X, Position.Y);

			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)position.X - 8, (int)position.Y - 8, 16, 16), new Color(40, 40, 40));

			if (frame == 0)
			{
				for (int i = 0; i < 4; i++) spriteBatch.Draw(textureNormal, position, null, color, MathHelper.PiOver2 * i, Origin, 1f, SpriteEffects.None, 0f);
			}
			else if (frame == 511)
			{
				Vector2 origin = Origin + new Vector2(1);
				for (int i = 0; i < 4; i++) spriteBatch.Draw(textureAll, position, null, color, MathHelper.PiOver2 * i, origin, 1f, SpriteEffects.None, 0f);
			}
			else
			{
				for (int i = 0; i < 4; i++)
				{
					float angle = MathHelper.PiOver2 * i;

					ushort pow = (ushort)Math.Pow(4, i);
					bool bit1 = (frame & (1 * pow)) == 0;
					bool bit2 = (frame & (2 * pow)) == 0;
					bool bit3 = (frame & (4 * pow)) == 0;

					if (bit1 && bit2 && bit3) spriteBatch.Draw(textureNormal, position, null, color, angle, Origin, 1f, SpriteEffects.None, 0f);
					else if (bit1 && !bit2 && bit3) spriteBatch.Draw(textureDiagonal, position, null, color, angle, Origin, 1f, SpriteEffects.None, 0f);
					else if (!bit1 && bit2 && !bit3) spriteBatch.Draw(textureIntersection, position, null, color, angle, Origin + new Vector2(2), 1f, SpriteEffects.None, 0f);
					else if (bit1 && bit2) spriteBatch.Draw(textureStraightV, position, null, color, angle, Origin, 1f, SpriteEffects.None, 0f);
					else if (!bit1 && bit2) spriteBatch.Draw(textureStraightH, position, null, color, angle, Origin, 1f, SpriteEffects.None, 0f);
					else if (!bit1 && !bit3) spriteBatch.Draw(textureAll, position, null, color, angle, Origin + new Vector2(1), 1f, SpriteEffects.None, 0f);
				}
			}

			//spriteBatch.Draw(Main.magicPixel, new Rectangle((int)(position.X - 8), (int)(position.Y - 8), 16, 16), Network.debugColor);

			Module?.Draw(spriteBatch);
		}

		public override void Update()
		{
			Module?.Update();
		}

		public override bool Interact() => Module?.Interact() ?? false;

		public override TagCompound Save()
		{
			TagCompound tag = new TagCompound();
			if (Module != null)
			{
				tag["Module"] = new TagCompound
				{
					["Type"] = Module.GetType().AssemblyQualifiedName,
					["Data"] = Module.Save()
				};
			}

			return tag;
		}

		public override void Load(TagCompound tag)
		{
			if (tag.ContainsKey("Module"))
			{
				TagCompound module = tag.GetCompound("Module");
				Module = (BaseModule)Activator.CreateInstance(Type.GetType(module.GetString("Type")));
				Module.Parent = this;
				Module.Load(module.GetCompound("Data"));
			}
		}
	}
}