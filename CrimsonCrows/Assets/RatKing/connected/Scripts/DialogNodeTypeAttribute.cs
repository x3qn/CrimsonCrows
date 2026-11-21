namespace RatKing.Connected.Usage {

	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
	public class DialogNodeTypeAttribute : System.Attribute {
		public string NodeType { get; private set; } = string.Empty;
		public DialogNodeTypeAttribute(string identifier) {
			NodeType = identifier;
		}
	}

}