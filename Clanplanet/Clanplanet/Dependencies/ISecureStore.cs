using Clanplanet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Clanplanet.Dependencies
{
    public interface ISecureStore
    {
        string UserName { get; }

        string Password { get; }

        string ClanId { get; }

        bool Reminder { get; }

        bool AutoRemind { get; }

        Cookie SessionCookie { get; }

        void StoreCookie(Cookie cookie);

        void SaveCredentials(Login login);

        void DeleteCredentials();

        bool DoCredentialsExist();

        Login GetLogin();
    }
}
