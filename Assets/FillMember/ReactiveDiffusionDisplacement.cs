using UnityEngine;
using System.Collections;

namespace FillMember {

	[RequireComponent(typeof(Camera))]
	public class ReactiveDiffusionDisplacement : MonoBehaviour {

		#region Public properties and methods

		[Range(1,32)]
		public uint iterations = 2;

		[Range(0,1f)]
		public float diffuseA = 0.85f;
		[Range(0,1f)]
		public float diffuseB = 1.00f;


		[Range(0,0.2f)]
		public float feedRate = 0.03500f;
		[Range(0,0.2f)]
		public float killRate = 0.06400f;

		[Range(0.0001f,0.005f)]
		public float texelSize = 0.0015f;

		[Range(0,1)]
		public float decayRate = 0.98f;

		public bool distortOnly = false;

		[Range(0,2)]
		public int state = 0;

		// public bool mosh = true;
		// public bool distort = true;

		#endregion


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
				ReleaseBuffer (rdBuffer);
				rdBuffer = NewBuffer (source);
				Graphics.Blit (source, rdBuffer);

				ReleaseBuffer (workBuffer);
				workBuffer = NewBuffer (source);
				Graphics.Blit (source, workBuffer);

				ReleaseBuffer( accumulated );
				accumulated = NewBuffer( source );
				Graphics.Blit (source, accumulated, material, 0);

				// Render : copy source to destination
				Graphics.Blit (source, destination);

				material.SetTexture ("originalTex", workBuffer);

			} else if (state == 1) {

				if (Time.frameCount != lastFrame) {

					material.SetFloat ("killRate", killRate);
					material.SetFloat ("feedRate", feedRate);
					material.SetFloat ("diffuseA", diffuseA);
					material.SetFloat ("diffuseB", diffuseB);
					material.SetFloat ("texelSize", texelSize);

					material.SetTexture ("rdTex", rdBuffer);

					for (int i = 0; i < iterations; i++) {
						Graphics.Blit (rdBuffer, rdBuffer, material, 1);
					}

					lastFrame = Time.frameCount;

					// calculate accumulated
					material.SetTexture ("accumulatedMotionVector", accumulated);
					material.SetFloat ("decayRate", decayRate);
					Graphics.Blit (source, accumulated, material, 2);
					
				}

				// write to destination
				if ( distortOnly ) {
					Graphics.Blit (source, workBuffer, material, 4);
				} else {
					Graphics.Blit (source, workBuffer, material, 3);
				}

				Graphics.Blit (workBuffer, destination);

			} else if (state == 2) {

				if (Time.frameCount != lastFrame) {

					material.SetFloat ("killRate", killRate);
					material.SetFloat ("feedRate", feedRate);
					material.SetFloat ("diffuseA", diffuseA);
					material.SetFloat ("diffuseB", diffuseB);
					material.SetFloat ("texelSize", texelSize);

					material.SetTexture ("rdTex", rdBuffer);

					for (int i = 0; i < iterations; i++) {
						Graphics.Blit (rdBuffer, rdBuffer, material, 1);
					}

					lastFrame = Time.frameCount;

				}

				Graphics.Blit (rdBuffer, destination);

			}

		}

		#endregion

	}

}
