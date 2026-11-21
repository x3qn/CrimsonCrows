using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RatKing.Connected.Usage {

	public class ExampleDialog : MonoBehaviour {
		public TextAsset jsonDialog = null;
		public TextAsset jsonLocalisation = null;
		public string talkerID = "NPC";

		[System.NonSerialized] public string dialogName = "";
		[System.NonSerialized] public string jsonDialogContent = "";
		[System.NonSerialized] public string jsonLocalisationContent = "";
		[System.NonSerialized] public System.Action OnExit = null;
		[System.NonSerialized] public DialogGraph.CreateNodeDelegate OnCreateNode;
		[System.NonSerialized] public DialogGraph.LogDelegate OnLog;

		ExampleDialogUI uiDialog;
		IBlackboard blackboard;

		//

		void OnEnable() {
			if (string.IsNullOrWhiteSpace(jsonDialogContent)) {
				if (jsonDialog == null || !Begin()) { Exit(); }
			}
		}

		bool Begin() {
			if (jsonDialog != null && string.IsNullOrWhiteSpace(jsonDialogContent)) { jsonDialogContent = jsonDialog.text; dialogName = jsonDialog.name; }
			if (string.IsNullOrWhiteSpace(jsonDialogContent)) { Debug.LogWarning("No Dialog defined", this); Exit(); return false; }

			if (uiDialog == null) { uiDialog = GetComponentInChildren<ExampleDialogUI>(true); }

			// a blackboard is needed to save the different variables inside a dialog
			// and also to "remember" which player choices were already done
			blackboard = new ExampleBlackboard();

			// connected (usually) doesn't save the texts in the dialog file, but in an extra localisation file
			// ATTENTION: in the lite version, this is not the case
			if (jsonLocalisation != null && string.IsNullOrWhiteSpace(jsonLocalisationContent)) { jsonLocalisationContent = jsonLocalisation.text; }
			var hasTextsFile = !string.IsNullOrWhiteSpace(jsonLocalisationContent);
			Dictionary<string, string> textsByKey = hasTextsFile ? new() : null;
			if (hasTextsFile) {
				// create a dictionary out of the json data for the texts, so they have the needed form
				// (e.g. "dialogs/<dialogID>/GRH64R" = "Greetings traveler!")
				static void FillTextsDictionary(Dictionary<string, string> textsByKey, string curKey, SimpleJSON.JSONNode field) {
					if (field.IsString) { textsByKey[curKey] = field.Value; return; }
					foreach (var key in field.Keys) { FillTextsDictionary(textsByKey, (curKey == "" ? "" : curKey + "/") + key, field[key]); }
				}
				FillTextsDictionary(textsByKey, "", SimpleJSON.JSON.Parse(jsonLocalisationContent));
			}

			// the method to create nodes
			OnCreateNode ??= CreateNode;
			OnLog ??= Log;

			// first create the dialog graph, then let the UI know about it
			var graph = new DialogGraph(blackboard, uiDialog);
			if (!graph.Create(OnCreateNode, OnLog, ref jsonDialogContent, dialogName, talkerID)) { Exit(); return false; }
			gameObject.SetActive(true);
			uiDialog.StartDialog(graph, hasTextsFile ? textKey => textsByKey[$"dialogs/{uiDialog.CurGraph.CurDialogID}/{textKey}"] : textKey => textKey);

			return true;
		}

		//

		public bool Activate() {
			if (!Begin()) { gameObject.SetActive(false); return false; }
			return true;
		}

		public void Restart() {
			if (uiDialog == null) { return; }
			blackboard.OnReset();
			uiDialog.Rewind();
		}

		public void TalkAgain() {
			if (uiDialog == null) { return; }
			uiDialog.Rewind();
		}

		public void Exit() {
			gameObject.SetActive(false);
			OnExit?.Invoke();
		}
		
		// logging

		void Log(string message, int level) {
			switch (level) {
				default: Debug.Log(message); break;
				case 1: Debug.LogWarning(message); break;
				case 2: Debug.LogError(message); break;
			}
		}

		// creation of different node types

		Dictionary<string, System.Type> dialogNodeTypes = null;
		
		// create nodes via reflection and an attribute
		// from https://stackoverflow.com/questions/63362034/using-reflection-to-get-all-classes-that-implement-a-interface-and-only-initiali
		DialogNode CreateNode(DialogGraph graph, string nodeTypeName, int id) {
			if (dialogNodeTypes == null) {
				var types = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(DialogNode).IsAssignableFrom(p));
				dialogNodeTypes = new Dictionary<string, System.Type>();
				foreach (var type in types) {
					var att = type.GetCustomAttribute<DialogNodeTypeAttribute>(false);
					var attNodeType = att?.NodeType;
					if (!string.IsNullOrWhiteSpace(attNodeType)) { dialogNodeTypes[attNodeType] = type; }
				}
			}

			if (dialogNodeTypes.TryGetValue(nodeTypeName, out var nodeType)) { return (DialogNode)System.Activator.CreateInstance(nodeType, graph, id); }

			return null;
		}
	}

}