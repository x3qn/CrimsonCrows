using RatKing.Connected.Usage;

[DialogNodeType("cho")]
public class DialogNode_Choice : DialogNode {
	string textKey;
	int icon = 0; // " ", "!", "X", "◀"
	string iconID; // TODO: not used right now
	int onlyOnce = 0; // "Always", "Once Now", "Once Ever"

	//

	public DialogNode_Choice(DialogGraph graph, int id) : base(graph, id) { }

	protected override void OnContinue() {
		base.OnContinue();
		if (onlyOnce == 1) { graph.CurDialogChoices.Add($"{graph.GetCurrentDialogID()}_{ID}"); }
		if (onlyOnce == 2) { graph.CurBlackboard.DialogChoicesMade.Add($"{graph.GetCurrentDialogID()}_{ID}"); }
	}

	protected override void OnExecute(int inPortIdx) {
		if (onlyOnce == 1 && graph.CurDialogChoices.Contains($"{graph.GetCurrentDialogID()}_{ID}")) { return; }
		if (onlyOnce == 2 && graph.CurBlackboard.DialogChoicesMade.Contains($"{graph.GetCurrentDialogID()}_{ID}")) { return; }
		graph.UI.AddChoice(this, icon, ref textKey);
	}

	protected override bool OnLoadJSON(SimpleJSON.JSONNode json) {
		textKey = json["text"];
		icon = json["icon"];
		iconID = json["iconID"];
		onlyOnce = json["once"];
		return true;
	}
}