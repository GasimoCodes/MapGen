using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ALOB.Map
{
	/// <summary>
	/// Class to contain a catalogue of rooms designated to spawn in a certain zone. Abstracted to prevent defining the rooms in the inspector directly and to provide additional support down the road.
	/// </summary>
	[System.Serializable]
	public class catalogueEntry : ICloneable
	{
		public RoomData room;

		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}
}