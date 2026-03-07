#version 330 core
out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D fontTexture;
uniform vec4 col;

void main()
{	
	vec4 c = texture(fontTexture, TexCoord);
	if(c.a < 0.05){
		discard;
	}
	vec4 rgba = col * c;
	FragColor = rgba;
} 