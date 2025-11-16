package com.example.amfootball.data.dtos.suporrtDto

import android.net.Uri

data class TeamDto(
    val id: String,
    val name: String,
    val image: Uri? = Uri.EMPTY
)