using System;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using AshLib;

class AABB2D{
	static Shader boxShader;
	static Mesh boxMesh;
	
	public static void initialize(){
		boxShader = Shader.fromAssembly("shaders.AABB2D");
		boxShader.setVector3("color", Color3.Red);
		
		float[] vertices = {
			0.5f, -0.5f,
			-0.5f, -0.5f,
			//0.5f, 0.5f,
			-0.5f, 0.5f,
			0.5f, 0.5f,
			0.5f, -0.5f,
			//-0.5f, -0.5f,
		};
		
		boxMesh = new Mesh("2", vertices, PrimitiveType.LineStrip, "box");
	}
	
	public float up{get; private set;}
	public float down{get; private set;}
	public float left{get; private set;}
	public float right{get; private set;}
	
	public AABB2D(Vector2 a, Vector2 b){
		if(a.X > b.X){
			left = b.X;
			right = a.X;
		}else{
			left = a.X;
			right = b.X;
		}
		
		if(a.Y > b.Y){
			down = b.Y;
			up = a.Y;
		}else{
			down = a.Y;
			up = b.Y;
		}
	}
	
	public AABB2D(float u, float d, float l, float r){
		up = u;
		down = d;
		left = l;
		right = r;
	}
	
	public void expand(float r){
		up += r;
		down -= r;
		left -= r;
		right += r;
	}
	
	public static bool contained(Vector2 a, AABB2D b){ //a is contained in b
		return b.left < a.X && b.right > a.X && b.down < a.Y && b.up > a.Y;
	}
	
	public static bool contained(AABB2D a, AABB2D b){ //a is contained in b
		return b.left < a.left && b.right > a.right && b.down < a.down && b.up > a.up;
	}
	
	public static bool collide(AABB2D a, AABB2D b){
		return a.left <= b.right && a.right >= b.left && a.down <= b.up && a.up >= b.down;
	}
	
	public static bool collide(AABB2D a, Vector2 b){
		return a.left <= b.X && a.right >= b.X && a.down <= b.Y && a.up >= b.Y;
	}
	
	public static bool collide(Vector2 a, AABB2D b){
		return collide(b, a);
	}
	
	public static AABB2D union(params AABB2D[] boxes){
		if(boxes.Length == 0){
			return null;
		}
		float newLeft = boxes[0].left;
		float newRight = boxes[0].right;
		float newDown = boxes[0].down;
		float newUp = boxes[0].up;
		
		foreach (AABB2D other in boxes){
			newLeft = MathF.Min(newLeft, other.left);
			newRight = MathF.Max(newRight, other.right);
			newDown = MathF.Min(newDown, other.down);
			newUp = MathF.Max(newUp, other.up);
		}
		
		return new AABB2D(newUp, newDown, newLeft, newRight);
	}
	
	public bool collide(AABB2D b){
		return collide(this, b);
	}
	
	public bool collide(Vector2 b){
		return collide(this, b);
	}
	
	public static AABB2D operator +(AABB2D a, AABB2D b){
		return union(a, b);
	}
	
	public override string ToString(){
		return "AABB2D(Left: " + left + ", Right: " + right + ", Down: " + down + ", Up: " + up + ")";
	}
	
	public void draw(Renderer ren){
		float w = right - left;
		float h = up - down;
		
		Matrix4 model = Matrix4.CreateScale(new Vector3(w, h, 0f)) * Matrix4.CreateTranslation(new Vector3(left + w / 2f, down + h / 2f, 0f));
		
		boxShader.use();
		boxShader.setMatrix4("projection", ren.projection);
		boxShader.setMatrix4("view", ren.view);
		boxShader.setMatrix4("model", model);
		
		boxMesh.draw();
	}
}