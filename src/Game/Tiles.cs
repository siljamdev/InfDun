using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

static class Tile{
	//Texture atlas size
	static readonly Vector2i tileSpriteNum = new Vector2i(16, 2);
	
	public readonly static int[] transitables = new int[]{Floor, Puddle, Exit, Start, Skull, Fungi, SmallPuddle};
	public readonly static int[] withRandomRotation = new int[]{Floor, Puddle, Fungi, SmallPuddle};
	
	public const int Air = 0;
	public const int Floor = 1;
	public const int Puddle = 2;
	
	public const int Exit = 17;
	public const int Start = 18;
	
	public const int Skull = 19;
	public const int Fungi = 20;
	public const int SmallPuddle = 21;
	
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
					0 => new int[]{0, 1, 2, 0, 2, 3},
					1 => new int[]{1, 2, 3, 1, 3, 0},
					2 => new int[]{2, 3, 0, 2, 0, 1},
					3 => new int[]{3, 0, 1, 3, 1, 2},
					_ => new int[]{0, 1, 2, 0, 2, 3}
				} : new int[]{0, 1, 2, 0, 2, 3};
				
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