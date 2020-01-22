using BaseLibrary;
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
		public enum DuctTier
		{
			Basic,
			Advanced,
			Elite
		}

		public override string Texture => "";

		public override int DropItem => ModContent.ItemType<BasicDuct>();
		private byte frame;
		public BaseModule Module;
		public RoutedNetwork Network;

		public DuctTier Tier = DuctTier.Basic;

		public int Speed;

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Position.ToScreenCoordinates(false) - new Vector2(6);
			Color color;

			switch (Tier)
			{
				case DuctTier.Basic:
					color = Color.LimeGreen;
					break;
				case DuctTier.Advanced:
					color = Color.Red;
					break;
				case DuctTier.Elite:
					color = Color.LightBlue;
					break;
				default:
					color = Lighting.GetColor(Position.X, Position.Y);
					break;
			}

			spriteBatch.Draw(texture, position, new Rectangle(frame % 16 * 30, frame / 16 * 30, 28, 28), color, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);

			Module?.Draw(spriteBatch);
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

		public override bool Interact()
		{
			/* SpriteBatch spriteBatch = Main.spriteBatch;
			Color color = Color.White;

			RenderTarget2D target = new RenderTarget2D(Main.graphics.GraphicsDevice, 28, 28, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

			Item i = new Item();
			i.SetDefaults(ModContent.ItemType<BasicDuct>());
			BaseLayerItem layerItem = i.modItem as BaseLayerItem;

			for (ushort f = 0; f < 256; f++)
			{
				Main.graphics.GraphicsDevice.SetRenderTarget(target);
				Main.graphics.GraphicsDevice.Clear(Color.Transparent);

				Debug.WriteLine(f);

				Main.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

				bool left = (f & 1) != 0;
				bool topLeft = (f & 2) != 0;
				bool top = (f & 4) != 0;
				bool topRight = (f & 8) != 0;
				bool right = (f & 16) != 0;
				bool bottomRight = (f & 32) != 0;
				bool bottom = (f & 64) != 0;
				bool bottomLeft = (f & 128) != 0;

				Layer.data.Clear();

				Duct element = new Duct
				{
					Position = new Point16(20, 20),
					Frame = Point16.Zero,
					Layer = Layer
				};
				Layer.data.Add(new Point16(20, 20), element);
				element.OnPlace(layerItem);

				element.UpdateFrame();
				foreach (Duct neighbor in element.GetNeighbors())
					neighbor.UpdateFrame();

				if (left)
				{
					element = new Duct
					{
						Position = new Point16(20 - 1, 20),
						Frame = Point16.Zero,
						Layer = Layer
					};
					Layer.data.Add(new Point16(20 - 1, 20), element);
					element.OnPlace(layerItem);

					element.UpdateFrame();
					foreach (Duct neighbor in element.GetNeighbors())
						neighbor.UpdateFrame();
				}

				if (topLeft)
				{
					element = new Duct
					{
						Position = new Point16(20 - 1, 20 - 1),
						Frame = Point16.Zero,
						Layer = Layer
					};
					Layer.data.Add(new Point16(20 - 1, 20 - 1), element);
					element.OnPlace(layerItem);

					element.UpdateFrame();
					foreach (Duct neighbor in element.GetNeighbors())
						neighbor.UpdateFrame();
				}

				if (top)
				{
					element = new Duct
					{
						Position = new Point16(20, 20 - 1),
						Frame = Point16.Zero,
						Layer = Layer
					};
					Layer.data.Add(new Point16(20, 20 - 1), element);
					element.OnPlace(layerItem);

					element.UpdateFrame();
					foreach (Duct neighbor in element.GetNeighbors())
						neighbor.UpdateFrame();
				}

				if (topRight)
				{
					element = new Duct
					{
						Position = new Point16(20 + 1, 20 - 1),
						Frame = Point16.Zero,
						Layer = Layer
					};
					Layer.data.Add(new Point16(20 + 1, 20 - 1), element);
					element.OnPlace(layerItem);

					element.UpdateFrame();
					foreach (Duct neighbor in element.GetNeighbors())
						neighbor.UpdateFrame();
				}

				if (right)
				{
					element = new Duct
					{
						Position = new Point16(20 + 1, 20),
						Frame = Point16.Zero,
						Layer = Layer
					};
					Layer.data.Add(new Point16(20 + 1, 20), element);
					element.OnPlace(layerItem);

					element.UpdateFrame();
					foreach (Duct neighbor in element.GetNeighbors())
						neighbor.UpdateFrame();
				}

				if (bottomRight)
				{
					element = new Duct
					{
						Position = new Point16(20 + 1, 20 + 1),
						Frame = Point16.Zero,
						Layer = Layer
					};
					Layer.data.Add(new Point16(20 + 1, 20 + 1), element);
					element.OnPlace(layerItem);

					element.UpdateFrame();
					foreach (Duct neighbor in element.GetNeighbors())
						neighbor.UpdateFrame();
				}

				if (bottom)
				{
					element = new Duct
					{
						Position = new Point16(20, 20 + 1),
						Frame = Point16.Zero,
						Layer = Layer
					};
					Layer.data.Add(new Point16(20, 20 + 1), element);
					element.OnPlace(layerItem);

					element.UpdateFrame();
					foreach (Duct neighbor in element.GetNeighbors())
						neighbor.UpdateFrame();
				}

				if (bottomLeft)
				{
					element = new Duct
					{
						Position = new Point16(20 - 1, 20 + 1),
						Frame = Point16.Zero,
						Layer = Layer
					};
					Layer.data.Add(new Point16(20 - 1, 20 + 1), element);
					element.OnPlace(layerItem);

					element.UpdateFrame();
					foreach (Duct neighbor in element.GetNeighbors())
						neighbor.UpdateFrame();
				}

				foreach (var value in Layer.data.Values)
				{
					value.UpdateFrame();
				}

				var prev = f;
				foreach (var duct in Layer.data)
				{
					Vector2 position = new Vector2(14, 14) + new Vector2((duct.Value.Position.X - 20) * 16, (duct.Value.Position.Y - 20) * 16);

					f = duct.Value.frame;
					left = (f & 1) != 0;
					topLeft = (f & 2) != 0;
					top = (f & 4) != 0;
					topRight = (f & 8) != 0;
					right = (f & 16) != 0;
					bottomRight = (f & 32) != 0;
					bottom = (f & 64) != 0;
					bottomLeft = (f & 128) != 0;

					spriteBatch.Draw(Main.magicPixel, new Rectangle((int)(position.X - 8), (int)(position.Y - 8), 16, 16), null, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 1f);

					#region straight
					if (left)
					{
						if (!top)
							spriteBatch.Draw(textureStraightH, position, null, color, 0, Origin, 1f, SpriteEffects.None, 0.75f);
						if (!bottom)
							spriteBatch.Draw(textureStraightH, position + new Vector2(0, 22), null, color, 0, Origin, 1f, SpriteEffects.None, 0.75f);
					}

					if (right)
					{
						if (!top)
							spriteBatch.Draw(textureStraightH, position + new Vector2(8, 0), null, color, 0, Origin, 1f, SpriteEffects.None, 0.75f);
						if (!bottom)
							spriteBatch.Draw(textureStraightH, position + new Vector2(8, 22), null, color, 0, Origin, 1f, SpriteEffects.None, 0.75f);
					}

					if (top)
					{
						if (!left)
							spriteBatch.Draw(textureStraightV, position, null, color, 0, Origin, 1f, SpriteEffects.None, 0.75f);
						if (!right)
							spriteBatch.Draw(textureStraightV, position + new Vector2(22, 0), null, color, 0, Origin, 1f, SpriteEffects.None, 0.75f);
					}

					if (bottom)
					{
						if (!left)
							spriteBatch.Draw(textureStraightV, position + new Vector2(0, 8), null, color, 0, Origin, 1f, SpriteEffects.None, 0.75f);
						if (!right)
							spriteBatch.Draw(textureStraightV, position + new Vector2(22, 8), null, color, 0, Origin, 1f, SpriteEffects.None, 0.75f);
					}
					#endregion

					#region diagonal
					if (!left && topLeft && !top)
					{
						spriteBatch.Draw(textureDiagonal, position, null, color, 0, Origin, 1f, SpriteEffects.None, 0.5f);
					}

					if (!top && topRight && !right)
					{
						spriteBatch.Draw(textureDiagonal, position, null, color, MathHelper.PiOver2, Origin, 1f, SpriteEffects.None, 0.5f);
					}

					if (!right && bottomRight && !bottom)
					{
						spriteBatch.Draw(textureDiagonal, position, null, color, MathHelper.PiOver2 * 2, Origin, 1f, SpriteEffects.None, 0.5f);
					}

					if (!bottom && bottomLeft && !left)
					{
						spriteBatch.Draw(textureDiagonal, position, null, color, MathHelper.PiOver2 * 3, Origin, 1f, SpriteEffects.None, 0.5f);
					}
					#endregion

					#region corner
					if (!left && !topLeft && !top)
					{
						spriteBatch.Draw(textureNormal, position, null, color, 0, Origin, 1f, SpriteEffects.None, 0.5f);
					}

					if (!top && !topRight && !right)
					{
						spriteBatch.Draw(textureNormal, position, null, color, MathHelper.PiOver2, Origin, 1f, SpriteEffects.None, 0.5f);
					}

					if (!right && !bottomRight && !bottom)
					{
						spriteBatch.Draw(textureNormal, position, null, color, MathHelper.PiOver2 * 2, Origin, 1f, SpriteEffects.None, 0.5f);
					}

					if (!bottom && !bottomLeft && !left)
					{
						spriteBatch.Draw(textureNormal, position, null, color, MathHelper.PiOver2 * 3, Origin, 1f, SpriteEffects.None, 0.5f);
					}
					#endregion

					#region intersection
					if (left && !topLeft && top)
					{
						spriteBatch.Draw(textureIntersection, position, null, color, 0, Origin + new Vector2(2), 1f, SpriteEffects.None, 0.5f);
					}

					if (top && !topRight && right)
					{
						spriteBatch.Draw(textureIntersection, position, null, color, MathHelper.PiOver2, Origin + new Vector2(2), 1f, SpriteEffects.None, 0.5f);
					}

					if (right && !bottomRight && bottom)
					{
						spriteBatch.Draw(textureIntersection, position, null, color, MathHelper.PiOver2 * 2, Origin + new Vector2(2), 1f, SpriteEffects.None, 0.5f);
					}

					if (bottom && !bottomLeft && left)
					{
						spriteBatch.Draw(textureIntersection, position, null, color, MathHelper.PiOver2 * 3, Origin + new Vector2(2), 1f, SpriteEffects.None, 0.5f);
					}
					#endregion

					if (left && topLeft && top)
					{
						spriteBatch.Draw(textureAll, position, null, color, 0f, Origin, 1f, SpriteEffects.None, 0.5f);
					}

					if (top && topRight && right)
					{
						spriteBatch.Draw(textureAll, position + new Vector2(14, 0), null, color, 0f, Origin, 1f, SpriteEffects.None, 0.5f);
					}

					if (right && bottomRight && bottom)
					{
						spriteBatch.Draw(textureAll, position + new Vector2(14, 14), null, color, 0f, Origin, 1f, SpriteEffects.None, 0.5f);
					}

					if (bottom && bottomLeft && left)
					{
						spriteBatch.Draw(textureAll, position + new Vector2(0, 14), null, color, 0f, Origin, 1f, SpriteEffects.None, 0.5f);
					}
				}

				f = prev;
				Main.spriteBatch.End();

				Main.graphics.GraphicsDevice.SetRenderTarget(null);
				using (FileStream stream = new FileStream($@"G:\C#\Terraria\Mods\Routed\Textures\Duct_Gen\Frame_{f}.png", FileMode.Create))
				{
					target.SaveAsPng(stream, 28, 28);
				}
			}*/

			return Module?.Interact() ?? false;
		}

		public override void Load(TagCompound tag)
		{
			try
			{
				Tier = (DuctTier)tag.GetInt("Tier");
				switch (Tier)
				{
					case DuctTier.Basic:
						Speed = 20;
						break;
					case DuctTier.Advanced:
						Speed = 12;
						break;
					case DuctTier.Elite:
						Speed = 5;
						break;
				}

				if (tag.ContainsKey("Module"))
				{
					TagCompound module = tag.GetCompound("Module");
					Module = (BaseModule)Activator.CreateInstance(Type.GetType(module.GetString("Type")));
					Module.Parent = this;
					Module.Load(module.GetCompound("Data"));
				}
			}
			catch
			{
			}
		}

		public override void OnPlace(BaseLayerItem item)
		{
			if (item is BaseDuct d)
			{
				Tier = d.Tier;
				switch (Tier)
				{
					case DuctTier.Basic:
						Speed = 20;
						break;
					case DuctTier.Advanced:
						Speed = 12;
						break;
					case DuctTier.Elite:
						Speed = 5;
						break;
				}
			}

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

			foreach (Duct duct in network.Tiles)
			{
				if ((duct.frame & 1) != 0 && (duct.frame & 16) != 0 && (duct.frame & 4) == 0 && (duct.frame & 64) == 0) duct.isNode = false;
				else if ((duct.frame & 1) == 0 && (duct.frame & 16) == 0 && (duct.frame & 4) != 0 && (duct.frame & 64) != 0) duct.isNode = false;
				else duct.isNode = true;
			}
		}

		private bool isNode;

		public override void OnRemove()
		{
			if (Network.Tiles.Count == 1) RoutedNetwork.Networks.Remove(Network);
			else if (GetNeighbors().Count() == 1)
			{
				Network.Tiles.Remove(this);

				foreach (Duct duct in Network.Tiles)
				{
					if ((duct.frame & 1) != 0 && (duct.frame & 16) != 0 && (duct.frame & 4) == 0 && (duct.frame & 64) == 0) duct.isNode = false;
					else if ((duct.frame & 1) == 0 && (duct.frame & 16) == 0 && (duct.frame & 4) != 0 && (duct.frame & 64) != 0) duct.isNode = false;
					else duct.isNode = true;
				}
			}
			else
			{
				List<Point16> visited = new List<Point16>();
				List<List<Point16>> newNetworks = new List<List<Point16>>();

				foreach (Duct duct in GetNeighbors())
				{
					if (visited.Contains(duct.Position)) continue;

					visited.Add(duct.Position);

					List<Point16> p = new List<Point16> { Position, duct.Position };
					GetNeighborsRecursive(duct, p);
					visited.AddRange(p);

					p.Remove(Position);
					newNetworks.Add(p);
				}

				if (newNetworks.Count <= 1)
				{
					Network.Tiles.Remove(this);
					Network.CheckPaths();
				}
				else
				{
					for (int i = 0; i < newNetworks.Count; i++)
					{
						RoutedNetwork network = new RoutedNetwork
						{
							Tiles = newNetworks[i].Select(position => Layer[position]).ToList(),
							NetworkItems = newNetworks[i].Select(position => Layer[position].Network).Distinct().SelectMany(routedNetwork => routedNetwork.NetworkItems).ToList()
						};
						network.CheckPaths();
						foreach (Duct duct in network.Tiles)
						{
							duct.Network.NetworkItems.Clear();
							RoutedNetwork.Networks.Remove(duct.Network);
							duct.Network = network;

							if ((duct.frame & 1) != 0 && (duct.frame & 16) != 0 && (duct.frame & 4) == 0 && (duct.frame & 64) == 0) duct.isNode = false;
							else if ((duct.frame & 1) == 0 && (duct.frame & 16) == 0 && (duct.frame & 4) != 0 && (duct.frame & 64) != 0) duct.isNode = false;
							else duct.isNode = true;
						}
					}
				}
			}
		}

		public override TagCompound Save()
		{
			TagCompound tag = new TagCompound { ["Tier"] = (int)Tier };
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

		public override void Update()
		{
			Module?.InternalUpdate();
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
		}

		#region Static
		//private static Vector2 Origin = new Vector2(14);

		//private static Texture2D textureNormal;
		//private static Texture2D textureDiagonal;
		//private static Texture2D textureAll;
		//private static Texture2D textureIntersection;
		//private static Texture2D textureStraightV;
		//private static Texture2D textureStraightH;

		//private const string TextureLocation = "Routed/Textures/Duct/";

		private static Texture2D texture;

		internal static void Initialize()
		{
			texture = ModContent.GetTexture("Routed/Textures/Duct/Atlas");

			//Bitmap bmp = new Bitmap(30 * 16, 30 * 16);

			//using (Graphics g = Graphics.FromImage(bmp))
			//{
			//	g.Clear(System.Drawing.Color.Transparent);

			//	for (int i = 0; i < 256; i++)
			//	{
			//		Image img = Image.FromFile($@"G:\C#\Terraria\Mods\Routed\Textures\Duct_Gen\Frame_{i}.png");

			//		int indexX = i % 16;
			//		int indexY = i / 16;

			//		g.FillRectangle(Brushes.DeepPink, indexX * 30 + 28, indexY * 30, 2, 30);
			//		g.FillRectangle(Brushes.DeepPink, indexX * 30, indexY * 30 + 28, 30, 2);
			//		g.DrawImage(img, indexX * 30, indexY * 30, 28, 28);
			//	}
			//}

			//bmp.Save(@"G:\C#\Terraria\Mods\Routed\Textures\Duct\Atlas.png", ImageFormat.Png);

			//textureNormal = ModContent.GetTexture(TextureLocation + "Normal");
			//textureDiagonal = ModContent.GetTexture(TextureLocation + "Diagonal");
			//textureAll = ModContent.GetTexture(TextureLocation + "All");
			//textureIntersection = ModContent.GetTexture(TextureLocation + "Intersection");
			//textureStraightV = ModContent.GetTexture(TextureLocation + "StraightV");
			//textureStraightH = ModContent.GetTexture(TextureLocation + "StraightH");
		}
		#endregion
	}
}