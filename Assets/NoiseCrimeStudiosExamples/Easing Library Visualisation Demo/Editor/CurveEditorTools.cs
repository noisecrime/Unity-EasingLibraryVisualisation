
// CurveEditorTools.cs
/******************** 
 * 
 * Utility window to Add / Remove / Manage additional presets in the AnimationCurve-
 * Editorwindow of the Unity3D editor.
 * 
 * This is an editor script and should be placed in a subfolder called "Editor".
 * 
 * Written 2013.02.14 by Bunny83 for the UnityAnswers question:
 * http://answers.unity3d.com/questions/37664/editorguicurvefield-presets.html
 *
 * Usage:
 * To open the window select "CurveEditorTools" from the "Tools" menu inside Unity
 * If you don't have a "Tools" menu, you might need to open any other menu to make it appear
 * for the first time. If the menu still won't show up, check for any compiler errors and
 * make sure you placed the script in an "Editor" folder and named the file "CurveEditorTools.cs".
 *
 * This script uses reflection to access internal classes of the UnityEditor. Keep in mind that
 * those classes aren't ment to be used by extensions and might get changed by a future Update
 * of Unity. This could break the functionality of the extension or might even cause other problems.
 * I tried to catch as much errors as possible, but it's still a minor risk.
 *
 ********************/
 
using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
 
public class CurveEditorWindowWrapper
{
    private static Type m_CurveEditorWindowType = null;
    private static FieldInfo m_SharedInstanceField = null;
    private static FieldInfo m_CurveField = null;
    private static FieldInfo m_PresetsField = null;
 
    static CurveEditorWindowWrapper()
    {
        var assembly = Assembly.GetAssembly(typeof(EditorWindow));
        var types = assembly.GetTypes();

        foreach(var T in types)
        {
            if (T.Name == "CurveEditorWindow")  m_CurveEditorWindowType = T;            
        }

        if (m_CurveEditorWindowType == null) throw new Exception("Can't get CurveEditorWindow type. Maybe it has been removed in a newer version of Unity");
 
        m_SharedInstanceField  = m_CurveEditorWindowType.GetField("s_SharedCurveEditor", BindingFlags.NonPublic | BindingFlags.Static);
        if (m_SharedInstanceField == null) throw new Exception("Can't get static var 's_SharedCurveEditor' of CurveEditorWindow. Maybe it has been removed in a newer version of Unity");

        m_CurveField = m_CurveEditorWindowType.GetField("m_Curve", BindingFlags.NonPublic | BindingFlags.Instance);
		if (m_CurveField == null) throw new Exception("Can't get var 'm_Curve' of CurveEditorWindow. Maybe it has been removed in a newer version of Unity");

		m_PresetsField = m_CurveEditorWindowType.GetField("m_Presets", BindingFlags.NonPublic | BindingFlags.Instance);
        if (m_PresetsField == null) throw new Exception("Can't get var 'm_PresetsField' of CurveEditorWindow. Maybe it has been removed in a newer version of Unity");

	/*	m_CurvePresetsField = m_CurveEditorWindowType.GetField("m_CurvePresets", BindingFlags.NonPublic | BindingFlags.Instance);
        if (m_CurvePresetsField == null) throw new Exception("Can't get var 'm_CurvePresets' of CurveEditorWindow. Maybe it has been removed in a newer version of Unity");
		*/
    }
 
    public static EditorWindow GetCurrentCurveEditor()
    {
        return (EditorWindow)m_SharedInstanceField.GetValue(null);
    }
 
    public static AnimationCurve GetCurrentAnimationCurve()
    {
        var CEW = GetCurrentCurveEditor();
        if (CEW == null)
            return null;
        return (AnimationCurve)m_CurveField.GetValue(CEW);
    }
 
    public static AnimationCurve[] Presets
    {
        get
        {
            var CEW = GetCurrentCurveEditor();
            if (CEW == null) return null;
            return (AnimationCurve[])m_PresetsField.GetValue(CEW);
        }
 
        set
        {
            var CEW = GetCurrentCurveEditor();
            if (CEW == null) return;
            m_PresetsField.SetValue(CEW,value);
        }
    }
}
 
