package com.example.amfootball.data.dtos.leadboard

import android.net.Uri

data class InfoTeamLeadboard(
    val id: String,
    val name: String,
    val currentPoints: Int,
    val nameRank: String,
    val logoTeam: Uri? = Uri.EMPTY
)