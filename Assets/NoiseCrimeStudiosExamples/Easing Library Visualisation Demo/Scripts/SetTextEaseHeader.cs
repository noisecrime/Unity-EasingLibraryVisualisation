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
	/// Dyanmically updates the Text component with the name of the Ease Equation being used by the slider.
	/// </summary>
	public class SetTextEaseHeader : MonoBehaviour
	{
		
		public 	Slider					m_SliderOrNull;
		private float					m_Value;

		public float value
		{
			get { return m_Value; }
			set { m_Value = value; SetText(value); }
		}
		
		void Start()
		{
			if( null == m_SliderOrNull)
				SetText ( GetComponentInParent<Slider>().value );
			else
				SetText ( m_SliderOrNull.value );
		}

		void SetText( float value )
		{
			GetComponent<Text>().text = ( (EasingEquationsDouble.Equations)(int)value ).ToString();
		}
	}
}
