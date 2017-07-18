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
using NoiseCrimeStudios.Core.Features;
using System;

namespace NoiseCrimeStudios.Demo.Easing
{
	/// <summary>
	/// Tweening class to animate a RectTransform.
	/// Example of using EasingEquationsDouble class with CreateDelegate to select the desired Ease Equation.
	/// </summary>
	public class TweenAction_RectTransformSlide : MonoBehaviour
	{
		private Vector3                         m_Position;

		private float                           m_Duration  = 1f;
		private float                           m_Timer     = 0f;

		private float                           m_Start     = 0f;
		private float                           m_Offset    = 1f;

		private Action<bool>                    m_OnComplete;

		public delegate double					Ease( double t, double b, double c, double d );
		public Ease                             m_EaseMethod;

		void OnEnable() { }
			

		void Update()
		{
			m_Timer = m_Timer + Time.unscaledDeltaTime;

			if ( m_Timer > m_Duration )
			{
				m_Timer = m_Duration;
				enabled = false;

				if ( m_OnComplete != null ) m_OnComplete( true );
				m_OnComplete = null;
			}

			m_Position.x = ( float )m_EaseMethod( m_Timer, m_Start, m_Offset, m_Duration );
			transform.position = m_Position;
		}


		static public T Begin<T>( GameObject go, float start, float offset, float duration, EasingEquationsDouble.Equations easeEquation, Action<bool> onComplete = null ) where T : TweenAction_RectTransformSlide
		{
			T component = go.GetComponent<T>();

			// If component doesn't exist add it
			if ( null == component ) component = go.AddComponent<T>();
			
			// Clamp duration
			if ( duration < 0.1f ) duration = 0.1f;

			component.m_Position	= go.transform.position;
			component.m_EaseMethod	= ( Ease )Delegate.CreateDelegate( typeof( Ease ), typeof( EasingEquationsDouble ).GetMethod( easeEquation.ToString() ) );
			component.m_Timer		= 0f;
			component.m_Duration	= duration;
			component.m_Start		= start;
			component.m_Offset		= offset;

			component.m_OnComplete	= onComplete;
			component.enabled		= true;

			return component;
		}
	}
}

