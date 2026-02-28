using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

public class Mesh : IDisposable{
	
	static int boundVBO;
	static int boundVAO;
	
	bool isDynamic;
	
	public int? VBO {get; private set;}
	public int? EBO {get; private set;}
	
	public int VAO {get; private set;}
	
	int floatsPerVertex; //per vertex, floats per vertex
	int numberOfVertices;
	
	PrimitiveType drawType;
	
	//Dynamic mesh
	public Mesh(string components, int maxElements, PrimitiveType d, string name = null){
		isDynamic = true;
		
		drawType = d;
		
		floatsPerVertex = 0;
		for(int i = 0; i < components.Length; i++){
			if(int.TryParse(new string(components[i], 1), out int j)){
				floatsPerVertex += j;
			}
		}
		
		//initialize vbo
		VBO = GL.GenBuffer();
		
		#if DEBUG
			GL.ObjectLabel(
				ObjectLabelIdentifier.Buffer,
				(int) VBO,
				-1,
				name + " <VBO>"
			);
		#endif
		
		GL.BindBuffer(BufferTarget.ArrayBuffer, (int) VBO);
		GL.BufferData(BufferTarget.ArrayBuffer, maxElements * floatsPerVertex * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
		
		//Vao
		VAO = GL.GenVertexArray(); //Initialize VAO
		
		#if DEBUG
			GL.ObjectLabel(
				ObjectLabelIdentifier.VertexArray,
				VAO,
				-1,
				name + " <VAO>"
			);
		#endif
		
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
	public Mesh(string components, float[] vertices, PrimitiveType d, string name = null){
		isDynamic = false;
		
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
		
		#if DEBUG
			GL.ObjectLabel(
				ObjectLabelIdentifier.Buffer,
				(int) VBO,
				-1,
				name + " <VBO>"
			);
		#endif
		
		GL.BindBuffer(BufferTarget.ArrayBuffer, (int) VBO);
		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
		
		//Vao
		VAO = GL.GenVertexArray(); //Initialize VAO
		
		#if DEBUG
			GL.ObjectLabel(
				ObjectLabelIdentifier.VertexArray,
				VAO,
				-1,
				name + " <VAO>"
			);
		#endif
		
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
		
		//DONT DO THIS THIS MESSED EVERYTHING UP!!!!!!!
		//GL.DeleteBuffer((int) VBO); //Delete VBO, we wont even need it anymore. If we delete before unbinding the VAO, it will unbind or something idk just dont do it
		VBO = null;
		
		boundVBO = 0;
		boundVAO = 0;
	}
	
	//Static mesh with indices
	public Mesh(string components, float[] vertices, uint[] indices, PrimitiveType d, string name = null){
		isDynamic = false;
		
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
		
		#if DEBUG
			GL.ObjectLabel(
				ObjectLabelIdentifier.Buffer,
				(int) VBO,
				-1,
				name + " <VBO>"
			);
		#endif
		
		GL.BindBuffer(BufferTarget.ArrayBuffer, (int) VBO);
		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
		
		//initialize ebo
		EBO = GL.GenBuffer();
		
		#if DEBUG
			GL.ObjectLabel(
				ObjectLabelIdentifier.Buffer,
				(int) EBO,
				-1,
				name + " <EBO>"
			);
		#endif
		
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, (int) EBO);
		GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
		
		//Vao
		VAO = GL.GenVertexArray(); //Initialize VAO
		
		#if DEBUG
			GL.ObjectLabel(
				ObjectLabelIdentifier.VertexArray,
				VAO,
				-1,
				name + " <VAO>"
			);
		#endif
		
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
		
		//DONT DO THIS THIS MESSED EVERYTHING UP!!!!!!!
		//GL.DeleteBuffer((int) VBO); //Delete VBO, we wont even need it anymore. If we delete before unbinding the VAO, it will unbind or something idk just dont do it
		VBO = null;
		
		//DONT DO THIS THIS MESSED EVERYTHING UP!!!!!!!
		//GL.DeleteBuffer(EBO); //Delete EBO, we wont need it anymore
		
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
		if(EBO != null){
			GL.DrawElements(drawType, numberOfVertices, DrawElementsType.UnsignedInt, 0);
		}else{
			GL.DrawArrays(drawType, 0, numberOfVertices);
		}
	}
	
	public void drawInstanced(int numberOfInstances){
		bind();
		if(EBO != null){
			GL.DrawElementsInstanced(drawType, numberOfVertices, DrawElementsType.UnsignedInt, IntPtr.Zero, numberOfInstances);
		}else{
			GL.DrawArraysInstanced(drawType, 0, numberOfVertices, numberOfInstances);
		}
	}
	
	public static void cleanup(int VAO, int? VBO, int? EBO){
		GL.DeleteVertexArray(VAO);
		
		if(VBO != null){
			GL.DeleteBuffer((int) VBO);
		}
		
		if(EBO != null){
			GL.DeleteBuffer((int) EBO);
		}
	}
	
	public void Dispose(){
		Dungeon.meshesMarkedForDisposal.Add((VAO, VBO, EBO));
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