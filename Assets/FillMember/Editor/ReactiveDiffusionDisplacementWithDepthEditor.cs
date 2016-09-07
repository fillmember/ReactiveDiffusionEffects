using UnityEngine;
using UnityEditor;

namespace FillMember {

		[CanEditMultipleObjects]
		[CustomEditor(typeof(ReactiveDiffusionDisplacementWithDepth))]
		public class ReactiveDiffusionDisplacementEditor : Editor {

			SerializedProperty iterations;
			SerializedProperty feedRate;
			SerializedProperty killRate;
			SerializedProperty mvAddAmount;
			SerializedProperty texelSize;
			SerializedProperty displaceStrength;
			SerializedProperty mvDecayRate;
			SerializedProperty dryWet;
			SerializedProperty displacePositionOnly;

			public int selGridInt = 0;
			public string[] selStrings = new string[] {"radio1", "radio2", "radio3"};

			void OnEnable() {

				iterations = serializedObject.FindProperty("iterations");
				feedRate = serializedObject.FindProperty("feedRate");
				killRate = serializedObject.FindProperty("killRate");
				mvAddAmount = serializedObject.FindProperty("mvAddAmount");
				texelSize = serializedObject.FindProperty("texelSize");
				displaceStrength = serializedObject.FindProperty("displaceStrength");
				mvDecayRate = serializedObject.FindProperty("mvDecayRate");
				dryWet = serializedObject.FindProperty("dryWet");
				displacePositionOnly = serializedObject.FindProperty("displacePositionOnly");

			}

			public override void OnInspectorGUI() {

				serializedObject.Update();

				EditorGUILayout.PropertyField( iterations  , new GUIContent("Simulation : iteration") );
				EditorGUILayout.PropertyField( feedRate    , new GUIContent("Simulation : feed rate") );
				EditorGUILayout.PropertyField( killRate    , new GUIContent("Simulation : kill rate") );
				EditorGUILayout.PropertyField( mvAddAmount , new GUIContent("Simulation : feed by motion") );
				EditorGUILayout.PropertyField( texelSize   , new GUIContent("Simulation : step size") );
				EditorGUILayout.PropertyField( displaceStrength , new GUIContent("Displacement Strength") );
				EditorGUILayout.PropertyField( mvDecayRate      , new GUIContent("Motion vector decay") );
				EditorGUILayout.PropertyField( dryWet           , new GUIContent("Effect dry/wet") );
				EditorGUILayout.PropertyField( displacePositionOnly , new GUIContent("Only displace position") );

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
