#version 330 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aScale;
layout (location = 2) in float aRotation;
layout (location = 3) in vec4 aCol;

out vec4 color;
out vec2 scale;
out float rotation;

uniform mat4 view;
uniform float zoom;

void main()
{
	gl_Position = view * vec4(aPos, 0.0, 1.0); //The position
	
	color = aCol;
	rotation = aRotation;
	scale = zoom * aScale;
}