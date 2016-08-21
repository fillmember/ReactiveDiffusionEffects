using UnityEngine;
using System.Collections;

namespace FillMember {

	[RequireComponent(typeof(Camera))]
	public class ReactiveDiffusionDisplacement : MonoBehaviour {

		#region Public properties and methods

		[SerializeField , Range(1,32)]
		[Tooltip("simulation iterations per frame")]
		public uint iterations = 2;

		[SerializeField , Range(0,0.2f)]
		[Tooltip("Feed rate of reactive diffusion simulation")]
		public float feedRate = 0.03500f;

		[SerializeField , Range(0,0.2f)]
		[Tooltip("Kill rate of reactive diffusion simulation")]
		public float killRate = 0.06400f;

		[SerializeField , Range(0.0001f,0.005f)]
		[Tooltip("Simulation texel size. ")]
		public float texelSize = 0.0015f;

		[SerializeField , Range(0,1)]
		[Tooltip("Displacement decay rate")]
		public float decayRate = 0.98f;

		[SerializeField]
		public bool displacePositionOnly = false;


		#endregion

		[SerializeField]
		private int state = 0;

		#region Private properties

		Shader shader;

		Material material;

		RenderTexture rdBuffer = null;
		RenderTexture workBuffer = null;
		RenderTexture accumulated = null;

		int lastFrame;

		void ReleaseBuffer(RenderTexture buffer) {
			if (buffer != null) RenderTexture.ReleaseTemporary(buffer);
		}

		RenderTexture NewBuffer(RenderTexture source) {
			return RenderTexture.GetTemporary(source.width, source.height);
		}

		#endregion


		#region Monobehaviour functions

		public void StopEffect(){

			state = 0;

		}

		public void StartEffect(){

			state = 1;

		}

		public void RDDebug() {

			state = 3;

		}

		// Simulate: Reactive Diffusion Simulation
		void Simulate() {

			if (Time.frameCount != lastFrame) {

				material.SetFloat ("killRate", killRate);
				material.SetFloat ("feedRate", feedRate);
				material.SetFloat ("texelSize", texelSize);

				material.SetTexture ("rdTex", rdBuffer);

				for (int i = 0; i < iterations; i++) {
					Graphics.Blit ( rdBuffer , rdBuffer , material, 1 );
				}

				lastFrame = Time.frameCount;
				
			}

		}

		void OnEnable() {

			// Material
			material = new Material (Shader.Find ("Hidden/FillMember/ReactiveDiffusionDisplacement"));
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
			ReleaseBuffer (workBuffer);
			ReleaseBuffer (accumulated);
			rdBuffer = null;
			workBuffer = null;
			accumulated = null;

		}

		void OnRenderImage( RenderTexture source , RenderTexture destination ) {

			if (state == 0) {

				// update buffers
				ReleaseBuffer (workBuffer);
				workBuffer = NewBuffer (source);
				Graphics.Blit (source, workBuffer);

				// Material
				material.SetTexture ("originalTex", workBuffer);

				// Render : copy source to destination
				Graphics.Blit (source, destination);


			} else if (state == 1) {

				// update buffers
				ReleaseBuffer (rdBuffer);
				rdBuffer = NewBuffer (source);
				Graphics.Blit (source, rdBuffer);

				// accumulated
				ReleaseBuffer( accumulated );
				accumulated = NewBuffer( source );
				Graphics.Blit (source, accumulated, material, 0);

				state = 2;

			} else if (state == 2) {

				Simulate();

				// calculate accumulated
				material.SetTexture ("accumulatedMotionVector", accumulated);
				material.SetFloat ("decayRate", decayRate);
				Graphics.Blit (source, accumulated, material, 2);

				// write to destination
				if ( displacePositionOnly ) {
					Graphics.Blit (source, workBuffer, material, 3);
				} else {
					Graphics.Blit (source, workBuffer, material, 4);
				}

				Graphics.Blit (workBuffer, destination);

			} else if (state == 3) {

				Simulate();

				Graphics.Blit (rdBuffer, destination);

			}

		}

		#endregion

	}

}
