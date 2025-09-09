#version 330 core

layout (location = 0) in vec2 aPos;

out vec2 TexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform ivec2 sprite;

uniform vec2 inverseAltasSize;

void main()
{
	TexCoord = vec2(aPos.x + 0.5, 1.0 - aPos.y);
	TexCoord *= inverseAltasSize;
	TexCoord += vec2(sprite) * inverseAltasSize;
	gl_Position = projection * view * model * vec4(aPos, 0.0, 1.0); //The position
}