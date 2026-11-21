using System.Collections.Generic;
using UnityEngine;

namespace RatKing.Connected.Usage {

	public struct DialogInfo {
		public int fileVersion;
	}

	public class DialogGraph {

		static readonly char[] dialogIDTrimChars = new[] { ' ', '$', '%' };

		public DialogInfo Info { get; private set; }
		public List<DialogNode> StartNodes { get; private set; } = new();
		public IBlackboard CurBlackboard { get; private set; }
		public IDialogUI UI { get; private set; }
		public string CurMainTalkerID { get; private set; }
		public string CurDialogID { get; private set; }
		public HashSet<string> CurDialogChoices { get; private set; } = new();

		// dialog IDs are necessary for the CHOICE nodes that can only be picked once per dialog, or ever
		readonly List<string> dialogIDs = new();

		public delegate DialogNode CreateNodeDelegate(DialogGraph graph, string nodeTypeName, int id);
		public delegate void LogDelegate(string message, int level); // level 0..2

		LogDelegate onLog;

		//

		public DialogGraph(IBlackboard blackboard, IDialogUI ui) {
			CurBlackboard = blackboard;
			UI = ui;
		}

		public string GetCurrentDialogID() {
			return dialogIDs[dialogIDs.Count - 1].Trim(dialogIDTrimChars);
		}

		public bool Create(CreateNodeDelegate createNode, LogDelegate log, ref string contents, string dialogID, string talkerID) {
			if (log != null) { onLog = log; }

			if (string.IsNullOrWhiteSpace(contents)) {
				onLog?.Invoke($"Dialog content is empty!", 2);
				return false;
			}

			CurDialogChoices.Clear();
			CurDialogID = dialogID;
			CurMainTalkerID = talkerID;

			var jsonDialog = SimpleJSON.JSONNode.Parse(contents);
			var jsonInfo = jsonDialog["info"];
			dialogIDs.Add($"{CurDialogID}_{dialogIDs.Count}");
			Info = new DialogInfo() {
				fileVersion = jsonInfo["version"],
			};
			var jsonNodes = jsonDialog["nodes"].AsArray;

			// load the single nodes, fill each with data
			var countNodes = jsonNodes.Count;
			var connections = new Dictionary<int, List<List<int>>>(countNodes);
			var nodesByID = new Dictionary<int, DialogNode>(countNodes);
			for (int i = 0; i < countNodes; ++i) {
				var jsonNode = jsonNodes[i];
				var nodeType = jsonNode["$type"].Value;
				var dialogNode = createNode?.Invoke(this, nodeType, jsonNode["$id"]);
				if (dialogNode == null) {
					onLog?.Invoke($"Unknown node type '{nodeType}' found!", 1);
					return false;
				}
				if (!dialogNode.LoadJSON(jsonNode)) {
					onLog?.Invoke($"Error in dialog! Node ({i}) {dialogNode.ID}/{nodeType}", 1);
					return false;
				}
				if (dialogNode.PortsOutCount > 0) {
					connections.Add(dialogNode.ID, new List<List<int>>());
					// get connections
					var portsOut = jsonNode["$conns"].AsArray;
					for (int c = 0; c < dialogNode.PortsOutCount; ++c) {
						var port = portsOut[c].AsArray;
						int count = port == null ? 0 : port.Count;
						var l = new List<int>(count);
						for (int t = 0; t < count; ++t) { l.Add(port[t]); }
						connections[dialogNode.ID].Add(l);
					}
				}

				nodesByID.Add(dialogNode.ID, dialogNode);

				if (nodeType == "beg") {
					StartNodes.Add(dialogNode);
				}
			}

			// now connect all nodes
			foreach (var node in connections) {
				var source = nodesByID[node.Key];
				for (int i = 0, iCount = node.Value.Count; i < iCount; ++i) {
					for (int j = 0, jCount = node.Value[i].Count; j < jCount; j += 2) {
						var target = nodesByID[node.Value[i][j]];
						var targetPortInIdx = node.Value[i][j + 1];
						source.AddNextNode(i, target, targetPortInIdx);
					}
				}
			}

			return true;
		}

		public void Log(string message, int level) {
			onLog?.Invoke(message, level);
		}

		public void QuitDialog() {
			CurBlackboard.OnEndTalking();
		}
	}

}