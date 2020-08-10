using Clanplanet.Dependencies;
using Clanplanet.Models;
using Clanplanet.Pages;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Clanplanet.Service
{
    public class LoginService
    {
        //public static event EventHandler<LoginFinishedEventArgs> LoginFinished;
        //public static event EventHandler<HaveEventsEventArgs> HaveEvents;
        //public static event EventHandler<HaveCWDetailsEventArgs> HaveCWDetails;
        //public static event EventHandler<PostEventArgs> SubscribeFinished;
        //public static event EventHandler<PostEventArgs> UnSubscribeFinished;

        public static Login CurrentLogin;

        public static Cookie CurrentCookie;

        private static bool loggingIn;
        public static void PerformLogin(Login login, Action<bool, Cookie> onComplete = null)
        {
            try
            {
                if (!loggingIn || true)
                {
                    loggingIn = true;
                    //var storedCookie = GetStoredCookie();
                    //if (CheckCookie(storedCookie))
                    //{
                    //    CurrentCookie = storedCookie;
                    //    OnLoginFinished(true, storedCookie);
                    //}
                    //else
                    //{
                    CurrentLogin = login;
                    string formUrl = "https://www.clanplanet.de/index.asp?rn=";
                    string formParams = string.Format("session=start&kennung={0}&passwort={1}", login.Username.Replace(' ', '+'), login.Password);
                    string formParamsWithoutPassword = string.Format("session=start&kennung={0}", login.Username.Replace(' ', '+'));
                    WebRequest req = WebRequest.Create(formUrl + "&device=m");
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.Method = "POST";
                    byte[] bytes = Encoding.UTF8.GetBytes(formParams);
                    if (GlobalErrorValues.Current.IsCollectingReportData)
                    {
                        SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = formParamsWithoutPassword, Origin = "PerformLogin" });
                    }

                    req.BeginGetRequestStream(ar =>
                    {
                        using (var stream =
                            req.EndGetRequestStream(ar))
                        {
                            stream.Write(bytes, 0, bytes.Length);
                        }
                        WebResponse antwort = null;
                        req.BeginGetResponse(rr =>
                        {
                            antwort = req.EndGetResponse(rr);
                            if (GlobalErrorValues.Current.IsCollectingReportData)
                            {
                                using (StreamReader leser = new StreamReader(antwort.GetResponseStream(), Encoding.UTF8))
                                {
                                    if (leser != null)
                                    {
                                        var inhalt = leser.ReadToEnd();
                                        if (inhalt != null)
                                        {
                                            SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = inhalt, Origin = "PerformLogin" });
                                        }
                                    }
                                    leser.Dispose();
                                }
                            }
                            var cookie = new Cookie("ClanSession", antwort.Headers["Set-cookie"]);
                            CurrentCookie = cookie;
                            //DependencyService.Get<ISecureStore>().StoreCookie(cookie);
                            if (antwort.Headers["Set-cookie"].Contains("sls="))
                            {
                                OnLoginFinished(true, cookie, onComplete);
                            }
                            else
                            {
                                OnLoginFinished(false, null, onComplete);
                            }
                        }, req);
                    }, req);
                    //}
                }
            }
            catch (Exception)
            {
                loggingIn = false;
            }
        }

        private static void OnLoginFinished(bool loggedin, Cookie login, Action<bool, Cookie> onComplete)
        {
            onComplete?.Invoke(loggedin, login);
            loggingIn = false;
            //LoginFinished?.Invoke(null, new LoginFinishedEventArgs(loggedin, login));
        }

        private static Cookie GetStoredCookie()
        {
            return DependencyService.Get<ISecureStore>().SessionCookie;
        }

        private static bool CheckCookie(Cookie cookieToCheck = null)
        {
            if (cookieToCheck != null)
            {
                if (cookieToCheck.Value.Contains("sls="))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool gettingClans;
        public static void GetClans(Action<List<Clan>> onComplete = null)
        {
            try
            {
                if (!gettingClans || true)
                {
                    gettingClans = true;
                    if (CurrentCookie != null && CurrentCookie.Value != null && CheckCookie(CurrentCookie))
                    {
                        string getUrl = "https://www.clanplanet.de/account.asp?rn=";
                        WebRequest getRequest = WebRequest.Create(getUrl + "&device=m");
                        getRequest.Headers["Cookie"] = CurrentCookie.Value;
                        WebResponse antwort = null;
                        getRequest.BeginGetResponse(ar =>
                        {
                            antwort = getRequest.EndGetResponse(ar);
                            using (StreamReader leser = new StreamReader(antwort.GetResponseStream(), Encoding.UTF8))
                            {
                                if (leser != null)
                                {
                                    var inhalt = leser.ReadToEnd();
                                    if (inhalt != null)
                                    {
                                        if (GlobalErrorValues.Current.IsCollectingReportData)
                                        {
                                            SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = inhalt, Origin = "GetClans" });
                                        }
                                        List<Clan> clans = new List<Clan>();
                                        var table = inhalt;
                                        HtmlDocument doctable = new HtmlDocument();
                                        doctable.LoadHtml(table);
                                        var clanTables = doctable.DocumentNode.Descendants("table").Where(x => x.ChildNodes.Count >= 2 && x.InnerText.StartsWith( "\r\n\r\n\tDeine Clanmitgliedschaften")).First().Descendants("table");
                                        foreach (var clanTable in clanTables)
                                        {
                                            var props = clanTable.Elements("tr").ToList();
                                            int idAddition = props.Count - 6;
                                            var clanName = GetRidOfStupidCodes( props[0].Elements("td").ToList()[1].InnerText);
                                            var clanRang = GetRidOfStupidCodes(props[3 + idAddition].Elements("td").ToList()[1].InnerText);
                                            DateTime clanSince = DateTime.ParseExact(props[4 + idAddition].Elements("td").ToList()[1].InnerText, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                                            var clanId = props[5 + idAddition].Elements("td").ToList()[1].ChildNodes["a"].Attributes["href"].Value;
                                            clanId = clanId.Remove(0, clanId.IndexOf("&id=") + 4);

                                            clans.Add(new Clan() { Name = clanName, Id = clanId, Rang = clanRang, MemberSinceDT = clanSince });
                                        }
                                        gettingClans = false;
                                        onComplete?.Invoke(clans);
                                    }
                                }
                                leser.Dispose();
                            }
                        }, getRequest);
                    }
                    else
                    {
                        onComplete?.Invoke(null);
                        gettingClans = false;
                    }
                }
            }
            catch (Exception)
            {
                gettingDetails = false;
            }
        }

        private static bool gettingDetails;
        public static void GetEventDetails(ClanEvent cEvent, Action<ClanEvent> onComplete = null)
        {
            try
            {
                if (!gettingDetails || true)
                {
                    gettingDetails = true;
                    if (CurrentCookie != null && CurrentCookie.Value != null && CheckCookie(CurrentCookie))
                    {
                        string getUrl = string.Format("http://www.clanplanet.de/_sites/events_edit.asp?rn=&clanid={0}&action=mld&id={1}&show=", cEvent.ClanId, cEvent.Id);
                        WebRequest getRequest = WebRequest.Create(getUrl + "&device=m");
                        getRequest.Headers["Cookie"] = CurrentCookie.Value;
                        WebResponse antwort = null;
                        getRequest.BeginGetResponse(ar =>
                        {
                            antwort = getRequest.EndGetResponse(ar);
                            using (StreamReader leser = new StreamReader(antwort.GetResponseStream(), Encoding.UTF8))
                            {
                                if (leser != null)
                                {
                                    var inhalt = leser.ReadToEnd();
                                    if (inhalt != null)
                                    {
                                        if (GlobalErrorValues.Current.IsCollectingReportData)
                                        {
                                            SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = inhalt, Origin = "GetEventDetails" });
                                        }
                                        List<ClanEvent> clanEvents = new List<ClanEvent>();
                                        cEvent.Meldungen.Clear();
                                        if (inhalt.IndexOf("<table><tr><th width=\"80\">") > 0)
                                        {
                                            var table = inhalt.Remove(0, inhalt.IndexOf("<table><tr><th width=\"80\">"));
                                            table = table.Remove(table.IndexOf("</table>"));
                                            HtmlDocument doctable = new HtmlDocument();
                                            doctable.LoadHtml(table);
                                            foreach (var item in doctable.DocumentNode.ChildNodes[0].ChildNodes)
                                            {
                                                if (item.ChildNodes.Count == 4 && item.ChildNodes[0].InnerText != "Meldung")
                                                {
                                                    MeldeStatus meldeStatus = item.ChildNodes[0].InnerText == "Anmeldung" ? MeldeStatus.Anmeldung : MeldeStatus.Abwesend;
                                                    string username = item.ChildNodes[1].ChildNodes[0].InnerText;
                                                    string bemerkung = item.ChildNodes[2].InnerText;
                                                    DateTime meldeTag = DateTime.ParseExact(item.ChildNodes[3].InnerText, "dd.MM.", CultureInfo.InvariantCulture);

                                                    cEvent.Meldungen.Add(new Meldung()
                                                    {
                                                        User = GetRidOfStupidCodes(username),
                                                        Status = meldeStatus,
                                                        MeldeTag = meldeTag,
                                                        Bemerkung = GetRidOfStupidCodes(bemerkung) });
                                                }
                                            }
                                        }
                                        clanEvents.Add(cEvent);
                                        gettingDetails = false;
                                        //OnHaveEvents(clanEvents);
                                        onComplete?.Invoke(cEvent);
                                    }
                                }
                                leser.Dispose();
                            }
                        }, getRequest);
                    }
                    else
                    {
                        //OnHaveEvents(null);
                        onComplete?.Invoke(null);
                        gettingDetails = false;
                    }
                }
            }
            catch (Exception)
            {
                gettingDetails = false;
            }
        }

        private static bool gettingCWDetails;
        public static void GetClanWarDetails(ClanEvent cEvent, Action<ClanEvent> onComplete = null)
        {
            try
            {
                if (!gettingCWDetails || true)
                {
                    gettingCWDetails = true;
                    if (CurrentCookie != null && CurrentCookie.Value != null && CheckCookie(CurrentCookie))
                    {
                        string getUrl = string.Format("http://www.clanplanet.de/_sites/clanwar_details.asp?rn=&clanid={0}&id={1}", cEvent.ClanId, cEvent.Id);
                        WebRequest getRequest = WebRequest.Create(getUrl + "&device=m");
                        getRequest.Headers["Cookie"] = CurrentCookie.Value;
                        WebResponse antwort = null;
                        getRequest.BeginGetResponse(ar =>
                        {
                            antwort = getRequest.EndGetResponse(ar);
                            using (StreamReader leser = new StreamReader(antwort.GetResponseStream(), Encoding.UTF8))
                            {
                                if (leser != null)
                                {
                                    var inhalt = leser.ReadToEnd();
                                    if (inhalt != null)
                                    {
                                        if (GlobalErrorValues.Current.IsCollectingReportData)
                                        {
                                            SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = inhalt, Origin = "GetClanWarDetails" });
                                        }
                                        if (inhalt.IndexOf("<table class=\"w100\">") > 0)
                                        {
                                            var table = inhalt.Remove(0, inhalt.IndexOf("<table class=\"w100\">"));
                                            table = table.Remove(table.IndexOf("</table>"));
                                            HtmlDocument doctable = new HtmlDocument();
                                            doctable.LoadHtml(table);
                                            var cwdPart = doctable.DocumentNode.ChildNodes[0];
                                            var enemyNamePart = cwdPart.ChildNodes[2].ChildNodes[1];
                                            string kontakt = "";
                                            if (enemyNamePart.ChildNodes.Count > 2)
                                            {
                                                kontakt = enemyNamePart.ChildNodes[2].InnerText;
                                                kontakt = kontakt.Remove(0, kontakt.LastIndexOf("(Kontakt: ") + 10);
                                                kontakt = kontakt.Remove(kontakt.Length - 1);
                                            }
                                            ClanWarDetails cwDetails = new ClanWarDetails()
                                            {
                                                GegnerName = GetRidOfStupidCodes(enemyNamePart.Element("a").InnerText),
                                                Kürzel = GetRidOfStupidCodes(enemyNamePart.ChildNodes[0].InnerText.Remove(enemyNamePart.ChildNodes[0].InnerText.Length - 3)),
                                                Kontakt = GetRidOfStupidCodes(kontakt),
                                                GegnerLink = GetRidOfStupidCodes( enemyNamePart.Element("a").GetAttributeValue("href", "")),
                                                Spiel = GetRidOfStupidCodes(cwdPart.ChildNodes[3].ChildNodes[1].InnerText),
                                                Squad = GetRidOfStupidCodes(cwdPart.ChildNodes[4].ChildNodes[1].InnerText),
                                                Ort = GetRidOfStupidCodes(cwdPart.ChildNodes[5].ChildNodes[1].InnerText),
                                                Details = GetRidOfStupidCodes(cwdPart.ChildNodes[6].ChildNodes[1].InnerText),
                                                Sichtbarkeit = GetRidOfStupidCodes(cwdPart.ChildNodes[7].ChildNodes[1].InnerText),
                                                ZuschauerOrt = GetRidOfStupidCodes(cwdPart.ChildNodes[8].ChildNodes[1].InnerText),
                                                Teilnahme = GetRidOfStupidCodes(cwdPart.ChildNodes[9].ChildNodes[1].InnerText),
                                                Ergebnis = cwdPart.ChildNodes[10].ChildNodes[1].InnerText.Trim()
                                            };
                                            cEvent.CwDetails = cwDetails;
                                        }
                                        OnHaveCWDetails(cEvent, onComplete);
                                    }
                                }
                                leser.Dispose();
                            }
                        }, getRequest);
                    }
                    else
                    {
                        OnHaveCWDetails(null, onComplete);
                    }
                }
            }
            catch (Exception)
            {
                gettingCWDetails = false;
            }
        }

        private static void OnHaveCWDetails(ClanEvent cEvent, Action<ClanEvent> onComplete)
        {
            onComplete?.Invoke(cEvent);
            gettingCWDetails = false;
            //HaveCWDetails?.Invoke(null, new HaveCWDetailsEventArgs(cEvent));
        }

        public static void GetEvents(int month, string cid = null, Action<bool, List<ClanEvent>> onComplete = null)
        {
            try
            {
                if (!gettingEvents || true)
                {
                    gettingEvents = true;
                    if (string.IsNullOrWhiteSpace(cid) && CurrentLogin != null)
                    {
                        cid = CurrentLogin.ClanID;
                    }
                    if (CurrentCookie != null && CurrentCookie.Value != null && CheckCookie(CurrentCookie) && !string.IsNullOrEmpty(cid))
                    {
                        string getUrl = "http://www.clanplanet.de/_sites/events.asp?rn=&clanid=" + cid;
                        if (month == 0)
                        {
                            getUrl += "&show=previous";
                        }
                        else if (month == 2)
                        {
                            getUrl += "&show=next";
                        }
                        WebRequest getRequest = WebRequest.Create(getUrl);
                        getRequest.Headers["Cookie"] = CurrentCookie.Value;
                        WebResponse antwort = null;
                        getRequest.BeginGetResponse(ar =>
                        {
                            antwort = getRequest.EndGetResponse(ar);
                            using (StreamReader leser = new StreamReader(antwort.GetResponseStream(), Encoding.UTF8))
                            {
                                if (leser != null)
                                {
                                    var inhalt = leser.ReadToEnd();
                                    if (inhalt != null)
                                    {
                                        if (GlobalErrorValues.Current.IsCollectingReportData)
                                        {
                                            SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = inhalt, Origin = "GetEvents" });
                                        }

                                        #region GetEvents
                                        List<ClanEvent> clanEvents = new List<ClanEvent>();
                                        int tableIndex = inhalt.IndexOf("<table class=\"w100\"><tr><th colspan=\"4\">");
                                        if (tableIndex > 0)
                                        {
                                            var table = inhalt.Remove(0, tableIndex);
                                            table = table.Remove(table.IndexOf("</table>"));

                                            int groupTableIndex = inhalt.IndexOf("<table class=\"w100\"><tr><th colspan=\"3\">");
                                            if (groupTableIndex > 0)
                                            {
                                                var groupTable = inhalt.Remove(0, groupTableIndex);
                                                groupTable = groupTable.Remove(tableIndex - groupTableIndex);
                                                if (groupTable.Contains(">Neues Event eintragen</a></div>"))
                                                {
                                                    CurrentLogin.CanEditEvents = true;
                                                }
                                                else
                                                {
                                                    CurrentLogin.CanEditEvents = false;
                                                }
                                            }
                                            else
                                            {
                                                CurrentLogin.CanEditEvents = false;
                                            }

                                            HtmlDocument doctable = new HtmlDocument();
                                            doctable.LoadHtml(table);
                                            foreach (var item in doctable.DocumentNode.ChildNodes[0].ChildNodes)
                                            {
                                                if (item.ChildNodes.Count == 4 && item.ChildNodes[2].InnerText != "&nbsp;")
                                                {
                                                    string datestring = item.ChildNodes[1].InnerText.Remove(6);

                                                    var eventItems = item.ChildNodes[2].Elements("span").ToList();
                                                    var brObjects = item.ChildNodes[3].InnerHtml.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
                                                    for (int i = 0; i < eventItems.Count; i++)
                                                    {
                                                        string dateTimeString = datestring + " " + eventItems[i].InnerText.Remove(eventItems[i].InnerText.IndexOf('|'));
                                                        DateTime date = DateTime.ParseExact(dateTimeString, "dd.MM. HH:mm ", CultureInfo.InvariantCulture);
                                                        HtmlNode extrasNode = null;
                                                        if (brObjects.Length > i)
                                                        {
                                                            HtmlDocument extrasDoc = new HtmlDocument();
                                                            extrasDoc.LoadHtml(brObjects[i]);
                                                            extrasNode = extrasDoc.DocumentNode;
                                                        }
                                                        string idStr = string.Empty;
                                                        bool canMeld = false;
                                                        MeldeStatus status = MeldeStatus.Ungemeldet;
                                                        if (extrasNode != null)
                                                        {
                                                            int idPos = -1;
                                                            int editIdPos = -1;
                                                            var links = extrasNode.Elements("a").ToList();
                                                            if (links.Count > 0)
                                                            {
                                                                idStr = links.ToList()[0].Attributes[0].Value;
                                                                idPos = idStr.IndexOf("&id=");
                                                                editIdPos = idStr.IndexOf("eventid=");
                                                            }
                                                            if (idPos > 0)
                                                            {
                                                                idStr = idStr.Remove(0, idPos + 4).Remove(6);
                                                            }
                                                            else if (editIdPos > 0)
                                                            {
                                                                idStr = idStr.Remove(0, editIdPos + 8).Remove(6);
                                                            }
                                                            else
                                                            {
                                                                idStr = eventItems[i].InnerHtml;
                                                                int idPos2 = idStr.IndexOf("&id=");
                                                                if (idPos2 > 0)
                                                                {
                                                                    idStr = idStr.Remove(0, idPos2 + 4).Remove(6);
                                                                }
                                                                else
                                                                {
                                                                    idStr = string.Empty;
                                                                }
                                                            }

                                                            canMeld = extrasNode.InnerHtml.Contains("button_minus");

                                                            if (extrasNode.InnerHtml.Contains("button_haken"))
                                                            {
                                                                status = MeldeStatus.Anmeldung;
                                                                canMeld = true;
                                                            }
                                                            else if (extrasNode.InnerHtml.Contains("button_x"))
                                                            {
                                                                status = MeldeStatus.Abwesend;
                                                                canMeld = true;
                                                            }
                                                        }

                                                        bool isClanwar = eventItems[i].InnerHtml.Contains("degen_mini");

                                                        string eventFullName = eventItems[i].InnerText.Remove(0, eventItems[i].InnerText.IndexOf('|') + 1).Trim();
                                                        int meldungenCount = -1;
                                                        string eventShortName = eventFullName;
                                                        int meldungenStart = eventFullName.LastIndexOf('(');
                                                        if (meldungenStart > 0)
                                                        {
                                                            string meldungenCountString = eventFullName.Remove(0, meldungenStart + 1);
                                                            int meldungenEnd = meldungenCountString.IndexOf('M');
                                                            if (meldungenEnd > 0)
                                                            {
                                                                eventShortName = eventFullName.Remove(meldungenStart - 1);
                                                                meldungenCountString = meldungenCountString.Remove(meldungenEnd -1);
                                                                int.TryParse(meldungenCountString, out meldungenCount);
                                                            }
                                                        }

                                                        ClanEvent clanEvent = new ClanEvent()
                                                        {
                                                            Day = date,
                                                            DateString = dateTimeString,
                                                            EventName = GetRidOfStupidCodes(eventShortName),
                                                            FullName = GetRidOfStupidCodes(eventFullName),
                                                            ClanId = cid,
                                                            MeldungenCount = meldungenCount,
                                                            ClanWar = isClanwar,
                                                            Id = idStr,
                                                            Meldung = status,
                                                            MeldungAllowed = canMeld
                                                        };
                                                        clanEvents.Add(clanEvent);
                                                    }
                                                }
                                            }
                                            onComplete?.Invoke(true, clanEvents);
                                        }
                                        else
                                        {
                                            onComplete?.Invoke(false, null);
                                        }
                                        #endregion
                                    }
                                }
                                leser.Dispose();
                            }
                        }, getRequest);
                    }
                    else
                    {
                        onComplete?.Invoke(false, null);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private static string GetRidOfStupidCodes(string input)
        {
            return HtmlEntity.DeEntitize(input);
            //return input.Replace("&#228;", "ä").Replace("&#246;", "ö").Replace("&#252;", "ü").Replace("&#223;", "ß");
        }

        private static bool isSubscribing;
        public static void SubscribeEvent(ClanEvent cEvent, bool abwesend, string bemerkung, Action<bool> onComplete = null)
        {
            try
            {
                if (!isSubscribing || true)
                {
                    isSubscribing = true;
                    if (CurrentCookie != null && CurrentCookie.Value != null && CheckCookie(CurrentCookie))
                    {
                        if (string.IsNullOrEmpty(bemerkung))
                        {
                            bemerkung = "";
                        }
                        if (bemerkung.Length > 80)
                        {
                            bemerkung = bemerkung.Remove(80);
                        }
                        string abwesendStr = abwesend ? "1" : "0";
                        string formUrl = string.Format("http://www.clanplanet.de/_sites/events_edit.asp?rn=&clanid={0}&action=adduser&show=", cEvent.ClanId);
                        string formParams = string.Format("id={0}&abwesend={1}&bemerkung={2}", cEvent.Id, abwesendStr, bemerkung.Replace(' ', '+'));
                        WebRequest req = WebRequest.Create(formUrl + "&device=m");
                        req.Headers["Cookie"] = CurrentCookie.Value;
                        req.ContentType = "application/x-www-form-urlencoded";
                        req.Method = "POST";
                        byte[] bytes = Encoding.UTF8.GetBytes(formParams);
                        if (GlobalErrorValues.Current.IsCollectingReportData)
                        {
                            SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = formParams, Origin = "SubscribeEvent" });
                        }

                        req.BeginGetRequestStream(ar =>
                        {
                            using (var stream =
                                req.EndGetRequestStream(ar))
                            {
                                stream.Write(bytes, 0, bytes.Length);
                            }
                            WebResponse antwort = null;
                            req.BeginGetResponse(rr =>
                            {
                                antwort = req.EndGetResponse(rr);
                                if (GlobalErrorValues.Current.IsCollectingReportData)
                                {
                                    using (StreamReader leser = new StreamReader(antwort.GetResponseStream(), Encoding.UTF8))
                                    {
                                        if (leser != null)
                                        {
                                            var inhalt = leser.ReadToEnd();
                                            if (inhalt != null)
                                            {
                                                SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = inhalt, Origin = "SubscribeEvent" });
                                            }
                                        }
                                        leser.Dispose();
                                    }
                                }
                                OnSubscribeFinished(true, onComplete);
                            }, req);
                        }, req);
                    }
                }
            }
            catch (Exception)
            {
                isSubscribing = false;
            }
        }
        private static void OnSubscribeFinished(bool success, Action<bool> onComplete)
        {
            onComplete?.Invoke(success);
            isSubscribing = false;
            //SubscribeFinished?.Invoke(null, new PostEventArgs(success));
        }

        private static bool isUnsubscribing;
        private static bool gettingEvents;

        public static void UnSubscribeEvent(ClanEvent cEvent, Action<bool> onComplete = null)
        {
            if (!isUnsubscribing || true)
            {
                isUnsubscribing = true;
                if (CurrentCookie != null && CurrentCookie.Value != null && CheckCookie(CurrentCookie))
                {
                    string formUrl = string.Format("http://www.clanplanet.de/_sites/events_edit.asp?rn=&clanid={0}&action=deluser&id={1}&show=", cEvent.ClanId, cEvent.Id);
                    string formParams = string.Format("rn=&clanid={0}&action=deluser&id={1}&show=", cEvent.ClanId, cEvent.Id);
                    WebRequest req = WebRequest.Create(formUrl + "&device=m");
                    req.Headers["Cookie"] = CurrentCookie.Value;
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.Method = "POST";
                    byte[] bytes = Encoding.UTF8.GetBytes(formParams);
                    if (GlobalErrorValues.Current.IsCollectingReportData)
                    {
                        SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = formParams, Origin = "UnSubscribeEvent" });
                    }

                    req.BeginGetRequestStream(ar =>
                    {
                        using (var stream =
                            req.EndGetRequestStream(ar))
                        {
                            stream.Write(bytes, 0, bytes.Length);
                        }
                        WebResponse antwort = null;
                        req.BeginGetResponse(rr =>
                        {
                            antwort = req.EndGetResponse(rr);
                            if (GlobalErrorValues.Current.IsCollectingReportData)
                            {
                                using (StreamReader leser = new StreamReader(antwort.GetResponseStream(), Encoding.UTF8))
                                {
                                    if (leser != null)
                                    {
                                        var inhalt = leser.ReadToEnd();
                                        if (inhalt != null)
                                        {
                                            SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = inhalt, Origin = "UnSubscribeEvent" });
                                        }
                                    }
                                    leser.Dispose();
                                }
                            }
                            OnUnSubscribeFinished(true, onComplete);
                        }, req);
                    }, req);
                }
            }
        }
        private static void OnUnSubscribeFinished(bool success, Action<bool> onComplete)
        {
            onComplete?.Invoke(success);
            isUnsubscribing = false;
            //UnSubscribeFinished?.Invoke(null, new PostEventArgs(success));
        }

        public static void GetCreateInfos(ClanEvent cEvent = null, string cid = null, Action<bool, CreateInfos> onComplete = null)
        {
            try
            {
                gettingDetails = true;
                if (CurrentCookie != null && CurrentCookie.Value != null && CheckCookie(CurrentCookie))
                {
                    if (string.IsNullOrWhiteSpace(cid) && CurrentLogin != null)
                    {
                        cid = CurrentLogin.ClanID;
                    }
                    string getUrl = string.Format("http://www.clanplanet.de/_sites/events_edit.asp?rn=&clanid={0}{1}&show=", cid, cEvent != null ? "&eventid=" + cEvent.Id : "");
                    WebRequest getRequest = WebRequest.Create(getUrl + "&device=m");
                    getRequest.Headers["Cookie"] = CurrentCookie.Value;
                    WebResponse antwort = null;
                    getRequest.BeginGetResponse(ar =>
                    {
                        antwort = getRequest.EndGetResponse(ar);
                        using (StreamReader leser = new StreamReader(antwort.GetResponseStream(), Encoding.UTF8))
                        {
                            if (leser != null)
                            {
                                var inhalt = leser.ReadToEnd();
                                if (inhalt != null)
                                {
                                    if (GlobalErrorValues.Current.IsCollectingReportData)
                                    {
                                        SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = inhalt, Origin = "GetCreateInfos" });
                                    }
                                    CreateInfos infos = new CreateInfos();
                                    int formIndex = inhalt.IndexOf("<form method=\"Post\" action=\"events_edit.asp?rn=&clanid=");
                                    if (formIndex > 0)
                                    {
                                        var form = inhalt.Remove(0, formIndex);
                                        form = form.Remove(form.IndexOf("</form>"));
                                        HtmlDocument doctable = new HtmlDocument();
                                        doctable.LoadHtml(form);
                                        foreach (var item in doctable.DocumentNode.Element("table").Elements("tr"))
                                        {
                                            if (item.ChildNodes.Count == 5)
                                            {
                                                if (item.ChildNodes[1].InnerText == "CP Gegner:")
                                                {
                                                    var select = item.ChildNodes[3].Element("select");
                                                    if (select != null)
                                                    {
                                                        for (int i = 1; i < select.ChildNodes.Count; i += 2)
                                                        {
                                                            infos.Enemies.Add(new ClanPlanetEnemy()
                                                            {
                                                                ClanId = select.ChildNodes[i].Attributes.Count >= 2 ? select.ChildNodes[i].GetAttributeValue("value", "0") : "0",
                                                                Name = GetRidOfStupidCodes(select.ChildNodes[i + 1].InnerText),
                                                                Selected = select.ChildNodes[i].Attributes.Contains("selected")
                                                            });
                                                        }
                                                    }
                                                }
                                                else if (item.ChildNodes[1].InnerText == "Gruppe:")
                                                {
                                                    var select = item.ChildNodes[3].Element("select");
                                                    if (select != null)
                                                    {
                                                        for (int i = 1; i < select.ChildNodes.Count; i += 2)
                                                        {
                                                            infos.Gruppen.Add(new Gruppe()
                                                            {
                                                                Id = select.ChildNodes[i].Attributes[0].Value,
                                                                Name = select.ChildNodes[i + 1].InnerText.Remove(select.ChildNodes[i + 1].InnerText.IndexOf("\r\n")),
                                                                Selected = select.ChildNodes[i].Attributes.Contains("selected")
                                                            });
                                                        }
                                                    }
                                                }
                                                else if (item.ChildNodes[1].InnerText == "Spiel:")
                                                {
                                                    var select = item.ChildNodes[3].Element("select");
                                                    if (select != null)
                                                    {
                                                        for (int i = 1; i < select.ChildNodes.Count; i += 2)
                                                        {
                                                            infos.Games.Add(new Game()
                                                            {
                                                                Id = select.ChildNodes[i - 1].Attributes.Count > 0 ? select.ChildNodes[i - 1].Attributes[0].Value : "0",
                                                                Name = select.ChildNodes[i].InnerText,
                                                                Selected = select.ChildNodes[i - 1].Attributes.Contains("selected")
                                                            });
                                                        }
                                                    }
                                                }
                                                else if (item.ChildNodes[1].InnerText == "Squad:")
                                                {
                                                    var select = item.ChildNodes[3].Element("select");
                                                    if (select != null)
                                                    {
                                                        for (int i = 1; i < select.ChildNodes.Count; i += 2)
                                                        {
                                                            infos.Squads.Add(new Squad()
                                                            {
                                                                Id = select.ChildNodes[i - 1].Attributes.Count > 0 ? select.ChildNodes[i - 1].Attributes[0].Value : "0",
                                                                Name = select.ChildNodes[i].InnerText,
                                                                Selected = select.ChildNodes[i - 1].Attributes.Contains("selected")
                                                            });
                                                        }
                                                    }
                                                }
                                                else if (item.ChildNodes[1].InnerText == "Sichtbar:")
                                                {
                                                    var select = item.ChildNodes[3].Elements("input").ToList();
                                                    if (select != null && select.Count == 2)
                                                    {
                                                        if (select[0].Attributes.Contains("checked"))
                                                        {
                                                            infos.Intern = true;
                                                        }
                                                        else
                                                        {
                                                            infos.Intern = false;
                                                        }
                                                    }
                                                }
                                                else if (item.ChildNodes[1].InnerText == "Anmeldungen:")
                                                {
                                                    var selectOptions = item.ChildNodes[3].Element("select").Elements("option").ToList();
                                                    for (int i = 0; i < selectOptions.Count; i++)
                                                    {
                                                        if (selectOptions[i].Attributes.Contains("selected"))
                                                        {
                                                            infos.SelectedType = i;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        onComplete?.Invoke(true, infos);
                                    }
                                    else
                                    {
                                        onComplete?.Invoke(false, null);
                                    }
                                }
                            }
                            leser.Dispose();
                        }
                    }, getRequest);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void CreateEvent(ClanEvent eventPrototype, ClanWarDetails cwPrototype, string groupID, string sichtbarkeit, string anmeldung, Action<bool> onComplete = null)
        {
            if (CurrentCookie != null && CurrentCookie.Value != null && CheckCookie(CurrentCookie))
            {
                string formUrl = string.Format("http://www.clanplanet.de/_sites/events_edit.asp?rn=&clanid={0}&action=store&show=", eventPrototype.ClanId, eventPrototype.Id);
                string formParams = string.Format(  "eintrag={0}" +
                                                    "&mdatum=TT.MM.YYYY&datum={1}" +
                                                    "&stunde={2}&minute={3}" +
                                                    "&gruppenid={4}" +                                                    
                                                    "&eventanzeige={5}&anmeldungen={6}", eventPrototype.EventName.Replace(' ', '+'), eventPrototype.Day.ToString("dd.MM.yyyy"),
                                                    eventPrototype.Day.ToString("HH"), eventPrototype.Day.ToString("mm"),
                                                    groupID,
                                                    sichtbarkeit, anmeldung);
                if (cwPrototype != null)
                {
                    formParams += cwPrototype.GetParamsString();
                }
                if (!string.IsNullOrEmpty(eventPrototype.Id))
                {
                    formParams = string.Format("eventid={0}&", eventPrototype.Id) + formParams;
                }
                WebRequest req = WebRequest.Create(formUrl + "&device=m");
                req.Headers["Cookie"] = CurrentCookie.Value;
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";
                byte[] bytes = Encoding.UTF8.GetBytes(formParams);
                if (GlobalErrorValues.Current.IsCollectingReportData)
                {
                    SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = formParams, Origin = "CreateEvent" });
                }

                req.BeginGetRequestStream(ar =>
                {
                    using (var stream =
                        req.EndGetRequestStream(ar))
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    WebResponse antwort = null;
                    req.BeginGetResponse(rr =>
                    {
                        antwort = req.EndGetResponse(rr);
                        if (GlobalErrorValues.Current.IsCollectingReportData)
                        {
                            using (StreamReader leser = new StreamReader(antwort.GetResponseStream(), Encoding.UTF8))
                            {
                                if (leser != null)
                                {
                                    var inhalt = leser.ReadToEnd();
                                    if (inhalt != null)
                                    {
                                        SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = inhalt, Origin = "CreateEvent" });
                                    }
                                }
                                leser.Dispose();
                            }
                        }
                        onComplete?.Invoke(true);
                    }, req);
                }, req);
            }
            //
        }

        public static void DeleteEvent(ClanEvent cEvent, Action<bool> onComplete = null)
        {
            if (CurrentCookie != null && CurrentCookie.Value != null && CheckCookie(CurrentCookie))
            {
                string formUrl = string.Format("http://www.clanplanet.de/_sites/events_edit.asp?rn=&clanid={0}&action=del&show=", cEvent.ClanId);
                string formParams = string.Format("id={0}&confirm=true", cEvent.Id);
                WebRequest req = WebRequest.Create(formUrl + "&device=m");
                req.Headers["Cookie"] = CurrentCookie.Value;
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";
                byte[] bytes = Encoding.UTF8.GetBytes(formParams);
                if (GlobalErrorValues.Current.IsCollectingReportData)
                {
                    SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = formParams, Origin = "DeleteEvent" });
                }

                req.BeginGetRequestStream(ar =>
                {
                    using (var stream =
                        req.EndGetRequestStream(ar))
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    WebResponse antwort = null;
                    req.BeginGetResponse(rr =>
                    {
                        antwort = req.EndGetResponse(rr);
                        if (GlobalErrorValues.Current.IsCollectingReportData)
                        {
                            using (StreamReader leser = new StreamReader(antwort.GetResponseStream(), Encoding.UTF8))
                            {
                                if (leser != null)
                                {
                                    var inhalt = leser.ReadToEnd();
                                    if (inhalt != null)
                                    {
                                        SendErrorPage.EDPackages.Add(new ErrorDataPackage() { TimeStamp = DateTime.Now, ErrorData = inhalt, Origin = "DeleteEvent" });
                                    }
                                }
                                leser.Dispose();
                            }
                        }
                        onComplete?.Invoke(true);
                    }, req);
                }, req);
            }
        }
    }

    public class HaveCWDetailsEventArgs
    {
        public ClanEvent cEvent;

        public HaveCWDetailsEventArgs(ClanEvent events)
        {
            this.cEvent = events;
        }
    }

    //
    public class HaveEventsEventArgs
    {
        public List<ClanEvent> Events;

        public HaveEventsEventArgs(List<ClanEvent> events)
        {
            this.Events = events;
        }
    }

    public class LoginFinishedEventArgs
    {
        public bool LoggedIn;
        public Cookie LoginCookie;

        public LoginFinishedEventArgs(bool loggedin, Cookie login)
        {
            LoggedIn = loggedin;
            LoginCookie = login;
        }
    }

    public class PostEventArgs
    {
        public bool Success;

        public PostEventArgs(bool success)
        {
            Success = success;
        }
    }
}
