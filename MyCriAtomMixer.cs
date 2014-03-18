using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class MyCriAtomMixer : EditorWindow {
	#region Variables
	private Vector2 scrollPos;
	private Vector2 scrollPos_Window;
	private Rect windowRect = new Rect(10, 10, 100, 100);
	private bool scaling = true;
	private Texture2D progressBackground;
	private Texture2D progressForground;
	// Public
	public string dspBusSetting = "DspBusSetting_0";
	#endregion
	[MenuItem("CRI/My/Open CRI Atom Mixer ...")]
	static void OpenWindow()
	{
		EditorWindow.GetWindow<MyCriAtomMixer>(false, "CRI Atom Mixer");
	}

	static MyCriAtomMixer()
	{
		//EditorApplication.update += Update;

	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (/*EditorApplication.isCompiling && */EditorApplication.isPlaying)
		{
			if(progressBackground == null){
				progressBackground = new Texture2D(16,16);
				progressForground = new Texture2D(16,16);
			}

			Repaint ();
		}
	}
	private void ScalingWindow(int windowID)
	{
		GUILayout.Box("", GUILayout.Width(20), GUILayout.Height(20));
		if (Event.current.type == EventType.MouseUp)
			this.scaling = false;
		else if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
			this.scaling = true;
		
		if (this.scaling)
			this.windowRect = new Rect(windowRect.x, windowRect.y, windowRect.width + Event.current.delta.x, windowRect.height + Event.current.delta.y);
		
	}

	private void OnGUI()
	{
		this.windowRect = GUILayout.Window(0, windowRect, ScalingWindow, "resizeable", GUILayout.MinHeight(80), GUILayout.MaxHeight(200));

		this.scrollPos_Window = GUILayout.BeginScrollView(this.scrollPos_Window);
		{
			if (/*EditorApplication.isCompiling && */EditorApplication.isPlaying)
			{
				GUIDspSettings();
			}
		}
		GUILayout.EndScrollView();
	}

	private void GUIDspSettings()
	{
		//this.acfPath = EditorGUILayout.TextField("ACF File Path", this.acfPath, EditorStyles.label);
		this.dspBusSetting = EditorGUILayout.TextField("DSP Bus Setting", this.dspBusSetting, EditorStyles.label);
		
		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical();
		GUILayout.Space(32.0f);
		EditorGUILayout.EndVertical();

		CriAtomEx.AttachDspBusSetting(dspBusSetting); //バス変更
		CriAtom.SetBusAnalyzer(true); // バス解析器を有効化

		//CriAtomExAsr.BusAnalyzerInfo lBusInfo = CriAtom.GetBusAnalyzerInfo(0); //バス0
		//Debug.Log("level:" + lBusInfo.peakLevels[0].ToString()); //チャンネル0(Left)

		for(int i = 0;i<8;i++){
			for(int ch =0;ch<2;ch++){
				Rect r = GUILayoutUtility.GetLastRect();
				r.x += 8;
				r.width -= 16;
				r.y = 20 + 24*i+10*ch;
				r.height = 12;
				EditorGUILayout.BeginVertical();
				//GUILayout.Space(24.0f);
				CriAtomExAsr.BusAnalyzerInfo lBusInfo = CriAtom.GetBusAnalyzerInfo(i);
				//EditorGUI.ProgressBar(r, lBusInfo.rmsLevels[ch], "BUS"+i+":"+this.getDb(lBusInfo.rmsLevels[ch]));
				DrawProgress(new Vector2(r.x,r.y),new Vector2(r.width,r.height),lBusInfo.peakLevels[ch],lBusInfo.peakHoldLevels[ch],"BUS"+i+" : "+this.getDb(lBusInfo.rmsLevels[ch]));
				EditorGUILayout.EndVertical();
			}
		}
	}

	private string getDb(float volume)
	{
		float retValue = Mathf.Floor( 20.0f * Mathf.Log10(volume)*100f)/100f;
		if(retValue< -96){
			retValue = -96;
		}
		return string.Format("{0:##.#0} dB",retValue);
	}

	private void DrawProgress(Vector2 location ,Vector2 size,float progress,float progressHold,string valueString)
	{
		GUI.color = Color.gray;
		GUI.DrawTexture(new Rect(location.x, location.y, size.x, size.y), progressBackground);
		if(progress > 1){
			GUI.color = Color.red;
		} else {
			GUI.color = Color.green;
		}
		EditorGUI.DrawTextureAlpha(new Rect(location.x, location.y, size.x * progress, size.y), progressForground); 
		EditorGUI.DrawTextureAlpha(new Rect(size.x * progressHold-1f, location.y, 2f, size.y), progressForground); 
		//EditorGUI.DrawTextureAlpha
		GUI.color = Color.white;
		EditorGUI.DropShadowLabel(new Rect(location.x, location.y, size.x, size.y), valueString); 
	}
}
