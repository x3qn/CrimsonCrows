using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RatKing.Connected.Usage {

	public class ExampleBlackboard : IBlackboard {
		public HashSet<string> DialogChoicesMade { get; set; } = new HashSet<string>();
		public bool HasTalkedWith { get; set; } = false;
		
		readonly HashSet<string> tags = new HashSet<string>();
		readonly Dictionary<string, string> strings = new Dictionary<string, string>();
		readonly Dictionary<string, float> numbers = new Dictionary<string, float>();

		
		public void OnStartTalking() {
		}

		public void OnEndTalking() {
			// could delete temporary values here
		}

		public void OnReset() {
			// for testing
			HasTalkedWith = false;
			tags.Clear();
			strings.Clear();
			numbers.Clear();
		}

		//

		public void AddTag(string tag) {
			tags.Add(tag);
		}

		public void RemoveTag(string tag) {
			tags.Remove(tag);
		}

		public bool HasTag(string tag) {
			return tags.Contains(tag);
		}

		public void SetNumber(string variable, float newValue) {
			numbers[variable] = newValue;
			Debug.Log("set number " + variable + " to " + newValue);
		}

		public void SetString(string variable, string newValue) {
			strings[variable] = newValue;
			Debug.Log("set string " + variable + " to " + newValue);
		}

		public bool TryGetNumber(string variable, out float value) {
			if (numbers.TryGetValue(variable, out value)) {
				Debug.Log("number " + variable + ": " + value);
				return true;
			}
			else {
				Debug.Log("number " + variable + " does not exist");
				return false;
			}
		}

		public bool TryGetString(string variable, out string value) {
			if (strings.TryGetValue(variable, out value)) {
				Debug.Log("string " + variable + ": " + value);
				return true;
			}
			else {
				Debug.Log("string " + variable + " does not exist");
				return false;
			}
		}
	}

}
