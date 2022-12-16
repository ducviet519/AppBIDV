using System;
using System.DirectoryServices;

namespace AppBIDV.Authorizations
{
    public class ActiveDirectoryHelper
    {
        private string _path;
        //private string _filterAttribute;

        public ActiveDirectoryHelper(string path)
        {
            _path = path;
        }

        public bool IsAuthenticated(string domainName, string userName, string password)
        {
            string domainAndUsername = domainName + @"\" + userName;
            DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, password);

            try
            {
                // Bind to the native AdsObject to force authentication.
                var obj = entry.NativeObject;

                DirectorySearcher search = new DirectorySearcher(entry)
                {
                    Filter = "(SAMAccountName=" + userName + ")"
                };
                search.PropertiesToLoad.Add("cn");

                SearchResult result = search.FindOne();

                if (null == result)
                {
                    return false;
                }
                // Update the new path to the user in the directory
                _path = result.Path;
                //_filterAttribute = (string)result.Properties["cn"][0];
                
            }
            catch(Exception ex)
            {
                var errorMessage = ex.Message;
                return false;
            }
            return true;
        }
    }
}
