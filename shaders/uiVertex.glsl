#version 330 core

layout (location = 0) in vec2 aPos;

out vec2 TexCoord;

uniform mat4 model;
uniform mat4 projection;

void main()
{
	TexCoord = vec2(aPos.x, 1.0 - aPos.y);
	gl_Position = projection * model * vec4(aPos, 0.0, 1.0); //The position
}