public class AnimationCurvePreset
{
    public const string m_ItemSeperator = "|";
    public const string m_SubItemSeperator = "^";
    public string name;
    public AnimationCurve curve;
    public bool active = true;
 
    public AnimationCurvePreset(string aName, AnimationCurve aCurve)
    {
        name = aName.Replace(m_ItemSeperator,"").Replace(m_SubItemSeperator,"");
        curve = aCurve;
    }
 
    public AnimationCurvePreset(string aSerializedData)
    {
        var tmp = aSerializedData.Split(m_SubItemSeperator[0]);
        int index = 0;
        name = tmp[index++];
        active = bool.Parse(tmp[index++]);
        int keyFrameCount = int.Parse(tmp[index++]);
        Keyframe[] keyframes = new Keyframe[keyFrameCount];
        for (int i = 0; i < keyFrameCount; i++)
        {
            float time = float.Parse(tmp[index++]);
            float val = float.Parse(tmp[index++]);
            float inTan = float.Parse(tmp[index++]);
            float outTan = float.Parse(tmp[index++]);
            keyframes[i] = new Keyframe(time,val, inTan, outTan);
        }
        curve = new AnimationCurve(keyframes);
    }
 
    public string Serialize()
    {
        string Data = name;
        Data += m_SubItemSeperator + active;
        Data += m_SubItemSeperator + curve.keys.Length;
        foreach(var K in curve.keys)
        {
            Data += m_SubItemSeperator + K.time;
            Data += m_SubItemSeperator + K.value;
            Data += m_SubItemSeperator + K.inTangent;
            Data += m_SubItemSeperator + K.outTangent;
        }
        return Data;
    }
}
 
 
public class CurveEditorTools : EditorWindow
{
    [MenuItem("Tools/CurveEditorTools")]
    public static void Init()
    {
        var win = GetWindow<CurveEditorTools>();
		try
		{
			CurveEditorWindowWrapper.GetCurrentCurveEditor();
		}
		catch ( Exception e )
        {
            win.Close();
			Debug.Log("error: " + e);
        }
    }
 
    AnimationCurve m_Curve = null;
    List<AnimationCurvePreset> m_LocalPresets = new List<AnimationCurvePreset>();
    List<AnimationCurvePreset> m_DeleteList = new List<AnimationCurvePreset>();
    string m_NewCurveName = "NewCurve";
    bool m_AutoSet = false;
    Vector2 m_ScrollPos = Vector2.zero;
 
    void OnEnable()
    {   
        EditorApplication.update += OnUpdate;
        LoadSettings();
    }
 
    void OnDisable()
    {
        EditorApplication.update -= OnUpdate;
    }
 
    bool CompareCurves(AnimationCurve C1, AnimationCurve C2)
    {
        if ((C1 == C2)                              )            return true;
        if (C1 == null || C2 == null)                            return false;
        if (C1.keys.Length != C2.keys.Length)                    return false;
        for (int i = 0; i < C1.keys.Length; i++)
        {
            if (C1.keys[i].inTangent != C2.keys[i].inTangent)    return false;
            if (C1.keys[i].outTangent != C2.keys[i].outTangent)  return false;
            if (C1.keys[i].time != C2.keys[i].time)              return false;
            if (C1.keys[i].value != C2.keys[i].value)            return false;
        }
        return true;
    }
 
