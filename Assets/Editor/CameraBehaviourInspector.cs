using UnityEngine;
using System.Collections;
using UnityEditor;

//This is a custom inspector for my third person camera control
[CustomEditor(typeof(CameraControl))]
public class CameraBehaviourInspector : Editor 
{
    //VARIABLES
    //bools for hiding the inspector variables in seperate sections
	bool gizmosEnabled = false;
	bool distanceEnabled = false;
	bool variablesEnabled = false;
	bool debugValuesEnabled = false;

	public override void OnInspectorGUI ()
	{
		CameraControl cam = (CameraControl)target;
	
		//Draw Gizmo options.
		gizmosEnabled = EditorGUILayout.Foldout (gizmosEnabled, "Gizmo Commands");
		if(gizmosEnabled)
		{
			EditorGUILayout.HelpBox("This area is only relevant when the camera is locked", MessageType.Info);
			EditorGUILayout.LabelField("Draw Movement Bounds");
			//EditorGUILayout.BeginHorizontal ();
			cam.drawXZone = EditorGUILayout.Toggle ("X", cam.drawXZone);
			cam.drawYZone = EditorGUILayout.Toggle ("Y", cam.drawYZone);
			cam.drawZZone = EditorGUILayout.Toggle ("Z", cam.drawZZone);
			//EditorGUILayout.EndHorizontal ();
		}

		//Draw Distance inputs
		distanceEnabled = EditorGUILayout.Foldout (distanceEnabled, "Distance Variables");
		if(distanceEnabled)
		{
			EditorGUILayout.HelpBox("This area is for the standard camera distances", MessageType.Info);
            //Target (player)
            cam.target = (Transform)EditorGUILayout.ObjectField("Camera Target", cam.target, typeof(Transform), true);
			//X Variables
			EditorGUILayout.LabelField("X variables");
			EditorGUILayout.BeginVertical();
			cam.disBaseX = EditorGUILayout.Slider("Base X Position", cam.disBaseX, -10, 10);
			cam.disVarianceX = EditorGUILayout.Slider("X Variance", cam.disVarianceX, 0, 10);
			EditorGUILayout.Separator();
			EditorGUILayout.EndVertical();
			//Y Variables
			EditorGUILayout.LabelField("Y variables");
			EditorGUILayout.BeginVertical();
			cam.disBaseY = EditorGUILayout.Slider("Base Y Position", cam.disBaseY, -10, 10);
			cam.disVarianceY = EditorGUILayout.Slider("Y Variance", cam.disVarianceY, 0, 10);
			EditorGUILayout.Separator();
			EditorGUILayout.EndVertical();
			//Z Variables
			EditorGUILayout.LabelField("Z variables");
			EditorGUILayout.BeginVertical();
			cam.disBaseZ = EditorGUILayout.Slider("Base Z Position", cam.disBaseZ, -10, 10);
			cam.disVarianceZ = EditorGUILayout.Slider("Z Variance", cam.disVarianceZ, 0, 10);
			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
            //Orbit Base Pitch
            EditorGUILayout.LabelField("Orbit variables");
            EditorGUILayout.BeginVertical();
            cam.basePitch = EditorGUILayout.Slider("Orbit Base Pitch", cam.basePitch, cam.yMinLimit, cam.yMaxLimit);
            cam.yMinLimit = EditorGUILayout.IntSlider("Pitch minimum angle", cam.yMinLimit, 0, 45);
            cam.yMaxLimit = EditorGUILayout.IntSlider("Pitch maximum angle", cam.yMaxLimit, 0, 45);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
        }

		//Draw variable inputs
		variablesEnabled = EditorGUILayout.Foldout (variablesEnabled, "Control Variables");
		if(variablesEnabled)
		{
			EditorGUILayout.HelpBox("This area is the camera controls and speeds", MessageType.Info);
			EditorGUILayout.LabelField("Mouse/Joystick Controls");
			EditorGUILayout.LabelField("Mouse/Joystick Dead Zone");
			cam.deadZone = EditorGUILayout.Slider(cam.deadZone.ToString (), cam.deadZone, 0, 10);
			EditorGUILayout.LabelField("Mouse/Joystick Sensitivity");
			cam.sensitivity = EditorGUILayout.Slider(cam.sensitivity.ToString (), cam.sensitivity, 0, 10);
			EditorGUILayout.Separator();
			//Camera locking.
			cam.cameraLocked = EditorGUILayout.Toggle("Camera Locked", cam.cameraLocked);
			EditorGUILayout.LabelField("Player Control Lock Duration");
			cam.playerContLockDuration = EditorGUILayout.Slider(cam.playerContLockDuration.ToString(), cam.playerContLockDuration, 0, 10);
			//Pace variable
			cam.pace = EditorGUILayout.Slider("Chase Speed", cam.pace, 0.1f, 10);
			//Occulusion avoidance speed
			EditorGUILayout.LabelField("Speed of the camera's occlusion avoidance");
			cam.whiskerPanStrength = EditorGUILayout.Slider(cam.whiskerPanStrength.ToString(), cam.whiskerPanStrength, 0, 10);
            //Inverted
            EditorGUILayout.LabelField("Is pitch inverted?");
            cam.isInverted = EditorGUILayout.IntSlider(cam.isInverted == -1? "Yes" : "No" ,cam.isInverted, -1, 1);
        }

		//Draw 'Debug' section
		debugValuesEnabled = EditorGUILayout.Foldout (debugValuesEnabled, "Debug Readout");
		if(debugValuesEnabled)
		{
			EditorGUILayout.HelpBox("This area is for the debug values and options", MessageType.Info);
			cam.dummy = (Transform)EditorGUILayout.ObjectField("Dummy Object", cam.dummy, typeof(Transform), true);
			//EditorGUILayout.LabelField("Show Whisker Raycasts");
			cam.showWhiskers = EditorGUILayout.Toggle("Show Whisker Raycasts", cam.showWhiskers);
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Pan functionality active on axis:");
			//EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField("X: " + cam.panX.ToString());
			EditorGUILayout.LabelField("Y: " + cam.panY.ToString());
			EditorGUILayout.LabelField("Z: " + cam.panZ.ToString());
			//EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Player is controlling pitch/yaw");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField("Pitch: " + cam.playerCont_Pitch.ToString());
            EditorGUILayout.LabelField(cam.playerPitchTimer.ToString());
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Yaw: " + cam.playerCont_Yaw.ToString());
            EditorGUILayout.LabelField(cam.playerYawTimer.ToString());
            EditorGUILayout.EndHorizontal ();
		}
		//base.OnInspectorGUI ();
	}
}
