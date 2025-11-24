package com.example.amfootball.navigation.objects.page

object CrudTeamRoutes {
    const val ARG_TEAM_ID = "teamId"
    const val UPDATE_TEAM = "UpdateTeam"

    const val PROFILE_TEAM = "ProfileTeam"

    const val PROFILE_TEAM_URL = "${PROFILE_TEAM}/{${ARG_TEAM_ID}}"
}