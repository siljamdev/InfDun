#version 330 core

in vec2 TexCoord;

out vec4 FragColor;

uniform sampler2D tex;
uniform bool isHurt;
uniform float alpha;

void main()
{
	
	vec4 c = (isHurt ? vec4(1.0, 0.0, 0.0, alpha) : vec4(1.0, 1.0, 1.0, alpha)) * texture(tex, TexCoord);
	if(c.a < 0.05){
		discard;
	}

	FragColor = c;
}