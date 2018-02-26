﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using MonoLibUsb;

public class FreiburgSyncbox : MonoBehaviour 
{
	private const short FREIBURG_SYNCBOX_VENDOR_ID  = 0x0403;
	private const short FREIBURG_SYNCBOX_PRODUCT_ID = 0x6001;
	private const int FREIBURG_SYNCBOX_TIMEOUT_MS = 500;
	private const int FREIBURG_SYNCBOX_PIN_COUNT = 8;
    private const int FREIBURG_SYNCBOX_ENDPOINT = 2;
    private const int FREIBURG_SYNCBOX_INTERFACE_NUMBER = 0;

    private const float TIME_BETWEEN_PULSES_MIN = 0.8f;
    private const float TIME_BETWEEN_PULSES_MAX = 1.2f;

	// Use this for initialization
	void Start () 
	{
		StartCoroutine(FreiburgPulse());
	}

	private IEnumerator FreiburgPulse()
	{
		MonoLibUsb.MonoUsbSessionHandle sessionHandle = new MonoUsbSessionHandle();
		MonoLibUsb.Profile.MonoUsbProfileList profileList = null;

		if (sessionHandle.IsInvalid)
			throw new ExternalException("Failed to initialize context.");

		MonoUsbApi.SetDebug(sessionHandle, 0);

		profileList = new MonoLibUsb.Profile.MonoUsbProfileList();

		// The list is initially empty.
		// Each time refresh is called the list contents are updated. 
		int profileListRefreshResult;
		profileListRefreshResult = profileList.Refresh(sessionHandle);
		if (profileListRefreshResult < 0) throw new ExternalException("Failed to retrieve device list.");
		Debug.Log(profileListRefreshResult.ToString() + " device(s) found.");

		// Iterate through the profile list.
		// If we find the device, write 00000000 to its endpoint 2.
		foreach (MonoLibUsb.Profile.MonoUsbProfile profile in profileList)
		{
			//if (profile.DeviceDescriptor.ProductID == FREIBURG_SYNCBOX_PRODUCT_ID && profile.DeviceDescriptor.VendorID == FREIBURG_SYNCBOX_VENDOR_ID)
			{
				while (true)
				{
                    yield return new WaitForSeconds (Random.Range (TIME_BETWEEN_PULSES_MIN, TIME_BETWEEN_PULSES_MAX));
					MonoLibUsb.MonoUsbDeviceHandle deviceHandle = new MonoUsbDeviceHandle(profile.ProfileHandle);
					deviceHandle = profile.OpenDeviceHandle();
                    Debug.Log(MonoUsbApi.ClaimInterface(deviceHandle, FREIBURG_SYNCBOX_INTERFACE_NUMBER));

					int actual_length;
					if (deviceHandle == null)
						throw new ExternalException("The ftd USB device was found but couldn't be opened");
                    Debug.Log(MonoUsbApi.BulkTransfer(deviceHandle, FREIBURG_SYNCBOX_ENDPOINT, byte.MinValue, FREIBURG_SYNCBOX_PIN_COUNT / 8, out actual_length, FREIBURG_SYNCBOX_TIMEOUT_MS));
					Debug.Log(actual_length.ToString() + " bytes written.");

                    MonoUsbApi.ReleaseInterface(deviceHandle, FREIBURG_SYNCBOX_INTERFACE_NUMBER);
					//deviceHandle.Close();
					//profile.Close ();
				}
			}
		}


		//profileList.Close();
		//sessionHandle.Close();
		Debug.Log ("end of freiburgpulse");
	}
}