package com.example.amfootball.data.dtos

data class CreateRoomRequest(
    val roomName: String,
    val teamIds: List<String>
)

data class CreateRoomResponse(
    val roomId: String
)