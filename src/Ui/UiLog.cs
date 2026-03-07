using System;
using System.Text;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiLog : UiElement{
	public string[] text {get; private init;}
	
	string[][] lines;
	
	float topMargin;
	Vector2 margin; //Lateral margin
	
	float scrollOffset;
	
	public Color3 color;
	
	public UiLog(float lm, float rm, float tm, params string[] t) : base(Placement.TopLeft, lm, tm){
		text = t;
		
		margin = new Vector2(lm, rm);
		topMargin = tm;
		
		color = Renderer.textColor;
	}
	
	public UiLog setColor(Color3 c){
		color = c;
		return this;
	}
	
	public bool scroll(float f){
		f = -f;
		if(scrollOffset <= 0f && f < 0f){
			return false;
		}
		if(scrollOffset > size.Y && f > 0f){
			return false;
		}
		
		scrollOffset += f * 10f;
		return true;
	}
	
	public override void draw(Renderer ren, Vector2d m){
		float d = pos.Y + scrollOffset; //Y pos
		
		float maxXsize = ren.width - margin.X - margin.Y;
		
		for(int i = 0; i < lines.Length; i++){
			for(int j = 0; j < lines[i].Length; j++){
				ren.fr.drawText(lines[i][j], pos.X, d, Renderer.textSize, color);
				d -= Renderer.textSize.Y;
			}
			d -= Renderer.fieldSeparation - Renderer.textSize.Y;
		}
	}
	
	protected override Vector2 updateSize(Renderer ren){
		lines = new string[text.Length][];
		
		scrollOffset = 0f;
		float ySize = 0f;
		
		float maxXsize = ren.width - margin.X - margin.Y;
		
		for(int i = 0; i < text.Length; i++){
			string[] l = divideLines(ren, text[i], maxXsize);
			lines[i] = l;
			ySize += Renderer.textSize.Y * l.Length;
			ySize += Renderer.fieldSeparation - Renderer.textSize.Y;
		}
		
		return new Vector2(maxXsize, ySize);
	}
	
	string[] divideLines(Renderer ren, string input, float maxXsize){
		List<string> lines = new List<string>();
		string[] words = input.Split(' ');
		StringBuilder currentLine = new();
		float currentLineXsize = 0f;
		
		float spaceXsize = ren.fr.getXadvance(' ', Renderer.textSize);
		
		foreach(string word in words){
			float wordXsize = ren.fr.getXsize(word, Renderer.textSize);
			
			float extra = currentLine.Length > 0 ? spaceXsize : 0f; //For the space
			
			if(wordXsize > maxXsize){
				if(currentLine.Length > 0){
					lines.Add(currentLine.ToString());
					currentLine.Clear();
					currentLineXsize = 0f;
				}
				
				for(int i = 0; i < word.Length; i++){
					float charXsize = ren.fr.getXadvance(word[i], Renderer.textSize);
					
					if(currentLineXsize != 0f && currentLineXsize + charXsize > maxXsize){
						lines.Add(currentLine.ToString());
						currentLine.Clear();
						currentLineXsize = charXsize;
						
						currentLine.Append(word[i]);
					}else{
						currentLine.Append(word[i]);
						currentLineXsize += charXsize;
					}
				}
			}else if(currentLineXsize + wordXsize + extra <= maxXsize){
				if(currentLine.Length > 0){
					currentLine.Append(" ");
					currentLineXsize += spaceXsize;
				}
				
				currentLine.Append(word);
				currentLineXsize += wordXsize;
			}else{
				//Start a new line if the word doesn't fit
				if(currentLine.Length > 0){
					lines.Add(currentLine.ToString());
					currentLine.Clear();
					currentLineXsize = 0f;
				}
				
				currentLine.Append(word);
				currentLineXsize += wordXsize;
			}
		}
		
		//Add the last line if not empty
		if(currentLine.Length > 0){
			lines.Add(currentLine.ToString());
		}
		
		return lines.ToArray();
	}
}