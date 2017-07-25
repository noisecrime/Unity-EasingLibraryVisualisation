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
using NoiseCrimeStudios.Demo.Easing.CurveCreation;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Reflection;
using System.Linq;
using EaseEquations = NoiseCrimeStudios.Core.Features.Easing.EasingEquationsDouble.Equations;


namespace NoiseCrimeStudios.Demo.Easing
{
	/// <summary>
	/// Manager for the entire Easing Library Visualisation Demo.
	/// Responsible for setting up and managing the scene as most aspects are dynamically created.
	/// </summary>
	/// <remarks>
	/// Not very optimised as all graphs are evaluated and rendered every frame regardless of whether they are visible or not.
	/// However since we are only interested in the visualisation this doesn't really matter.
	/// </remarks>
	public class EasingGraphManager : MonoBehaviour
	{
		[Header("Materials")]
		public  Material    m_UnlitColorMat;
		public  Material    m_UnlitPointerMat;

		[Header("Prefabs")]
		public  GameObject  m_TextPrefab;
		public  GameObject  m_DrawerPrefab;

		[Header("Transforms")]
		public  Transform   m_Labels;
		public  Transform   m_Drawers;

		[Header("UI Toggles")]
		public	Toggle		m_ShowEquationToggle;
		public	Toggle		m_ShowAnimationToggle;

		[Header("Grid View")]
		public  int         m_MaxDisplayColumns	= 8;
		public  int         m_MaxDisplayRows    = 8;
		public  int         m_GridSpacingH      = 32;
		public  int         m_GridSpacingV      = 48;
		public  float       m_ScrollSpeed       = 400f;
		
		[Header("AnimationCurveCache")]	
		public EasingAnimationCurveCache					m_EasingAnimationCurveCache;

		private	Material	m_LineAlt1Mat;
		private	Material	m_LineAlt2Mat;
		
		private Dictionary<EaseEquations, Mesh>				m_AnimCurveMeshes		= new Dictionary<EaseEquations, Mesh>();
		private Dictionary<EaseEquations, Mesh>				m_AnimCurvePointMeshes	= new Dictionary<EaseEquations, Mesh>();

		private Dictionary<EaseEquations, MethodInfo>		m_EaseMethodInfoList    = new Dictionary<EaseEquations, MethodInfo>();
		private Dictionary<EaseEquations, Mesh>				m_GraphMeshes           = new Dictionary<EaseEquations, Mesh>();
		private List<EaseEquations>							m_EaseEquationNames     = new List<EaseEquations>();

		private int     m_NumOfUsedEquations;
		private int     m_NumOfEaseEquations;

		private int     m_Rows					= 2;
		private int     m_MaxRows				= 2;
		private int     m_GraphWidth			= 0;
		private int     m_GraphHeight			= 0;

		private float   m_ScrollPos				= 0f;
		private float   m_ScrollMax				= 100f;
		private	float	m_OrgGridSpacingV		= 48;

		private int     m_DrawerWidth			= 192;		// Note: This is overriden at runtime based on screen width
		private bool    m_ShowDrawers			= false;
		private bool    m_ShowEquationGraphs	= true;
		private bool    m_ShowAnimationCurves   = false;
	
		private float   m_Duration;
		private float   m_Time;
		
		private Color   m_Gray          = Color.gray;
		private Color   m_White         = Color.white;
		private Color   m_Orange        = new Color( 1f, 0.5f, 0f, 0.85f);	// new Color( 206f/255f, 111f/255f, 40f/255f, 0.7f);
		private Color   m_Guides        = new Color( 0f ,1f ,0f, 0.5f);		// new Color( 1f ,1f ,1f, 0.75f);
				


		#region ACCESSORS
		public bool showDrawers			{ get { return m_ShowDrawers; } set { m_ShowDrawers = value; } }
	
		public bool showEquationGraphs	{ get { return m_ShowEquationGraphs; } set { m_ShowEquationGraphs = value; } }
		public bool showAnimationCurves	{ get { return m_ShowAnimationCurves; } set { m_ShowAnimationCurves = value; } }

