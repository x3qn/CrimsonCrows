using RatKing.Connected.Usage;

[DialogNodeType("swi")]
public class DialogNode_Switch : DialogNode {
	System.Func<int> onChoose = null;

	//

	public DialogNode_Switch(DialogGraph graph, int id) : base(graph, id) { }

	protected override void OnContinue() {
		Execute(0);
	}

	protected override void OnExecute(int inPortIdx) {
		ExecuteChildren(onChoose());
	}

	protected override bool OnLoadJSON(SimpleJSON.JSONNode json) {
		switch (json["switchType"].AsInt) {
			case 0: // number
				{
					var variable = json["variable"].Value;
					var cases = new float[PortsOutCount];
					for (int i = 1; i < PortsOutCount; ++i) { cases[i] = json["valNum" + i].AsFloat; }
					onChoose = () => {
						if (!graph.CurBlackboard.TryGetNumber(variable, out var curValue)) { curValue = 0f; }
						for (int i = 1; i < PortsOutCount; ++i) { if (UnityEngine.Mathf.Approximately(cases[i], curValue)) { return i; } }
						return 0;
					};
				}
				break;
			case 1: // string
				{
					var variable = json["variable"].Value;
					var cases = new string[PortsOutCount];
					for (int i = 1; i < PortsOutCount; ++i) { cases[i] = json["valStr" + i].Value; }
					onChoose = () => {
						if (!graph.CurBlackboard.TryGetString(variable, out var curValue)) { curValue = ""; }
						for (int i = 1; i < PortsOutCount; ++i) { if (cases[i] == curValue) { return i; } }
						return 0;
					};
				}
				break;
			case 2: // random
				{
					var cases = new float[PortsOutCount];
					var totalFactor = 0f;
					for (int i = 0; i < PortsOutCount; ++i) {
						cases[i] = UnityEngine.Mathf.Max(0f, json["valRnd" + i].AsFloat);
						totalFactor += cases[i];
					}
					onChoose = () => {
						int chosen = -1;
						var random = UnityEngine.Random.Range(0f, totalFactor);
						while (random > 0f) { chosen++; random -= cases[chosen]; }
						return chosen;
					};
				}
				break;
			default:
				return false;
		}
		return true;
	}
}