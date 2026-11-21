using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RatKing.Connected.Usage {

	public class ExampleDialogUI : MonoBehaviour, IDialogUI {

		enum TalkMode { None, NPC, Player }

		[Header("Style")]
		[SerializeField] Sprite[] choiceSprites = null;
		[Header("UI Top")]
		[SerializeField] UnityEngine.UI.ScrollRect uiTexts = null;
		[SerializeField] TMPro.TextMeshProUGUI uiProtoText = null;
		[SerializeField] UnityEngine.UI.Button uiButtonContinue = null;
		[Header("UI Bottom")]
		[SerializeField] ExampleDialogUI_Choice uiProtoChoice = null;
		[SerializeField] UnityEngine.UI.ScrollRect uiChoices = null;

		public DialogNode CurNode { private get; set; }
		public Stack<DialogNode> EndNodes { get; private set; } = new Stack<DialogNode>(); // TODO: to be used when dialogs are continuing other dialogs
		public DialogGraph CurGraph { get; private set; }
		public string CurTalkerID { get; private set; }

		TalkMode mode;
		System.Func<string, string> ProcessTextKey;
		System.Action OnQuit;
		string[] curTextsNPC;
		int curTextsNPCIndex;
		readonly List<GameObject> curChoices = new List<GameObject>();
		readonly List<GameObject> allTextObjects = new List<GameObject>();

		//
		
		///////////////////////////////////////////////////////////////////////
		// DIALOG CONTROL
		///////////////////////////////////////////////////////////////////////
		
		// only used when testing
		public void Rewind() {
			if (CurGraph == null) { return; }
			CurGraph.CurDialogChoices.Clear();
			ClearChoices();
			RemoveAllTexts();
			CurNode = CurGraph.StartNodes[0];
			gameObject.SetActive(true);
			ContinueDialog();
		}

		public void Continue() {
			if (curTextsNPCIndex < curTextsNPC.Length - 1) {
				AddTextByNPC(curTextsNPCIndex + 1); // start showing next text
			}
			else {
				ContinueDialog(); // get next dialog node
			}
		}

		/// <summary>
		/// Start a dialog based on the filename. The dialog is a json file, created in Connected
		/// </summary>
		public void StartDialog(DialogGraph graph, System.Func<string, string> processTextKey, System.Action onQuit = null) {
			uiProtoText.gameObject.SetActive(false);
			uiProtoChoice.gameObject.SetActive(false);
			EndNodes.Clear();

			CurGraph = graph;
			ProcessTextKey = processTextKey;
			OnQuit = onQuit;
			
			gameObject.SetActive(true);

			RemoveAllTexts();
			uiTexts.verticalNormalizedPosition = 1f;
			CurNode = CurGraph.StartNodes[0];
			ContinueDialog();
		}

		public void QuitDialog() {
			CurNode = null;
			uiButtonContinue.gameObject.SetActive(false);
			CurGraph?.QuitDialog();
			OnQuit?.Invoke();
		}

		public void ContinueDialog() {
			CurNode ??= EndNodes.Count > 0 ? EndNodes.Pop() : null;
			if (CurNode == null) {
				QuitDialog();
				return;
			}

			mode = TalkMode.None;
			ClearChoices();
			var node = CurNode;
			CurNode = null;
			node.Continue();

			if (mode == TalkMode.None && CurNode == null) {
				ContinueDialog();
			}
		}

		// string handling

		string ProcessDialogLine(string line) {
			// change the text here as you wish, for example interpreting certain symbols so they change the dialog's formatting
			return line;
		}

		string[] ProcessAndSplitText(string textID) {
			if (string.IsNullOrWhiteSpace(textID)) { CurGraph.Log("Empty text!", 1); return new[] { "" }; }
			var text = Base.StringExtras.Unescape(ProcessTextKey(textID));
			//var text = Base.StringExtras.Unescape(SLH.Localisation.Do($"dialogs/{CurLoader.CurDialogID}/{textID}"));
			return text.Split(new[] { "\n\n", "\r\n\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
		}

		///////////////////////////////////////////////////////////////////////
		// PLAYER CHOICES
		///////////////////////////////////////////////////////////////////////

		void ClearChoices() {
			foreach (var choice in curChoices) { Destroy(choice); }
			curChoices.Clear();
		}

		public void AddChoice(DialogNode node, int icon, ref string text) {
			var texts = ProcessAndSplitText(text);
			AddChoice(node, icon, texts, 0);
		}

		public void AddChoice(DialogNode node, int icon, string[] texts, int textIndex) {
			if (texts == null || texts.Length == 0) { CurGraph.Log("Empty choice!", 1); return; }

			if (mode != TalkMode.Player) {
				ClearChoices();
				uiChoices.gameObject.SetActive(true);
				uiChoices.verticalNormalizedPosition = 1f;
				uiButtonContinue.gameObject.SetActive(false);
				mode = TalkMode.Player;
			}

			var uiChoice = Instantiate(uiProtoChoice, uiProtoChoice.transform.parent);
			var processedText = ProcessDialogLine(texts[textIndex]);
			uiChoice.Init(processedText, choiceSprites[icon], () => {
				AddText($"<color=yellow><i>{processedText}</i></color>");
				if (node != null && textIndex < texts.Length - 1) { // this choice node had more than one paragraph?
					ClearChoices();
					AddChoice(node, 0, texts, textIndex + 1); // no icon anyomore
				}
				else { // otherwise, normal proceed
					CurNode = node;
					ContinueDialog();
				}
			});
			curChoices.Add(uiChoice.gameObject);
		}

		///////////////////////////////////////////////////////////////////////
		// NPC DIALOG
		///////////////////////////////////////////////////////////////////////

		public void AddTextByNPC(DialogNode node, ref string text, string talkerID = null) {
			CurTalkerID = string.IsNullOrEmpty(talkerID) ? CurGraph.CurMainTalkerID : talkerID;
			curTextsNPC = ProcessAndSplitText(text); // TODO: can be not key but full text
			if (curTextsNPC.Length == 0) { CurGraph.Log("Empty text!", 1); return; }
			for (int i = 0; i < curTextsNPC.Length; ++i) { curTextsNPC[i] = ProcessDialogLine(curTextsNPC[i]); }
			CurNode = node;
			AddTextByNPC(0);
		}

		void AddTextByNPC(int textIndex) {
			curTextsNPCIndex = textIndex;
			mode = TalkMode.NPC;
			uiChoices.gameObject.SetActive(false);
			uiButtonContinue.gameObject.SetActive(true);
			AddText(curTextsNPC[textIndex]);
		}
		
		///////////////////////////////////////////////////////////////////////
		//
		///////////////////////////////////////////////////////////////////////

		void AddText(string text) {
			if (string.IsNullOrEmpty(text)) { return; }
			var uiText = Instantiate(uiProtoText, uiProtoText.transform.parent);
			uiText.gameObject.SetActive(true);
			uiText.text = text.Trim();
			allTextObjects.Add(uiText.gameObject);
			StartCoroutine(ScrollDownCR());
		}

		IEnumerator ScrollDownCR() {
			yield return null;
			uiTexts.verticalNormalizedPosition = 0f;
		}

		public void RemoveAllTexts() {
			foreach (var t in allTextObjects) { Destroy(t); }
			allTextObjects.Clear();
		}
	}

}