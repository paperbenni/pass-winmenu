using System.Collections.Generic;
using System.IO.Abstractions;

namespace PassWinmenu.PasswordManagement
{
	internal interface IPasswordManager
	{
		IEnumerable<PasswordFile> GetPasswordFiles();

		IEnumerable<PasswordFile> GetPasswordFiles(IDirectoryInfo subDirectory);

		KeyedPasswordFile DecryptPassword(PasswordFile file, bool passwordOnFirstLine);

		PasswordFile EncryptPassword(DecryptedPasswordFile file);
		
		PasswordFile AddPassword(string path, string password, string metadata);
	}
}
