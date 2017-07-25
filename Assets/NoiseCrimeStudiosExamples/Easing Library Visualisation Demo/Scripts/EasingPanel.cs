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
using UnityEngine.UI;
using NoiseCrimeStudios.Core.Features.Easing;

namespace NoiseCrimeStudios.Demo.Easing
{
	/// <summary>
	/// Left screen slide-in panel interface for controlling various aspects of the demo.
	/// </summary>
	public class EasingPanel : MonoBehaviour
	{
		public  RectTransform               m_PanelRectTransform;
		public  GameObject                  m_PanelLinearGO;

		public  Slider                      m_PanelEaseEq;
		public  Slider                      m_SliderColumn;

		public  Text                        m_PanelNumEquations;
		public  float                       m_Duration				= 2f;
		public	bool						m_SlideByEaseEquation	= true;

		private RectTransform               m_RectTransform;
		private bool                        m_IsActive				= false;
		private bool                        m_IsAnimating			= false;

		private float                       m_EaseEquationIndex		= 0;
		private float                       m_SlideWidth			= 1f;

		private EasingGraphManager          m_EasingGraphManager;

		private float						m_LastNumGridDisplay = 8f;

		public float easeEquationIndex
		{
			get { return m_EaseEquationIndex; }
			set
			{
				if ( null != m_EasingGraphManager && value >= 0 && value < m_EasingGraphManager.numOfEaseEquations )
				{
					m_PanelEaseEq.value = value;
					m_EaseEquationIndex = value;
				}
			}
		}

		public float duration
		{
			get { return m_Duration; }
			set { m_Duration = value; if ( m_Duration < 0.1f ) m_Duration = 0.1f; }
		}

		public bool SlideByEaseEquation
		{
			get { return m_SlideByEaseEquation; }
			set { m_SlideByEaseEquation = value;  }
		}

		void Start()
		{
			m_EasingGraphManager = FindObjectOfType<EasingGraphManager>();

			m_SlideWidth = m_PanelRectTransform.sizeDelta.x;
			m_RectTransform = GetComponent<RectTransform>();

			easeEquationIndex = m_PanelEaseEq.value;
			m_PanelNumEquations.text = m_EasingGraphManager.numOfUsedEquations.ToString();
		}

		void Update()
		{
			if ( Input.GetMouseButtonDown( 0 ) && null != m_EasingGraphManager && !( m_IsActive && Input.mousePosition.x < m_RectTransform.sizeDelta.x ) ) easeEquationIndex = ( float )m_EasingGraphManager.GetClickedGraph();
			if ( Input.GetMouseButtonDown( 1 ) && null != m_EasingGraphManager && !( m_IsActive && Input.mousePosition.x < m_RectTransform.sizeDelta.x ) )
			{
				int index = m_EasingGraphManager.GetClickedGraph();
				
				if ( m_EasingGraphManager.numDisplayColumns == 1 && m_EasingGraphManager.numDisplayRows == 1 )
				{
					m_SliderColumn.value	= m_LastNumGridDisplay;
					// m_SliderRow.value		= m_LastNumGridDisplay;
				}
				else
				{
					m_LastNumGridDisplay	= m_SliderColumn.value;
					m_SliderColumn.value	= 1f;
				//	m_SliderRow.value		= 1f;					
					m_EasingGraphManager.FocusOnGraph( index );
				}
			}
		}

		public void TogglePanel()
		{
			if ( m_IsAnimating ) return;

			m_IsActive = !m_IsActive;
			m_IsAnimating = true;
			
			float start     = m_IsActive ? -m_SlideWidth : 0f;
			float finish    = m_IsActive ?  m_SlideWidth : -m_SlideWidth;
			
			TweenActionEase_RectTransformSlide.Begin<TweenActionEase_RectTransformSlide>( m_PanelLinearGO, start, finish, m_Duration, EasingEquationsDouble.Equations.Linear, onComplete );

			if ( m_SlideByEaseEquation )
				TweenActionEase_RectTransformSlide.Begin<TweenActionEase_RectTransformSlide>( gameObject, start, finish, m_Duration, ( EasingEquationsDouble.Equations )( int )m_EaseEquationIndex, onComplete );
			else
				TweenActionCurve_RectTransformSlide.Begin<TweenActionCurve_RectTransformSlide>( gameObject, start, finish, m_Duration, m_EasingGraphManager.m_EasingAnimationCurveCache.m_AnimationCurves[(int)m_EaseEquationIndex], onComplete );			
		}


		void onComplete( bool result )
		{
			m_IsAnimating = false;
		}
	}
}
