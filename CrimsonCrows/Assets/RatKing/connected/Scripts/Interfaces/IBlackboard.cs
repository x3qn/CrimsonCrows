using System.Collections;
using System.Collections.Generic;

namespace RatKing.Connected.Usage {

	public interface IBlackboard {
		void OnStartTalking();
		void OnEndTalking();
		void OnReset(); // for testing
		HashSet<string> DialogChoicesMade { get; set; }
		
		void AddTag(string tag);
		void RemoveTag(string tag);
		bool HasTag(string tag);
		
		bool TryGetNumber(string variable, out float value);
		void SetNumber(string variable, float newValue);
		
		bool TryGetString(string variable, out string value);
		void SetString(string variable, string newValue);
	}

}
