using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TweetSharp;

namespace TwitterFollowerCopier
{
    class Program
    {
        static void Main(string[] args)
        {
            var oldAccount = "O_Kahia";
            var newAccount = "O_Kahyaoglu";

            OAuthAccessToken access;
            var service = Authentication(out access);
            var users = FindAllFollowing(service, access);
            SaveAllMembersInTextFile(users);
        }

        private static void SaveAllMembersInTextFile(Dictionary<long, string> users)
        {
            var desktopFolder = new DirectoryInfo( Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            var folder = desktopFolder.CreateSubdirectory("TwitterFollowerCopier");
            var str = string.Join(Environment.NewLine, users.Select(u => $"{u.Key},{u.Value}"));
            var csv = new FileInfo(Path.Combine(folder.FullName, "users.csv"));
            File.WriteAllText(csv.FullName, str);
            Console.WriteLine("Saved users to path " + csv.FullName);
        }

        private static Dictionary<long, string> FindAllFollowing(TwitterService service, OAuthAccessToken access)
        {
            long? cursor = -1;
            var counter = 1;
            var users = new Dictionary<long, string>();
            while (true)
            {
                var following = service.ListFriends(new ListFriendsOptions()
                {
                    IncludeUserEntities = false,
                    SkipStatus = true,
                    Cursor = cursor,
                    UserId = access.UserId
                });
                if (following == null)
                {
                    Console.WriteLine("API limiti bitti.");
                    break;
                }

                cursor = following.NextCursor;
                if (following.Count == 0)
                    break;
                following.ForEach(f => users.Add(f.Id, f.Name));
                following.ForEach(f => Console.WriteLine($"{counter++:D5}: {f.Name}"));
            }

            return users;
        }

        private static TwitterService Authentication(out OAuthAccessToken access)
        {
            var twiConsumerKey = "xv5IIJfLygyDg4L78M0jHuF61";
            var twiConsumerSecret = "vk6R7TXhShLzyI3yZlSgwA9AU7sqDcvaBNG1f5IMbiXaZunB6H";

            // Pass your credentials to the service
            TwitterService service = new TwitterService(twiConsumerKey, twiConsumerSecret);

            // Step 1 - Retrieve an OAuth Request Token
            OAuthRequestToken requestToken = service.GetRequestToken();

            // Step 2 - Redirect to the OAuth Authorization URL
            Uri uri = service.GetAuthorizationUri(requestToken);
            Process.Start(uri.ToString());
            var pinCode = Console.ReadLine(); // <-- This is input into your application by your user
            // Step 3 - Exchange the Request Token for an Access Token
            access = service.GetAccessToken(requestToken, pinCode);

            // Step 4 - User authenticates using the Access Token
            service.AuthenticateWith(access.Token, access.TokenSecret);
            return service;
        }
    }
}
