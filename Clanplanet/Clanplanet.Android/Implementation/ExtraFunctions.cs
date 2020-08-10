using System;
using Android.Content;
using Clanplanet.Droid.Implementation;
using Clanplanet.Dependencies;
using Android.Provider;
using Xamarin.Forms;
using Java.Util;
using System.IO;
using Android.Widget;

[assembly: Dependency(typeof(ExtraFunctions))]
namespace Clanplanet.Droid.Implementation
{
    public class ExtraFunctions : IExtraFunctions
    {
        public ReminderReturn DeleteReminder(string eventuri, string reminderuri)
        {
            try
            {
                var eventDelete = Forms.Context.ContentResolver.Delete(Android.Net.Uri.Parse(eventuri), null, null);
                var reminderDelete = Forms.Context.ContentResolver.Delete(Android.Net.Uri.Parse(reminderuri), null, null);
                return new ReminderReturn() { HasError = false };
            }
            catch (Exception exc)
            {
                return new ReminderReturn() { HasError = true, Error = exc };
            }
        }

        public string GetDatabasePath(string fileName)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(path, fileName);
        }

        ReminderReturn IExtraFunctions.AddReminder(string title, DateTime time)
        {
            try
            {
                DateTime sd = time;
                DateTime ed = time.AddHours(1);
                ContentValues eventValues = new ContentValues();
                eventValues.Put(CalendarContract.Events.InterfaceConsts.CalendarId, 1);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.Title, title);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.Description, "Event-Erinnerung von der Clanplanet App");
                eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtstart, GetDateTimeMS(sd.Year, sd.Month, sd.Day, sd.Hour, sd.Minute));
                eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtend, GetDateTimeMS(ed.Year, ed.Month, ed.Day, ed.Hour, ed.Minute));
                eventValues.Put(CalendarContract.Events.InterfaceConsts.AllDay, "0");
                eventValues.Put(CalendarContract.Events.InterfaceConsts.HasAlarm, "1");
                eventValues.Put(CalendarContract.Events.InterfaceConsts.EventTimezone, "UTC");
                eventValues.Put(CalendarContract.Events.InterfaceConsts.EventEndTimezone, "UTC");
                var eventUri = Forms.Context.ContentResolver.Insert(CalendarContract.Events.ContentUri, eventValues);
                long eventID = long.Parse(eventUri.LastPathSegment);
                ContentValues remindervalues = new ContentValues();
                remindervalues.Put(CalendarContract.Reminders.InterfaceConsts.Minutes, 30);
                remindervalues.Put(CalendarContract.Reminders.InterfaceConsts.EventId, eventID);
                remindervalues.Put(CalendarContract.Reminders.InterfaceConsts.Method, (int)Android.Provider.RemindersMethod.Alert);
                var reminderURI = Forms.Context.ContentResolver.Insert(CalendarContract.Reminders.ContentUri, remindervalues);
                return new ReminderReturn() { AndroidEventUri = eventUri.ToString(), AndroidReminderUri = reminderURI.ToString(), HasError = false };
            }
            catch (Exception exc)
            {
                return new ReminderReturn() { HasError = true, Error = exc };
            }
        }

        public void LongAlert(string message)
        {
            Toast.MakeText(Forms.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(Forms.Context, message, ToastLength.Short).Show();
        }

        long GetDateTimeMS(int yr, int month, int day, int hr, int min)
        {
            Calendar c = Calendar.GetInstance(Java.Util.TimeZone.Default);

            c.Set(CalendarField.Year, yr);
            c.Set(CalendarField.Month, month - 1);
            c.Set(CalendarField.DayOfMonth, day);
            c.Set(CalendarField.HourOfDay, hr);
            c.Set(CalendarField.Minute, min);
            c.Set(CalendarField.Second, 0);

            return c.TimeInMillis;
        }
    }
}