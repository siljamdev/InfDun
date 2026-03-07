#version 330 core

layout (location = 0) in vec2 aPos;

out vec2 TexCoord;

uniform mat4 model;
uniform mat4 projection;

void main()
{
	TexCoord = vec2(aPos.x, -aPos.y); //Reverse in y because vertices are from -1 to 0
	gl_Position = projection * model * vec4(aPos, -1.0, 1.0); //The position
}