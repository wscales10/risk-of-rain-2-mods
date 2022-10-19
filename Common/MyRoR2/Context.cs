using RoR2;
using System;
using System.Collections.Generic;
using Utils;
using Utils.Reflection;
using Utils.Reflection.Properties;

namespace MyRoR2
{
	public struct Context : IEquatable<Context>
	{
		public MyScene SceneName { get; set; }

		public SceneType SceneType { get; set; }

		public int? StageNumber { get; set; }

		public int? WaveNumber { get; set; }

		public int? LoopIndex { get; set; }

		public Entity BossBodyName { get; set; }

		public IList<Entity> Bosses { get; set; }

		public bool IsBossEncounter { get; set; }

		public TeleporterInteraction.ActivationState? TeleporterState { get; set; }

		public int ScenePart { get; set; }

		public RunType RunType { get; set; }

		public Entity Survivor { get; set; }

		public RunOutcome? Outcome { get; set; }

		public static bool operator ==(Context c1, Context c2) => c1.Equals(c2);

		public static bool operator !=(Context c1, Context c2) => !c1.Equals(c2);

		public override bool Equals(object obj) => obj is Context context && Equals(context);

		public bool Equals(Context c) => this.PropertywiseEquals(c);

		public override int GetHashCode() => this.MyHashCode();
	}
}