    void OnUpdate()
    {
        try
        {
            var curve = CurveEditorWindowWrapper.GetCurrentAnimationCurve();
            if (curve != null)
            {
                if (!CompareCurves(curve, m_Curve))
                {
                    m_Curve = curve;
                    Repaint();
                }
            }
            if (m_AutoSet)
            {
                var tmp = new List<AnimationCurve>();
                foreach (var P in m_LocalPresets)
                {
                    if (P.active)
                        tmp.Add(P.curve);
                }
                CurveEditorWindowWrapper.Presets = tmp.ToArray();
            }
        }
        catch(Exception e)
        {
            Debug.LogError("CurveEditorTools: Error: " + e.Message);
            Debug.LogWarning("CurveEditorTools: Something went wrong during the Update callback, window closed!");
            Close();
        }
    }
 
 
    void OnGUI()
    {
        Color oldColor = GUI.color;
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical("", "box", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
        GUILayout.Label("Last Curve");
        Rect R = GUILayoutUtility.GetRect(300,300,100,100,GUILayout.Width(200));
        GUI.enabled = m_Curve != null;
        if (GUI.GetNameOfFocusedControl() == "PresetName" && m_NewCurveName != null && m_Curve != null)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                m_LocalPresets.Add(new AnimationCurvePreset(m_NewCurveName, m_Curve));
                SaveSettings();
            }
        }
 
        GUI.SetNextControlName("PresetName");
        m_NewCurveName = GUILayout.TextField(m_NewCurveName);
        if (m_NewCurveName != "" && GUILayout.Button("Save Curve As\n"+m_NewCurveName))
        {
            m_LocalPresets.Add(new AnimationCurvePreset(m_NewCurveName, m_Curve));
            SaveSettings();
            m_NewCurveName = "";
        }
        GUI.enabled = true;
        if (m_Curve != null)
        {
            EditorGUIUtility.DrawCurveSwatch(R, m_Curve, null, Color.green, Color.black);
        }
        else
        {
            GUI.Label(R,"No Curve yet","box");
        }
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUI.color = (m_AutoSet)?Color.green:Color.red;
        m_AutoSet = GUILayout.Toggle(m_AutoSet, "Overwrite Preset array", "Button");
        GUI.color = oldColor;
        m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos);
        GUILayout.BeginHorizontal("","box");
 
        foreach (var P in m_LocalPresets)
        {
            GUI.color = (P.active)?Color.green:oldColor;
 
            GUILayout.BeginVertical("", "box", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            GUILayout.Label(P.name);
            Rect R2 = GUILayoutUtility.GetRect(120, 120, 80, 80, GUILayout.Width(120));
            EditorGUIUtility.DrawCurveSwatch(R2, P.curve, null, Color.green, Color.black);
            GUILayout.BeginHorizontal();
            P.active = GUILayout.Toggle(P.active,"On","Button");
            if (GUILayout.Button("delete"))
            {
                if (EditorUtility.DisplayDialog("Delete Preset?", "Are you sure you want to delete the preset named '"+P.name+"' ?", "ok", "cancel"))
                {
                    m_DeleteList.Add(P);
                }
            }
            GUILayout.EndHorizontal();
 
            GUILayout.EndVertical();
        }
        GUI.color = oldColor;
        if (GUI.changed)
            SaveSettings();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        if (m_DeleteList.Count > 0 && Event.current.type == EventType.Repaint)
        {
            foreach (var P in m_DeleteList)
            {
                m_LocalPresets.Remove(P);
            }
            m_DeleteList.Clear();
            Repaint();
            SaveSettings();
        }
    }
 
    void SaveSettings()
    {
        string Data = "";
        foreach (var P in m_LocalPresets)
        {
            Data += P.Serialize() + AnimationCurvePreset.m_ItemSeperator;
        }
        Data = Data.Remove(Data.Length-1);
        EditorPrefs.SetString("CustomCurveEditorPresets", Data);
        EditorPrefs.SetBool("CustomCurveEditor_AutoSet", m_AutoSet);
    }
 
    void LoadSettings()
    {
        m_AutoSet = EditorPrefs.GetBool("CustomCurveEditor_AutoSet", false);
        string Data = EditorPrefs.GetString("CustomCurveEditorPresets", "");
        if (Data != "")
        {
            m_LocalPresets.Clear();
            var tmp = Data.Split(AnimationCurvePreset.m_ItemSeperator[0]);
            foreach (var S in tmp)
            {
                m_LocalPresets.Add(new AnimationCurvePreset(S));
            }
        }
    }
}
