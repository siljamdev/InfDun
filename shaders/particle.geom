#version 330 core

layout (points) in;
layout (triangle_strip, max_vertices = 4) out;

in vec2 scale[];
in vec4 color[];
in float rotation[];

out vec4 col;

uniform mat4 projection;

//turns out traingle strips are special or smth
void main()
{
	col = color[0];
	
	vec2 sca = scale[0];
	
	float c = cos(rotation[0]);
	float s = sin(rotation[0]);
	mat2 rot = mat2(c, s, -s, c);
	
	//Upper triangle
	
	vec2 off = rot * vec2(0.0, 0.0);
	vec4 p = vec4(off, 0.0, 0.0);
	gl_Position = projection * (gl_in[0].gl_Position + p);
	EmitVertex();
	
	off = rot * vec2(sca.x, 0.0);
	p = vec4(off, 0.0, 0.0);
	gl_Position = projection * (gl_in[0].gl_Position + p);
	EmitVertex();
	
	off = rot * vec2(0.0, -sca.y);
	p = vec4(off, 0.0, 0.0);
	gl_Position = projection * (gl_in[0].gl_Position + p);
	EmitVertex();
	
	//Down triangle
	
	/*FragPos = vec2(-1.0, -1.0);
	p = vec4(-radius[0], -radius[0], 0.0, 0.0);
	gl_Position = projection * (gl_in[0].gl_Position + p);
	EmitVertex(); */
	
	off = rot * vec2(sca.x, -sca.y);
	p = vec4(off, 0.0, 0.0);
	gl_Position = projection * (gl_in[0].gl_Position + p);
	EmitVertex();
	
	/* FragPos = vec2(1.0, 1.0);
	p = vec4(radius[0], radius[0], 0.0, 0.0);
	gl_Position = projection * (gl_in[0].gl_Position + p);
	EmitVertex(); */
	
	EndPrimitive();
}