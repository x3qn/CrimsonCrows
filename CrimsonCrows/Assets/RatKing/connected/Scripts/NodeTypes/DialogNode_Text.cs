using RatKing.Connected.Usage; 

[DialogNodeType("txt")]
public class DialogNode_Text : DialogNode {
	string textKey;
	int byType = 0; // (0) self, (1) someone else
	string by = "";

	//

	public DialogNode_Text(DialogGraph graph, int id) : base(graph, id) { }

	protected override void OnExecute(int inPortIdx) {
		graph.UI.AddTextByNPC(this, ref textKey, by);
	}

	protected override bool OnLoadJSON(SimpleJSON.JSONNode json) {
		textKey = json["text"];
		byType = json["byType"];
		if (byType != 0) { by = json["by"]; }
		return true;
	}
}