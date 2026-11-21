using RatKing.Connected.Usage;
	
[DialogNodeType("beg")]
public class DialogNode_Start : DialogNode {
	public DialogNode_Start(DialogGraph graph, int id) : base(graph, id) { }

	protected override bool OnLoadJSON(SimpleJSON.JSONNode json) {
		return true;
	}
}