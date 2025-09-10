using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

abstract class Entity{
	#region static
	//public static Matrix4 proj;
	protected static Shader entityShader;
	protected static Mesh entityMesh;
	protected static Texture2D entityTexture;
	
	protected static readonly Matrix4 leftRotation = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(180f));
	
	protected static readonly Vector2i atlasSize = new Vector2i(16, 16);
	
	public static void initialize(){
		entityShader = Shader.fromAssembly("shaders.entity");
		
		entityShader.setVector2("inverseAltasSize", new Vector2(1f / atlasSize.X, 1f / atlasSize.Y));
		
		float[] vertices = {
			0.5f, 0.0f,
			0.5f, 1.0f,
			-0.5f, 0.0f,
			0.5f, 1.0f,
			-0.5f, 1.0f,
			-0.5f, 0.0f,
		};
		
		entityMesh = new Mesh("2", vertices, PrimitiveType.Triangles, "entity");
		
		entityTexture = Texture2D.fromAssembly("res.textures.entities.png", TextureParams.Default);
		
		Living.initialize();
		Coin.initialize();
		Medkit.initialize();
		Orb.initialize();
		Altar.initialize();
		BefriendingAltar.initialize();
		DripGenerator.initialize();
	}
	
	public static void setProjection(Matrix4 m){
		//proj = m;
		entityShader.setMatrix4("projection", m);
	}
	
	public static void setView(Matrix4 m){
		entityShader.setMatrix4("view", m);
	}
	#endregion static
	
	public Vector2i position;
	public Vector2i oldPosition;
	public EntityState state;
	
	protected virtual int atlasColumn => 0;
	
	public bool facingLeft;
	
	public bool doCollision = true;
	
	public Entity(){
		
	}
	
	public Entity(Vector2i p){
		position = p;
		oldPosition = p;
	}
	
	public float distanceSquared(Entity s){
		return Vector2.DistanceSquared(position, s.position);
	}
	
	public float distanceSquared(Vector2i s){
		return Vector2.DistanceSquared(position, s);
	}
	
	public float distance(Vector2i s){
		return (Vector2.Distance(position, s) + Vector2.Distance(oldPosition, s)) / 2f;
	}
	
	public float distance(Entity s){
		return (Vector2.Distance(position, s.position) + Vector2.Distance(oldPosition, s.oldPosition)) / 2f;
	}
	
	public virtual void endSmooth(Scene sce){		
		if(state == EntityState.moving){
			oldPosition = position;
			state = EntityState.idle;
		}
	}
	
	//If this didnt exist, most Living entities would set their targets on the first turn, causing a missive lag spike
	public virtual void newLevel(Scene sce){
		
	}
	
	public virtual int getAnimationFrame(Scene sce) => 0;
	
	public virtual bool onClick(Scene sce){
		return false;
	}
	
	public virtual Vector2 getPos(Scene sce){
		Vector2 pos;
		if(state == EntityState.moving){
			pos = (Vector2) ((Vector2d) oldPosition + (Vector2d) (position - oldPosition) * (sce.smoothTime/sce.smoothTimeMax));
		}else{
			pos = (Vector2) position;
		}
		return pos;
	}
	
	protected virtual Matrix4 getModel(Scene sce){
		Vector2 pos = getPos(sce); 
		
		if(facingLeft){
			return leftRotation * Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		}else{
			return Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		}
	}
	
	public virtual float getAlpha() => 1f;
	
	public virtual bool getHurt() => false;
	
	public virtual void draw(Scene sce){
		Matrix4 model = getModel(sce);
		
		entityShader.use();
		entityShader.setMatrix4("model", model);
		entityShader.setVector2i("sprite", new Vector2i(getAnimationFrame(sce), atlasColumn));
		entityShader.setBool("isHurt", getHurt());
		entityShader.setFloat("alpha", getAlpha());
		
		entityTexture.bind();
		
		entityMesh.draw();
	}
	
	protected static double getRandom(Scene sce, double min, double max){
		return sce.rand.NextDouble() * (max - min) + min;
	}
}

enum EntityState : byte{
	idle = 0, moving = 1, action = 2
}