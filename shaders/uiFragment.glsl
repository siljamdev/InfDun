#version 330 core

in vec2 TexCoord;

out vec4 FragColor;

uniform vec4 col;
uniform sampler2D tex;

void main()
{
	vec4 c = texture(tex, TexCoord);
	if(c.a < 0.05){
		discard;
	}
	vec4 rgb = col * c;
	FragColor = rgb;
}