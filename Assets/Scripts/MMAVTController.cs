using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Milan.MMAVT;

namespace Milan.MMAVT
{
    public class MMAVTController : MonoBehaviour
    {
        public Animator animator;
        [SerializeField]
        [Tooltip("HFEM Groups to read from")]
        public HFEMGroup[] hfemGroups;
        [SerializeField]
        public MBodyGroup[] mbodyGroups;
        #if UNITY_EDITOR
        [Tooltip("Points to .json file to read data from")]
        public UnityEngine.Object mmavtJson;
        #endif

        const int type_head = 0;
        const int type_Face = 1;
        const int type_Eyes = 2;
        const int type_LeftEye = 3;
        const int type_RightEye = 4;
        const int type_Mouth = 5;
        
        void MMAVT_MBodyOperation(string data)
        {
            int groupFirstDigit = ((int)data[0] - 48) * 10;
            int groupSecondDigit = ((int)data[1] - 48);
            int finalGroupNumber = groupFirstDigit + groupSecondDigit;

            int objFirstDigit = ((int)data[2] - 48) * 10;
            int objSecondDigit = ((int)data[3] - 48);
            int finalObjNumber = objFirstDigit + objSecondDigit;
            Debug.Log($"MBody: Group:{finalGroupNumber} + Object:{finalObjNumber}");

            for (int g = 0; g < mbodyGroups.Length; g++)
            {
                for (int r = 0; r < mbodyGroups[g].meshRenderers.Length; r++)
                {
                    if (g == finalGroupNumber && r == finalObjNumber)
                        mbodyGroups[g].meshRenderers[r].enabled = true;
                    else if (g == finalGroupNumber && r != finalObjNumber)
                        mbodyGroups[g].meshRenderers[r].enabled = false;
                }
            }
        }
        void SetHFEMGroups()
        {

        }
        void SetMbodyGroups()
        {
            
        }
        void MMAVT_HFEMOperation(string data)
        {
            int head = GetHFEMValueFromInput(ref data,0);
            int face = GetHFEMValueFromInput(ref data,3);
            int eye = GetHFEMValueFromInput(ref data,6);
            int eyeLeft = GetHFEMValueFromInput(ref data,9);
            int eyeRight = GetHFEMValueFromInput(ref data,12);
            int mouth = GetHFEMValueFromInput(ref data,15);

            Debug.Log($"HFEM: head:{head},face:{face},eye:{eye},lefteye:{eyeLeft},righteye:{eyeRight},mouth:{mouth}");
            SetHFEMVisibility(type_head,groupData => groupData.head == head);
            SetHFEMVisibility(type_Face,groupData => groupData.head == head && groupData.face == face);
            SetHFEMVisibility(type_Eyes,groupData => groupData.head == head && groupData.face == face && groupData.eye == eye);
            SetHFEMVisibility(type_LeftEye,groupData => groupData.head == head && groupData.face == face && groupData.eyeLeft == eyeLeft && eye == -1);
            SetHFEMVisibility(type_RightEye,groupData => groupData.head == head && groupData.face == face && groupData.eyeRight == eyeRight && eye == -1);
            SetHFEMVisibility(type_Mouth,groupData => groupData.head == head && groupData.face == face && groupData.mouth == mouth);
            
        }
        void SetHFEMVisibility(int groupNum,Predicate<HFEMGroupData> predicate)
        {
            for (int d = 0; d < hfemGroups[groupNum].groupData.Length; d++)
            {
                HFEMGroupData groupData = hfemGroups[groupNum].groupData[d];
                if(predicate(groupData))
                    groupData.meshRenderer.enabled = true;
                else
                    groupData.meshRenderer.enabled = false;
            }
        }
        void OnGUI() 
        {
            GUILayout.BeginArea(new Rect(0,0,200,500));
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if(GUILayout.Button(clip.name))
                {
                    Debug.Log("Playing animation clip: " + clip.name);
                    animator.Play(clip.name);
                }
            }    
            GUILayout.EndArea();
        }
        int GetHFEMValueFromInput(ref string baseString,int offSet)
        {
            int multiplier = 1;
            if (baseString[0 + offSet] == '-')
                multiplier = -1;

            int objFirstDigit = ((int)baseString[1 + offSet] - 48) * 10;
            int objSecondDigit = ((int)baseString[2 + offSet] - 48);
            int finalObjNumber = (objFirstDigit + objSecondDigit) * multiplier;
            return finalObjNumber;
        }
    } 
}