		public float numDisplayColumns	{ get { return m_MaxDisplayColumns; } set { m_MaxDisplayColumns = ( int )value; } }
		public float numDisplayRows		{ get { return m_MaxDisplayRows; } set { m_MaxDisplayRows = ( int )value; } }
		public float scrollSpeed		{ get { return m_ScrollSpeed; } set { m_ScrollSpeed = value * 100f; } }
		public float duration			{ get { return m_Duration; } set { m_Duration = value; if ( m_Duration < 0.1f ) m_Duration = 0.1f; } }
		public float numOfUsedEquations { get { return m_NumOfUsedEquations; } }
		public float numOfEaseEquations { get { return m_NumOfEaseEquations; } }
		#endregion

		public void ToggleGraphCurves()
		{
			m_ShowEquationGraphs	= !showEquationGraphs;
			m_ShowAnimationCurves	= !m_ShowAnimationCurves;

			m_ShowEquationToggle.isOn	= m_ShowEquationGraphs;
			m_ShowAnimationToggle.isOn	= m_ShowAnimationCurves;
		}

		void Awake()
		{
			// For standalone, set resolution to non-defalt - useful for making videos.
			// Screen.SetResolution(888, 666, false );

			m_LineAlt1Mat		= new Material( m_UnlitColorMat );
			m_LineAlt1Mat.color	= new Color32(0, 68, 146, 255);

			m_LineAlt2Mat		= new Material( m_UnlitColorMat );
			m_LineAlt2Mat.color	= new Color32(255, 255, 255, 255);
		}

		void OnDestroy()
		{
			Destroy(m_LineAlt1Mat);
			Destroy(m_LineAlt2Mat);
		}


		void OnEnable()
		{
			m_Duration = 2.0f;

			BuildMethodCallsViaEnum();
			StartCoroutine( Cycle() );
		}

		void Update()
		{
			if( Input.GetKey( KeyCode.Escape ) ) Application.Quit();
			if ( Input.GetKey( KeyCode.DownArrow ) ) m_ScrollPos -= Time.deltaTime * m_ScrollSpeed;
			if ( Input.GetKey( KeyCode.UpArrow ) ) m_ScrollPos += Time.deltaTime * m_ScrollSpeed;
			m_ScrollPos += Input.mouseScrollDelta.y * m_ScrollSpeed / 8f;

			if ( m_ScrollPos < -m_ScrollMax ) m_ScrollPos = -m_ScrollMax;
			if ( m_ScrollPos > 0f ) m_ScrollPos = 0f;

			m_Labels.position = new Vector3( 0f, -m_ScrollPos, 0f );
		}

		IEnumerator Cycle()
		{
			while ( true )
			{
				// Pause
				m_Time = 0.0f;
				yield return new WaitForSeconds( 1f );

				// Forward
				while ( m_Time < m_Duration )
				{
					m_Time += Time.deltaTime;
					yield return null;
				}

				// Pause
				m_Time = m_Duration;
				yield return new WaitForSeconds( 1f );

				// Backwards
				while ( m_Time > 0.0 )
				{
					m_Time -= Time.deltaTime;
					yield return null;
				}
			}
		}

		public int GetClickedGraph()
		{
			float px = Input.mousePosition.x;
			float py = Input.mousePosition.y + m_ScrollPos;

			// Is mouse within graph
			for ( int x = 0; x < m_MaxDisplayColumns; x++ )
			{
				float left = (x*m_GraphWidth) + (x*m_GridSpacingH) + m_GridSpacingH;

				if ( px > left && px < left + m_GraphWidth )
				{
					for ( int y = 0; y < m_MaxRows; y++ )
					{
						float top =  Screen.height - ( (y*m_GraphHeight) + (y*m_GridSpacingV) + m_GridSpacingV); // - (int)m_ScrollPos);
																												 //	Debug.Log("py: " + py + " top: " + top + " height: " + ( top - m_GraphHeight) + " result: " + ( py <= top && py >= (top - m_GraphHeight)) + "  " + (y*m_MaxDisplayColumns + x));

						if ( py <= top && py >= ( top - m_GraphHeight ) )
						{
							return y * m_MaxDisplayColumns + x;
						}
					}
				}
			}

			return -1;
		}

