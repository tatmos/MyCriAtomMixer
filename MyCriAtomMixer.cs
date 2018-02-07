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
	private CriAtom atom;
	private CriAtomExAsr.BusAnalyzerInfo[] lBusInfoList = new CriAtomExAsr.BusAnalyzerInfo[8];
	private CriAtomExAsr.BusAnalyzerInfo[] lBusInfoDrawList = new CriAtomExAsr.BusAnalyzerInfo[8];
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
				progressForground = new Texture2D(2,2);
				progressForground.SetPixel(0, 0, new Color(1,1,1,0.5f));
				progressForground.SetPixel(0, 2, new Color(1,1,1,1f));
				
				progressForground.Apply();
				
				CriAtom.SetBusAnalyzer(true); // バス解析器を有効化for(int i = 0;i<8;i++){
				for(int i =0;i<8;i++){
					lBusInfoList[i] = CriAtom.GetBusAnalyzerInfo(i);
					
					lBusInfoDrawList[i] = CriAtom.GetBusAnalyzerInfo(i);
				}
			}
			
			Repaint ();
		}
	}
	
	void GetSource()
	{
		if (this.atom == null) {
			// ref : http://qiita.com/shin5734/items/fcf02aa84516dfad5d9c
			// Project & Sceneにある GameObject を持つ全オブジェクトを取得
			foreach(GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
			{
				string path = AssetDatabase.GetAssetOrScenePath(obj);
				
				string sceneExtension = ".unity";
				bool isExistInScene = Path.GetExtension(path).Equals(sceneExtension);
				if(isExistInScene){ 
					CriAtom tmpAtom = obj.GetComponent<CriAtom>();
					if(tmpAtom != null)
					{
						this.atom = tmpAtom;//シーン上のAtomSourceを借りる（再生用）
						break;
					}
					// シーンのオブジェクトを格納するリストに登録 
				}else{ 
					// プロジェクトのオブジェクトを格納するリストに登録 
				}
			}
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

	private void Reload()
	{
		this.GetSource();
		if(atom.dspBusSetting != ""){
			this.dspBusSetting = atom.dspBusSetting;
		} 
		
		CriAtomEx.AttachDspBusSetting(dspBusSetting); //バス変更
		CriAtom.SetBusAnalyzer(true); // バス解析器を有効化

		for(int i = 0;i<8;i++){
			busVolumes[i] = 1;
		}
	}
	
	int channnleNum = 8;
	int busNum = 1;
	float[] busVolumes = new float[] {1,1,1,1, 1,1,1,1};
	bool useBusVolumeGUI = false;

	private void GUIDspSettings()
	{
		//this.acfPath = EditorGUILayout.TextField("ACF File Path", this.acfPath, EditorStyles.label);
		
		EditorGUILayout.BeginHorizontal();
		this.dspBusSetting = EditorGUILayout.TextField("DSP Bus Setting", this.dspBusSetting);
		GUI.color = Color.green;
		if(GUILayout.Button("Reload"))
		{
			Reload();		
		}
		
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button(channnleNum +"ch")){
			if(channnleNum == 2){
				channnleNum = 8;
			} else {
				channnleNum = 2;
			}	
		}
		if(GUILayout.Button(busNum +"bus")){
			busNum *= 2;
			if(busNum > 8)busNum = 1;
		}
		if(GUILayout.Button("Use BusVolume")){
			if(useBusVolumeGUI)
			{
				useBusVolumeGUI = false;
			} else {
				useBusVolumeGUI = true;
			}
		}
		EditorGUILayout.EndHorizontal();

		if(useBusVolumeGUI){
			for(int i = 0;i<busNum;i++){
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Bus"+i+" Volume:" + busVolumes[i].ToString("f2") + " (" + getDb(busVolumes[i])+ ")");
				busVolumes[i] = GUILayout.HorizontalSlider( busVolumes[i],0,1);
				CriAtomExAsr.SetBusVolume(i,busVolumes[i]);
				EditorGUILayout.EndHorizontal();
			}
		}
		
		EditorGUILayout.Space();
		
		EditorGUILayout.BeginVertical();
		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();
		
		//CriAtomExAsr.BusAnalyzerInfo lBusInfo = CriAtom.GetBusAnalyzerInfo(0); //バス0
		//Debug.Log("level:" + lBusInfo.peakLevels[0].ToString()); //チャンネル0(Left)


		for(int i = 0;i<busNum;i++){

			for(int ch =0;ch<channnleNum;ch++){
				Rect r = GUILayoutUtility.GetLastRect();
				r.x += 8;
				r.width -= 16;
				if(useBusVolumeGUI){
					r.y = 22*busNum + 40 + (10*(ch)) + i*10*channnleNum;
				} else {
					r.y = 40 + (10*(ch)) + i*10*channnleNum;
				}
				r.height = 10;
				EditorGUILayout.BeginVertical();
				
				if(CriAtom.GetBusAnalyzerInfo(i).peakLevels[ch] != lBusInfoList[i].peakLevels[ch])
				{	
					lBusInfoDrawList[i].peakLevels[ch] = lBusInfoList[i].peakLevels[ch];
				}
				if(CriAtom.GetBusAnalyzerInfo(i).peakHoldLevels[ch] != lBusInfoList[i].peakHoldLevels[ch])
				{	
					lBusInfoDrawList[i].peakHoldLevels[ch] = lBusInfoList[i].peakHoldLevels[ch];
				}
				
				DrawProgress(new Vector2(r.x,r.y),
				             new Vector2(r.width,r.height),
				             lBusInfoDrawList[i].peakLevels[ch],
				             lBusInfoDrawList[i].peakHoldLevels[ch],"BUS"+i+" (" + ch + ") : "+this.getDb(lBusInfoDrawList[i].peakLevels[ch]),
				             (i % 2 == 1) ? Color.gray : Color.black
				             );
				
				
				lBusInfoDrawList[i].peakLevels[ch] = Mathf.Lerp( lBusInfoDrawList[i].peakLevels[ch],0,Time.deltaTime);
				//lBusInfoDrawList[i].peakHoldLevels[ch] = Mathf.Lerp( lBusInfoDrawList[i].peakHoldLevels[ch],0,Time.deltaTime);
				
				EditorGUILayout.EndVertical();
			}
			lBusInfoList[i] = CriAtom.GetBusAnalyzerInfo(i);	


		}


	}
	
	private string getDb(float volume)
	{
		float retValue = Mathf.Floor( 20.0f * Mathf.Log10(volume)*100f)/100f;
		if(retValue< -96){
			retValue = -96;
		}
		return string.Format("{0:#0.#0} dB",retValue);
	}
	
	private void DrawProgress(Vector2 location ,Vector2 size,float progress,float progressHold,string valueString,Color bgColor)
	{
		GUI.color = bgColor;//Color.gray;
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
