using System.Text.Json;
using FinRiskLensAI.Core.Models.User_Activity;

namespace FinRiskLensAI.Common
{
    /// <summary>
    /// Well-known session keys. Use these constants instead of raw strings so a
    /// typo can't silently read/write the wrong slot.
    /// </summary>
    public static class SessionKeys
    {
        public const string CurrentUser = "CurrentUser";
    }

    /// <summary>
    /// Strongly-typed helpers over ISession, which natively only stores byte[]/
    /// string/int. Objects are round-tripped as JSON under a single key.
    /// </summary>
    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
            => session.SetString(key, JsonSerializer.Serialize(value));

        public static T? GetObject<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>The logged-in MSME user, or null if not signed in.</summary>
        public static UserSessionModel? GetCurrentUser(this ISession session)
            => session.GetObject<UserSessionModel>(SessionKeys.CurrentUser);

        public static void SetCurrentUser(this ISession session, UserSessionModel user)
            => session.SetObject(SessionKeys.CurrentUser, user);
    }
}
