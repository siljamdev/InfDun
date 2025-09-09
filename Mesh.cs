using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

public class Mesh : IDisposable{
	
	static int boundVBO;
	static int boundVAO;
	
	bool isDynamic;
	bool usesIndex;
	
	public int? VBO {get; private set;}
	public int VAO {get; private set;}
	
	int floatsPerVertex; //per vertex, floats per vertex
	int numberOfVertices;
	
	PrimitiveType drawType;
	
	//Dynamic mesh
	public Mesh(string components, int maxElements, PrimitiveType d){
		isDynamic = true;
		usesIndex = false;
		
		drawType = d;
		
		floatsPerVertex = 0;
		for(int i = 0; i < components.Length; i++){
			if(int.TryParse(new string(components[i], 1), out int j)){
				floatsPerVertex += j;
			}
		}
		
		//initialize vbo
		VBO = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, (int) VBO);
		GL.BufferData(BufferTarget.ArrayBuffer, maxElements * floatsPerVertex * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
		
		//Vao
		VAO = GL.GenVertexArray(); //Initialize VAO
		GL.BindVertexArray(VAO); //Bind VAO
		
		int sum = 0;
		for(int i = 0; i < components.Length; i++){
			if(int.TryParse(new string(components[i], 1), out int j)){
				GL.VertexAttribPointer(i, j, VertexAttribPointerType.Float, false, floatsPerVertex * sizeof(float), sum * sizeof(float)); //Set parameters so it knows how to process it. 
				GL.EnableVertexAttribArray(i); //It is in layout i
				sum += j;
			}
		}
		
		//unbind
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0); //Unbind VBO
		GL.BindVertexArray(0); //Unbind VAO
		
		boundVBO = 0;
		boundVAO = 0;
	}
	
	//Static mesh
	public Mesh(string components, float[] vertices, PrimitiveType d){
		isDynamic = false;
		usesIndex = false;
		
		drawType = d;
		
		floatsPerVertex = 0;
		for(int i = 0; i < components.Length; i++){
			if(int.TryParse(new string(components[i], 1), out int j)){
				floatsPerVertex += j;
			}
		}
		
		numberOfVertices = vertices.Length / floatsPerVertex;
		
		//initialize vbo
		VBO = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, (int) VBO);
		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
		
		//Vao
		VAO = GL.GenVertexArray(); //Initialize VAO
		GL.BindVertexArray(VAO); //Bind VAO
		
		int sum = 0;
		for(int i = 0; i < components.Length; i++){
			if(int.TryParse(new string(components[i], 1), out int j)){
				GL.VertexAttribPointer(i, j, VertexAttribPointerType.Float, false, floatsPerVertex * sizeof(float), sum * sizeof(float)); //Set parameters so it knows how to process it. 
				GL.EnableVertexAttribArray(i); //It is in layout i
				sum += j;
			}
		}
		
		//unbind
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0); //Unbind VBO
		GL.BindVertexArray(0); //Unbind VAO
		GL.DeleteBuffer((int) VBO); //Delete VBO, we wont even need it anymore. If we delete before unbinding the VAO, it will unbind or something idk just dont do it
		VBO = null;
		
		boundVBO = 0;
		boundVAO = 0;
	}
	
	//Static mesh with indices
	public Mesh(string components, float[] vertices, uint[] indices, PrimitiveType d){
		isDynamic = false;
		usesIndex = true;
		
		drawType = d;
		
		floatsPerVertex = 0;
		for(int i = 0; i < components.Length; i++){
			if(int.TryParse(new string(components[i], 1), out int j)){
				floatsPerVertex += j;
			}
		}
		
		numberOfVertices = indices.Length;
		
		//initialize vbo
		VBO = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, (int) VBO);
		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
		
		//initialize ebo
		int EBO = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, (int) EBO);
		GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
		
		//Vao
		VAO = GL.GenVertexArray(); //Initialize VAO
		GL.BindVertexArray(VAO); //Bind VAO
		
		int sum = 0;
		for(int i = 0; i < components.Length; i++){
			if(int.TryParse(new string(components[i], 1), out int j)){
				GL.VertexAttribPointer(i, j, VertexAttribPointerType.Float, false, floatsPerVertex * sizeof(float), sum * sizeof(float)); //Set parameters so it knows how to process it. 
				GL.EnableVertexAttribArray(i); //It is in layout i
				sum += j;
			}
		}
		
		//unbind
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0); //Unbind VBO
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0); //Unbind EBO
		GL.BindVertexArray(0); //Unbind VAO
		GL.DeleteBuffer((int) VBO); //Delete VBO, we wont even need it anymore. If we delete before unbinding the VAO, it will unbind or something idk just dont do it
		VBO = null;
		GL.DeleteBuffer(EBO); //Delete EBO, we wont need it anymore
		
		boundVBO = 0;
		boundVAO = 0;
	}
	
	public void bindVBO(){
		if(!isDynamic){
			return;
		}
		if(boundVBO == VBO){
			return;
		}
		GL.BindBuffer(BufferTarget.ArrayBuffer, (int) VBO);
		boundVBO = (int) VBO;
	}
	
	public static void unbindVBO(){
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		boundVBO = 0;
	}
	
	public void addDynamicData(float[] v){
		if(!isDynamic){
			return;
		}
		bindVBO();
		GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, v.Length * sizeof(float), v); //update data
		numberOfVertices = v.Length / floatsPerVertex;
	}
	
	public void bind(){
		if(boundVAO == VAO){
			return;
		}
		GL.BindVertexArray(VAO);
		boundVAO = VAO;
	}
	
	public void unbind(){
		GL.BindVertexArray(0);
		boundVAO = 0;
	}
	
	public void draw(){
		bind();
		if(usesIndex){
			GL.DrawElements(drawType, numberOfVertices, DrawElementsType.UnsignedInt, 0);
		}else{
			GL.DrawArrays(drawType, 0, numberOfVertices);
		}
	}
	
	public void drawInstanced(int numberOfInstances){
		bind();
		if(usesIndex){
			GL.DrawElementsInstanced(drawType, numberOfVertices, DrawElementsType.UnsignedInt, IntPtr.Zero, numberOfInstances);
		}else{
			GL.DrawArraysInstanced(drawType, 0, numberOfVertices, numberOfInstances);
		}
	}
	
	public void Dispose(){
		Dungeon.meshesMarkedForDisposal.Add((VAO, VBO));
		if(boundVAO == VAO){
			unbind();
			boundVAO = 0;
		}
		
		VAO = 0;
		
		if(VBO != null){
			if(boundVBO == VBO){
				unbindVBO();
				boundVBO = 0;
			}
			
			VBO = null;
		}
		
		GC.SuppressFinalize(this);
	}
	
	~Mesh(){
		Dispose();
	}
}