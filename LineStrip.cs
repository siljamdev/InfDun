using System;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using AshLib;

//The only purpose of this is to display debug info, feel free to delete it
class LineStrip{
	static Shader lineShader;
	
	static Matrix4 view;
	
	public static void initialize(){
		lineShader = Shader.fromAssembly("shaders.AABB"); //Same as AABBs
		lineShader.setVector3("color", Color3.Green);
		
		lineShader.setMatrix4("model", Matrix4.Identity); //Meshes are generated with world positions, so its not necessary
	}
	
	public static void setProjection(Matrix4 m){
		lineShader.setMatrix4("projection", m);
	}
	
	public static void setView(object s, EventArgs a){
		view = ((Camera) s).view;
	}
	
	Mesh lineMesh;
	
	public LineStrip(params Vector2d[] pts){
		float[] vertices = pts.SelectMany(n => new[]{(float) n.X, (float) n.Y}).ToArray();
		
		lineMesh = new Mesh("2", vertices, PrimitiveType.LineStrip);
	}
	
	public override string ToString(){
		return "LineStrip";
	}
	
	public void drawWorld(){
		lineShader.use();
		lineShader.setMatrix4("view", view);
		
		lineMesh.draw();
	}
	
	public void drawAbs(){
		lineShader.use();
		lineShader.setMatrix4("view", Matrix4.Identity);
		
		lineMesh.draw();
	}
}