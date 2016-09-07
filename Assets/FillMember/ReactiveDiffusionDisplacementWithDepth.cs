using UnityEngine;
using System.Collections;

namespace FillMember {

	[RequireComponent(typeof(Camera))]
	public class ReactiveDiffusionDisplacementWithDepth : MonoBehaviour {

		#region Public properties and methods

		[SerializeField , Range(1,16)]
		[Tooltip("Simulation iterations per frame. Higher value results in faster simulation but bad for performance. ")]
		public uint iterations = 2;

		[SerializeField , Range(0.0050f,0.1110f)]
		[Tooltip("Feed rate of reactive diffusion simulation. ")]
		public float feedRate = 0.035f;

		[SerializeField , Range(0.0300f,0.0740f)]
		[Tooltip("Kill rate of reactive diffusion simulation. ")]
		public float killRate = 0.060f;

		[SerializeField , Range(0,1)]
		[Tooltip("Simulation add rate by motion vector. ")]
		public float mvAddAmount = 0.005f;

		[SerializeField , Range(0.0001f,0.005f)]
		[Tooltip("Simulation texel size. ")]
		public float texelSize = 0.0015f;

		[SerializeField , Range(-10,10)]
		[Tooltip("Displacement strength. ")]
		public float displaceStrength = 0.1f;

		[SerializeField , Range(0,1)]
		[Tooltip("Motion vector decay rate. ")]
		public float mvDecayRate = 0.001f;

		[SerializeField , Range(0,1)]
		[Tooltip("Intensity of the effect. ")]
		public float dryWet = 0.9f;

		[SerializeField]
		public bool displacePositionOnly = false;


		#endregion

		private int state = 0;

		private bool pingpong_rdBuffer = false;
		private bool pingpong_workBuffer = false;

		private RenderTexture rdBuffer = null;
		private RenderTexture rdBuffer2 = null;
		private RenderTexture workBuffer = null;
		private RenderTexture workBuffer2 = null;

		#region Private properties

		Material material;

		int lastFrame;

		void ReleaseBuffer(RenderTexture buffer) {
			if (buffer != null) RenderTexture.ReleaseTemporary(buffer);
		}

		RenderTexture NewBuffer(string name , RenderTexture source) {
			RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height);
			rt.name = name;
			rt.filterMode = FilterMode.Point;
			return rt;
		}

		#endregion


		#region Monobehaviour functions

		public void StopEffect(){

			state = 0;

		}

		public void StartEffect(){

			if (state == 0) {

				state = 1;

			} else {

				state = 2;

			}

		}

		public void RDDebug() {

			state = 3;

		}

		// Simulate: Reactive Diffusion Simulation
		void Simulate() {

			if (Time.frameCount != lastFrame) {

				for (int i = 0; i < iterations; i++) {
					if (pingpong_rdBuffer) {
						Graphics.Blit ( rdBuffer , rdBuffer2 , material, 1 );
					} else {
						Graphics.Blit ( rdBuffer2 , rdBuffer , material, 1 );
					}
					pingpong_rdBuffer = ! pingpong_rdBuffer;
				}

				lastFrame = Time.frameCount;
				
			}

		}

		void OnEnable() {

			// Material
			material = new Material (Shader.Find ("Hidden/FillMember/ReactiveDiffusionDisplacementWithDepth"));
			material.hideFlags = HideFlags.DontSave;

			// Camera
			GetComponent<Camera> ().depthTextureMode |=
				DepthTextureMode.Depth | DepthTextureMode.MotionVectors;

			state = 0;

		}

		void OnDisable() {

			// Material
			DestroyImmediate (material);
			material = null;

			// Buffers
			ReleaseBuffer (rdBuffer);
			ReleaseBuffer (rdBuffer2);
			ReleaseBuffer (workBuffer);
			ReleaseBuffer (workBuffer2);
			rdBuffer = null;
			rdBuffer2 = null;
			workBuffer = null;
			workBuffer2 = null;

		}

		void OnRenderImage( RenderTexture source , RenderTexture destination ) {

			if (pingpong_workBuffer) {
				material.SetTexture ("_workBuffer", workBuffer2);
			} else {
				material.SetTexture ("_workBuffer", workBuffer);
			}
			if (pingpong_rdBuffer) {
				material.SetTexture ("_rdTex", rdBuffer2);
			} else {
				material.SetTexture ("_rdTex", rdBuffer);
			}

			material.SetFloat ("killRate", killRate);
			material.SetFloat ("feedRate", feedRate);
			material.SetFloat ("texelSize", texelSize);
			material.SetFloat ("displaceStrength", displaceStrength);
			material.SetFloat ("mvDecayRate", mvDecayRate);
			material.SetFloat ("mvAddAmount", mvAddAmount);
			material.SetFloat ("dryWet", dryWet);

			if (state == 0) {

				// Render : copy source to destination
				Graphics.Blit (source, destination);

			} else if (state == 1) {

				// update buffers
				ReleaseBuffer ( workBuffer );
				ReleaseBuffer ( workBuffer2 );
				ReleaseBuffer ( rdBuffer );
				ReleaseBuffer ( rdBuffer2 );
				workBuffer  = NewBuffer ( "workA" , source );
				workBuffer2 = NewBuffer ( "workB" , source );
				rdBuffer    = NewBuffer ( "rdA" , source );
				rdBuffer2   = NewBuffer ( "rdB" , source );
				
				Graphics.Blit (source, workBuffer);
				Graphics.Blit (source, workBuffer2);
				Graphics.Blit (source, rdBuffer);
				Graphics.Blit (source, rdBuffer);

				state = 2;

			} else if (state == 2) {

				Simulate();

				int pass = displacePositionOnly ? 2 : 3;

				// write to destination
				if ( pingpong_workBuffer ) {
					Graphics.Blit (source, workBuffer, material, pass);
					Graphics.Blit (workBuffer, destination);
				} else {
					Graphics.Blit (source, workBuffer2, material, pass);
					Graphics.Blit (workBuffer2, destination);
				}

				pingpong_workBuffer = ! pingpong_workBuffer;
				
			} else if (state == 3) {

				Simulate();

				Graphics.Blit (rdBuffer, destination);

			}

		}

		#endregion

	}

}