		public void FocusOnGraph( int index)
		{
			CalculateGrid();
			m_ScrollPos = -( (index * m_GraphHeight) + ( index * m_GridSpacingV));
		}

		void CalculateGrid()
		{
			int oldWidth    = m_GraphWidth;
			int oldHeight   = m_GraphHeight;

			// Calcaulte Rows
			m_MaxRows = m_NumOfUsedEquations / m_MaxDisplayColumns;
			if ( m_NumOfUsedEquations % m_MaxDisplayColumns > 0 ) m_MaxRows++;
			m_Rows = m_MaxRows;
			if ( m_Rows > m_MaxDisplayRows ) m_Rows = m_MaxDisplayRows;

			// Are Drawers Visible
			m_DrawerWidth = m_ShowDrawers ? Screen.width / 10 : 0;
			
			// Calculate Graph dimensions
			m_GraphWidth  = ( Screen.width - m_DrawerWidth - ( m_GridSpacingH * ( m_MaxDisplayColumns + 1 ) ) ) / m_MaxDisplayColumns;
			
			// Adjust height to accout of bound exceeding graphs
			m_GridSpacingV = ( int )( m_OrgGridSpacingV + (m_GraphWidth * 0.14f));

			m_GraphHeight = ( Screen.height - ( m_GridSpacingV * ( m_Rows + 1 ) ) ) / m_Rows;
	
			// Limit Graph dimensions
			if ( m_GraphWidth < 64 ) m_GraphWidth = 64;
			

			// Only update Graph Meshes and Labels if something has changed.
			if ( oldWidth != m_GraphWidth || oldHeight != m_GraphHeight )
			{
				CalculateAnimCurveMeshes( m_GraphWidth, m_GraphHeight);
				CalculateGraphMesh( m_GraphWidth, m_GraphHeight );
				ConstructLabels();
				ConstructDrawers();

				// Calculate Scrolling
				m_ScrollPos = 0f;
				m_ScrollMax = ( m_MaxRows * m_GraphHeight ) + ( ( m_MaxRows + 1 ) * m_GridSpacingV ) + -Screen.height;

				// Debug.Log( "EasingGraphManager: CalculateGrid: m_ScrollPos: " + m_ScrollPos + "  m_ScrollMax: " + m_ScrollMax );
			}
		}


		void BuildMethodCallsViaEnum()
		{
			m_EaseMethodInfoList.Clear();
			m_EaseEquationNames.Clear();

			string[]    names   = Enum.GetNames( typeof(EaseEquations) );
			Type        source  = typeof(EasingEquationsDouble);
		
			foreach ( string name in names )
			{
				m_EaseMethodInfoList.Add( ( EaseEquations )Enum.Parse( typeof( EaseEquations ), name ), source.GetMethod( name ) );
			}

			m_EaseEquationNames = Enum.GetValues( typeof( EaseEquations ) ).Cast<EaseEquations>().ToList();

			// We excluded the linear Ease equation from the graph display as its not useful.
			m_NumOfUsedEquations = m_EaseMethodInfoList.Count - 1;
			m_NumOfEaseEquations = m_EaseMethodInfoList.Count;

			Debug.Log( "EasingGraphManager: Number of Used Easing Equations: " + m_NumOfUsedEquations + " Available: " + m_EaseMethodInfoList.Count );
		}
	
		

