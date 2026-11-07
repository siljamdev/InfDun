using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

static class Tile{
	//Texture atlas size
	static readonly Vector2i tileSpriteNum = new Vector2i(16, 2);
	
	public readonly static HashSet<int> transitables = new HashSet<int>(){Floor, Puddle, Exit, Start, Skull, Fungi, SmallPuddle, Pots, Crack};
	public readonly static HashSet<int> withRandomRotation = new HashSet<int>(){Floor, Puddle, Fungi, SmallPuddle, Crack};
	public readonly static HashSet<int> withRandomHorizontalFlip = new HashSet<int>(){Skull, Pots};
	public readonly static HashSet<int> withRandomFlip = new HashSet<int>(){Floor, Fungi, Puddle, SmallPuddle, Crack};
	
	public const int Air = 0;
	public const int Floor = 1;
	public const int Puddle = 2;
	
	public const int Exit = 17;
	public const int Start = 18;
	
	public const int Skull = 19;
	public const int Fungi = 20;
	public const int SmallPuddle = 21;
	public const int Pots = 23;
	public const int Crack = 24;
	
	public const int Wall = 3;
	public const int TorchWall = 16;
	public const int MossyWall = 22;
	
	//Create the mesh for the tile shader
	public static Mesh createTilesMesh(int[] tiles, int x, int y, Random rand){
		List<float> ver = new();
		
		//Prevent weird issues
		float epsilon = 1e-4f;
		
		//vertex positions
		Vector2[] p = new Vector2[]{
			new Vector2(0.5f + epsilon, 0.5f + epsilon),
			new Vector2(-0.5f - epsilon, 0.5f + epsilon),
			new Vector2(-0.5f - epsilon, -0.5f - epsilon),
			new Vector2(0.5f + epsilon, 0.5f + epsilon),
			new Vector2(-0.5f - epsilon, -0.5f - epsilon),
			new Vector2(0.5f + epsilon, -0.5f - epsilon)
		};
		
		Vector2 invSpNum = new Vector2(1f/tileSpriteNum.X, 1f/tileSpriteNum.Y);
		
		Vector2[] r = new Vector2[]{
			new Vector2(invSpNum.X - epsilon, epsilon), //0
			new Vector2(epsilon, epsilon), //1
			new Vector2(epsilon, invSpNum.Y - epsilon), //2
			new Vector2(invSpNum.X - epsilon, invSpNum.Y - epsilon) //3
		};
		
		for(int i = 0; i < y; i++){
			for(int j = 0; j < x; j++){
				int m = tiles[i * x + j];
				
				//Air doesnt need anything
				if(m == 0){
					continue;
				}
				
				//Handle random rotations, these are indices for r
				int[] texCorners = Tile.withRandomRotation.Contains(m) ? rand.Next(4) switch{
					0 => new int[]{0, 1, 2, 0, 2, 3}, //No rot
					1 => new int[]{1, 2, 3, 1, 3, 0}, //90 degree clockwise
					2 => new int[]{2, 3, 0, 2, 0, 1}, //180 degree
					3 => new int[]{3, 0, 1, 3, 1, 2}, //270 degree
					_ => new int[]{0, 1, 2, 0, 2, 3} //No rot
				} : new int[]{0, 1, 2, 0, 2, 3};
				
				//Handle flips, indices for r
				if(Tile.withRandomFlip.Contains(m)){
					switch(rand.Next(3)){
						case 0: //no flip
							break;
						case 1: //hor flip
							for(int k = 0; k < 6; k++){
								texCorners[k] = texCorners[k] switch{
									0 => 1,
									1 => 0,
									2 => 3,
									3 => 2,
									_ => texCorners[k]
								};
							}
							break;
						case 2: //v flip
							for(int k = 0; k < 6; k++){
								texCorners[k] = texCorners[k] switch{
									0 => 3,
									1 => 2,
									2 => 1,
									3 => 0,
									_ => texCorners[k]
								};
							}
							break;
					}
				}else if(Tile.withRandomHorizontalFlip.Contains(m) && rand.Next(2) == 0){
					for(int k = 0; k < 6; k++){
						texCorners[k] = texCorners[k] switch{
							0 => 1,
							1 => 0,
							2 => 3,
							3 => 2,
							_ => texCorners[k]
						};
					}
				}
				
				m -= 1; //Adjust for texture index, as 0 is air
				
				Vector2 textureIndex = new Vector2i(m % tileSpriteNum.X, m / tileSpriteNum.X);
				textureIndex /= (Vector2) tileSpriteNum;
				
				for(int k = 0; k < 6; k++){
					ver.Add((float) j + p[k].X);
					ver.Add((float) i + p[k].Y);
					ver.Add(textureIndex.X + r[texCorners[k]].X);
					ver.Add(textureIndex.Y + r[texCorners[k]].Y);
				}
			}
		}
		
		return new Mesh("22", ver.ToArray(), PrimitiveType.Triangles, "tiles");
	}
}