#version 330 core

in vec2 TexCoord;

out vec4 FragColor;

uniform sampler2D tex;

const vec2 atlasSize = vec2(3, 1);

float hash(in vec2 p) {
    float h = fract(sin(dot(p, vec2(12.9898,78.233))) * 43758.5453123);
    return h;
}

void main()
{
	vec2 loc = floor(TexCoord);
	
	float n = hash(loc);
	vec2 spritePos = vec2(n < 0.87 ? 0 : n < 0.93 ? 1 : 2, 0);
	vec2 uv = (fract(TexCoord) + spritePos) / atlasSize;
	
	//vec4 c = hashBool(loc) ? texture(tex2, TexCoord) : texture(tex, TexCoord);
	vec4 c = textureLod(tex, uv, 0.0); //No idea why this works

	FragColor = c;
}