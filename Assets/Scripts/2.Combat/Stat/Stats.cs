using System;
using System.Collections.Generic;
using UnityEngine;

namespace bkTools
{
	[DisallowMultipleComponent]
	public class Stats : MonoBehaviour
	{
		[SerializeField] private List<StatSO> statDefinitions = new();
		[SerializeField] private List<Stat> stats = new();

		readonly Dictionary<string, Stat> idToStat = new();
		bool initialized;

		void Awake()
		{
			RebuildFromDefinitions();
		}

		void Update()
		{
			float dt = Time.deltaTime;
			for (int i = 0; i < stats.Count; i++)
			{
				stats[i].Tick(dt);
			}
		}

		public IReadOnlyList<Stat> All => stats;

		public bool TryGet(string id, out Stat stat)
		{
			if (!initialized) BuildIndex();
			return idToStat.TryGetValue(id, out stat);
		}

		public Stat GetOrCreate(string id, float min = 0f, float max = 100f, float start = 100f, float regenPerSecond = 0f)
		{
			if (!initialized) BuildIndex();
			if (idToStat.TryGetValue(id, out var stat)) return stat;
			// 우선 정의에서 찾기
			var def = statDefinitions.Find(s => s != null && s.Id == id);
			if (def != null)
			{
				var createdFromDef = def.CreateRuntimeInstance();
				stats.Add(createdFromDef);
				idToStat[id] = createdFromDef;
				return createdFromDef;
			}
			// 정의가 없으면 이전 방식 호환
			var created = new Stat();
			created.Initialize(id, min, max, start, regenPerSecond);
			stats.Add(created);
			idToStat[id] = created;
			return created;
		}

		public void AddOrSet(Stat value)
		{
			if (value == null) return;
			if (!initialized) BuildIndex();
			if (idToStat.TryGetValue(value.Id, out var existing))
			{
				int index = stats.IndexOf(existing);
				stats[index] = value;
				idToStat[value.Id] = value;
			}
			else
			{
				stats.Add(value);
				idToStat[value.Id] = value;
			}
		}

		public void Remove(string id)
		{
			if (!initialized) BuildIndex();
			if (idToStat.TryGetValue(id, out var stat))
			{
				stats.Remove(stat);
				idToStat.Remove(id);
			}
		}

		void BuildIndex()
		{
			idToStat.Clear();
			for (int i = 0; i < stats.Count; i++)
			{
				var s = stats[i];
				if (s == null || string.IsNullOrEmpty(s.Id)) continue;
				idToStat[s.Id] = s;
			}
			initialized = true;
		}

		public void RebuildFromDefinitions()
		{
			stats.Clear();
			for (int i = 0; i < statDefinitions.Count; i++)
			{
				var def = statDefinitions[i];
				if (def == null) continue;
				stats.Add(def.CreateRuntimeInstance());
			}
			initialized = false;
			BuildIndex();
		}
	}
}