		/// <summary>
		/// Constructs a mesh per easing equation using linestrip topology.
		/// By building a mesh each time the graph width or height changes we ensure high performance over using GL.Lines to draw the graph each frame.
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		void CalculateGraphMesh( int width, int height )
		{
			foreach ( KeyValuePair<EaseEquations, Mesh> kvp in m_GraphMeshes )
			{
				DestroyImmediate( kvp.Value );
			}

			m_GraphMeshes.Clear();

			int graphWidth          = width - m_UnlitPointerMat.mainTexture.width;
			object[] methodParams   = new object[4] { 0.0, 0.0, (double)height, (double)graphWidth };
			Type supportEasing      = typeof(EasingEquationsDouble);

			foreach ( KeyValuePair<EaseEquations, MethodInfo> kvp in m_EaseMethodInfoList )
			{
				Mesh        tmpMesh = new Mesh();
				Vector3[]   points  = new Vector3[graphWidth+1];
				int[]       indices = new int[graphWidth+1];

				for ( int i = 0; i < graphWidth; i++ )
				{
					methodParams[ 0 ] = ( double )i;
					float y = (float)(double)kvp.Value.Invoke( supportEasing, methodParams);
					points[ i ] = new Vector3( i, y, 1 );
					indices[ i ] = i;
				}

				// Add final linestrip point
				points[ graphWidth ] = new Vector3( graphWidth, height, 1 );
				indices[ graphWidth ] = graphWidth;

				// Build the Mesh
				tmpMesh.vertices = points;
				tmpMesh.SetIndices( indices, MeshTopology.LineStrip, 0 );

				m_GraphMeshes.Add( kvp.Key, tmpMesh );
			}
		}

		void CalculateAnimCurveMeshes( int width, int height )
		{
			foreach ( KeyValuePair<EaseEquations, Mesh> kvp in m_AnimCurveMeshes )
			{
				DestroyImmediate( kvp.Value );
			}

			foreach ( KeyValuePair<EaseEquations, Mesh> kvp in m_AnimCurvePointMeshes )
			{
				DestroyImmediate( kvp.Value );
			}

			m_AnimCurvePointMeshes.Clear();
			m_AnimCurveMeshes.Clear();

			int graphWidth          = width - m_UnlitPointerMat.mainTexture.width;
			float factor			= 1f/graphWidth;
					
			for( int k =0; k < m_EasingAnimationCurveCache.m_AnimationCurves.Length; k++)
			{
				Mesh        tmpMeshLines	= new Mesh();
				Vector3[]   points			= new Vector3[graphWidth+1];
				int[]       indices			= new int[graphWidth+1];

				// Cache AnimationCurve
				AnimationCurve ac = m_EasingAnimationCurveCache.m_AnimationCurves[k];

				for ( int i = 0; i < graphWidth; i++ )
				{
					float y		= ac.Evaluate( i * factor ) * height ;				
					points[ i ] = new Vector3( i, y, 1 );
					indices[ i ] = i;
				}

				// Add final linestrip point
				points[ graphWidth ]	= new Vector3( graphWidth, height, 1 );
				indices[ graphWidth ]	= graphWidth;

				// Build the Mesh
				tmpMeshLines.vertices = points;
				tmpMeshLines.SetIndices( indices, MeshTopology.LineStrip, 0 );
				m_AnimCurveMeshes.Add( (EaseEquations)k, tmpMeshLines );


				Mesh	tmpMeshPoints	= new Mesh();
				Keyframe[] kf = ac.keys;

				points  = new Vector3[kf.Length];
				indices = new int[kf.Length];

				for ( int i = 0; i < kf.Length; i++ )
				{
					float y		= kf[i].value * height;	
					float x		= kf[i].time * graphWidth;			
					points[ i ] = new Vector3( x, y, 1 );
					indices[ i ] = i;
				}

				tmpMeshPoints.vertices = points;
				tmpMeshPoints.SetIndices( indices, MeshTopology.Points, 0 );
				m_AnimCurvePointMeshes.Add( (EaseEquations)k, tmpMeshPoints );
			}
		}


