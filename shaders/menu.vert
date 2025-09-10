#version 330 core

layout (location = 0) in vec2 aPos;

out vec2 TexCoord;

uniform float iTime;
uniform vec2 iResolution;

void main()
{
	vec2 t;
	t = aPos;
	t.x *= iResolution.x/iResolution.y;
	t *= 2.;
	t -= vec2(-iTime*0.12, iTime*0.09);
	t.y = 1.0 - t.y;
	
	t += vec2(10., 2.);
	
	TexCoord = t;
	
	gl_Position = vec4(aPos, 0.0, 1.0); //The position
}