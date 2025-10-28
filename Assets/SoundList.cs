using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct SoundGroup
{
    [Tooltip("Name shown above the sounds array in the inspector")]
    public string groupName;

    [Tooltip("Audio clips that belong to this sound group")]
    public AudioClip[] clips;

    // Read-only accessors for other scripts
    public string GroupName => groupName;
    public AudioClip[] Clips => clips;
}

public class SoundList : MonoBehaviour
{
    [Tooltip("Top-level array: each element is a sound group/type. Expand a group to edit its clips.")]
    public SoundGroup[] groups;

    public AudioClip GetRandomClip(string groupName)
    {
        if (string.IsNullOrEmpty(groupName) || groups == null) return null;

        for (int i = 0; i < groups.Length; i++)
        {
            if (string.Equals(groups[i].GroupName, groupName, StringComparison.OrdinalIgnoreCase))
                return groups[i].Clips[Random.Range(0, groups[i].Clips.Length)];
        }

        return null;
    }

    public AudioClip GetFirstClip(string groupName)
    {
        if (string.IsNullOrEmpty(groupName) || groups == null) return null;

        for (int i = 0; i < groups.Length; i++)
        {
            if (string.Equals(groups[i].GroupName, groupName, StringComparison.OrdinalIgnoreCase))
                return groups[i].Clips[0];

        }

        return null;
    }

    // Returns the clip by group name and clip index (case-insensitive group lookup), or null if not found/invalid.
    public AudioClip GetClip(string groupName, bool getRandom = false)
    {
        if (string.IsNullOrEmpty(groupName) || groups == null) return null;

        for (int i = 0; i < groups.Length; i++)
        {
            if (string.Equals(groups[i].GroupName, groupName, StringComparison.OrdinalIgnoreCase))
            {
                if(getRandom)
                    return groups[i].Clips[Random.Range(0, groups[i].Clips.Length)];
                else
                    return groups[i].Clips[0];
            }
        }

        return null;
    }
}