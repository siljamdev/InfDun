using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class MainMenuRenderer : Renderer{
	Mesh menuMesh;
	
	Shader menuShader;
	
	Texture2D menu;
	
	public MainMenuRenderer(Engine eng){
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
		//menuShader.setInt("tex2", 1); //Texture unit 1
		
		//main menu textures
		menu = Texture2D.fromAssembly("res.textures.menu.png", TextureParams.Default);
		
		onResize(eng.width, eng.height);
		
		eng.onMouseMove += (x, y) => {
			menuShader?.setVector2("iMouse", new Vector2(x, y));
		};
	}
	
	protected override void onResize(int w, int h){
		menuShader?.setVector2("iResolution", new Vector2(w, h));
	}
	
	public override void draw(){
		menuShader.use();
		menuShader.setFloat("iTime", (float) Engine.instance.dh.GetTime());
		
		menu.bind();
		
		menuMesh.draw();
	}
}