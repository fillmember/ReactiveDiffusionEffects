using UnityEngine;
using UnityEditor;

namespace FillMember
{

		[CanEditMultipleObjects]
		[CustomEditor(typeof(ReactiveDiffusionDisplacement))]
		public class ReactiveDiffusionDisplacementEditor : Editor {

			SerializedProperty iterations;
			SerializedProperty feedRate;
			SerializedProperty killRate;
			SerializedProperty texelSize;
			SerializedProperty displaceStrength;
			SerializedProperty dryWet;
			SerializedProperty displacePositionOnly;

			SerializedProperty rdBuffer;
			SerializedProperty workBuffer;
			SerializedProperty rdBuffer2;
			SerializedProperty workBuffer2;

			public int selGridInt = 0;
			public string[] selStrings = new string[] {"radio1", "radio2", "radio3"};

			void OnEnable() {

				iterations = serializedObject.FindProperty("iterations");
				feedRate = serializedObject.FindProperty("feedRate");
				killRate = serializedObject.FindProperty("killRate");
				texelSize = serializedObject.FindProperty("texelSize");
				displaceStrength = serializedObject.FindProperty("displaceStrength");
				dryWet = serializedObject.FindProperty("dryWet");
				displacePositionOnly = serializedObject.FindProperty("displacePositionOnly");

				rdBuffer = serializedObject.FindProperty("rdBuffer");
				workBuffer = serializedObject.FindProperty("workBuffer");
				rdBuffer2 = serializedObject.FindProperty("rdBuffer2");
				workBuffer2 = serializedObject.FindProperty("workBuffer2");

			}

			public override void OnInspectorGUI() {

				serializedObject.Update();

				EditorGUILayout.PropertyField( iterations );
				EditorGUILayout.PropertyField( feedRate );
				EditorGUILayout.PropertyField( killRate );
				EditorGUILayout.PropertyField( texelSize );
				EditorGUILayout.PropertyField( displaceStrength );
				EditorGUILayout.PropertyField( dryWet );
				EditorGUILayout.PropertyField( displacePositionOnly );

				EditorGUILayout.PropertyField( rdBuffer );
				EditorGUILayout.PropertyField( rdBuffer2 );
				EditorGUILayout.PropertyField( workBuffer );
				EditorGUILayout.PropertyField( workBuffer2 );

				serializedObject.ApplyModifiedProperties();

				EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
				EditorGUILayout.BeginHorizontal();

				if ( GUILayout.Button("Clear") ) {

					foreach ( ReactiveDiffusionDisplacement d in targets ) d.StopEffect();

				}

				if ( GUILayout.Button("Start") ) {

					foreach ( ReactiveDiffusionDisplacement d in targets ) d.StartEffect();

				}

				if ( GUILayout.Button("Debug") ) {

					foreach ( ReactiveDiffusionDisplacement d in targets ) d.RDDebug();

				}

				EditorGUILayout.EndHorizontal();
				EditorGUI.EndDisabledGroup ();

			}

		}

}
