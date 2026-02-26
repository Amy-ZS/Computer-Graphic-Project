using Godot;
using System;
using System.Runtime.InteropServices;

public partial class Main : Node3D
{
    private RenderingDevice rendering_device;
    private Rid shader;
    private Rid pipeline;

    [Export] public int count = 5000;
    [Export] public Vector3 bounds = new(5, 5, 5);

    [Export] public float radius = 1f;
    [Export] public float target_density = 2f;
    [Export] public float pressure = 20f;
    [Export] public float viscosity = .8f;
    [Export] public MultiMeshInstance3D mesh;

    private Rid pos_buffer;
    private Rid vel_buffer;
    private Rid den_buffer;
    
    private Rid uniform_set;

    public override void _Ready() {
        rendering_device = RenderingServer.CreateLocalRenderingDevice();

        RDShaderFile shaderFile = GD.Load<RDShaderFile>("res://scripts/compute_shader.glsl");
        shader = rendering_device.ShaderCreateFromSpirV(shaderFile.GetSpirV());
        pipeline = rendering_device.ComputePipelineCreate(shader);

        float[] starting_positions = new float[count * 4];
        float spacing = 2;

        for (int i = 0; i < count; i++) {
            starting_positions[i*4] = (float)GD.RandRange(-spacing, spacing);
            starting_positions[i*4 + 1] = (float)GD.RandRange(-spacing, spacing);
            starting_positions[i*4 + 2] = (float)GD.RandRange(-spacing, spacing);
            starting_positions[i*4 + 3] = 0.0f;
        }

        byte[] starting_positions_bytes = MemoryMarshal.AsBytes(starting_positions.AsSpan()).ToArray();
    
        pos_buffer = rendering_device.StorageBufferCreate((uint)count*16, starting_positions_bytes); // 4 floats
        vel_buffer = rendering_device.StorageBufferCreate((uint)(count*16)); // 4 floats
        den_buffer = rendering_device.StorageBufferCreate((uint)(count*4)); // 1 float  

        RDUniform bind0 = new RDUniform { Binding = 0, UniformType = RenderingDevice.UniformType.StorageBuffer }; bind0.AddId(pos_buffer);
        RDUniform bind1 = new RDUniform { Binding = 1, UniformType = RenderingDevice.UniformType.StorageBuffer }; bind1.AddId(vel_buffer);
        RDUniform bind2 = new RDUniform { Binding = 2, UniformType = RenderingDevice.UniformType.StorageBuffer }; bind2.AddId(den_buffer);
        uniform_set = rendering_device.UniformSetCreate(new Godot.Collections.Array<RDUniform> { bind0, bind1, bind2 }, shader, 0);

        mesh.Multimesh.InstanceCount = count;
    }

    public override void _Process(double delta) {
        RunCompute((float)delta);

        byte[] pos_bytes = rendering_device.BufferGetData(pos_buffer);
        ReadOnlySpan<float> pos_data = MemoryMarshal.Cast<byte, float>(pos_bytes);

        for (int i = 0; i < count; i++) {
            Vector3 position = new Vector3(pos_data[i*4], pos_data[i*4+1], pos_data[i*4+2]);
            mesh.Multimesh.SetInstanceTransform(i, new Transform3D(Basis.Identity, position));
        }
    }

    private void RunCompute(float dt)
    {
        float[] vals = { //total 48
            radius, //4
            count, //4
            dt, //4, change this to 1/30f if u wanna normalize it
            target_density, //4 
            pressure, //4
            viscosity, //4
            0f, //4
            0f, //4
            bounds.X, //4
            bounds.Y, //4
            bounds.Z, //4
            0f //4
        };

        //turn our info into bytes
        byte[] bytes = MemoryMarshal.AsBytes(vals.AsSpan()).ToArray();

        long list = rendering_device.ComputeListBegin(); //begin gpu instruction
        rendering_device.ComputeListBindComputePipeline(list, pipeline); //give it our shader

        rendering_device.ComputeListBindUniformSet(list, uniform_set, 0); //tells the program where to put our info
        
        rendering_device.ComputeListSetPushConstant(list, bytes, (uint)bytes.Length);
        rendering_device.ComputeListDispatch(list, (uint)Mathf.Ceil(count / 64.0f), 1, 1); //separates the processing into groups of 64
        //we are also doing this in a 1d array

        rendering_device.ComputeListEnd();
        rendering_device.Submit();
        rendering_device.Sync();
    }
}