using RatKing.Connected.Usage; 

[DialogNodeType("con")]
public class DialogNode_Condition : DialogNode {
	System.Func<bool> onFulfilled = () => false;

	//

	public DialogNode_Condition(DialogGraph graph, int id) : base(graph, id) { }

	protected override void OnContinue() {
		Execute(0);
	}

	protected override void OnExecute(int inPortIdx) {
		ExecuteChildren(onFulfilled != null && onFulfilled() ? 0 : 1);
	}

	protected override bool OnLoadJSON(SimpleJSON.JSONNode json) {
		switch (json["conditionType"].AsInt) {
			case 0: // has tag
				 {
					var variable = json["variable"].Value;
					onFulfilled = () => graph.CurBlackboard.HasTag(variable);
					break;
				}
			case 1: // compare number
				{
					var variable = json["variable"].Value;
					var value = json["value"].AsFloat;
					float CurValue() { return graph.CurBlackboard.TryGetNumber(variable, out var curValue) ? curValue : 0f; }
					switch (json["compareType"].AsInt) { // "=", "≠", "<", ">", "≤", "≥"
						default:
						case 0: onFulfilled = () => RatKing.Base.Math.Approx(value, CurValue(), 0.001f); break;
						case 1: onFulfilled = () => !RatKing.Base.Math.Approx(value, CurValue(), 0.001f); break;
						case 2: onFulfilled = () => CurValue() < value; break;
						case 3: onFulfilled = () => CurValue() > value; break;
						case 4: onFulfilled = () => CurValue() <= value; break;
						case 5: onFulfilled = () => CurValue() >= value; break;
					}
				}
				break;
			case 2: // compare string
				{
					var variable = json["variable"].Value;
					var value = json["value"].Value;
					string CurValue() { return graph.CurBlackboard.TryGetString(variable, out var curValue) ? curValue : ""; }
					switch (json["compareType"].AsInt) { // "=", "≠"
						default:
						case 0: onFulfilled = () => value == CurValue(); break;
						case 1: onFulfilled = () => value != CurValue(); break;
					}
				}
				break;
			case 3: // compare flags
				{
					var variable = json["variable"].Value;
					var value = json["value"].AsUInt;
					int CurValue() { return graph.CurBlackboard.TryGetNumber(variable, out var curValueFloat) ? (int)curValueFloat : 0; }
					switch (json["compareType"].AsInt) { // "Single Set", "Group Set", "Exact", "Single Not Set", "Group Not Set"
						default:
						case 0: onFulfilled = () => (CurValue() & value) != 0; break;
						case 1: onFulfilled = () => (CurValue() & value) == value; break;
						case 2: onFulfilled = () => CurValue() == value; break;
						case 3: onFulfilled = () => (CurValue() & value) != value; break;
						case 4: onFulfilled = () => (CurValue() & value) == 0; break;
					}
				}
				break;
			case 4: // custom script
				{
					var script = json["script"].Value;
					if (string.IsNullOrWhiteSpace(script)) { return false; }
#if CONNECTED_TEST_MODE
					onFulfilled = () => { return UnityEngine.Random.value < 0.5f; };
#else
					// TODO: YOUR CODE HERE TO ACTUALLY EXECUTE THE SCRIPT
#endif
				}
				break;
			default:
				return false;
		}
		return true;
	}
}