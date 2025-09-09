#version 330 core
layout (location = 0) in vec2 aPos;

out vec2 TexCoord; 

uniform vec2 position; //Position in the screen
uniform vec2 size; //Size
uniform mat4 projection;

uniform ivec2 glyphStructure; //Rows and columns of the font texture

uniform int letters[256];

void main()
{
	vec2 inGlyphCoords = vec2(aPos.x, - aPos.y); //Sum in y because vertices are from -1 to 0. Ranges from 0 to 1
	
	//Get the index in x and y instead of linear. Also the y component is swapped because in opengl textures start in the bottom left corner instead of the top left one
	vec2 letterIndex = vec2(float(letters[gl_InstanceID] % glyphStructure.x), float(letters[gl_InstanceID] / glyphStructure.x));
	
	TexCoord = (letterIndex + inGlyphCoords) / glyphStructure; //Output the texture coords, divided by the rows and columns so it renders properly
	
	//Multiply the vertex by size to scale it and sum the position. z in -1 because if it was 0 then sometimes it failed the depth test with some objects of the scene
	vec4 p = vec4((aPos * size) + position, -1.0, 1.0);
	
	p.x += size.x * gl_InstanceID; //Because text renders intanced, move it to the side
	p = projection * p;
	
	gl_Position = p;
}