using UnityEditor;
using UnityEngine;
using NoiseCrimeStudios.Demo.Easing.CurveCreation;



[CustomEditor( typeof( EasingAnimationCurveCache ) )]
public class EasingAnimationCurveCacheEditor : Editor
{
	EasingAnimationCurveCache		m_Target;

	 public void OnEnable()
     {
         m_Target = (EasingAnimationCurveCache)target;
     }


	public override void OnInspectorGUI()
	{
		EditorGUIUtility.labelWidth = 192f;
		base.DrawDefaultInspector();

		EditorGUILayout.Space();

		if ( GUILayout.Button( "Convert All Ease Equations" ) ) m_Target.ConvertAllEaseEquations();

		EditorGUIUtility.labelWidth = 0f;

		EditorUtility.SetDirty(m_Target);
	}
}
