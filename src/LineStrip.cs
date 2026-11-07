using System;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using AshLib;

//The only purpose of this is to display debug info, feel free to delete it
class LineStrip{
	static Shader lineShader;
	
	public static void initialize(){
		lineShader = Shader.fromAssembly("shaders.AABB2D"); //Same as AABB2Ds
		lineShader.setVector3("color", Color3.Green);
		
		lineShader.setMatrix4("model", Matrix4.Identity); //Meshes are generated with world positions, so its not necessary
	}
	
	Mesh lineMesh;
	
	public LineStrip(params Vector2[] pts){
		float[] vertices = pts.SelectMany(n => new[]{n.X, n.Y}).ToArray();
		
		lineMesh = new Mesh("2", vertices, PrimitiveType.LineStrip, "linestrip");
	}
	
	public override string ToString(){
		return "LineStrip";
	}
	
	public void draw(Renderer ren){
		lineShader.use();
		lineShader.setMatrix4("projection", ren.projection);
		lineShader.setMatrix4("view", ren.view);
		
		lineMesh.draw();
	}
}