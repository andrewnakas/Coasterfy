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
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using UnityEngine;
using Tango;

/// <summary>
/// Point cloud visualize using depth frame API.
/// </summary>
public class Pointcloud : DepthListener
{

    [HideInInspector]
    public float m_overallZ = 0.0f;
    [HideInInspector]
    public int m_pointsCount = 0;

	public bool m_useADF = false;

    // Some const value.
    private const int DEPTH_BUFFER_WIDTH = 320;
    private const int DEPTH_BUFFER_HEIGHT = 180;
    private const float MILLIMETER_TO_METER = 0.001f;
    private const float INCH_TO_METER = 0.0254f;
    private const int VERT_COUNT = 61440;
    private const int FOCUS_LENGTH = 312;//half of 624.
    
    // m_vertices will be assigned to this mesh.
    private Mesh m_mesh;
    private MeshCollider m_meshCollider;
    
    // Mesh data.
    private Vector3[] m_vertices;
    private int[] m_triangles;
    private bool m_isDirty;
    private float m_timeSinceLastDepthFrame = 0.0f;
    private int m_numberOfDepthSamples = 0;
    private float m_previousDepthDeltaTime = 0.0f;

	private TangoApplication m_tangoApplication;

    /// <summary>
    /// Use this for initialization.
    /// </summary>
    public void Start() 
    {
        // get the reference of mesh
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        if (mf == null) 
        {
            MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
            meshFilter.mesh = m_mesh = new Mesh();
            MeshRenderer renderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            renderer.material.shader = Shader.Find("Mobile/Unlit (Supports Lightmap)");
        } 
        else 
        {
            m_mesh = mf.mesh;
        }
        m_isDirty = false;
        _CreateMesh();
        transform.localScale = new Vector3(transform.localScale.x,
                                           transform.localScale.y * -1.0f,
                                           transform.localScale.z);

		m_tangoApplication = FindObjectOfType<TangoApplication>();
		
		if(m_tangoApplication != null)
		{
			// Request Tango permissions
			m_tangoApplication.RequestNecessaryPermissions();
			
			if(TangoApplication.HasGrantedPermissions())
			{
				m_tangoApplication.InitApplication();
				
				if(m_useADF)
				{
					// Query the full adf list.
					PoseProvider.RefreshADFList();
					// loading last recorded ADF
					string uuid = PoseProvider.GetLatestADFUUID().GetStringDataUUID();
					m_tangoApplication.InitProviders(uuid);
				}
				else
				{
					m_tangoApplication.InitProviders(string.Empty);
				}
				
				m_tangoApplication.ConnectToService();
			}
			else
			{
				UnityEngine.Debug.Log("Tango can't be initialized because of invalid permissions");
			}
		}
		else
		{
			UnityEngine.Debug.Log("No Tango Manager found in scene.");
		}
    }
    
    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    private void Update() 
    {
        if (m_isDirty)
        {
			// Update the point cloud.
            _UpdateMesh();
            m_isDirty = false;
        }
    }

	/// <summary>
	/// Gets the time since last depth data collection.
	/// </summary>
	/// <returns>The time since last frame.</returns>
    public float GetTimeSinceLastFrame()
    {
        return (m_timeSinceLastDepthFrame * 1000.0f);
    }

	/// <summary>
	/// Callback that gets called when depth is available
	/// from the Tango Service.
	/// DO NOT USE THE UNITY API FROM INSIDE THIS FUNCTION!
	/// </summary>
	/// <param name="callbackContext">Callback context.</param>
	/// <param name="xyzij">Xyzij.</param>
    protected override void _OnDepthAvailable(IntPtr callbackContext, TangoXYZij xyzij)
    {
		// Calculate the time since the last successful depth data
		// collection.
        if (m_previousDepthDeltaTime == 0.0f)
        {
            m_previousDepthDeltaTime = Time.realtimeSinceStartup;
        } 
        else
        {
            m_numberOfDepthSamples++;
            m_timeSinceLastDepthFrame = Time.realtimeSinceStartup - m_previousDepthDeltaTime;
            m_previousDepthDeltaTime = Time.realtimeSinceStartup;
        }

		// Fill in the data to draw the point cloud.
        if (xyzij != null && m_vertices != null)
        {
            int numberOfActiveVertices = xyzij.xyz_count;
            m_pointsCount = numberOfActiveVertices;

            if(numberOfActiveVertices > 0)
            {
                float[] allPositions = new float[numberOfActiveVertices * 3];
                Marshal.Copy(xyzij.xyz[0], allPositions, 0, allPositions.Length);
                
                for(int i = 0; i < m_vertices.Length; ++i)
                {
                    if( i < xyzij.xyz_count )
                    {
                        m_vertices[i].x = allPositions[i * 3];
                        m_vertices[i].y = allPositions[(i * 3) + 1];
                        m_vertices[i].z = allPositions[(i * 3) + 2];
                    }
                    else
                    {
                        m_vertices[i].x = m_vertices[i].y = m_vertices[i].z = 0.0f;
                    }
                }
                m_isDirty = true;
            }
        }
    }
    
    /// <summary>
    /// Create the mesh to visualize the point cloud
    /// data.
    /// </summary>
    private void _CreateMesh()
    {
        m_vertices = new Vector3[VERT_COUNT];
        m_triangles = new int[VERT_COUNT];
        // Assign triangles, note: this is just for visualizing point in the mesh data.
        for (int i = 0; i < VERT_COUNT; i++)
        {
            m_triangles[i] = i;
        }

        m_mesh.Clear();
        m_mesh.vertices = m_vertices;
        m_mesh.triangles = m_triangles;
        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();
    }

    /// <summary>
    /// Update the mesh m_vertices and m_triangles.
    /// </summary>
    private void _UpdateMesh()
    {
        _UpdateMeshFromGetPointcloud();

        // update the m_vertices
        m_mesh.Clear();
        m_mesh.vertices = m_vertices;
        m_mesh.triangles = m_triangles;
        m_mesh.RecalculateBounds();
        m_mesh.SetIndices(m_triangles, MeshTopology.Points, 0);
    }

    /// <summary>
    /// Update the mesh.
    /// </summary>
    private void _UpdateMeshFromGetPointcloud()
    {
        float validPointCount = 0;
        m_overallZ = 0.0f;

        // Calculate the average z depth
        for (int i = 0; i<m_vertices.Length; i++)
        {
            if(m_vertices[i].z != 0.0f)
            {
                m_overallZ += m_vertices[i].z;
                ++validPointCount;
            }
        }

        // Don't divide by zero!
        if (validPointCount != 0)
        {
            m_overallZ = m_overallZ / (validPointCount);
        } else
        {
            m_overallZ = 0;
        }
    }
}         