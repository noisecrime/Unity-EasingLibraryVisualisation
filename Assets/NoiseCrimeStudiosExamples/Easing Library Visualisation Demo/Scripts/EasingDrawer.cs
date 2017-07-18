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
using UnityEngine.EventSystems;
using NoiseCrimeStudios.Core.Features;

namespace NoiseCrimeStudios.Demo.Easing
{
	/// <summary>
	/// Interaction and animation control for a simple 'drawer' for a specific ease equation on the right side of the screen.
	/// </summary>
	public class EasingDrawer : MonoBehaviour, IPointerEnterHandler
	{
		bool                                    m_IsOpening     = false;
		bool                                    m_IsAnimating   = false;

		private EasingGraphManager              m_EasingGraphManager;
		private EasingEquationsDouble.Equations m_Equation;
		private float                           m_Start     = 0f;
		private float                           m_Offset    = 1f;


		public void OnPointerEnter( PointerEventData eventData )
		{
			//Debug.Log("OnPointerEnter");
			TriggerDrawer();
		}

		public void OnPointerDown( PointerEventData eventData )
		{
			//Debug.Log("OnPointerDown");
			TriggerDrawer();
		}


		void TriggerDrawer()
		{
			if ( m_IsAnimating ) return;

			m_IsOpening		= !m_IsOpening;
			m_IsAnimating	= true;


			float start     = m_IsOpening ?  m_Start     : m_Start-m_Offset;
			float finish    = m_IsOpening ? -m_Offset    : m_Offset;

			TweenAction_RectTransformSlide.Begin<TweenAction_RectTransformSlide>( gameObject, start, finish, m_EasingGraphManager.duration, m_Equation, onComplete );
		}

		void onComplete( bool result )
		{
			m_IsAnimating = false;
		}


		static public T Begin<T>( GameObject go, float start, float offset, EasingEquationsDouble.Equations easeEquation, EasingGraphManager easingGraphManager ) where T : EasingDrawer
		{
			T component = go.GetComponent<T>();			
			if ( null == component ) component = go.AddComponent<T>();

			component.m_EasingGraphManager	= easingGraphManager;
			component.m_Equation			= easeEquation;
			component.m_Start				= start;
			component.m_Offset				= offset;
			component.enabled				= true;

			return component;
		}
	}

}
