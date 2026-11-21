using RatKing.Connected.Usage; 

[DialogNodeType("set")]
public class DialogNode_SetVar : DialogNode {
	System.Action onExecute = null;

	//

	public DialogNode_SetVar(DialogGraph graph, int id) : base(graph, id) { }

	protected override void OnExecute(int inPortIdx) {
		onExecute?.Invoke();
	}

	protected override bool OnLoadJSON(SimpleJSON.JSONNode json) {
		switch (json["varType"].AsInt) {
			case 0: // add tag
				{
					var tag = json["variable"].Value;
					onExecute = () => graph.CurBlackboard.AddTag(tag);
				}
				break;
			case 1: // remove tag
				{
					var tag = json["variable"].Value;
					onExecute = () => graph.CurBlackboard.RemoveTag(tag);
				}
				break;
			case 2: // number
				{
					var variable = json["variable"].Value;
					var value = json["value"].AsFloat;
					switch (json["setType"].AsInt) { // "=", "+", "-"
						case 0: // set value
							if (json["owMode"].AsInt == 1) {
								onExecute = () => {
									if (graph.CurBlackboard.TryGetNumber(variable, out var _)) { return; }
									graph.CurBlackboard.SetNumber(variable, value);
								};
							}
							else {
								onExecute = () => graph.CurBlackboard.SetNumber(variable, value);
							}
							break;
						case 1: // add value
							onExecute = () => {
								if (!graph.CurBlackboard.TryGetNumber(variable, out var oldValue)) { oldValue = 0f; }
								graph.CurBlackboard.SetNumber(variable, oldValue + value);
							};
							break;
						case 2: // subtract value
							onExecute = () => {
								if (!graph.CurBlackboard.TryGetNumber(variable, out var oldValue)) { oldValue = 0f; }
								graph.CurBlackboard.SetNumber(variable, oldValue - value);
							};
							break;
					}
				}
				break;
			case 3: // string
				{
					var variable = json["variable"].Value;
					var value = json["value"].Value;
					if (json["owMode"].AsInt == 1) {
						onExecute = () => {
							if (graph.CurBlackboard.TryGetString(variable, out var _)) { return; }
							graph.CurBlackboard.SetString(variable, value);
						};
					}
					else {
						onExecute = () => graph.CurBlackboard.SetString(variable, value);
					}
				}
				break;
			case 4: // flags
				{
					var variable = json["variable"].Value;
					var value = json["value"].AsUInt;
					switch (json["changeType"].AsInt) { // "Set Additionally [OR]", "Filter [AND]", "Set Exactly [=]", "Unset Group"
						case 0:
							onExecute = () => {
								var oldValue = graph.CurBlackboard.TryGetNumber(variable, out var oldValueFloat) ? (uint)oldValueFloat : 0;
								graph.CurBlackboard.SetNumber(variable, oldValue | value);
							};
							break;
						case 1:
							onExecute = () => {
								var oldValue = graph.CurBlackboard.TryGetNumber(variable, out var oldValueFloat) ? (uint)oldValueFloat : 0;
								graph.CurBlackboard.SetNumber(variable, oldValue & value);
							};
							break;
						case 2:
							if (json["owMode"].AsInt == 1) {
								onExecute = () => {
									if (graph.CurBlackboard.TryGetNumber(variable, out var _)) { return; }
									graph.CurBlackboard.SetNumber(variable, value);
								};
							}
							else {
								onExecute = () => graph.CurBlackboard.SetNumber(variable, value);
							}
							break;
						case 3:
							onExecute = () => {
								var oldValue = graph.CurBlackboard.TryGetNumber(variable, out var oldValueFloat) ? (uint)oldValueFloat : 0;
								graph.CurBlackboard.SetNumber(variable, oldValue & (~value));
							};
							break;
					}
				}
				break;
			default:
				return false;
		}
		return true;
	}
}