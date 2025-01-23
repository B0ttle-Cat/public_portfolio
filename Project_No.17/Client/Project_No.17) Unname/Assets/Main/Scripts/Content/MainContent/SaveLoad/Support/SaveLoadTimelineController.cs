using System;
using System.Collections.Generic;

using BC.Base;

using UnityEngine;
using UnityEngine.Playables;

namespace TFContent
{
	public class SaveLoadTimelineController : MonoBehaviour, ISaveLoad
	{
		[Serializable]
		public struct TimelineControllerData
		{
			public AnimatorData animatorData;
			public PlayableDirectorData playableDirectorData;
		}
		[Serializable]
		public struct PlayableDirectorData
		{
			public List<PlayableDirectorParameter> parameterList;
		}
		[Serializable]
		public struct PlayableDirectorParameter
		{
			public PlayState state;
			public double time;
			internal DirectorUpdateMode timeUpdateMode;
			internal DirectorWrapMode extrapolationMode;
			internal double initialTime;
			internal bool playOnAwake;

			public bool enabled { get; internal set; }
		}

		[Serializable]
		public struct AnimatorData
		{
			public List<AnimatorParameter> parameterList;
		}
		[Serializable]
		public struct AnimatorParameter
		{
			public AnimatorControllerParameterType type;
			public int intValue;
			public float floatValue;
			public bool boolValue;
		}

		public ISaveLoad.SaveLoadData? Save(SaveLoader saveLoader, string savePath)
		{
			if(!TryGetComponent<TimelineController>(out var timelineController)) return null;
			TimelineControllerData timelineControllerData = new TimelineControllerData();

			timelineControllerData.animatorData = GetAnimatorData();
			timelineControllerData.playableDirectorData = GetPlayableDirectorData();
			AnimatorData GetAnimatorData()
			{
				Animator animator = timelineController.GetComponent<Animator>();

				AnimatorData animatorData = new ();
				animatorData.parameterList = new List<AnimatorParameter>();

				int parameterCount = animator.parameterCount;
				for(int i = 0 ; i<parameterCount ; i++)
				{
					var par = animator.GetParameter(i);
					AnimatorParameter? parameterData = par.type switch {
						AnimatorControllerParameterType.Float => new AnimatorParameter  {
							type = par.type,
							floatValue = animator.GetFloat(par.name)
						},
						AnimatorControllerParameterType.Int => new AnimatorParameter  {
							type = par.type,
							floatValue = animator.GetInteger(par.name)
						},
						AnimatorControllerParameterType.Bool => new AnimatorParameter  {
							type = par.type,
							boolValue = animator.GetBool(par.name)
						},
						AnimatorControllerParameterType.Trigger => new AnimatorParameter  {
							type = par.type,
							boolValue = animator.GetBool(par.name)
						},
						_ => null
					};
					if(parameterData.HasValue)
						animatorData.parameterList.Add(parameterData.Value);
				}
				return animatorData;
			}
			PlayableDirectorData GetPlayableDirectorData()
			{
				PlayableDirectorData playableDirectorData = new ();
				playableDirectorData.parameterList = new List<PlayableDirectorParameter>();

				int stateMappingsCount = timelineController.stateMappings.Length;
				for(int i = 0 ; i < stateMappingsCount ; i++)
				{
					var stateMapping = timelineController.stateMappings[i];
					PlayableDirectorParameter parameter = new PlayableDirectorParameter(){
						state = stateMapping.playableDirector.state,
						time = stateMapping.playableDirector.time,
						timeUpdateMode = stateMapping.playableDirector.timeUpdateMode,
						enabled = stateMapping.playableDirector.enabled,
						extrapolationMode = stateMapping.playableDirector.extrapolationMode,
						initialTime = stateMapping.playableDirector.initialTime,
						playOnAwake = stateMapping.playableDirector.playOnAwake,
					};
					playableDirectorData.parameterList.Add(parameter);
				}
				return playableDirectorData;
			}


			ISaveLoad.SaveLoadData saveLoadData = new ISaveLoad.SaveLoadData(saveLoader, savePath, timelineControllerData);
			return saveLoadData;
		}

		public void Load(ISaveLoad.SaveLoadData loadData)
		{
			if(!TryGetComponent<TimelineController>(out var timelineController)) return;

			bool oldActive = gameObject.activeSelf;
			gameObject.SetActive(false);
			TimelineControllerData timelineControllerData = JsonUtility.FromJson<TimelineControllerData>(loadData.jsonData);
			SetAnimatorData(timelineControllerData.animatorData);
			SetPlayableDirectorData(timelineControllerData.playableDirectorData);
			void SetAnimatorData(AnimatorData animatorData)
			{
				Animator animator = timelineController.GetComponent<Animator>();
				int parameterCount = animator.parameterCount;
				for(int i = 0 ; i<parameterCount ; i++)
				{
					var par = animator.GetParameter(i);
					switch(par.type)
					{
						case AnimatorControllerParameterType.Float:
						{
							animator.SetFloat(par.name, animatorData.parameterList[i].floatValue);
						}
						break;
						case AnimatorControllerParameterType.Int:
						{
							animator.SetInteger(par.name, animatorData.parameterList[i].intValue);
						}
						break;
						case AnimatorControllerParameterType.Bool:
						{
							animator.SetBool(par.name, animatorData.parameterList[i].boolValue);
						}
						break;
						case AnimatorControllerParameterType.Trigger:
						{
							if(animatorData.parameterList[i].boolValue)
								animator.SetTrigger(par.name);
							else
								animator.ResetTrigger(par.name);
						}
						break;
					};
				}
			}
			void SetPlayableDirectorData(PlayableDirectorData playableDirectorData)
			{
				int stateMappingsCount = timelineController.stateMappings.Length;
				for(int i = 0 ; i < stateMappingsCount ; i++)
				{
					var playableDirector = timelineController.stateMappings[i].playableDirector;
					var parameter = playableDirectorData.parameterList[i];
					playableDirector.time = parameter.time;
					playableDirector.timeUpdateMode = parameter.timeUpdateMode;
					playableDirector.enabled = parameter.enabled;
					playableDirector.extrapolationMode = parameter.extrapolationMode;
					playableDirector.initialTime = parameter.initialTime;
					playableDirector.playOnAwake = parameter.playOnAwake;
					if(parameter.state == PlayState.Playing)
					{
						playableDirector.Play();
					}
					else if(parameter.state == PlayState.Paused)
					{
						playableDirector.Pause();
					}
				}
			}

			gameObject.SetActive(oldActive);
		}
	}
}
