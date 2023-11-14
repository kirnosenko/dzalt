using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DZALT.Entities
{
	public class DamageLog : Log
	{
		public enum Reason
		{
			HIT = 0,
			UNCONSCIOUS = 1,
			CONSCIOUS = 2,
			SUICIDE = 10,
			MURDER = 11,
			ACCIDENT = 12,
		}

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public int? KillerId { get; set; }
		public float? KillerX { get; set; }
		public float? KillerY { get; set; }
		public float? KillerZ { get; set; }

		public string Details { get; set; }

		public Player Killer { get; set; }
	}
}
