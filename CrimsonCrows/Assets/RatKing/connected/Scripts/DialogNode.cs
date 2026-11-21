using System.Collections;
using System.Collections.Generic;

namespace RatKing.Connected.Usage {

	public abstract class DialogNode {

		struct Next {
			public DialogNode node;
			public int portInIdx;
		}

		public int PortsOutCount { get; private set; } // can get changed by node configs!
		public int ID { get; private set; }
		protected DialogGraph graph;
		List<List<Next>> next;

		static int curExecutionDepth = 0;

		//

		public DialogNode(DialogGraph graph, int id) {
			this.graph = graph;
			ID = id;
			next = new List<List<Next>>();
		}

		// executing

		public void Continue() {
			curExecutionDepth = 0;
			OnContinue();
		}

		public void Execute(int inPortIdx) {
			++curExecutionDepth;
			if (curExecutionDepth > 1000) {
				graph.Log("Node Execution Stack Overflow at " + GetType().Name + " (" + ID + ")", 2);
				graph.QuitDialog();
				return;
			}
			OnExecute(inPortIdx);
		}

		protected virtual void OnContinue() {
			for (int i = 0; i < PortsOutCount; ++i) {
				ExecuteChildren(i);
			}
		}

		protected virtual void OnExecute(int inPortIdx) {
		}

		//

		protected void ExecuteChildren(int portIndex) {
			if (next == null || portIndex < 0 || portIndex >= next.Count || next[portIndex] == null) { return; }
			foreach (var n in next[portIndex]) {
				n.node.Execute(n.portInIdx);
			}
		}

		// next nodes

		public void AddNextNode(int portOutIdx, DialogNode target, int targetPortInIdx) {
			if (next == null) { next = new List<List<Next>>(1); }
			while (portOutIdx >= next.Count) { next.Add(new List<Next>()); }
			next[portOutIdx].Add(new Next() { node = target, portInIdx = targetPortInIdx });
		}

		// loading

		public bool LoadJSON(SimpleJSON.JSONNode json) {
			PortsOutCount = json["$conns"].Count;
			return OnLoadJSON(json);
		}

		protected abstract bool OnLoadJSON(SimpleJSON.JSONNode json);
	}
}