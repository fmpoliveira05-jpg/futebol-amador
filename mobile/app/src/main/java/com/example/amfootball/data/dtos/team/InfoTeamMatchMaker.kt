package com.example.amfootball.data.dtos.team

import android.net.Uri

data class InfoTeamMatchMaker(
    val id: String,
    val name: String,
    val logoTeam: Uri = Uri.EMPTY,
    val rank: String
)