		void ConstructLabels()
		{
			List<GameObject> children = new List<GameObject>();
			foreach ( Transform child in m_Labels ) children.Add( child.gameObject );
			children.ForEach( child => DestroyImmediate( child ) ); // children.ForEach(child => Destroy(child));
			children.Clear();

			int index = 0; 
		//	int fontsize = Mathf.Min ( 40, (int)(9 * Screen.width/1024f * (m_GridSpacingV/m_OrgGridSpacingV) ) );
			int fontsize = Mathf.Max( 11, Mathf.Min ( 32, (int)(0.125f * m_GraphWidth ) ));
		
			for ( int y = 0; y < m_MaxRows; y++ )
			{
				for ( int x = 0; x < m_MaxDisplayColumns; x++ )
				{
					if ( index < m_NumOfUsedEquations )
					{
						float left  = (x*m_GraphWidth)  + (x*m_GridSpacingH) + m_GridSpacingH ;
						float top   = Screen.height - ( (y*m_GraphHeight) + (y*m_GridSpacingV) + m_GridSpacingV + m_GraphHeight );//- (m_GridSpacingV - m_OrgGridSpacingV));

						GameObject go = (GameObject) Instantiate( m_TextPrefab, new Vector3(left + m_GraphWidth/2 - m_UnlitPointerMat.mainTexture.width/2, top - m_GridSpacingV/3 , 0), Quaternion.identity);

						go.transform.SetParent( m_Labels, false );
						go.GetComponent<RectTransform>().sizeDelta = new Vector2( m_GraphWidth, fontsize + 10 );

						go.GetComponent<Text>().text = m_EaseEquationNames[ index++ ].ToString();
						go.GetComponent<Text>().fontSize = fontsize;
					}
				}
			}
		}


		void ConstructDrawers()
		{
			List<GameObject> children = new List<GameObject>();
			foreach ( Transform child in m_Drawers ) children.Add( child.gameObject );
			children.ForEach( child => DestroyImmediate( child ) );
			children.Clear();

			if ( !m_ShowDrawers ) return;

			Color32 odd         = new Color32(93, 191, 191, 230);
			Color32 even        = new Color32(64, 139, 139, 230);

			float drawerHeight  = Screen.height / (float)m_NumOfUsedEquations;
			int fontSize        = (int)(drawerHeight * 0.7f);

			for ( int y = 0; y < m_NumOfUsedEquations; y++ )
			{
				GameObject go   = (GameObject) Instantiate( m_DrawerPrefab, new Vector3(m_DrawerWidth-32, -y*drawerHeight, 0), Quaternion.identity);

				go.transform.SetParent( m_Drawers, false );
				go.GetComponent<RectTransform>().sizeDelta = new Vector2( m_DrawerWidth, drawerHeight );
				go.GetComponent<Image>().color = ( y % 2 ) == 0 ? even : odd;
				go.transform.GetChild( 0 ).GetComponent<RectTransform>().sizeDelta = new Vector2( m_DrawerWidth, drawerHeight );
				go.transform.GetChild( 0 ).GetComponent<Text>().text = m_EaseEquationNames[ y ].ToString();
				go.transform.GetChild( 0 ).GetComponent<Text>().fontSize = fontSize;

				EasingDrawer.Begin<EasingDrawer>( go, go.transform.position.x, m_DrawerWidth - 32, m_EaseEquationNames[ y ], this );
			}
		}


		#region DRAW GRAPHS
		void OnRenderObject()
		{
			CalculateGrid();

			GL.PushMatrix();
			GL.LoadPixelMatrix();

			int index = 0;

			for ( int y = 0; y < m_MaxRows; y++ )
			{
				for ( int x = 0; x < m_MaxDisplayColumns; x++ )
				{
					if ( index < m_NumOfUsedEquations )
					{
						DrawGraph(	( x * m_GraphWidth ) + ( x * m_GridSpacingH ) + m_GridSpacingH,
									( y * m_GraphHeight ) + ( y * m_GridSpacingV ) + m_GridSpacingV + ( int )m_ScrollPos,
									m_GraphWidth,
									m_GraphHeight, m_EaseEquationNames[ index++ ] );
					}
				}
			}

			GL.PopMatrix();
		}



