using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using StbImageSharp;

public class Texture2D : IDisposable{
	
	static int[] activeTexture = new int[8];
	
	public int id{get; private set;}
	
	public int width{get; private set;}
	public int height{get; private set;}
	
	public PixelInternalFormat internalFormat{get; private set;}
	
	int _unit;
	
	public TextureUnit unit => unitFromInt(_unit);
	
	public Texture2D(ImageResult image, TextureParams tp, int u = 0){	
		this.internalFormat = PixelInternalFormat.Rgba8;
		
		this.width = image.Width; //Extract this needed values
		this.height = image.Height;
		
		this._unit = u;
		
		this.id = GL.GenTexture(); //Generate the handle for the texture
		
		GL.ActiveTexture(unit);
		GL.BindTexture(TextureTarget.Texture2D, this.id); //bind it
		
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) tp.wrapS); //Set the wrap parameters
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) tp.wrapT);
		
		if(tp.wrapS == TextureWrapMode.ClampToBorder || tp.wrapT == TextureWrapMode.ClampToBorder){
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, new float[]{tp.borderColor.X, tp.borderColor.Y, tp.borderColor.Z}); //if needed set the border color
		}
		
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) tp.filterMin); //set upscaling/downscaling filter options
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) tp.filterMax);
		
		
		GL.TexImage2D(TextureTarget.Texture2D, 0, this.internalFormat, this.width, this.height, 0, tp.imageFormat, PixelType.UnsignedByte, image.Data); //actually generate the texture
		
		//if needed, generate mipmaps
		if(tp.filterMin == TextureMinFilter.NearestMipmapNearest || tp.filterMin == TextureMinFilter.LinearMipmapNearest || tp.filterMin == TextureMinFilter.NearestMipmapLinear || tp.filterMin == TextureMinFilter.LinearMipmapLinear){
			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
		}
    	
    	GL.BindTexture(TextureTarget.Texture2D, 0); //unbind texture
		activeTexture[_unit] = 0;
	}
	
	//Disposes of the stream
	public static Texture2D fromStream(Stream s, TextureParams tp, int unit = 0){
		ImageResult image = ImageResult.FromStream(s, ConvertFormat(tp.imageFormat));
		if (image == null || image.Data == null){
			throw new Exception("Image loading failed from stream");
		}
		Texture2D t = new Texture2D(image, tp, unit);
		return t;
	}
	
	public static Texture2D fromFile(string path, TextureParams tp){
		using FileStream fs = File.OpenRead(path);
		try{
			return fromStream(fs, tp);
		}catch(Exception e){
			throw new Exception("Image loading failed from file: " + path, e);
		}
	}
	
	public static Texture2D fromBytes(byte[] data, TextureParams tp){
		ImageResult image = ImageResult.FromMemory(data, ConvertFormat(tp.imageFormat));
		if (image == null || image.Data == null){
			throw new Exception("Image loading failed from byte array");
		}
		Texture2D t = new Texture2D(image, tp);
		return t;
	}
	
	public static Texture2D fromAssembly(string name, TextureParams tp, int unit = 0){
		using Stream s = AssemblyFiles.getStream(name);
		try{
			return fromStream(s, tp, unit);
		}catch(Exception e){
			throw new Exception("Image loading failed from assembly: " + name, e);
		}
	}
	
	static ColorComponents ConvertFormat(PixelFormat pixelFormat){
        switch (pixelFormat){
            case PixelFormat.Rgb:
                return ColorComponents.RedGreenBlue;
            case PixelFormat.Rgba:
                return ColorComponents.RedGreenBlueAlpha;
            // Add more cases for other pixel formats as needed
            default:
                throw new ArgumentException("Unsupported pixel format in conversion");
        }
    }
	
	public void bind(){
		if(activeTexture[_unit] == this.id){
			return;
		}
		GL.ActiveTexture(unit);
		GL.BindTexture(TextureTarget.Texture2D, this.id);
		activeTexture[_unit] = this.id;
	}
	
	public static void unbind(int unit = 0){
		GL.ActiveTexture(unitFromInt(unit));
		GL.BindTexture(TextureTarget.Texture2D, 0);
		activeTexture[unit] = 0;
	}
	
	private static TextureUnit unitFromInt(int u){
		switch(u){
			case 0:
			return TextureUnit.Texture0;
			
			case 1:
			return TextureUnit.Texture1;
			
			case 2:
			return TextureUnit.Texture2;
			
			case 3:
			return TextureUnit.Texture3;
			
			case 4:
			return TextureUnit.Texture4;
			
			case 5:
			return TextureUnit.Texture5;
			
			case 6:
			return TextureUnit.Texture6;
			
			case 7:
			return TextureUnit.Texture7;
			
			default:
			return TextureUnit.Texture0;
		}
	}
	
	public void Dispose(){
		if(activeTexture[_unit] == this.id){
			activeTexture[_unit] = 0;
		}
		
		Dungeon.texturesMarkedForDisposal.Add(this.id);
		GC.SuppressFinalize(this);
	}
	
	~Texture2D(){
		Dispose();
	}
}

public struct TextureParams{
	
	public static readonly TextureParams Default = new TextureParams();
	public static readonly TextureParams Smooth = new TextureParams(){filterMin = TextureMinFilter.LinearMipmapLinear, filterMax = TextureMagFilter.Linear};
	public static readonly TextureParams Noise = new TextureParams(){filterMin = TextureMinFilter.NearestMipmapLinear, filterMax = TextureMagFilter.Linear, wrapS = TextureWrapMode.MirroredRepeat, wrapT = TextureWrapMode.MirroredRepeat};
	
	public PixelFormat imageFormat = PixelFormat.Rgba;
	
	public TextureWrapMode wrapS = TextureWrapMode.Repeat;
	public TextureWrapMode wrapT = TextureWrapMode.Repeat;
	public Vector3 borderColor = new Vector3(0f, 0f, 0f);
	
	public TextureMinFilter filterMin = TextureMinFilter.NearestMipmapNearest;
	public TextureMagFilter filterMax = TextureMagFilter.Nearest;
	
	public TextureParams(){ //Constructor for setting defaults

	}
}