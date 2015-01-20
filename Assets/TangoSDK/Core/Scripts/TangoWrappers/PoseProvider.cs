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
using System.Runtime.InteropServices;
using UnityEngine;
using System;

namespace Tango
{
	/// <summary>
	/// Provide pose related functionality.
	/// </summary>
    public class PoseProvider
    {   

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void TangoService_onPoseAvailable(IntPtr callbackContext, [In,Out] TangoPoseData pose);

        private static readonly string CLASS_NAME = "PoseProvider";
        private const float MOUSE_LOOK_SENSITIVITY = 100.0f;
        private const float TRANSLATION_SPEED = 2.0f;


		// Keeps track of all the ADFs on the device.
		private static UUID_list m_adfList = new UUID_list();
		/// <summary>
		/// Sets the callback to be used when a new Pose is
		/// presented by the Tango Service.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public static void SetCallback(TangoCoordinateFramePair[] framePairs, TangoService_onPoseAvailable callback)
        {
			int returnValue =  PoseProviderAPI.TangoService_connectOnPoseAvailable(framePairs.Length, framePairs, callback);
            if (returnValue != Common.ErrorType.TANGO_SUCCESS)
            {
                DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_ERROR,
                                                   CLASS_NAME + ".SetCallback() Callback was not set!");
            }
            else
            {
                DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_INFO,
				                                   CLASS_NAME + ".SetCallback() OnPose callback was set!");
            }
        }

		/// <summary>
		/// Gets the pose at a given time.
		/// </summary>
		/// <param name="poseData">Pose data.</param>
		/// <param name="timeStamp">Time stamp.</param>
        public static void GetPoseAtTime([In,Out] TangoPoseData poseData, 
		                                 double timeStamp, 
		                                 TangoCoordinateFramePair framePair)
        {
            int returnValue = PoseProviderAPI.TangoService_getPoseAtTime(timeStamp, framePair, poseData);
            if (returnValue != Common.ErrorType.TANGO_SUCCESS)
            {
                DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_ERROR,
				                                   CLASS_NAME + ".GetPoseAtTime() Could not get pose at time : " + timeStamp);
            }
        }

        /// <summary>
        /// Sets the listener coordinate frame pairs.
        /// </summary>
        /// <param name="count">Count.</param>
        /// <param name="frames">Frames.</param>
		public static void SetListenerCoordinateFramePairs(int count,
		                                                   ref TangoCoordinateFramePair frames)
		{
			int returnValue = PoseProviderAPI.TangoService_setPoseListenerFrames (count, ref frames);
			if (returnValue != Common.ErrorType.TANGO_SUCCESS)
			{
				DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_ERROR,
				                                   CLASS_NAME + ".SetListenerCoordinateFramePairs() Could not set frame pairs");
			}
		}

		/// <summary>
		/// Resets the motion tracking.
		/// </summary>
        public static void ResetMotionTracking()
        {
            PoseProviderAPI.TangoService_resetMotionTracking();
        }

        /// <summary>
        /// Gets the mouse emulation.
        /// </summary>
        /// <param name="controllerPostion">Controller postion.</param>
        /// <param name="controllerRotation">Controller rotation.</param>
        public static void GetMouseEmulation(ref Vector3 controllerPostion, ref Quaternion controllerRotation)
        {
            Vector3 position = controllerPostion;
            Quaternion rotation;
            Vector3 directionForward, directionRight, directionUp;
            float rotationX;
            float rotationY;
            
            rotationX = controllerRotation.eulerAngles.x - Input.GetAxis("Mouse Y") * MOUSE_LOOK_SENSITIVITY * Time.deltaTime;
            rotationY = controllerRotation.eulerAngles.y + Input.GetAxis("Mouse X") * MOUSE_LOOK_SENSITIVITY * Time.deltaTime;
            Vector3 eulerAngles = new Vector3(rotationX,rotationY,0);
            controllerRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
            rotation = Quaternion.Euler(eulerAngles);
            
            directionForward = rotation * Vector3.forward;
            directionRight =  rotation * Vector3.right;
            directionUp = rotation * Vector3.up;
            position = position + Input.GetAxis("Vertical") * directionForward * TRANSLATION_SPEED * Time.deltaTime;
            position = position + Input.GetAxis("Horizontal") * directionRight * TRANSLATION_SPEED * Time.deltaTime;
            if(Input.GetKey(KeyCode.R)) // Go Up
            {
                position += directionUp * TRANSLATION_SPEED * Time.deltaTime;
            }
            if(Input.GetKey(KeyCode.F))  // Go Down
            {
                position -= directionUp * TRANSLATION_SPEED * Time.deltaTime;
            }
            
            controllerRotation = rotation;
            controllerPostion = position;
        }

		#region ADF Functionality
		/// <summary>
		/// Helper method to retrieve a list of saved area description files.
		/// </summary>
		/// <returns>The cached ADF list.</returns>
		public static UUID_list GetCachedADFList()
		{
			return m_adfList;
		}

		/// <summary>
		/// Returns the UUID of the most recent ADF file.
		/// </summary>
		/// <returns>A string object encoded in UTF-8 format containing the UUID of the requested ADF.</returns>
		public static UUIDUnityHolder GetLatestADFUUID()
		{
			if(m_adfList == null)
			{
				return null;
			}
			return (m_adfList.GetLatestADFUUID());
		}

		public static bool IsUUIDValid(UUIDUnityHolder toCheck)
		{
			return toCheck != null && toCheck.IsObjectValid();
		}

		/// <summary>
		/// Gets the UUID of the ADF at the specified index. It will be encoded in UTF-8.
		/// </summary>
		/// <returns>The ADF UUID as string.</returns>
		/// <param name="index">The ADF format that we want to know the UUID of.</param>
		public static string GetUUIDAsString(int index)
		{
			if(m_adfList == null)
			{
				return string.Empty;
			}
			return m_adfList.GetUUIDAsString(index);
		}

		/// <summary>
		/// Gets the UUID of the ADF at the specified index. It will be encoded in UTF-8.
		/// </summary>
		/// <returns>The ADF UUID as a char array.</returns>
		/// <param name="index">The ADF format that we want to know the UUID of.</param>
		public static char[] GetUUIDAsCharArray(int index)
		{
			string uuidString = GetUUIDAsString(index);
			if(String.IsNullOrEmpty(uuidString))
			{
				return null;
			}
			return uuidString.ToCharArray();
		}

		/// <summary>
		/// This method is used to make sure that we have the most up to date information about ADF
		/// that are stored on device. It will make sure to cache it in a <c>UUID_list</c> object
		/// for easier access without querying the device API again. This method will also 
		/// retrieve the UUID of each ADF by performing a Marshal.Copy and store the information
		/// in a <c>UUID</c> object list.
		/// </summary>
		/// <returns>The ADF list.</returns>
		public static int RefreshADFList()
		{
			int returnValue = Common.ErrorType.TANGO_ERROR;
			IntPtr tempData = IntPtr.Zero;
			returnValue = PoseProviderAPI.TangoService_getAreaDescriptionUUIDList(ref tempData);

			if(returnValue != Common.ErrorType.TANGO_SUCCESS)
			{
				DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_ERROR,
				                                   CLASS_NAME + ".RefreshADFList() Could not get ADF list from device.");
			}
			else
			{
				byte[] charBuffer = new byte[sizeof(char)];
				System.Collections.Generic.List<byte> dataHolder = new System.Collections.Generic.List<byte>();
				Marshal.Copy(tempData, charBuffer, 0, 1);
				while (charBuffer[0] != 0 && charBuffer[0] != '\n')
				{
					dataHolder.Add(charBuffer[0]);
					tempData = new IntPtr(tempData.ToInt64() + 1);
					Marshal.Copy(tempData, charBuffer, 0, 1);
				}
				string uuidList = System.Text.Encoding.UTF8.GetString(dataHolder.ToArray());
				m_adfList.PopulateUUIDList(uuidList);
				if(!m_adfList.HasEntries())
				{
					DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_WARN,
					                                   CLASS_NAME + ".RefreshADFList() No area description files found on device.");
				}
			}
			return returnValue;
		}

		/// <summary>
		/// Saves an area description to device based on the UUID object contained in the adfID object holder.
		/// </summary>
		/// <returns><c>Common.ErrorType.TANGO_SUCCESS</c> if saving was successfull.</returns>
		/// <param name="adfID">The UUIDUnityHolder object that contains the desired UUID object.</param>
		public static int SaveAreaDescription(UUIDUnityHolder adfID)
		{
			// is learning mode on
			// are we localized?

			if(adfID == null)
			{
				DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_ERROR,
				                                   CLASS_NAME + 
				                                   ".SaveAreaDescription() Could not save area description. UUID Holder object specified is not initialized");
				return Common.ErrorType.TANGO_ERROR;
			}
			IntPtr idData = Marshal.AllocHGlobal(Common.UUID_LENGTH);
			int returnValue = PoseProviderAPI.TangoService_saveAreaDescription(idData);
			if (returnValue != Common.ErrorType.TANGO_SUCCESS)
			{
				DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_ERROR,
				                                   CLASS_NAME + ".SaveAreaDescripton() Could not save area description with ID: "
				                                   + adfID.GetStringDataUUID());
			}
			else
			{
				byte[] tempDataBuffer = new byte[Common.UUID_LENGTH];
				Marshal.Copy(idData, tempDataBuffer, 0, Common.UUID_LENGTH);
				adfID.SetDataUUID(tempDataBuffer);
			}
			return returnValue;
		}

		/// <summary>
		/// Takes care of saving a ADF file to the specified folder.
		/// </summary>
		/// <returns><c>Common.ErrorType.TANGO_SUCCESS</c> if the ADF file was exported successfully.</returns>
		/// <param name="UUID">The UUID of the ADF file we want to export.</param>
		/// <param name="filePath">File path where we want to export the ADF.</param>
		public static int ExportAreaDescriptionToFile(string UUID, string filePath)
		{
			if(string.IsNullOrEmpty(UUID))
			{
				DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_ERROR, "Can't export an empty UUID. Please define one.");
				return Common.ErrorType.TANGO_ERROR;
			}
			if(string.IsNullOrEmpty(filePath))
			{
				DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_ERROR, "Missing file path for exporting area description. Please define one.");
				return Common.ErrorType.TANGO_ERROR;
			}
			int returnValue = PoseProviderAPI.TangoService_exportAreaDescription(UUID, filePath);
			if(returnValue != Common.ErrorType.TANGO_SUCCESS)
			{
				DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_ERROR,
				                                   CLASS_NAME + ".ExportAreaDescription() Could not export area description: " + UUID +
				                                   " with path: " + filePath);
			}
			return returnValue;

		}
	
		/// <summary>
		/// Takes care of importing a adf file from the specified path. Important: make sure that the filepath
		/// does not contain ADF files already present on device, otherwise it will return an error, as duplicates
		/// can't be imported.
		/// </summary>
		/// <returns><c>Common.ErrorType.TANGO_SUCCESS</c> if the UUID was imported successfully.</returns>
		/// <param name="adfID">The <c>UUIDUnityHolder</c> object that will contain information about the retrieved ADF.</param>
		/// <param name="filePath">File path containing the ADF we want to export.</param>
		public static int ImportAreaDescriptionFromFile(UUIDUnityHolder adfID, string filePath)
		{
			if(adfID == null)
			{
				DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_ERROR,
				                                   CLASS_NAME + 
				                                   ".ImportAreaDescription() Could not  import area description. UUID Holder object specified is not initialized");
				return Common.ErrorType.TANGO_ERROR;
			}
			IntPtr uuidHolder = Marshal.AllocHGlobal(Common.UUID_LENGTH);
			int returnValue = PoseProviderAPI.TangoService_importAreaDescription(filePath, uuidHolder);
			if(returnValue != Common.ErrorType.TANGO_SUCCESS)
			{
				DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_ERROR,
				                                   CLASS_NAME + ".ImportAreaDescription() Could not import area description at path: " + filePath);
			}
			else
			{
				byte[] tempDataBuffer = new byte[Common.UUID_LENGTH];
				Marshal.Copy(uuidHolder, tempDataBuffer, 0, Common.UUID_LENGTH);
				adfID.SetDataUUID(tempDataBuffer);
			}
			return returnValue;
		}

		/// <summary>
		/// Deletes the area description with the specified UUID from the default folder where ADF maps are stored.
		/// This needs to be called before trying to import a ADF that is already present in the default ADF maps folder.
		/// </summary>
		/// <returns><c>Common.ErrorType.TANGO_SUCCESS</c> if the UUID was deleted successfully.</returns>
		/// <param name="toDeleteUUID">The UUID of the ADF we want to delete.</param>
		public static int DeleteAreaDescription(string toDeleteUUID)
		{
			if(string.IsNullOrEmpty(toDeleteUUID))
			{
				DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_ERROR,
				                                   CLASS_NAME + ".DeleteAreaDescription() Could not delete area description, UUID was empty or null.");
				return Common.ErrorType.TANGO_ERROR;
			}
			int returnValue = PoseProviderAPI.TangoService_deleteAreaDescription(toDeleteUUID);
			if(returnValue != Common.ErrorType.TANGO_SUCCESS)
			{
				DebugLogger.GetInstance.WriteToLog(DebugLogger.EDebugLevel.DEBUG_ERROR,
				                                   CLASS_NAME + ".DeleteAreaDescription() Could not delete area description, API returned invalid.");
			}
			return returnValue;
		}
		#endregion // ADF Functionality

		#region ADF Metadata Functionality

		#endregion // ADF Metadata Functionality

	#region API_Functions
        private struct PoseProviderAPI
        { 
    #if UNITY_ANDROID && !UNITY_EDITOR
            [DllImport(Common.TANGO_UNITY_DLL)]
            public static extern int TangoService_connectOnPoseAvailable(int count,
			                                                             TangoCoordinateFramePair[] framePairs,
			                                                             TangoService_onPoseAvailable onPoseAvailable);
            
            
            [DllImport(Common.TANGO_UNITY_DLL)]
            public static extern int TangoService_getPoseAtTime (double timestamp,
                                                                 TangoCoordinateFramePair framePair,
                                                                 [In, Out] TangoPoseData pose);

			[DllImport(Common.TANGO_UNITY_DLL)]
			public static extern int TangoService_setPoseListenerFrames(int count,
			                                                            ref TangoCoordinateFramePair frames);
            
            [DllImport(Common.TANGO_UNITY_DLL)]
            public static extern void TangoService_resetMotionTracking();

            [DllImport(Common.TANGO_UNITY_DLL)]
            public static extern int TangoService_saveAreaDescription(IntPtr uuid);

            [DllImport(Common.TANGO_UNITY_DLL)]
            public static extern int TangoService_getAreaDescriptionUUIDList(ref IntPtr uuid_list);       
            
            [DllImport(Common.TANGO_UNITY_DLL)]
			public static extern int TangoService_getAreaDescriptionMetadata([MarshalAs(UnmanagedType.LPStr)] string uuid,
			                                                                 ref IntPtr metadata);
            
            [DllImport(Common.TANGO_UNITY_DLL)]
			public static extern int TangoService_saveAreaDescriptionMetadata([MarshalAs(UnmanagedType.LPStr)] string uuid,
			                                                                  ref IntPtr metadata);

			[DllImport(Common.TANGO_UNITY_DLL)]
			public static extern int TangoService_importAreaDescription([MarshalAs(UnmanagedType.LPStr)] string source_file_path, IntPtr UUID);

			[DllImport(Common.TANGO_UNITY_DLL)]
			public static extern int TangoService_exportAreaDescription([MarshalAs(UnmanagedType.LPStr)] string UUID, 
			                                                            [MarshalAs(UnmanagedType.LPStr)] string dst_file_path);
            [DllImport(Common.TANGO_UNITY_DLL)]
			public static extern int TangoService_deleteAreaDescription([MarshalAs(UnmanagedType.LPStr)] string UUID);
            
			[DllImport(Common.TANGO_UNITY_DLL)]
			public static extern int TangoAreaDescriptionMetadata_free(IntPtr metadata);

			[DllImport(Common.TANGO_UNITY_DLL)]
			public static extern int TangoAreaDescriptionMetadata_get(IntPtr metadata, [MarshalAs(UnmanagedType.LPStr)] string key, 
			                                                          ref UInt32 value_size, ref IntPtr value);

			[DllImport(Common.TANGO_UNITY_DLL)]
			public static extern int TangoAreaDescriptionMetadata_set(IntPtr metadata, [MarshalAs(UnmanagedType.LPStr)] string key, 
			                                                          UInt32 value_size, [MarshalAs(UnmanagedType.LPStr)] string value);

			[DllImport(Common.TANGO_UNITY_DLL)]
			public static extern int TangoAreaDescriptionMetadata_listKeys(IntPtr metadata, ref IntPtr key_list);
    #else
            public static int TangoService_connectOnPoseAvailable(int count,
                                                                  TangoCoordinateFramePair[] framePairs,
                                                                  TangoService_onPoseAvailable onPoseAvailable)
            {
                return Common.ErrorType.TANGO_SUCCESS;
            }

            public static int TangoService_getPoseAtTime (double timestamp,
                                                          TangoCoordinateFramePair framePair,
                                                          [In, Out] TangoPoseData pose)
            {
                return Common.ErrorType.TANGO_SUCCESS;
			}

			public static int TangoService_setPoseListenerFrames(int count,
			                                                     ref TangoCoordinateFramePair frames)
			{
				return Common.ErrorType.TANGO_SUCCESS;
            }

            public static void TangoService_resetMotionTracking()
            {
            }

			public static int TangoService_saveAreaDescription(IntPtr uuid)
			{
				return Common.ErrorType.TANGO_SUCCESS;
			}

			public static int TangoService_getAreaDescriptionUUIDList(ref IntPtr uuid_list)
			{
				return Common.ErrorType.TANGO_SUCCESS;
			}
			
			public static int TangoService_getAreaDescriptionMetadata([MarshalAs(UnmanagedType.LPStr)] string uuid,
			                                                                 ref IntPtr metadata)
			{
				return Common.ErrorType.TANGO_SUCCESS;
			}
			
			public static int TangoService_saveAreaDescriptionMetadata([MarshalAs(UnmanagedType.LPStr)] string uuid,
			                                                                  ref IntPtr metadata)
			{
				return Common.ErrorType.TANGO_SUCCESS;
			}
			
			public static int TangoService_importAreaDescription([MarshalAs(UnmanagedType.LPStr)] string source_file_path, IntPtr UUID)
			{
				return Common.ErrorType.TANGO_SUCCESS;
			}
			
			public static int TangoService_exportAreaDescription([MarshalAs(UnmanagedType.LPStr)] string UUID, 
			                                                     [MarshalAs(UnmanagedType.LPStr)] string dst_file_path)
			{
				return Common.ErrorType.TANGO_SUCCESS;
			}

			public static int TangoService_deleteAreaDescription([MarshalAs(UnmanagedType.LPStr)] string UUID)
			{
				return Common.ErrorType.TANGO_SUCCESS;
			}
			
			public static int TangoAreaDescriptionMetadata_free(IntPtr metadata)
			{
				return Common.ErrorType.TANGO_SUCCESS;
			}
			
			public static int TangoAreaDescriptionMetadata_get(IntPtr metadata, [MarshalAs(UnmanagedType.LPStr)] string key, 
			                                                   ref UInt32 value_size, ref IntPtr value)
			{
				return Common.ErrorType.TANGO_SUCCESS;
			}
			
			public static int TangoAreaDescriptionMetadata_set(IntPtr metadata, [MarshalAs(UnmanagedType.LPStr)] string key, 
			                                                   UInt32 value_size, [MarshalAs(UnmanagedType.LPStr)] string value)
			{
				return Common.ErrorType.TANGO_SUCCESS;
			}
			
			public static int TangoAreaDescriptionMetadata_listKeys(IntPtr metadata, ref IntPtr key_list)
			{
				return Common.ErrorType.TANGO_SUCCESS;
			}
			#endif
		}
		#endregion
	}
}
