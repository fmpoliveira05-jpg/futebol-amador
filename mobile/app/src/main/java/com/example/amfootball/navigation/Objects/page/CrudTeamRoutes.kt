package com.example.amfootball.navigation.Objects.page

object CrudTeamRoutes {
    const val ARG_TEAM_ID = "teamId"
    const val CREATE_TEAM = "Create_Team"

    const val PROFILE_TEAM = "Profile_Team"

    const val PROFILE_TEAM_URL = "${PROFILE_TEAM}/{${ARG_TEAM_ID}}"
}