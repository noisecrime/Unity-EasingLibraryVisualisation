/* MIT License

Copyright (c) 2017 NoiseCrime

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using UnityEngine;
using System;
using System.Collections.Generic;
using NoiseCrimeStudios.Core.AnimationCurves;
using EaseEquations = NoiseCrimeStudios.Core.Features.Easing.EasingEquationsDouble.Equations;


namespace NoiseCrimeStudios.Demo.Easing.CurveCreation
{
	/// <summary>
	/// Scriptable object to both create and contain Ease Equations converted to AnimationCurves.
	/// </summary>
	[System.Serializable]
	[CreateAssetMenu( fileName = "EasingAnimationCurveCache", menuName = "NoiseCrimeStudios/EasingAnimationCurveCache", order = 21 )]
	public class EasingAnimationCurveCache : ScriptableObject
	{
		public AnimationCurve[]								m_AnimationCurves;
				
		[Header("Animation Curve Properties")]
		[Range(1, 2000)]		public int					m_NumEquationSteps      = 1000;
		[Range(1, 50)]			public float				m_MaxStepsBetweenPoints = 1f;
		[Range(40, 180)]		public float				m_SmoothTangentMaxAngle = 60f;
		public  RuntimeAnimationUtility.TangentMode			m_TangentMode           = RuntimeAnimationUtility.TangentMode.Auto;
				
		[Header("Preprocess Properties")]
		[Range(0.0001f, 0.1f)]	public float				m_RdpError              = 0.0035f;
		public  ePreprocessModes							m_PreprocessMode        = ePreprocessModes.RDP;
			
		[Header("CurveFit Properties")]
		public  bool										m_UseCurveFit           = false;
		[Range(0.0001f, 0.1f)]	public float				m_FitError              = 0.001f;
		[Range(0.0001f, 0.1f)]	public float				m_PointDistance         = 0.01f;

		[Space(10)]
		public  bool										m_DebugConversion       = false;


		/// <summary>
		/// Converts all Ease Equations into AnimationCurves.
		/// Certain Ease Equations are treated slithly differently - e,g, 'BounceEase' to get correct graph.
		/// </summary>
		public void ConvertAllEaseEquations()
		{
			ConversionProperties cp		= CreateConversionProperties();

			List<AnimationCurve> curves = new List<AnimationCurve>();

			// All Ease conversions share the same conversion properties with exception of SmoothTangentMaxAngle			
			foreach ( EaseEquations e in Enum.GetValues(typeof(EaseEquations) ))
			{
				switch ( e )
				{
					case EaseEquations.BounceEaseIn:
					case EaseEquations.BounceEaseInOut:
					case EaseEquations.BounceEaseOut:
					case EaseEquations.BounceEaseOutIn:
						cp.m_SmoothTangentMaxAngle =  60f; break;
					default:
						cp.m_SmoothTangentMaxAngle = 180f; break;
				}

				AnimationCurve ac = new AnimationCurve();
				EasingToAnimationCurve.ConvertEaseEquationToCurve( e , ac, cp, false );
				curves.Add( ac );
			}
		
			m_AnimationCurves = curves.ToArray();
		}		


		ConversionProperties CreateConversionProperties()
		{
			ConversionProperties cp     = new ConversionProperties();
			cp.m_FitError				= m_FitError;
			cp.m_RdpError				= m_RdpError;
			cp.m_PointDistance			= m_PointDistance;
			cp.m_NumEquationSteps		= m_NumEquationSteps;
			cp.m_MaxStepsBetweenPoints	= m_MaxStepsBetweenPoints;
			cp.m_UseCurveFit			= m_UseCurveFit;
			cp.m_TangentMode			= m_TangentMode;
			cp.m_PreprocessMode			= m_PreprocessMode;
			cp.m_SmoothTangentMaxAngle	= m_SmoothTangentMaxAngle;

			return cp;
		}
	}

}
