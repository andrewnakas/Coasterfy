/*
 * Copyright 2014 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using UnityEngine;

/// <summary>
/// FPS counter.
/// </summary>
public class FPSCounter : MonoBehaviour {
	
	public float m_updateFrequency = 1.0f;

    public string m_FPSText;
	private int m_currentFPS;
	private int m_framesSinceUpdate;
	private float m_accumulation;
	private float m_currentTime;
    private string m_currentLibrary = string.Empty;

    private Rect m_button;
    private Rect m_label;
	
	// Use this for initialization
	void Start () 
	{
		m_currentFPS = 0;
		m_framesSinceUpdate = 0;
		m_currentTime = 0.0f;
		m_FPSText = "Current FPS = Calculating";
		Application.targetFrameRate = 30;
        m_button = new Rect(Screen.width * 0.15f - 50, Screen.height * 0.45f - 25, 150.0f, 50.0f);
        m_label = new Rect(Screen.width * 0.025f - 50, Screen.height * 0.96f - 25, 600.0f, 50.0f);
        //m_currentLibrary = Tango.Utilities.GetVersionString();
	}
	
	// Update is called once per frame
	void Update () 
	{
		m_currentTime += Time.deltaTime;
		++m_framesSinceUpdate;
		m_accumulation += Time.timeScale / Time.deltaTime;
		if(m_currentTime >= m_updateFrequency)
		{
			m_currentFPS = (int)(m_accumulation/m_framesSinceUpdate);
			m_currentTime = 0.0f;
			m_framesSinceUpdate = 0;
			m_accumulation = 0.0f;
			m_FPSText = "Current FPS = " + m_currentFPS;
		}
	}
	
	void OnGUI()
	{
        GUI.Label(m_label,
                  "<size=20>" + m_FPSText + "</size>");
	}
}
