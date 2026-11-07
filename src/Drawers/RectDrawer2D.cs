using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class RectDrawer2D{
	Mesh uiMesh;
	static Shader rectShader;
	
	public RectDrawer2D(Mesh um){
		uiMesh = um;
		
		rectShader ??= Shader.fromAssembly("shaders.rect");
	}
	
	//Solid rect
	public void draw(Renderer ren, Vector2 pos, Vector2 sca, Color3 col, float alpha = 1f){
		Matrix4 model = Matrix4.CreateScale(new Vector3(sca.X, sca.Y, 0f)) * Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		
		rectShader.use();
		rectShader.setMatrix4("model", model);
		rectShader.setMatrix4("view", ren.view);
		rectShader.setMatrix4("projection", ren.projection);
		rectShader.setVector4("col", col, alpha);
		
		uiMesh.draw();
	}
	
	public void draw(Renderer ren, float xp, float xy, float sx, float sy, Color3 c, float alpha = 1f){
		draw(ren, new Vector2(xp, xy), new Vector2(sx, sy), c, alpha);
	}
}