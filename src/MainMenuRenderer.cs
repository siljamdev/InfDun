using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class MainMenuRenderer{
	Mesh menuMesh;
	
	Shader menuShader;
	
	Texture2D back;
	Texture2D back2;
	
	public MainMenuRenderer(){		
		float[] v2 = { //Just the full screen will be
			-1f, -1f,
			-1f, 1f,
			1f, -1f,
			1f,  1f,
			1f, -1f,
			-1f, 1f
		};
		
		menuMesh = new Mesh("2", v2, PrimitiveType.Triangles, "menu");
		
		menuShader = Shader.fromAssembly("shaders.menu");
		menuShader.setInt("tex2", 1); //Texture unit 1
		
		//main menu textures
		back = Texture2D.fromAssembly("res.textures.back.png", TextureParams.Default);
		back2 = Texture2D.fromAssembly("res.textures.back2.png", TextureParams.Default, 1);
	}
	
	public void updateSize(int w, int h){
		menuShader.setVector2("iResolution", new Vector2(w, h));
	}
	
	public void draw(){
		menuShader.use();
		menuShader.setFloat("iTime", (float) Dungeon.dh.GetTime());
		
		back.bind();
		back2.bind();
		
		menuMesh.draw();
	}
}