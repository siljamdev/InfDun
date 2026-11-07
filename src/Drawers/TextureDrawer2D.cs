using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class TextureDrawer2D{	
	Mesh uiMesh;
	static Shader texShader;
	
	public readonly Dictionary<string, Texture2D> textureBook = new();
	
	public TextureDrawer2D(Mesh um){
		uiMesh = um;
		
		texShader ??= Shader.fromAssembly("shaders.tex");
	}
	
	public void draw(Renderer ren, Texture2D t, Vector2 pos, Vector2 sca, Color3 col, float alpha = 1f){
		Matrix4 model = Matrix4.CreateScale(new Vector3(sca.X, sca.Y, 0f)) * Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		
		texShader.use();
		texShader.setMatrix4("model", model);
		texShader.setMatrix4("view", ren.view);
		texShader.setMatrix4("projection", ren.projection);
		texShader.setVector4("col", col, alpha);
		
		t.bind();
		
		uiMesh.draw();
	}
	
	public void draw(Renderer ren, Texture2D t, Matrix4 model, Color3 col, float alpha = 1f){
		texShader.use();
		texShader.setMatrix4("model", model);
		texShader.setMatrix4("view", ren.view);
		texShader.setMatrix4("projection", ren.projection);
		texShader.setVector4("col", col, alpha);
		
		t.bind();
		
		uiMesh.draw();
	}
	
	public void draw(Renderer ren, Texture2D t, float xpos, float ypos, float xsca, float ysca, Color3 col, float alpha = 1f){
		draw(ren, t, new Vector2(xpos, ypos), new Vector2(xsca, ysca), col, alpha);
	}
	
	public void draw(Renderer ren, Texture2D t, float xpos, float ypos, float sca, Color3 col, float alpha = 1f){
		draw(ren, t, new Vector2(xpos, ypos), new Vector2(sca, sca), col, alpha);
	}
	
	public void draw(Renderer ren, string n, Vector2 pos, Vector2 sca, Color3 col, float alpha = 1f){
		if(textureBook.TryGetValue(n, out Texture2D t)){
			draw(ren, t, pos, sca, col, alpha);
		}else{
			throw new KeyNotFoundException("Missing texture on texture book: '" + n + "'");
		}
	}
	
	public void draw(Renderer ren, string n, Matrix4 model, Color3 col, float alpha = 1f){
		if(textureBook.TryGetValue(n, out Texture2D t)){
			draw(ren, t, model, col, alpha);
		}else{
			throw new KeyNotFoundException("Missing texture on texture book: '" + n + "'");
		}
	}
	
	public void draw(Renderer ren, string n, float xpos, float ypos, float xsca, float ysca, Color3 col, float alpha = 1f){
		draw(ren, n, new Vector2(xpos, ypos), new Vector2(xsca, ysca), col, alpha);
	}
	
	public void draw(Renderer ren, string n, float xpos, float ypos, float sca, Color3 col, float alpha = 1f){
		draw(ren, n, new Vector2(xpos, ypos), new Vector2(sca, sca), col, alpha);
	}
}