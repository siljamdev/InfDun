#version 330 core

in vec2 TexCoord;

out vec4 FragColor;

uniform sampler2D tex;
uniform sampler2D tex2;

bool hashBool(in vec2 p) {
    float h = fract(sin(dot(p, vec2(12.9898,78.233))) * 43758.5453123);
    return h > 0.91;
}

void main()
{
	vec2 loc = floor(TexCoord);
	
	vec4 c = hashBool(loc) ? texture(tex2, TexCoord) : texture(tex, TexCoord);

	FragColor = c;
}