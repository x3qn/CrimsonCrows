using System.Text;

// Minimaler Ersatz für die fehlende RatKing.Base-Hilfsbibliothek.
// Enthält nur, was der connected-Beispielcode benutzt.
namespace RatKing.Base {

	public static class Math {
		/// <summary>True, wenn a und b höchstens um epsilon voneinander abweichen.</summary>
		public static bool Approx(float a, float b, float epsilon = 0.0001f) {
			return System.Math.Abs(a - b) <= epsilon;
		}
	}

	public static class StringExtras {
		/// <summary>
		/// Wandelt Escape-Sequenzen im Text (z. B. \n, \t, \\, \") in die echten Zeichen um.
		/// Wichtig, damit Absätze (zwei Zeilenumbrüche) später richtig getrennt werden.
		/// </summary>
		public static string Unescape(string s) {
			if (string.IsNullOrEmpty(s) || s.IndexOf('\\') < 0) { return s; }
			var sb = new StringBuilder(s.Length);
			for (int i = 0; i < s.Length; ++i) {
				char c = s[i];
				if (c != '\\' || i == s.Length - 1) { sb.Append(c); continue; }
				char n = s[++i];
				switch (n) {
					case 'n': sb.Append('\n'); break;
					case 'r': sb.Append('\r'); break;
					case 't': sb.Append('\t'); break;
					case '\\': sb.Append('\\'); break;
					case '"': sb.Append('"'); break;
					case '\'': sb.Append('\''); break;
					default: sb.Append('\\').Append(n); break; // unbekannt: unverändert lassen
				}
			}
			return sb.ToString();
		}
	}

}
