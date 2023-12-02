using Godot;
using System;
using Godot.Collections;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;

public partial class Dispatcher : Node
{
	float[] array = new float[256];
	// Called when the node enters the scene tree for the first time.
	RenderingDevice rd = RenderingServer.CreateLocalRenderingDevice();
	Rid shader;

	public override void _Ready()
	{
		var shaderFile = GD.Load<RDShaderFile>("res://Shaders/compute_shader.glsl");
		var shaderBytecode = shaderFile.GetSpirV();
		shader = rd.ShaderCreateFromSpirV(shaderBytecode);
		
		var arrayBytecode = new byte[array.Length * sizeof(float)]; // create array of bytes 
		Buffer.BlockCopy(array, 0, arrayBytecode, 0, arrayBytecode.Length); // copies data from one block to another
		Rid buffer = rd.StorageBufferCreate((uint)arrayBytecode.Length, arrayBytecode);

		// Create a uniform to assign the buffer to the rendering device
		var uniform = new RDUniform
		{
			UniformType = RenderingDevice.UniformType.StorageBuffer,
			Binding = 0
		};
		uniform.AddId(buffer);
		var uniformSet = rd.UniformSetCreate(new Array<RDUniform> { uniform }, shader, 0);
		var pipeline = rd.ComputePipelineCreate(shader);
		var computeList = rd.ComputeListBegin();
		rd.ComputeListBindComputePipeline(computeList, pipeline);
		rd.ComputeListBindUniformSet(computeList, uniformSet, 0);
		rd.ComputeListDispatch(computeList, xGroups: 5, yGroups: 1, zGroups: 1);
		rd.ComputeListEnd();
		rd.Submit();
		rd.Sync();	
	}
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
