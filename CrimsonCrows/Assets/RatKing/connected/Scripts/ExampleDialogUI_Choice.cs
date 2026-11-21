using UnityEngine;

namespace RatKing.Connected.Usage {

	public class ExampleDialogUI_Choice : MonoBehaviour {
		[SerializeField] TMPro.TextMeshProUGUI uiText = null;
		[SerializeField] UnityEngine.UI.Image uiIcon = null;
		[SerializeField] UnityEngine.UI.Button uiBtn = null;
		
		//

		public void Init(string text, Sprite icon, System.Action onClick) {
			gameObject.SetActive(true);
			uiText.SetText(text);
			if (icon != null) { uiIcon.sprite = icon; }
			else { uiIcon.enabled = false; }
			uiBtn.onClick.AddListener(() => onClick?.Invoke());
		}
	}

}
