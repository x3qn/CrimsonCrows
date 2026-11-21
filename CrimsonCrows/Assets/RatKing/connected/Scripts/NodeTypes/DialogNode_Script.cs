using RatKing.Connected.Usage;
	
[DialogNodeType("scr")]
public class DialogNode_Script : DialogNode {
	string script;

	//

	public DialogNode_Script(DialogGraph graph, int id) : base(graph, id) { }

	protected override void OnExecute(int inPortIdx) {
		if (string.IsNullOrWhiteSpace(script)) { return; }
#if !CONNECTED_TEST_MODE
		// TODO: YOUR CODE HERE TO ACTUALLY EXECUTE THE SCRIPT
		// TODO: USE ExecuteChildren() to continue the flow if needed
#endif
	}
		
	protected override bool OnLoadJSON(SimpleJSON.JSONNode json) {
		script = json["script"];
		return true;
	}
}