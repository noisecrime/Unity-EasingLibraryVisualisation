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
using System.Collections.Generic;
using NoiseCrimeStudios.Core.AnimationCurves;
using burningmime.curves;

namespace NoiseCrimeStudios.Demo.Easing.CurveCreation
{
	public enum ePreprocessModes
	{
		NONE,
		LINEAR,
		RDP
	}

	[System.Serializable]
	public struct ConversionProperties
	{
		// CurveFit Properties
		public bool                                     m_UseCurveFit;              // False
		public float                                    m_FitError;                 // 0.001
		public float                                    m_RdpError;                 // 0.0035
		public float                                    m_PointDistance;            // 0.01
		public ePreprocessModes                         m_PreprocessMode;           // rdp

		// Animation Curve Related Properties
		public RuntimeAnimationUtility.TangentMode      m_TangentMode;              // Auto
		public int                                      m_NumEquationSteps;         // 1000
		public float                                    m_MaxStepsBetweenPoints;    // 1		
		public float                                    m_SmoothTangentMaxAngle;    // 60f
	}

	/// <summary>
	/// Methods to convert input data into AnimationCurves
	/// </summary>
	public class ConvertToAnimationCurve
	{
		/// <summary>
		/// Various preprocesses available to apply to point data.
		/// </summary>
		/// <param name="pts">List of points on the curve.</param>
		/// <param name="preprocessMode">Mode, if any, to apply as preprocessing.</param>
		/// <param name="linearDist">See CurvePreprocess.Linearize.</param>
		/// <param name="rdpError">See CurvePreprocess.RdpReduce</param>
		/// <returns></returns>
		public static List<Vector2> Preprocess( List<Vector2> pts, ePreprocessModes preprocessMode, float linearDist, float rdpError )
		{
			switch ( preprocessMode )
			{
				case ePreprocessModes.NONE:		return pts;
				case ePreprocessModes.LINEAR:	return CurvePreprocess.Linearize( pts, linearDist );
				case ePreprocessModes.RDP:		return CurvePreprocess.RdpReduce( pts, rdpError );
				default:						return CurvePreprocess.RemoveDuplicates( pts );
			}
		}

		/// <summary>
		/// Given a list of points convert to animation curve using supplied conversionProperties.
		/// </summary>
		/// <param name="animCurve">AnimationCurve that contains converted results.</param>
		/// <param name="conversionProperties">Struct that defines various curve conversion properties.</param>
		/// <param name="inPts">List of 2D Points on curve.</param>
		/// <param name="debug">When true will log various debug statements.</param>
		public static void ConvertToAnimCurve( AnimationCurve animCurve, ConversionProperties conversionProperties, List<Vector2> inPts, bool debug = false )
		{
			Keyframe	keyFrame;
			Keyframe[]	keyFrames	= new Keyframe[inPts.Count+1];
			float[]		keyAngles	= new float[inPts.Count+1];
			
			for ( int i = 0; i < inPts.Count; i++ )
			{
				keyFrame = new Keyframe();
				
				if ( i > 0 && i < inPts.Count-1 )
				{
					// keyAngles[ i ] = Mathf.Atan2(inPts[ i ].y - inPts[ i-1 ].y, inPts[ i ].x - inPts[ i-1 ].x) * 180f / Mathf.PI;
					Vector2 left		= new Vector2( inPts[ i ].y - inPts[ i-1 ].y, inPts[ i ].x - inPts[ i-1 ].x );//.normalized;
					Vector2 right	= new Vector2( inPts[ i+1 ].y - inPts[ i ].y, inPts[ i+1 ].x - inPts[ i-1 ].x );//.normalized;
					keyAngles[ i ]  = Vector2.Angle( left, right);
					
					if ( keyAngles[ i ] > conversionProperties.m_SmoothTangentMaxAngle )
					{
						keyFrame.inTangent  =  Mathf.Atan2(inPts[ i ].y - inPts[ i-1 ].y, inPts[ i ].x - inPts[ i-1 ].x);
						keyFrame.outTangent =  Mathf.Atan2(inPts[ i+1 ].y - inPts[ i].y, inPts[ i+1 ].x - inPts[ i ].x);
					}
				}				

				keyFrame.value = inPts[ i ].y;
				keyFrame.time  = inPts[ i ].x;
				keyFrames[ i ] = keyFrame;
			}

			// Last Frame
			keyFrame = new Keyframe();
			keyFrame.value = inPts[ inPts.Count - 1 ].y;
			keyFrame.time  = inPts[ inPts.Count - 1 ].x;
			keyFrames[ inPts.Count ] = keyFrame;
			
			animCurve.keys = keyFrames;
			
			for ( int i = 0; i < keyFrames.Length; i++ )
			{				
				RuntimeAnimationUtility.TangentMode tangent =  ( keyAngles[ i ] > conversionProperties.m_SmoothTangentMaxAngle ) ? RuntimeAnimationUtility.TangentMode.Linear : conversionProperties.m_TangentMode;
				RuntimeAnimationUtility.SetKeyBroken(animCurve, i, ( keyAngles[ i ] > conversionProperties.m_SmoothTangentMaxAngle ) );
				RuntimeAnimationUtility.SetKeyLeftTangentMode(  animCurve, i, tangent );
				RuntimeAnimationUtility.SetKeyRightTangentMode( animCurve, i, tangent );
			}			

			if(debug) AnimationCurveToString( animCurve, keyAngles);
		}


		public static void ConvertToAnimCurve( AnimationCurve animCurve, ConversionProperties conversionProperties, CubicBezier[] curves, bool debug = false )
		{
			Keyframe keyFrame;
			Keyframe[] keyFrames        = new Keyframe[curves.Length+1];
			float curveLength           = 1f/curves.Length;

			float curveTime             = 0;
			for ( int i = 0; i < curves.Length; i++ )
			{
				keyFrame = new Keyframe();

				keyFrame.value = curves[ i ].p0.y;
				keyFrame.time = curveTime;
				curveTime += curveLength;
				keyFrames[ i ] = keyFrame;
			}

			keyFrame = new Keyframe();

			keyFrame.value = curves[ curves.Length - 1 ].p3.y;
			keyFrame.time = curveTime;
			curveTime += curveLength;
			keyFrames[ curves.Length ] = keyFrame;

			animCurve.keys = keyFrames;

			for ( int i = 0; i < keyFrames.Length; i++ )
			{
				RuntimeAnimationUtility.SetKeyLeftTangentMode(  animCurve, i, conversionProperties.m_TangentMode );
				RuntimeAnimationUtility.SetKeyRightTangentMode( animCurve, i, conversionProperties.m_TangentMode );
			}

			if(debug) AnimationCurveToString( animCurve);
		}


				
		#region LOGGING
		public static void AnimationCurveToString( AnimationCurve ac, float[] keyAngles = null)
		{
			string output = "NoisCrimeStudios: AnimationCurve Breakdown\n";
			Keyframe[] keys = ac.keys;
			// RuntimeAnimationUtility.TangentMode.ConstrainToPolynomialCurve()

			for ( int i = 0; i < keys.Length; i++ )
			{
				output += string.Format( "{0:D3}  Time: {1:F3}  Val: {2:F3} Tan: {3:F3} | {4:F4}  Mode: {5}  angle: {6:F3}\n",
					i, keys[ i ].time, keys[ i ].value, keys[ i ].inTangent, keys[ i ].outTangent, keys[ i ].tangentMode, ( null == keyAngles ) ? "Not Calculated" : keyAngles[ i ].ToString() );
			}			

			Debug.Log( output );
		}
		#endregion
	}
}
