package com.example.amfootball.navigation.Objects

import androidx.compose.material.icons.automirrored.filled.Login
import androidx.compose.material.icons.automirrored.filled.Logout
import androidx.compose.material.icons.automirrored.filled.Message
import androidx.compose.material.icons.filled.AccessibilityNew
import androidx.compose.material.icons.filled.ArrowCircleUp
import androidx.compose.material.icons.filled.CalendarMonth
import androidx.compose.material.icons.filled.EmojiEvents
import androidx.compose.material.icons.filled.Group
import androidx.compose.material.icons.filled.Groups3
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.Person4
import androidx.compose.material.icons.filled.RoomPreferences
import androidx.compose.material.icons.filled.Settings
import androidx.compose.material.icons.filled.SportsBaseball
import androidx.compose.ui.graphics.vector.ImageVector
import com.example.amfootball.R
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.EditCalendar
import androidx.compose.material.icons.filled.Flag
import androidx.compose.material.icons.filled.GroupAdd
import androidx.compose.material.icons.filled.PersonAdd

interface AppRouteInfo {
    val route: String
    val labelResId: Int
    val icon: ImageVector
    val contentDescription: Int
    val haveBackButton: Boolean
}

object Routes {
    val chatRoomId = "chatRoomId"


    // AGORA IMPLEMENTA RouteInfo
    enum class GeralRoutes(
        override val route: String,
        override val labelResId: Int,
        override val icon: ImageVector,
        override val contentDescription: Int,
        override val haveBackButton: Boolean = false
    ) : AppRouteInfo {
        SETTINGS("Settings", R.string.item_settings, Icons.Default.Settings, R.string.item_settings_description),
        PREFERENCE("Preference", R.string.item_preference, Icons.Default.AccessibilityNew, R.string.item_preference_description),
        HOMEPAGE("homepage", R.string.item_home, Icons.Default.Home, R.string.item_home_description),
        LEADERBOARD("leaderboard", R.string.item_leadboard, Icons.Default.EmojiEvents, R.string.item_leadboard_description),
    }

    // AGORA IMPLEMENTA RouteInfo
    enum class PlayerRoutes(
        override val route: String,
        override val labelResId: Int,
        override val icon: ImageVector,
        override val contentDescription: Int,
        override val haveBackButton: Boolean = false
    ) : AppRouteInfo {
        PLAYER_LIST("playerlist", R.string.item_list_players, Icons.Default.Group, R.string.item_list_players_description),
        TEAM_LIST("teamlist", R.string.item_list_team, Icons.Default.Groups3, R.string.item_list_team_description),
        CHAT_LIST("chatlist", R.string.item_list_chat, Icons.Default.Groups3, R.string.item_list_chat_description),

        LIST_MEMBERSHIP_REQUEST("playerMemberShipRequest", R.string.navbar_memberships_list, Icons.Default.Person, R.string.description_navbar_memberships_list),

        SINGLE_CHAT("chat/{${chatRoomId}}", R.string.single_chat, Icons.Default.Person, R.string.single_chat_description, true)
    }

    // AGORA IMPLEMENTA RouteInfo
    enum class UserRoutes(
        override val route: String,
        override val labelResId: Int,
        override val icon: ImageVector,
        override val contentDescription: Int,
        override val haveBackButton: Boolean = true
    ) : AppRouteInfo {
        PROFILE(
            "profile_user",
            R.string.page_profile_user,
            Icons.Default.Person,
            R.string.profile_user_title_page),
        LOGIN("Login", R.string.login_title, Icons.AutoMirrored.Filled.Login, R.string.login_title),
        SIGNUP("Signup", R.string.signup, Icons.Default.PersonAdd, R.string.signup_description),
        LOGOUT(
            "Logout",
            R.string.logout,
            Icons.AutoMirrored.Filled.Logout,
            R.string.logout_description,
            haveBackButton = false),
    }

