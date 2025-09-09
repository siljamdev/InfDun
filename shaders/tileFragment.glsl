#version 330 core

out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D tex;

void main()
{
	vec4 c = texture(tex, TexCoord);
	if(c.a < 0.05){
		discard;
	}

	FragColor = c;
} 