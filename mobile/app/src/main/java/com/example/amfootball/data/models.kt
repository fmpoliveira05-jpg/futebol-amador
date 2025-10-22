package com.example.amfootball.data

import java.time.Instant
import java.util.*

enum class UserRole { PLAYER, TEAM_ADMIN, SYSTEM_ADMIN }

data class User(
    val id: String = UUID.randomUUID().toString(),
    var name: String,
    var email: String,
    var role: UserRole = UserRole.PLAYER,
    val teamId: String? = null
)

data class Team(
    val id: String = UUID.randomUUID().toString(),
    var name: String,
    var rating: Int = 1500, // ELO-like rating
    val members: MutableList<String> = mutableListOf() // user ids
)

data class MatchLocation(
    val lat: Double,
    val lng: Double,
    val address: String? = null
)

data class Match(
    val id: String = UUID.randomUUID().toString(),
    val teamAId: String,
    val teamBId: String,
    val scheduledAt: Instant,
    val location: MatchLocation,
    val isRanked: Boolean = true,
    var result: MatchResult? = null
)

data class MatchResult(
    val teamAScore: Int,
    val teamBScore: Int,
    val finishedAt: Instant = Instant.now()
)