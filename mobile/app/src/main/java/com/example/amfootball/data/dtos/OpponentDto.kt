package com.example.amfootball.data.dtos

import android.net.Uri

data class OpponentDto(
    val id: String,
    val name: String,
    val image: Uri? = Uri.EMPTY
)
