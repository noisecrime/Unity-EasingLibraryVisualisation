using UnityEngine;
using UnityEngine.UI;

namespace NoiseCrime.UGUI.Widgets
{
	public class SetTextValue : MonoBehaviour
	{
		public	bool	m_ShowAsInt = true;

		private float 	m_Value = 0;
		private string	m_Format;


		public float value
		{
			get { return m_Value; }
			set { m_Value = value; GetComponent<Text>().text = m_Value.ToString(m_Format); }
		}

		void Start()
		{
			m_Format = m_ShowAsInt ? "F0" : "F2";
			GetComponent<Text>().text = GetComponentInParent<Slider>().value.ToString(m_Format);
		}
	}
}
