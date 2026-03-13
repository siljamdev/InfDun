using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Door : Inanimate{
	static Shader doorShader;
	static Mesh doorMesh;
	static Texture2D doorTexture;
	static Texture2D doorOpenTexture;
	static Sound openSound;
	
	public static void initialize(){
		doorShader = Shader.fromAssembly("shaders.door");
		
		float[] vertices = {
			0.5f, -0.5f,
			0.5f, 0.5f,
			-0.5f, -0.5f,
			0.5f, 0.5f,
			-0.5f, 0.5f,
			-0.5f, -0.5f,
		};
		
		doorMesh = new Mesh("2", vertices, PrimitiveType.Triangles, "door");
		
		doorTexture = Texture2D.fromAssembly("res.textures.door.png", TextureParams.Default);
		doorOpenTexture = Texture2D.fromAssembly("res.textures.doorOpen.png", TextureParams.Default);
		openSound = Sound.monoFromAssembly("res.sounds.door.ogg");
	}
	
	public static void setProjection(Matrix4 m){
		//proj = m;
		doorShader.setMatrix4("projection", m);
	}
	
	public static void setView(Matrix4 m){
		doorShader.setMatrix4("view", m);
	}
	
	Matrix4 model;
	public List<Entity> entitiesToAdd = new();
	public List<Entity> entitiesToRemove = new();
	
	bool opening;
	bool opened;
	
	public Door(Vector2i p) : base(p){
		doCollision = true;
		
		model = Matrix4.CreateTranslation(new Vector3(position.X, position.Y, 0f));
	}
	
	public override bool onClick(Scene sce){
		if(opening){
			return false;
		}
		if(distance(sce.p) > 1.5f){
			return false;
		}
		if(sce.p.isDying){
			return false;
		}
		if(!sce.p.hasKey){
			return false;
		}
		
		sce.entitiesToRemove.AddRange(entitiesToRemove);
		
		sce.sm.play(openSound, new Vector3(position.X, position.Y, 0f), 1.4f, 0.9f + (float)sce.randCasual.NextDouble() * 0.7f);
		
		int c = sce.randCasual.Next(5) + 3;
		for(int i = 0; i < c; i++){
			sce.pr.add(new DoorParticle(position));
		}
		
		opening = true;
		sce.p.score += 50;
		
		return true;
	}
	
	public override void endSmooth(Scene sce){
		if(opening && !opened){
			opened = true;
			sce.entitiesToAdd.AddRange(entitiesToAdd);
			doCollision = false;
			
			int c = sce.randCasual.Next(5) + 3;
			for(int i = 0; i < c; i++){
				sce.pr.add(new DoorParticle(position));
			}
		}
		
		base.endSmooth(sce);
	}
	
	public override void draw(Scene sce){		
		doorShader.use();
		doorShader.setMatrix4("model", model);
		
		if(opened){
			doorOpenTexture.bind();
		}else{
			doorTexture.bind();
		}
		
		
		doorMesh.draw();
	}
}