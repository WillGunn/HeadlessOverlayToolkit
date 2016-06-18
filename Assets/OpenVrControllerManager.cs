﻿using System;
using UnityEngine;
using System.Collections;
using System.ComponentModel;
using Valve.VR;

public class OpenVrControllerManager
{
    public static OpenVrControllerManager Instance
    {
        get { return _instance ?? (_instance = new OpenVrControllerManager()); }
    }
    public uint LeftIndex
    {
        get
        {
            FindControllers();
            return _leftIndex;
        }
    }
    public uint RightIndex
    {
        get
        {
            FindControllers();
            return _rightIndex;
        }
    }


    private static OpenVrControllerManager _instance;

    private uint _leftIndex;
    private uint _rightIndex;

    public void FindControllers()
    {
        var system = OpenVR.System;
        if (_leftIndex != OpenVR.k_unTrackedDeviceIndexInvalid && system.GetTrackedDeviceClass(_leftIndex) == ETrackedDeviceClass.Controller &&
            _rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid && system.GetTrackedDeviceClass(_rightIndex) == ETrackedDeviceClass.Controller)
        {
            // Assume we are still connected to the controllers..
            return;
        }
        //Debug.Log("Searching for Controllers..");
        if (system == null) return;
        _leftIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
        _rightIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);

        if (_leftIndex != OpenVR.k_unTrackedDeviceIndexInvalid && _rightIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            //Debug.Log("Found Left but not Right..");
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (i == _leftIndex || system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Controller)
                {
                    continue;
                }
                //Debug.Log("Found Right!");
                _rightIndex = i;
                break;
            }
        }
        else if (_leftIndex == OpenVR.k_unTrackedDeviceIndexInvalid && _rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            //Debug.Log("Found Right but not Left..");
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (i == _rightIndex || system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Controller)
                {
                    continue;
                }
                //Debug.Log("Found Right!");
                _leftIndex = i;
                break;
            }
        }
        else if (_leftIndex == OpenVR.k_unTrackedDeviceIndexInvalid && _rightIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            Debug.LogWarning("SteamVR Reports No Controllers..! Deep searching..");
            var foundUnassigned = 0;
            var slots = new uint[2];
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Controller)
                {
                    continue;
                }
                switch (system.GetControllerRoleForTrackedDeviceIndex(i))
                {
                    case ETrackedControllerRole.LeftHand:
                        Debug.Log("Found Controller ( Device: " + i + " ): Left");
                        _leftIndex = i;
                        break;
                    case ETrackedControllerRole.RightHand:
                        Debug.Log("Found Controller ( Device: " + i + " ): Right");
                        _rightIndex = i;
                        break;
                    case ETrackedControllerRole.Invalid:
                        Debug.Log("Found Controller ( Device: " + i + " ): Unassigned");
                        if (foundUnassigned <= 1)
                            slots[foundUnassigned++] = i;
                        break;
                }

                if (foundUnassigned == 2)
                {
                    break;
                }
            }
            switch (foundUnassigned)
            {
                case 2:
                    Debug.LogWarning("Found Two Unassigned Controllers! Randomly Assigning!");
                    _rightIndex = slots[0];
                    _leftIndex = slots[1];
                    break;
                case 1:
                    if (_leftIndex == OpenVR.k_unTrackedDeviceIndexInvalid &&
                       _rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
                    {
                        Debug.LogWarning("Only Found One Unassigned Controller, and Right was already assigned! Assigning To Left!");
                        _rightIndex = slots[0];
                    }
                    else
                    {
                        Debug.LogWarning("Only Found One Unassigned Controller! Assigning To Right!");
                        _rightIndex = slots[0];
                    }
                    break;
                case 0:
                    Debug.LogWarning("Couldn't Find Any Unassigned Controllers!");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}