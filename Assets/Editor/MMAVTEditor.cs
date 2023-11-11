using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Milan.MMAVT;

namespace Milan.MMAVT
{
    [CustomEditor(typeof(MMAVTController))]
    public class MMAVTEditor : Editor
    {
        MMAVTController mmavt;
        string dataInput = "";
        SerializedProperty prop;
        bool[] toggle;
        GUIStyle header;
        int animationClipTotal;

        void OnEnable() 
        {
            header = new GUIStyle();
            header.fontStyle = FontStyle.Bold;
            header.fontSize = 11;
            header.normal.textColor = new Color(0.8f,0.8f,0.8f,1);
            mmavt = (MMAVTController)target;
            int totalAnimationClips = mmavt.animator.runtimeAnimatorController.animationClips.Length;
            toggle = new bool[totalAnimationClips];
        }
        public override void OnInspectorGUI()
        {
            animationClipTotal = mmavt.animator.runtimeAnimatorController.animationClips.Length;
            if(animationClipTotal != toggle.Length)
                toggle = new bool[animationClipTotal];
            DrawDefaultInspector();
            JSONFileCheck();
            if(mmavt.animator.runtimeAnimatorController == null)
                return;
            // dataInput = EditorGUILayout.TextField(dataInput);
            // ^ enable this if you want to copy and paste your json data in, else just put in a json file
            if (dataInput != "")
            {
                ReadJson(dataInput);
                dataInput = "";
            }
            if (GUILayout.Button("Update animations with MMAVT"))
            {
                if(!JSONFileCheck())
                    return;
                ReadJson(mmavt.mmavtJson.ToString());
            }
            if (GUILayout.Button("Clear all MMAVT events"))
                ClearMMAVTEvents(mmavt.animator.runtimeAnimatorController.animationClips);
            GUILayout.Space(5);
            GUILayout.Label("Animations",header);
            GUILayout.Space(10);
            ShowListOfAnimations();
        }
        void ShowListOfAnimations()
        {
            int counter = 0;
            foreach (AnimationClip item in mmavt.animator.runtimeAnimatorController.animationClips)
            {
                string symbol = " ";
                symbol = toggle[counter] == true ?  "▼" : "►";
                GUILayout.BeginHorizontal();
                toggle[counter] = EditorGUILayout.Foldout(toggle[counter], item.name);
                // GUILayout.Label(item.name,GUILayout.Width(100));
                int mmavtCount = 0;
                foreach (AnimationEvent animEvent in item.events)
                {
                    if(animEvent.functionName.Contains("MMAVT"))
                        mmavtCount++;
                }
                GUILayout.Space(100);
                GUILayout.Label(mmavtCount.ToString(),GUILayout.Width(10));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUI.enabled = false;
                if(toggle[counter])
                {
                    foreach (AnimationEvent animEvent in item.events)
                    {
                        string term = animEvent.functionName == "MMAVT_HFEMOperation" ? "HFEM" : "MBODY";
                        GUILayout.BeginVertical();
                        GUILayout.BeginHorizontal();
                        EditorGUI.indentLevel+= 3;

                        GUILayout.Label(term,GUILayout.Width(80));
                        GUILayout.Label(animEvent.time.ToString("00.00") + ": ",GUILayout.Width(40));
                        GUILayout.Label(animEvent.stringParameter);
                        EditorGUI.indentLevel-= 3;
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }
                }
                GUI.enabled = true;
                counter++;
            }
        }

        bool JSONFileCheck()
        {
            if(mmavt.mmavtJson == null)
                return false;
            string assetPath = AssetDatabase.GetAssetPath(mmavt.mmavtJson);
            if(assetPath == "" || assetPath == null)
                return false;
            string fileType = assetPath.Substring(assetPath.Length - 5);
            if(fileType != ".json")
            {
                Debug.LogError("Error! File must be of type .json");
                mmavt.mmavtJson = null;
                return false;
            }
            return true;
        }

