package com.example.amfootball.data.dtos.match

data class ResultMatchDto(
    val idMatch: String,
    val idTeam: String,
    val numGoals: Int = 0,
    val idOpponent: String,
    val numGoalsOpponent: Int = 0
)