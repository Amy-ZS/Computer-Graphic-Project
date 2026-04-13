using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public partial class FluidSim : Node3D
{
	//this is using the Smoothed Particle Hydrodynamics way of simulating fluids
	//i learned this from Sebastion Lague on yt, u guys should check it out to better understand it

	//i followed a bit of https://personal.ems.psu.edu/~fkd/courses/EGEE520/2017Deliverables/SPH_2017.pdf
	//we can base the sim off the equations there	
	[Export] public GpuParticlesCollisionSdf3D sdf_node;

	private RenderingDevice rendering_device;
	private Rid shader;
	private Rid pipeline;

	[Export] public int count = 5000;
	[Export] public int surface_particle_count = 10000;
	[Export] public Vector3 bounds = new(50, 50, 50);

	[Export] public float radius = 1f;
	[Export] public float target_density = 2f;
	[Export] public float pressure = 20f;
	[Export] public float viscosity = .8f;
	[Export] public MultiMeshInstance3D mesh;
	[Export] public MultiMeshInstance3D static_mesh;
	[Export] public MeshInstance3D marched_mesh;

	private Rid pos_buffer;
	private Rid vel_buffer;
	private Rid den_buffer;
	
	private Rid uniform_set;
	private bool last_frame_stopped;
	private int total_count;

	public override async void _Ready() {
		total_count = count + surface_particle_count;
		last_frame_stopped = true;
		rendering_device = RenderingServer.CreateLocalRenderingDevice();

		RDShaderFile shaderFile = GD.Load<RDShaderFile>("res://scripts/compute_shader.glsl");
		shader = rendering_device.ShaderCreateFromSpirV(shaderFile.GetSpirV());
		pipeline = rendering_device.ComputePipelineCreate(shader);

		Godot.Collections.Array<Image> slices = RenderingServer.Texture3DGet(sdf_node.Texture.GetRid());
		List<Vector3> surface_points = new List<Vector3>();

		//particles on surfaces
		int res = 128;
		int voidZone = 10;
		float threshold = 0.08f;
		int step = 3;

		for (int z = voidZone; z < res - voidZone; z+=step) {
			for (int y = voidZone; y < res - voidZone; y+=step) {
				for (int x = voidZone; x < res - voidZone; x+=step) {
					
					float val = slices[z].GetPixel(x, y).R;
					
					if (Mathf.Abs(val) < threshold) {
						float nx = slices[z].GetPixel(x + 1, y).R;
						float ny = slices[z].GetPixel(x, y + 1).R;
						if (Mathf.Abs(val - nx) > 0.001f || Mathf.Abs(val - ny) > 0.001f) {
							
							Vector3 uvw = new Vector3(x, y, z) / (res - 1f);
							Vector3 world = sdf_node.GlobalPosition + (uvw - Vector3.One * 0.5f) * sdf_node.Size;
							
							surface_points.Add(world);
						}
					}

					if (surface_points.Count >= surface_particle_count) break;
				}
				if (surface_points.Count >= surface_particle_count) break;
			}
			if (surface_points.Count >= surface_particle_count) break;
		}

		float[] starting_positions = new float[total_count * 4];

		float spacing = 2;
		for (int i = 0; i < count; i++) {
			starting_positions[i*4] = (float)GD.RandRange(-spacing, spacing);
			starting_positions[i*4 + 1] = (float)GD.RandRange(-spacing, spacing)+100;
			starting_positions[i*4 + 2] = (float)GD.RandRange(-spacing, spacing);
			starting_positions[i*4 + 3] = 0.0f;
		}

		for (int i = 0; i < surface_particle_count; i++) {
			Vector3 p = surface_points[i % surface_points.Count];
			int idx = (count + i) * 4;
			starting_positions[idx]     = p.X;
			starting_positions[idx + 1] = p.Y;
			starting_positions[idx + 2] = p.Z;
			starting_positions[idx + 3] = 1.0f;
		}


		byte[] starting_positions_bytes = MemoryMarshal.AsBytes(starting_positions.AsSpan()).ToArray();

		int sliceSize = slices[0].GetData().Length;
		byte[] allData = new byte[sliceSize * slices.Count];

		for (int i = 0; i < slices.Count; i++) {
			byte[] sliceBytes = slices[i].GetData();
			Buffer.BlockCopy(sliceBytes, 0, allData, i * sliceSize, sliceSize);
		}

		RDTextureFormat tf = new RDTextureFormat {
			Format = RenderingDevice.DataFormat.R16Sfloat,
			Width = 128,
			Height = 128,
			Depth = 128,
			TextureType = RenderingDevice.TextureType.Type3D,
			UsageBits = RenderingDevice.TextureUsageBits.SamplingBit | RenderingDevice.TextureUsageBits.CanUpdateBit
		};

		var dataArr = new Godot.Collections.Array<byte[]> { allData };
		Rid local_sdf_rid = rendering_device.TextureCreate(tf, new RDTextureView(), dataArr);
		RDSamplerState samplerState = new RDSamplerState();
		Rid sampler_rid = rendering_device.SamplerCreate(samplerState);
	
		pos_buffer = rendering_device.StorageBufferCreate((uint)total_count*16, starting_positions_bytes); // 4 floats
		vel_buffer = rendering_device.StorageBufferCreate((uint)(total_count*16)); // 4 floats
		den_buffer = rendering_device.StorageBufferCreate((uint)(total_count*4)); // 1 float

		RDUniform bind0 = new RDUniform { Binding = 0, UniformType = RenderingDevice.UniformType.StorageBuffer }; bind0.AddId(pos_buffer);
		RDUniform bind1 = new RDUniform { Binding = 1, UniformType = RenderingDevice.UniformType.StorageBuffer }; bind1.AddId(vel_buffer);
		RDUniform bind2 = new RDUniform { Binding = 2, UniformType = RenderingDevice.UniformType.StorageBuffer }; bind2.AddId(den_buffer);
		RDUniform bind3 = new RDUniform { Binding = 3, UniformType = RenderingDevice.UniformType.SamplerWithTexture };
		bind3.AddId(sampler_rid);
		bind3.AddId(local_sdf_rid);

		uniform_set = rendering_device.UniformSetCreate(new Godot.Collections.Array<RDUniform> { bind0, bind1, bind2, bind3 }, shader, 0);

		mesh.Multimesh.InstanceCount = count;
		static_mesh.Multimesh.InstanceCount = surface_points.Count;
		for (int i = 0; i < surface_points.Count; i++) {
			static_mesh.Multimesh.SetInstanceTransform(i, new Transform3D(Basis.Identity, surface_points[i]));
		}
	}

	public override void _Process(double delta) {
		if(GameManager.time_stop) {
			if (last_frame_stopped) {
				return;
			} else {
				last_frame_stopped = true;

				byte[] stopped_bytes = rendering_device.BufferGetData(pos_buffer);
				float[] pos_floats = MemoryMarshal.Cast<byte, float>(stopped_bytes).ToArray();

				int img_width = Mathf.CeilToInt(Mathf.Sqrt(count));
				int img_height = Mathf.CeilToInt((float)count / img_width);

				Image img = Image.Create(img_width, img_height, false, Image.Format.Rgbaf);
				for (int i = 0; i < count; i++) {
					img.SetPixel(i % img_width, i / img_width, new Color(pos_floats[i*4], pos_floats[i*4+1], pos_floats[i*4+2]));
				}
				
				var particle_tex = ImageTexture.CreateFromImage(img);
				ShaderMaterial mat = (ShaderMaterial)marched_mesh.GetActiveMaterial(0);
				mat.SetShaderParameter("particle_data", particle_tex);
				mat.SetShaderParameter("count", count);

				mesh.Visible = false;
				marched_mesh.Visible = true;
			}
		} else {
			if (last_frame_stopped) {
				mesh.Visible = true;
				marched_mesh.Visible = false;
			}
			last_frame_stopped = false;
		}
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
		float[] vals = {
			radius, //4
			total_count, //8
			dt, //12
			target_density, //16
			pressure, //20
			viscosity, //24
			.5f, //28
			0f, //32 padding
			bounds.X, bounds.Y, bounds.Z, 0f,
			sdf_node.GlobalPosition.X, sdf_node.GlobalPosition.Y, sdf_node.GlobalPosition.Z, 0f,
			sdf_node.Size.X, sdf_node.Size.Y, sdf_node.Size.Z, 0f,
		};

		//turn our info into bytes
		byte[] bytes = MemoryMarshal.AsBytes(vals.AsSpan()).ToArray();

		long list = rendering_device.ComputeListBegin(); //begin gpu instruction
		rendering_device.ComputeListBindComputePipeline(list, pipeline); //give it our shader

		rendering_device.ComputeListBindUniformSet(list, uniform_set, 0); //tells the program where to put our info
		
		rendering_device.ComputeListSetPushConstant(list, bytes, (uint)bytes.Length);
		rendering_device.ComputeListDispatch(list, (uint)Mathf.Ceil(total_count / 64.0f), 1, 1); //separates the processing into groups of 64
		//we are also doing this in a 1d array

		rendering_device.ComputeListEnd();
		rendering_device.Submit();
		rendering_device.Sync();
	}
}
