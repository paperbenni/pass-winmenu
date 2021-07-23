using System.Collections.Generic;
using System.IO.Abstractions;

namespace PassWinmenu.ExternalPrograms
{
	internal interface ICryptoService
	{
		string Decrypt(string file);
		void Encrypt(string data, string outputFile, bool overwrite, params string[] recipients);
		string GetVersion();
		List<string> GetRecipients(string file);
		string FindShortKeyId(string target);
	}
}
