#version 330 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 tCoord;

out vec2 TexCoord;

uniform mat4 view;
uniform mat4 projection;

void main()
{
	gl_Position = projection * view * vec4(aPos, 0.0, 1.0); //The position
	
	TexCoord = tCoord;
}