        public void ReadJson(string jsonData)
        {
            SkinnedMeshRenderer exampleFilter = GetSkinnedMeshRendererOfChild(mmavt.gameObject);
            if(exampleFilter == null)
                return;
            string path = AssetDatabase.GetAssetPath(exampleFilter.sharedMesh);
            AnimationClip[] animationClips = mmavt.animator.runtimeAnimatorController.animationClips;
            MMAVTData mmavtData = JsonUtility.FromJson<MMAVTData>(jsonData);
            List<MBodyGroup> mbodyList = new List<MBodyGroup>();
            ClearMMAVTEvents(animationClips);
            foreach (MBodyObject item in mmavtData.mbody.objects)
            {
                MBodyGroup mmavtGroupList = new MBodyGroup();
                mmavtGroupList.groupName = item.name;
                List<SkinnedMeshRenderer> meshRenderers = new ();
                foreach (string obj in item.data)
                {
                    SkinnedMeshRenderer renderer = mmavt.transform.Find(obj).gameObject.
                        GetComponent<SkinnedMeshRenderer>();
                    meshRenderers.Add(renderer);
                }
                mmavtGroupList.meshRenderers = meshRenderers.ToArray();
                mbodyList.Add(mmavtGroupList);
            }
            mmavt.mbodyGroups = mbodyList.ToArray();
            foreach (MBodyAction action in mmavtData.mbody.actions)
            {
                AnimationClip clip = GetClipOfName(action.name,animationClips);
                List<AnimationEvent> eventList = AnimationUtility.GetAnimationEvents(clip).ToList();
                foreach (MBODYKeyframe keyframe in action.keyframes)
                {
                    AnimationEvent animEvent = new AnimationEvent();
                    animEvent.time = keyframe.time;
                    string groupIndex = keyframe.mmavtIndex.ToString("00");
                    string objectIndex = keyframe.objectIndex.ToString("00");
                    animEvent.stringParameter = groupIndex + objectIndex;
                    animEvent.functionName = "MMAVT_MBodyOperation";
                    eventList.Add(animEvent);
                }
                    AnimationUtility.SetAnimationEvents(clip,eventList.ToArray());
            }

            List<HFEMGroup> hfemList = new ();
            foreach (HFEMObject obj in mmavtData.hfem.objects)
            {
                HFEMGroup hfemGroupList = new ();
                hfemGroupList.groupName = obj.name;
                List<HFEMGroupData> hfemGroupDataList = new ();
                foreach (HFEMObjData hfemData in obj.data)
                {
                    HFEMGroupData groupData = new HFEMGroupData();
                    groupData.meshRenderer =  mmavt.transform.Find(hfemData.name).gameObject.
                        GetComponent<SkinnedMeshRenderer>();
                    groupData.head = hfemData.head;
                    groupData.face = hfemData.face;
                    groupData.eye = hfemData.eye;
                    groupData.eyeLeft = hfemData.eyeLeft;
                    groupData.eyeRight = hfemData.eyeRight;
                    groupData.mouth = hfemData.mouth;
                    hfemGroupDataList.Add(groupData);
                }
                hfemGroupList.groupData = hfemGroupDataList.ToArray();
                hfemList.Add(hfemGroupList);
            }   
            mmavt.hfemGroups = hfemList.ToArray();
            foreach (HFEMAction action in mmavtData.hfem.actions)
            {
                AnimationClip clip = GetClipOfName(action.name,animationClips);
                List<AnimationEvent> eventList = AnimationUtility.GetAnimationEvents(clip).ToList();
                foreach (HFEMKeyframe keyframe in action.keyframes)
                {
                    AnimationEvent animEvent = new AnimationEvent();
                    animEvent.time = keyframe.time;
                    string head = keyframe.head.ToString("00").PadLeft(3,'0');
                    string face = keyframe.face.ToString("00").PadLeft(3,'0');
                    string eye = keyframe.eye.ToString("00").PadLeft(3,'0');
                    string eyeLeft = keyframe.eyeLeft.ToString("00").PadLeft(3,'0');
                    string eyeRight = keyframe.eyeRight.ToString("00").PadLeft(3,'0');
                    string mouth = keyframe.mouth.ToString("00").PadLeft(3,'0');
                    animEvent.stringParameter = head + face + eye + eyeLeft + eyeRight + mouth;
                    animEvent.functionName = "MMAVT_HFEMOperation";
                    eventList.Add(animEvent);

                }
                AnimationUtility.SetAnimationEvents(clip,eventList.ToArray());
            }
        }
        SkinnedMeshRenderer GetSkinnedMeshRendererOfChild(GameObject go)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                GameObject child = go.transform.GetChild(i).gameObject;
                SkinnedMeshRenderer meshRenderer = child.GetComponent<SkinnedMeshRenderer>();
                if(meshRenderer != null)
                    return meshRenderer;
            }
            return null;
        }
        AnimationClip GetClipOfName(string name, AnimationClip[] clips)
        {
            foreach (AnimationClip item in clips)
            {
                if(name == item.name)
                    return item;
            }
            return null;
        }
        void ClearMMAVTEvents(AnimationClip[] clips)
        {
            foreach (AnimationClip clip in clips)
            {
                foreach (AnimationEvent item in clip.events)
                {
                    if(item.functionName.Contains("MMAVT"))
                    {
                        RemoveEventFromAnimator(item.functionName,clip);
                    }
                }
            }
        }
        AnimationEvent[] GenerateAnimationEventPackage(AnimationEvent[] events, AnimationEvent newClip)
        {
            Array.Resize(ref events, 1);
            events[events.Length - 1] = newClip;
            return events;
        }
        //Credits to SiarheiPilat, who fixed bugs from the original post on the unity forums https://forum.unity.com/threads/remove-animation-event.78957/
        void RemoveEventFromAnimator(string functionName, AnimationClip clip)
        {
            AnimationEvent[] animationEvents = clip.events;
            List<AnimationEvent> updatedAnimationEvents = new List<AnimationEvent>();
    
            for (int i = 0; i < animationEvents.Length; i++)
            {
                AnimationEvent animationEvent = animationEvents[i];
                if (animationEvent.functionName != functionName)
                {
                    updatedAnimationEvents.Add(animationEvent);
                }
            }
            AnimationUtility.SetAnimationEvents(clip,updatedAnimationEvents.ToArray());
        }
    }
}