		void DrawGraph( int x, int y, int width, int height, EaseEquations easeEquation )
		{
			Type supportEasing      = typeof(EasingEquationsDouble);

			// Note: Y is flipped 
			float offsetFlipY       = Screen.height - y - height ;
			float graphWidth        = width - m_UnlitPointerMat.mainTexture.width;

			// Set Up MethodParams for Pointer
			object[] methodParams   = new object[4] { m_Time, 0.0, 1.0, m_Duration };
			float result            = (float)(double)m_EaseMethodInfoList[easeEquation].Invoke( supportEasing, methodParams);
			float pointerY          = offsetFlipY + (float)height * result;
			float linearY           = offsetFlipY + (float)height * (float) (m_Time/m_Duration);


			// Set up MethodParams for graph
			methodParams[ 2 ] = ( double )height;
			methodParams[ 3 ] = ( double )graphWidth;


			m_UnlitColorMat.SetPass( 0 );

			// Background box
			GL.Begin( GL.QUADS );
			GL.Color( m_Gray );
			GL.Vertex3( x, offsetFlipY + height, 0 );
			GL.Vertex3( x + graphWidth, offsetFlipY + height, 0 );
			GL.Vertex3( x + graphWidth, offsetFlipY, 0 );
			GL.Vertex3( x, offsetFlipY, 0 );
			GL.End();


			// Top/Bottom/Linear Guide Lines
			GL.Begin( GL.LINES );
			GL.Color( m_Guides );
			GL.Vertex3( x, offsetFlipY, 0 );
			GL.Vertex3( x + graphWidth, offsetFlipY, 0 );
			GL.Vertex3( x, offsetFlipY + height, 0 );
			GL.Vertex3( x + graphWidth, offsetFlipY + height, 0 );
			GL.Color( m_Orange );
			GL.Vertex3( x, offsetFlipY, 0 );
			GL.Vertex3( x + graphWidth, offsetFlipY + height, 0 );
			GL.End();


			if ( showEquationGraphs ) // Draw Graph Mesh
			{
				m_UnlitColorMat.SetPass( 0 );
				Graphics.DrawMeshNow( m_GraphMeshes[ easeEquation ], new Vector3( x, offsetFlipY, 0 ), Quaternion.identity );
			}

			if ( showAnimationCurves ) // Draw AnimationCurve Mesh
			{
				m_LineAlt1Mat.SetPass( 0 );
				Graphics.DrawMeshNow( m_AnimCurveMeshes[ easeEquation ], new Vector3( x, offsetFlipY, 0 ), Quaternion.identity );

				m_LineAlt2Mat.SetPass( 0 );
				Graphics.DrawMeshNow( m_AnimCurvePointMeshes[ easeEquation ], new Vector3( x, offsetFlipY, 0 ), Quaternion.identity );				
			}

			// Draw Pointer Texture
			m_UnlitPointerMat.SetPass( 0 );

			GL.Begin( GL.QUADS );

			GL.Color( m_Orange );
			GL.TexCoord( new Vector3( 1, 1, 0 ) );
			GL.Vertex3( x + graphWidth + m_UnlitPointerMat.mainTexture.width, linearY - m_UnlitPointerMat.mainTexture.height / 2, 0 );
			GL.TexCoord( new Vector3( 0, 1, 0 ) );
			GL.Vertex3( x + graphWidth, linearY - m_UnlitPointerMat.mainTexture.height / 2, 0 );
			GL.TexCoord( new Vector3( 0, 0, 0 ) );
			GL.Vertex3( x + graphWidth, linearY + m_UnlitPointerMat.mainTexture.height / 2, 0 );
			GL.TexCoord( new Vector3( 1, 0, 0 ) );
			GL.Vertex3( x + graphWidth + m_UnlitPointerMat.mainTexture.width, linearY + m_UnlitPointerMat.mainTexture.height / 2, 0 );

			GL.Color( m_White );
			GL.TexCoord( new Vector3( 1, 1, 0 ) );
			GL.Vertex3( x + graphWidth + m_UnlitPointerMat.mainTexture.width, pointerY - m_UnlitPointerMat.mainTexture.height / 2, 0 );
			GL.TexCoord( new Vector3( 0, 1, 0 ) );
			GL.Vertex3( x + graphWidth, pointerY - m_UnlitPointerMat.mainTexture.height / 2, 0 );
			GL.TexCoord( new Vector3( 0, 0, 0 ) );
			GL.Vertex3( x + graphWidth, pointerY + m_UnlitPointerMat.mainTexture.height / 2, 0 );
			GL.TexCoord( new Vector3( 1, 0, 0 ) );
			GL.Vertex3( x + graphWidth + m_UnlitPointerMat.mainTexture.width, pointerY + m_UnlitPointerMat.mainTexture.height / 2, 0 );

			GL.End();
		}

		#endregion
	}

}
