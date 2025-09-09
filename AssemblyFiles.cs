using System;
using System.Reflection;

static class AssemblyFiles{
	static readonly Assembly assembly = Assembly.GetExecutingAssembly();
	
	static readonly string[] resourceNames = assembly.GetManifestResourceNames();
	
	const string assemblyName = "InfDun";
	
	public static byte[] getBytes(string name){
		byte[] b;
		
		string resourceName = resourceNames.FirstOrDefault(str => str == assemblyName + "." + name);
		
		if(resourceName == null){
			throw new Exception("No resource found called " + name);
		}
		
		using (Stream stream = assembly.GetManifestResourceStream(resourceName))
		using (MemoryStream memoryStream = new MemoryStream()){
			stream.CopyTo(memoryStream);  // Copy the stream to memory
			b = memoryStream.ToArray();
		}
		
		return b;
	}
	
	public static Stream getStream(string name){
		string resourceName = resourceNames.FirstOrDefault(str => str == assemblyName + "." + name);
		
		if(resourceName == null){
			throw new Exception("No resource found called " + name);
		}
		
		return assembly.GetManifestResourceStream(resourceName);
	}
	
	public static string getText(string name){
		string t;
		
		string resourceName = resourceNames.FirstOrDefault(str => str == assemblyName + "." + name);
		
		if(resourceName == null){
			throw new Exception("No resource found called " + name);
		}
		
		using (Stream stream = assembly.GetManifestResourceStream(resourceName))
		using (StreamReader reader = new StreamReader(stream)){
			t = reader.ReadToEnd();
		}
		
		return t;
	}
	
	public static bool exists(string name){
		return resourceNames.Any(str => str == assemblyName + "." + name);
	}
	
	public static string[] getAll(){
		return resourceNames.Where(n => n.StartsWith(assemblyName + ".")).Select(n => n.Substring(assemblyName.Length + 1)).ToArray();
	}
}