using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Milan.MMAVT;
namespace Milan.MMAVT
{
    [Serializable]
    public struct MBodyGroup
    {
        public string groupName;
        public SkinnedMeshRenderer[] meshRenderers;
    }
    [Serializable]
    public struct HFEMGroup
    {
        public string groupName;
        public HFEMGroupData[] groupData;
    }
    [Serializable]
    public struct HFEMGroupData
    {
        public SkinnedMeshRenderer meshRenderer;
        public int head;
        public int face;
        public int eye;
        public int eyeLeft;
        public int eyeRight;
        public int mouth;
    }
}