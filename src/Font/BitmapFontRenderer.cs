using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class BitmapFontRenderer : FontRenderer{
	const int maxChars = 256;
	
	const string defaultMap = "ABCDEFGHIJKLMNOPQRSTUVWXYZ .,:+-*/\\'\"$()[]^?!%~º1234567890 |□#<>abcdefghijklmnopqrstuvwxyz ;&@`_{}Ññ=€¿¡";
	
	char[] map;
	string mapRaw;
	
	int unfoundSymbolPos;
	
	int glyphRows; //Rows of the font texture
	int glyphColumns; //columns of the font texture
	
	public BitmapFontRenderer(Mesh m, Texture2D t, int row, int col, string r = defaultMap)
		:base(m, t){
		glyphRows = row;
		glyphColumns = col;
		mapRaw = r;
		generateMap();
		
		fontShader = Shader.fromAssembly("shaders.bitmapFont");
		
		fontShader.setVector2i("glyphStructure", new Vector2i(this.glyphRows, this.glyphColumns)); //Pass the number of rows and cols
	}
	
	void generateMap(){
		this.map = mapRaw.ToCharArray();
		for(int i = 0; i < this.map.Length; i++){
			if(this.map[i] == '□'){
				this.unfoundSymbolPos = i;
				break;
			}
		}
	}
	
	public int[] textToMap(string text){
		text = text.Length <= maxChars ? text : text.Substring(0, maxChars);
		int[] l = new int[text.Length];
		char[] c = text.ToCharArray();
		
		for(int i = 0; i < c.Length; i++){
			for(int j = 0; j < this.map.Length; j++){
				if(c[i] == this.map[j]){
					l[i] = j;
					break;
				}
				if(j == this.map.Length - 1){
					l[i] = this.unfoundSymbolPos;
				}
			}
		}
		return l;
	}
	
	public override float getXadvance(char c, Vector2 sca){
		return sca.Y;
	}
	
	public override float getXsize(string text, Vector2 sca){
		return Math.Min(text.Length, maxChars) * sca.X;
	}
	
	public override void drawText(string text, Vector2 pos, Vector2 sca, Placement p, Color3 col, float alpha = 1f){
		int[] l = textToMap(text);
		
		float totalSize = l.Length * sca.X;
		
		pos = getPos(pos, sca.Y, totalSize, p);
		
		fontShader.use();
		fontShader.setIntArray("letters[0]", l); //Set the letters so it knows wich glyphs to actually choose
		fontShader.setVector2("position", pos); //Starting position
		fontShader.setVector2("size", sca); //Size will be static no matter the size of the window
		fontShader.setVector4("col", col, alpha); //Pass the color
		
		font.bind();
		
		fontMesh.drawInstanced(l.Length);
	}
}