    enum class TeamRoutes(
        override val route: String,
        override val labelResId: Int,
        override val icon: ImageVector,
        override val contentDescription: Int,
        override val haveBackButton: Boolean = false
    ) : AppRouteInfo {
        //Trocar a description
        CREATE_TEAM("CreateTeam",R.string.navbar_create_tean,Icons.Default.GroupAdd,R.string.button_create_team,haveBackButton = true,
        ),
        HOMEPAGE("HomePageTeam",R.string.team_homepage,Icons.Default.Home,R.string.team_homepage_description),
        TEAM_PROFILE("teamprofile",R.string.team_profile,Icons.Default.Person4,R.string.team_profile_description,haveBackButton = true),

        SETTINGS("teamsettings",R.string.team_settings,Icons.Default.RoomPreferences,R.string.team_settings_description),
        MEMBERLIST("ListMembersTeam",R.string.navbar_members_list,Icons.Default.Group,R.string.navbar_members_list),

        //ADD_MEMBER("addmember", R.string.add_member, Icons.Default.PersonAddAlt1, R.string.add_member_description),
        //REMOVE_MEMBER("removemember", R.string.remove_member, Icons.Default.PersonRemoveAlt1, R.string.remove_member_description),
        SEND_MATCH_INVITE("SendMatchInvite",R.string.title_page_send_match_Invite,Icons.Default.Person,R.string.title_page_send_match_Invite,haveBackButton = true),

        CALENDAR("calendar",R.string.navbar_calendar,Icons.Default.CalendarMonth,R.string.description_navbar_calendar),

        LIST_MATCH_INVITES("ListMatchInvites",R.string.navbar_matchinvite_list,Icons.Default.SportsBaseball,R.string.description_navbar_matchinvite_list),

        LIST_MEMBERSHIP_REQUEST("teamMemberShipRequest",R.string.navbar_memberships_list,Icons.Default.Person,R.string.description_navbar_memberships_list),

        SEARCH_PLAYERS_WITH_OUT_TEAM("SearchPlayersWithoutTeam",R.string.navbar_players_without_team_list,Icons.Default.Person,R.string.description_navbar_players_without_team_list),

        SEARCH_TEAMS_TO_MATCH_INVITE("SearchTeamsToMatchInvite",R.string.navbar_teams_to_matchinvite_list,Icons.Default.Person,R.string.description_navbar_teams_to_matchinvite_list),

        SEARCH_COMPETIVE_MATCH("SearchCompetitiveMatch",R.string.navbar_find_competitive_match,Icons.Default.EmojiEvents,R.string.description_navbar_find_competitive_match),

        LIST_POST_PONE_MATCH("ListPostponeMatch",R.string.navbar_list_post_pone_match,Icons.Default.EditCalendar,R.string.description_navbar_list_post_pone_match),
        POST_PONE_MATCH("PostPoneMatch",R.string.button_post_pone_match,Icons.Default.EditCalendar,R.string.button_post_pone_match_description,haveBackButton = true),

        CANCEL_MATCH("CancelMatch",R.string.button_cancel_match,Icons.Default.Flag,R.string.button_cancel_match_description,haveBackButton = true
        ),
        FINISH_MATCH("FinishMatch",R.string.button_finish_match,Icons.Default.Flag,R.string.button_finish_match_description,haveBackButton = true),
        NEGOCIATE_MATCH_INVITE("NegociateMatchInvite",R.string.negociate_button,Icons.Default.EditCalendar,R.string.negociate_button_description,haveBackButton = true)
    }

    enum class BottomNavBarRoutes(
        override val route: String,
        override val labelResId: Int,
        override val icon: ImageVector,
        override val contentDescription: Int,
        override val haveBackButton: Boolean
    ) : AppRouteInfo {
        HOMEPAGE(
            GeralRoutes.HOMEPAGE.route,
            GeralRoutes.HOMEPAGE.labelResId,
            GeralRoutes.HOMEPAGE.icon,
            GeralRoutes.HOMEPAGE.contentDescription,
            GeralRoutes.HOMEPAGE.haveBackButton
        ),
        TEAM_LIST(
            PlayerRoutes.TEAM_LIST.route,
            PlayerRoutes.TEAM_LIST.labelResId,
            PlayerRoutes.TEAM_LIST.icon,
            PlayerRoutes.TEAM_LIST.contentDescription,
            PlayerRoutes.TEAM_LIST.haveBackButton
        ),
        PAGE_OPTIONS(
            "pageoptions",
            R.string.page_options,
            Icons.Default.ArrowCircleUp,
            R.string.page_options_description,
            haveBackButton = false),
        CHAT_LIST(
            PlayerRoutes.CHAT_LIST.route,
            PlayerRoutes.CHAT_LIST.labelResId,
            Icons.AutoMirrored.Filled.Message,
            PlayerRoutes.CHAT_LIST.contentDescription,
            PlayerRoutes.CHAT_LIST.haveBackButton
        ),
        USER_PROFILE(
            UserRoutes.PROFILE.route,
            UserRoutes.PROFILE.labelResId,
            UserRoutes.PROFILE.icon,
            UserRoutes.PROFILE.contentDescription,
            UserRoutes.PROFILE.haveBackButton)
    }
}
