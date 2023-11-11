using UnityEngine;
using System;

namespace Milan.MMAVT
{   
    //Classes for JSON parsing
    [Serializable]
    public class MMAVTData
    {
        public string metaData;
        public HFEM hfem;
        public MBody mbody;
    }
    [Serializable]
    public class MBody
    {
        public MBodyObject[] objects;
        public MBodyAction[] actions;
    }
    [Serializable]
    public class MBodyObject
    {
        public string name;
        public string[] data;
    }
    [Serializable]
    public class MBodyAction
    {
        public string name;
        public MBODYKeyframe[] keyframes;
    }
    [Serializable]
    public class MBODYKeyframe
    {
        public float time;
        public string mmavtName;
        public int mmavtIndex;
        public string objectName;
        public int objectIndex;
    }
    [Serializable]
    public class HFEM
    {
        public HFEMObject[] objects;
        public HFEMAction[] actions;
    }
    [Serializable]
    public class HFEMObject
    {
        public string name;
        public HFEMObjData[] data;
    }
    [Serializable]
    public class HFEMObjData
    {
        public string name;
        public int head;
        public int face;
        public int eye;
        public int eyeLeft;
        public int eyeRight;
        public int mouth;
    }
    [Serializable]
    public class HFEMAction
    {
        public string name;
        public HFEMKeyframe[] keyframes;
    }
    [Serializable]
    public class HFEMKeyframe
    {
        public float time;
        public int head;
        public int face;
        public int eye;
        public int eyeLeft;
        public int eyeRight;
        public int mouth;
    }
}