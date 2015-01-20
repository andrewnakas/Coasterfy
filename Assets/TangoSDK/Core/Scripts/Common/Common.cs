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
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tango
{
    /// <summary>
    /// This struct holds common global functionality used by
    /// this SDK.
    /// </summary>
    public struct Common
    {
        /// <summary>
        /// Codes returned by Tango API functions.
        /// </summary>
        public struct ErrorType
		{
			public static readonly int TANGO_NO_CAMERA_PERMISSION = -5;   /**< Camera access not allowed */
			public static readonly int TANGO_NO_ADF_PERMISSION = -4;  /**< ADF access not allowed */
			public static readonly int TANGO_NO_MOTION_TRACKING_PERMISSION = -3;  /**< Motion tracking not allowed*/
            public static readonly int TANGO_INVALID = -2;
            public static readonly int TANGO_ERROR = -1;
            public static readonly int TANGO_SUCCESS = 0;
        }
        
		public const string TANGO_UNITY_DLL = "tango_client_api";

		public const string TANGO_PERMISSION_STRING = "";
		public const string TANGO_MOTION_TRACKING_PERMISSIONS = "MOTION_TRACKING_PERMISSION";
		public const string TANGO_ADF_LOAD_SAVE_PERMISSIONS = "ADF_LOAD_SAVE_PERMISSION";
		public const string TANGO_NO_PERMISSIONS_ERROR = "This application requires all Tango permissions to run. Please restart the application and grant Tango permissions.";

        public const float UI_LABEL_START_X = 15.0f;
        public const float UI_LABEL_START_Y = 15.0f;
        public const float UI_LABEL_SIZE_X = 1000.0f;
        public const float UI_LABEL_SIZE_Y = 25.0f;
        public const float UI_LABEL_GAP_Y = 3.0f;
        public const float UI_BUTTON_SIZE_X = 125.0f;
        public const float UI_BUTTON_SIZE_Y = 65.0f;
		public const float UI_BUTTON_GAP_X = 5.0f;
        public const float UI_CAMERA_BUTTON_OFFSET = UI_BUTTON_SIZE_X + UI_BUTTON_GAP_X; 
        public const float UI_LABEL_OFFSET = UI_LABEL_GAP_Y + UI_LABEL_SIZE_Y;

        public const float UI_TANGO_VERSION_X = UI_LABEL_START_X;
        public const float UI_TANGO_VERSION_Y = UI_LABEL_START_Y;
        public const float UI_TANGO_APP_SPECIFIC_START_X = UI_TANGO_VERSION_X;
        public const float UI_TANGO_APP_SPECIFIC_START_Y = UI_TANGO_VERSION_Y + (UI_LABEL_OFFSET * 2);

		public const string UX_SERVICE_VERSION = "Service Version: {0}";
		public const string UX_TANGO_SERVICE_VERSION = "Tango Service Version: {0}";
		public const string UX_TANGO_SYSTEM_EVENT = "Tango System Event: {0}";
		public const string UX_TARGET_TO_BASE_FRAME = "Target->{0}, Base->{1}:";
		public const string UX_STATUS = "\tStatus: {0}, Count: {1}, Delta Time(ms): {2}, Pose(m):[{3}], Quat:[{4}]";
        
        public const int UUID_LENGTH = 37;
        
    #if (UNITY_EDITOR)
        private static bool m_mirroring = true; 
    #elif (UNITY_ANDROID) 
        private static bool m_mirroring = false; 
    #else 
        private static bool m_mirroring = false;
    #endif
        private static Resolution m_depthFrameResolution;
        private static int m_depthBufferSize;

        /// <summary>
        /// Property for mirroring.
        /// </summary>
        /// <value> Bool - sets mirroring.</value>
        public static bool Mirroring
        {
            get { return m_mirroring; }
            set { m_mirroring = value; }
        }

        /// <summary>
        /// Property for the current depth frame resolution.
        /// </summary>
        /// <value> Resolution - Sets depth frame resolution reference.</value>
        public static Resolution DepthFrameResolution
        {
            get { return m_depthFrameResolution; }
            set { m_depthFrameResolution = value; }
        }

        /// <summary>
        /// Property for the depth buffer size.
        /// </summary>
        /// <value> Bool - Sets the size of the depth buffer.</value>
        public static int DepthBufferSize
        {
            get { return m_depthBufferSize; }
            set { m_depthBufferSize = value; }
        }

        /// <summary>
        /// Get the world rotation.
        /// </summary>
        /// <returns> Quaternion representing the world rotation.</returns>
        public static Quaternion GetWorldRotation()
        {
            return OrientationManager.GetWorldRotation();
        } 
        
        /// <summary>
        /// Gets current window resoltion where width is
        /// always larger than height.
        /// </summary>
        /// <returns> Vector2 containing the screen width and height. </returns>
        public static Vector2 GetWindowResolution()
        {
            Vector2 screenSize;
            if (Screen.width > Screen.height)
            {
                screenSize = new Vector2(Screen.width, Screen.height);
            }
            else
            {
                screenSize = new Vector2(Screen.height, Screen.width);
            }
            return screenSize;
        }
        
        /// <summary>
        /// Get the aspect resolution of the window.
        /// </summary>
        /// <returns> Window resolution aspect ratio as a single
        /// precision floating point.</returns>
        public static float GetWindowResoltionAspect()
        {
            Vector2 resolution = GetWindowResolution();
            return resolution.x / resolution.y;
        }
    }
}