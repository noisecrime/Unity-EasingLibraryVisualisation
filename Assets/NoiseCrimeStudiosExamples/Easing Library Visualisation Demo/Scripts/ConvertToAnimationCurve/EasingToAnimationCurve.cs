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
using NoiseCrimeStudios.Core.Features.Easing;
using burningmime.curves;


namespace NoiseCrimeStudios.Demo.Easing.CurveCreation
{		
	/// <summary>
	/// Given an Ease Equation enumeration, generate an AnimationCurve.
	/// </summary>
	public class EasingToAnimationCurve
	{
		public delegate double Ease( double t, double b, double c, double d );
		public static Ease                      m_EaseMethod;
			
		/// <summary>
		/// Generates an AnimationCurve based on input Ease Equation Method.
		/// </summary>
		/// <param name="equation">Enum of Ease Equation To use.</param>
		/// <param name="animCurve">AnimationCurve that contains converted results.</param>
		/// <param name="conversionProperties">Struct that defines various curve conversion properties.</param>
		/// <param name="debug">When true will log various debug statements.</param>
		public static void ConvertEaseEquationToCurve(EasingEquationsDouble.Equations equation, AnimationCurve animCurve,  ConversionProperties conversionProperties, bool debug = false)
		{
			m_EaseMethod = ( Ease )Delegate.CreateDelegate( typeof( Ease ), typeof( EasingEquationsDouble ).GetMethod( equation.ToString() ) );

			List<Vector2> inPts = CreatePointsFromEaseEquation(conversionProperties, false);			
			List<Vector2> ppPts = ConvertToAnimationCurve.Preprocess(inPts, conversionProperties.m_PreprocessMode, conversionProperties.m_PointDistance, conversionProperties.m_RdpError);

			if ( debug ) Debug.LogFormat( "ConvertEaseEquationToCurve: {0:D3} {1:D4} : {2}", ppPts.Count, inPts.Count, equation);

			if ( conversionProperties.m_UseCurveFit )
			{
				CubicBezier[] curves = CurveFit.Fit(ppPts, conversionProperties.m_FitError);
				if (debug) Debug.LogFormat( "ConvertEaseEquationToCurve: {0} curves: {1}", equation, curves.Length );
				ConvertToAnimationCurve.ConvertToAnimCurve( animCurve, conversionProperties, curves );
			}
			else
			{
				ConvertToAnimationCurve.ConvertToAnimCurve( animCurve, conversionProperties, ppPts, debug );
			}
		}
				
		/// <summary>
		/// Simple method to generate points of interest from an Ease Equation. 
		/// The results can be further processed before conversion into an animationCurve.
		/// Intention is to capture curve changes in direction and provide regular interval points along curve.
		/// </summary>
		/// <param name="conversionProperties">Struct that defines various curve conversion properties.</param>
		/// <param name="debug">When true will log various debug statements.</param>
		/// <returns></returns>
		private static List<Vector2> CreatePointsFromEaseEquation( ConversionProperties conversionProperties, bool debug = false)
		{
			List<Vector2> inPts			= new List<Vector2>();

			float	step                = 1f/conversionProperties.m_NumEquationSteps;
			float	maxStep				= step * conversionProperties.m_MaxStepsBetweenPoints;
			float	previousVal         = (float)m_EaseMethod( 0, 0f, 1f, 1f);
			float   previousDirection   = 0;
			float   time                = 0;
			float   nexttime            = 0;
			float   val                 = 0f;

			string  pointStr            = "";

			inPts.Add( new Vector2( 0f, previousVal ) );

			for ( int i = 0; i < conversionProperties.m_NumEquationSteps; i++, time += step )
			{
				val = ( float )m_EaseMethod( time, 0f, 1f, 1f );
				float newDirection  = Mathf.Sign( val - previousVal);
				previousVal = val;

				if ( newDirection != previousDirection || ( time > nexttime ) )// && Mathf.Abs( val - previousVal ) > 0.0001f ) )
				{
					nexttime			= nexttime + maxStep;
					previousDirection	= newDirection;

					// If previous val is within timestep don't add new point re-use
			//		bool previousLessThanMaxStep	= time - inPts[ inPts.Count - 1 ].x <= maxStep;
			//		bool previousLongDistAway		= Mathf.Abs( val - inPts[ inPts.Count - 1 ].y ) > 0.001f;

			//		if ( previousLessThanMaxStep && previousLongDistAway )					
			//			inPts[ inPts.Count - 1 ].Set( time, val);					
			//		else					
						inPts.Add( new Vector2( time, val ) );
					

					if (debug) pointStr += string.Format( "{0:D3}  Time: {1:F3} val: {2:F3}\n", i, time, val );
				}
			}

			// last point
			time = 1f;
			val = ( float )EasingEquationsDouble.ElasticEaseIn( time, 0f, 1f, 1f );
			inPts.Add( new Vector2( 1, val ) );

			if ( debug )
			{
				pointStr += string.Format( "{0:D3}  Time: {1:F3} val: {2:F3}\n", conversionProperties.m_NumEquationSteps, time, val );
				Debug.Log( pointStr );
			}

			return inPts;
		}


	}
}
