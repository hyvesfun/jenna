using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Hyves.Api.Model
{
    public enum Visibility
    {
        [Description("private")]
        Private = 1,

        [Description("friend")]
        Friend = 2,

        [Description("friends_of_friends")]
        FriendsOfFriends = 3,

        [Description("public")]
        Public = 4,

        [Description("superpublic")]
        SuperPublic = 5
    }

    public class VisibilityItem
    {
        private Visibility visibility;
        public Visibility Visibility { get { return this.visibility; } }
        public override string ToString() 
        {
            return VisibilityHelper.GetVisibilityText(this.visibility);
        }
        public VisibilityItem(Visibility visibility) 
        {
            this.visibility = visibility;
        }
    }

    public static class VisibilityHelper
    {
        private static Dictionary<int, string> visibilityText = new Dictionary<int,string>();
        private static Dictionary<string, string> visibilityDiscriptionText = new Dictionary<string, string>();

        static VisibilityHelper() 
        {
            if (visibilityText.Count == 0)
            {
                visibilityText.Add((int)Visibility.Private, "Private");
                visibilityText.Add((int)Visibility.Friend, "Only  Friends");
                visibilityText.Add((int)Visibility.FriendsOfFriends, "Friends of Friends");
                visibilityText.Add((int)Visibility.Public, "Hyvers");
                visibilityText.Add((int)Visibility.SuperPublic, "Everyone");
            }

            if (visibilityDiscriptionText.Count == 0)
            {
                visibilityDiscriptionText.Add(EnumHelper.GetDescription(Visibility.Private), "Private");
                visibilityDiscriptionText.Add(EnumHelper.GetDescription(Visibility.Friend), "Only  Friends");
                visibilityDiscriptionText.Add(EnumHelper.GetDescription(Visibility.FriendsOfFriends), "Friends of Friends");
                visibilityDiscriptionText.Add(EnumHelper.GetDescription(Visibility.Public), "Hyvers");
                visibilityDiscriptionText.Add(EnumHelper.GetDescription(Visibility.SuperPublic), "Everyone");
            }

        }
        public static string GetVisibilityText(Visibility visibility) 
        {
            return visibilityText[(int)visibility];
        }
        public static string GetVisibilityText(string discribition) 
        {
            return visibilityDiscriptionText[discribition]; ;
        }
    }
}
