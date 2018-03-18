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
    /// <summary>
    /// 15 dkda bir 15 requeste inizn veriyor twitter, 300 300 takip ettigim kulanıcıları çekiyorum.
    /// </summary>
    class Program
    {
        static DirectoryInfo desktopFolder => new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        static DirectoryInfo folder = desktopFolder.GetDirectories("TwitterFollowerCopier").FirstOrDefault()?? new DirectoryInfo(Path.Combine(desktopFolder.FullName, "TwitterFollowerCopier"));
        static FileInfo csv => new FileInfo(Path.Combine(folder.FullName, "users.csv"));
        static long? lastCursor = -1;

        static void Main(string[] args)
        {
            if (!folder.Exists)
                folder.Create();
            OAuthAccessToken access;
            var service = Authentication(out access);
            var users = FindAllFollowing(service, access);
            SaveAllMembersInTextFile(users);
        }

        private static void SaveAllMembersInTextFile(Dictionary<long, string> users)
        {
            var str = string.Join(Environment.NewLine, users.Select(u => $"{u.Key};{u.Value}"));
            str += Environment.NewLine + lastCursor;
            File.AppendAllText(csv.FullName, str);
            Console.WriteLine("Saved users to path " + csv.FullName);
        }

        private static Dictionary<long, string> FindAllFollowing(TwitterService service, OAuthAccessToken access)
        {
            var pastFetches = csv.Exists ? File.ReadAllLines(csv.FullName) : null;
            lastCursor = Convert.ToInt64(pastFetches?.LastOrDefault() ?? "-1");
            var counter = 1;
            var users = new Dictionary<long, string>();
            while (true)
            {
                var following = service.ListFriends(new ListFriendsOptions()
                {
                    IncludeUserEntities = false,
                    SkipStatus = true,
                    Cursor = lastCursor,
                    UserId = access.UserId
                });
                if (following == null)
                {
                    Console.WriteLine("API limiti bitti.");
                    break;
                }

                lastCursor = following.NextCursor;
                if (following.Count == 0)
                    break;
                following.ForEach(f => users.Add(f.Id, f.ScreenName));
                following.ForEach(f => Console.WriteLine($"{counter++:D5}: {f.ScreenName}"));
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
