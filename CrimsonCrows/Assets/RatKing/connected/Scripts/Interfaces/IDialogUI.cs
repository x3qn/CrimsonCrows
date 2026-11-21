using System.Collections;
using System.Collections.Generic;

namespace RatKing.Connected.Usage {

	public interface IDialogUI {
		DialogNode CurNode { set; }
		Stack<DialogNode> EndNodes { get; }
		
		void AddChoice(DialogNode node, int icon, ref string text);
		void AddTextByNPC(DialogNode node, ref string text, string talkerID);
		
		void ContinueDialog();
	}

}