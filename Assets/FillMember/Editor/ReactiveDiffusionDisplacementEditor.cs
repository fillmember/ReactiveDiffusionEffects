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
			SerializedProperty decayRate;
			SerializedProperty displacePositionOnly;

			public int selGridInt = 0;
			public string[] selStrings = new string[] {"radio1", "radio2", "radio3"};

			void OnEnable() {

				iterations = serializedObject.FindProperty("iterations");
				feedRate = serializedObject.FindProperty("feedRate");
				killRate = serializedObject.FindProperty("killRate");
				texelSize = serializedObject.FindProperty("texelSize");
				decayRate = serializedObject.FindProperty("decayRate");
				displacePositionOnly = serializedObject.FindProperty("displacePositionOnly");

			}

			public override void OnInspectorGUI() {

				serializedObject.Update();

				EditorGUILayout.PropertyField( iterations );
				EditorGUILayout.PropertyField( feedRate );
				EditorGUILayout.PropertyField( killRate );
				EditorGUILayout.PropertyField( texelSize );
				EditorGUILayout.PropertyField( decayRate );
				EditorGUILayout.PropertyField( displacePositionOnly );

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
