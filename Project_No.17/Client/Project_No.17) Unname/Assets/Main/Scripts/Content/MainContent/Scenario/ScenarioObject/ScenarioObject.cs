using System;
using System.Collections.Generic;

using BC.Base;
using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

using Debug = UnityEngine.Debug;
namespace TFContent
{
	public class ScenarioObject : ObjectBehaviour//, IOdccUpdate
	{
		public enum ScenarioState
		{
			Waiting = 0,
			Playing = 1,
			Finished = 2
		}

		public ObserverValue<ScenarioState> scenarioSaveState;
		public bool pause;

		private PlayableDirector playableDirector;

		protected override void BaseAwake()
		{
			playableDirector = GetComponent<PlayableDirector>();
			playableDirector.playOnAwake = false;
			playableDirector.Stop();
			playableDirector.time = 0;
		}
		#region Controller
		internal async Awaitable Play()
		{
			scenarioSaveState.Value = ScenarioState.Playing;
			pause = false;

			await Awaitable.NextFrameAsync();

			scenarioSaveState.Value = ScenarioState.Finished;
		}

		internal void Stop()
		{
			scenarioSaveState.Value = ScenarioState.Finished;
		}

		internal void Pause()
		{
			pause = true;
		}

		internal void Resume()
		{
			pause = false;
		}
		#endregion
		#region PlayableDirector

		[Button("Print Timeline Hierarchy")]
		public void TimelineObjectBinding()
		{
			if(playableDirector == null)
			{
				playableDirector = GetComponent<PlayableDirector>();
				if(playableDirector == null)
				{
					Debug.LogError("PlayableDirector가 설정되지 않았습니다.");
					return;
				}
			}

			TimelineAsset timelineAsset = playableDirector.playableAsset as TimelineAsset;
			if(timelineAsset == null)
			{
				Debug.LogError("PlayableDirector에 연결된 TimelineAsset이 없습니다.");
				return;
			}


			Dictionary<string, List<TrackAsset>> trackBindingPath = new Dictionary<string, List<TrackAsset>>();
			foreach(var track in timelineAsset.GetRootTracks())
			{
				string path = track.name;
				GetTrackBindingType(track);
				if(track is GroupTrack groupTrack)
				{
					PrintTrackHierarchy(groupTrack, path);
				}
				else
				{
					if(!trackBindingPath.TryGetValue(path, out var trackAssets))
					{
						trackAssets = new List<TrackAsset>();
						trackBindingPath.Add(path, trackAssets);
					}
					trackAssets.Add(track);
				}
			}
			void PrintTrackHierarchy(GroupTrack parentTrack, string trackPath)
			{
				foreach(var childTrack in parentTrack.GetChildTracks())
				{
					GetTrackBindingType(childTrack);
					if(childTrack is GroupTrack groupTrack)
					{
						PrintTrackHierarchy(groupTrack, $"{trackPath}/{groupTrack.name}");
					}
					else
					{
						if(!trackBindingPath.TryGetValue(trackPath, out var trackAssets))
						{
							trackAssets = new List<TrackAsset>();
							trackBindingPath.Add(trackPath, trackAssets);
						}
						trackAssets.Add(childTrack);
					}
				}
			}

			foreach(var trackItem in trackBindingPath)
			{
				string path = trackItem.Key;

				GameObject bindingObject = GameObject.Find(path);
				if(bindingObject == null)
				{
					Debug.LogError($"해당 오브젝트를 찾을 수 없습니다: {path}");
					continue;
				}
				List<TrackAsset> trackAssets = trackItem.Value;
				int length = trackAssets.Count;
				for(int i = 0 ; i < length ; i++)
				{
					BindingTrack(bindingObject, trackAssets[i]);
				}
			}

			void BindingTrack(GameObject bindingObject, TrackAsset track)
			{
				Type bindingType = GetTrackBindingType(track);

				if(bindingType != null)
				{
					var component = bindingObject.GetComponentInChildren(bindingType, true);
					if(component != null)
					{
						playableDirector.SetGenericBinding(track, component);
						Debug.Log($"트랙({track.name}:<color=white>{bindingType.Name}</color>)에 오브젝트({bindingObject.name})가 바인딩 되었습니다.");
					}
					else
					{
						Debug.LogError($"트랙({track.name}:<color=red>{bindingType.Name}</color>)에 오브젝트({bindingObject.name})를 바인딩 할 수 없습니다.");
					}
				}
				Type GetTrackBindingType(TrackAsset track)
				{
					foreach(var output in track.outputs)
					{
						// 트랙 출력 타입 가져오기
						return output.outputTargetType;
					}
					return null; // 출력이 없을 경우 null 반환
				}
			}
		}
		private Type GetTrackBindingType(TrackAsset track)
		{
			foreach(var output in track.outputs)
			{
				// 트랙 출력 타입 가져오기
				Debug.Log("GetTrackBindingType:" + track.name+ ":" + output.outputTargetType.Name);
			}
			return null; // 출력이 없을 경우 null 반환
		}

		#endregion
	}
}