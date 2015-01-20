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
using UnityEngine;

/// <summary>
/// Helper functions for common android functionality.
/// </summary>
public class AndroidHelper : MonoBehaviour
{
	private const string PERMISSION_REQUESTER = "com.projecttango.permissionrequester.RequestManagerActivity";
#pragma warning disable 414
	private static AndroidJavaObject m_unityActivity = null;
#pragma warning restore 414

	/// <summary>
	/// Gets the unity activity.
	/// </summary>
	/// <returns>The unity activity.</returns>
	public static AndroidJavaObject GetUnityActivity()
	{
	#if UNITY_ANDROID && !UNITY_EDITOR
		if(m_unityActivity == null)
		{
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

			if(unityPlayer != null)
			{
				m_unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			}
		}
		return m_unityActivity;
	#else
		return null;
	#endif
	}

	/// <summary>
	/// Gets the current application label.
	/// </summary>
	/// <returns>The current application label.</returns>
	public static string GetCurrentApplicationLabel()
	{
		AndroidJavaObject unityActivity = GetUnityActivity();
		if(unityActivity != null)
		{
			string currentPackageName = GetCurrentPackageName();
			AndroidJavaObject packageManager = unityActivity.Call<AndroidJavaObject>("getPackageManager");
			AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", currentPackageName, 0);

			if(packageInfo != null)
			{
				AndroidJavaObject applicationInfo = packageInfo.Get<AndroidJavaObject>("applicationInfo");
				AndroidJavaObject applicationLabel = packageManager.Call<AndroidJavaObject>("getApplicationLabel", applicationInfo);
				
				return applicationLabel.Call<string>("toString");
			}
		}

		return "Not Set";
	}

	/// <summary>
	/// Gets the name of the current package.
	/// </summary>
	/// <returns>The current package name.</returns>
	public static string GetCurrentPackageName()
	{
		AndroidJavaObject unityActivity = GetUnityActivity();
		if(unityActivity != null)
		{
			return unityActivity.Call<string>("getPackageName");
		}

		return "Not Set";
	}

	/// <summary>
	/// Gets the package info.
	/// </summary>
	/// <returns>The package info.</returns>
	/// <param name="packageName">Package name.</param>
	public static AndroidJavaObject GetPackageInfo(string packageName)
	{
		AndroidJavaObject unityActivity = GetUnityActivity();
		if(unityActivity != null && !string.IsNullOrEmpty(packageName))
		{
			AndroidJavaObject packageManager = unityActivity.Call<AndroidJavaObject>("getPackageManager");
			return packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
		}

		return null;
	}

	/// <summary>
	/// Gets the name of the version.
	/// </summary>
	/// <returns>The version name.</returns>
	/// <param name="packageName">Package name.</param>
	public static string GetVersionName(string packageName)
	{
		AndroidJavaObject unityActivity = GetUnityActivity();
		if(unityActivity != null && !string.IsNullOrEmpty(packageName))
		{
			AndroidJavaObject packageInfo = GetPackageInfo(packageName);

			if(packageInfo != null)
			{
				return packageInfo.Get<string>("versionName");
			}
		}

		return "Not Set";
	}

	/// <summary>
	/// Gets the version code.
	/// </summary>
	/// <returns>The version code.</returns>
	/// <param name="packageName">Package name.</param>
	public static int GetVersionCode(string packageName)
	{
		AndroidJavaObject unityActivity = GetUnityActivity();
		if(unityActivity != null && !string.IsNullOrEmpty(packageName))
		{
			AndroidJavaObject packageInfo = GetPackageInfo(packageName);
			
			if(packageInfo != null)
			{		
				return packageInfo.Get<int>("versionCode");
			}
		}

		return -1;
	}

	/// <summary>
	/// Starts the activity for the provided class name.
	/// </summary>
	/// <param name="className">Class name.</param>
	public static void StartActivity(string className)
	{
		AndroidJavaObject unityActivity = GetUnityActivity();

		if(unityActivity != null)
		{
			string packageName = GetCurrentPackageName();

			AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

			intentObject.Call<AndroidJavaObject>("setClassName", packageName, className);

			unityActivity.Call("startActivity", intentObject);
		}
	}

	/// <summary>
	/// Starts the tango permissions activity of the provided type.
	/// </summary>
	/// <param name="permissionsType">Permissions type.</param>
	public static void StartTangoPermissionsActivity(string permissionsType)
	{
		AndroidJavaObject unityActivity = GetUnityActivity();
		
		if(unityActivity != null)
		{
			string packageName = GetCurrentPackageName();

			AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

			intentObject.Call<AndroidJavaObject>("setClassName", packageName, PERMISSION_REQUESTER);
			intentObject.Call<AndroidJavaObject>("putExtra", "classname", "com.google.atap.tango.RequestPermissionActivity");
			intentObject.Call<AndroidJavaObject>("putExtra", "string_args", "PERMISSIONTYPE:" + permissionsType);

			unityActivity.Call("startActivity", intentObject);
		}
	}
}
