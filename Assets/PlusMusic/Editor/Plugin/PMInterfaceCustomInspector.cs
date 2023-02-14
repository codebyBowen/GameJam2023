//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using PlusMusic;

//[CustomEditor(typeof(PlusMusicSoundtracking))]
//[System.Serializable]
//public class PMInterfaceCustomInspector : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();
//        PlusMusicSoundtracking theInterface = (PlusMusicSoundtracking)target;
        
//        if (GUILayout.Button("Register for the Game Jam at PlusMusic.ai"))
//        {
//            string theSite = "https://app.plusmusic.ai/creator/register?ref=gamejam1";
//            Application.OpenURL(theSite);
//        }
//        if (GUILayout.Button("Download Latest Soundtrack Pack for Project"))
//        {
//            string projectID = "";
//            string userID = "";
//            if (theInterface.ProjectID != null)
//            {
//                projectID = theInterface.ProjectID;
//            }
//            if (theInterface.UserID != null)
//            {
//                userID = theInterface.UserID;
//            }
//            string theSite = "https://app.plusmusic.ai/admin/search-song-clip-page?projectId=" + projectID + "&source=unity&userId=" + userID + "&action=open-project";
//            Application.OpenURL(theSite);
//        }
//        if (GUILayout.Button("Open Soundtrack Editor"))
//        {
//            string projectID = "";
//            string userID = "";
//            if (theInterface.ProjectID != null)
//            {
//                projectID = theInterface.ProjectID;
//            }
//            if (theInterface.UserID != null)
//            {
//                userID = theInterface.UserID;
//            }
//            string theSite = "https://app.plusmusic.ai/admin/search-song-clip-page?projectId=" + projectID + "&source=unity&userId=" + userID + "&action=open-project";
//            Application.OpenURL(theSite);
//        }
//        if (GUILayout.Button("Find More Music"))
//        {
//            string projectID = "";
//            string userID = "";
//            if (theInterface.ProjectID != null)
//            {
//                projectID = theInterface.ProjectID;
//            }
//            if (theInterface.UserID != null)
//            {
//                userID = theInterface.UserID;
//            }
//            string theSite = "https://app.plusmusic.ai/admin/search-song-clip-page?projectId=" + projectID + "&source=unity&userId=" + userID + "&action=open-project";
//            Application.OpenURL(theSite);
//        }
//        if (GUILayout.Button("Help"))
//        {
//            string theSite = "https://app.plusmusic.ai/admin/gamejam-plugin-instructions";
//            Application.OpenURL(theSite);
//        }
//    }
//}
