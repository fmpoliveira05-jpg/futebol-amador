package com.example.amfootball.data

import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import java.time.Instant
import kotlin.math.roundToInt
import kotlin.random.Random

class InMemoryRepository {

    private val _users = MutableStateFlow<List<User>>(emptyList())
    val users: StateFlow<List<User>> = _users.asStateFlow()

    private val _teams = MutableStateFlow<List<Team>>(emptyList())
    val teams: StateFlow<List<Team>> = _teams.asStateFlow()

    private val _matches = MutableStateFlow<List<Match>>(emptyList())
    val matches: StateFlow<List<Match>> = _matches.asStateFlow()

    init {
        seedDemoData()
    }

    private fun seedDemoData() {
        val t1 = Team(name = "Atlético Amador", rating = 1580)
        val t2 = Team(name = "FC Bairro", rating = 1520)
        val t3 = Team(name = "Estrela United", rating = 1490)
        val t4 = Team(name = "Nova Esperança", rating = 1450)
        _teams.value = listOf(t1, t2, t3, t4)

        val u1 = User(name = "João", email = "joao@example.com", role = UserRole.TEAM_ADMIN, teamId = t1.id)
        val u2 = User(name = "Maria", email = "maria@example.com", role = UserRole.TEAM_ADMIN, teamId = t2.id)
        _users.value = listOf(u1, u2)

        // sample matches with geo-locations
        val now = Instant.now()
        val m1 = Match(teamAId = t1.id, teamBId = t2.id, scheduledAt = now.plusSeconds(3600), location = MatchLocation(41.1579, -8.6291, "Campo A"))
        val m2 = Match(teamAId = t3.id, teamBId = t4.id, scheduledAt = now.plusSeconds(7200), location = MatchLocation(41.1600, -8.6200, "Campo B"))
        _matches.value = listOf(m1, m2)
    }

    fun createUser(user: User) {
        _users.value = _users.value + user
    }

    fun createTeam(team: Team) {
        _teams.value = _teams.value + team
    }

    fun createMatch(match: Match) {
        _matches.value = _matches.value + match
    }

    fun recordMatchResult(matchId: String, result: MatchResult) {
        _matches.value = _matches.value.map { m ->
            if (m.id == matchId) {
                val updated = m.copy(result = result)
                if (m.isRanked) {
                    updateRatingsAfterMatch(m, result)
                }
                updated
            } else m
        }
    }

    private fun updateRatingsAfterMatch(match: Match, result: MatchResult) {
        val teamA = _teams.value.find { it.id == match.teamAId } ?: return
        val teamB = _teams.value.find { it.id == match.teamBId } ?: return

        val scoreA = result.teamAScore
        val scoreB = result.teamBScore

        val outcomeA = when {
            scoreA > scoreB -> 1.0
            scoreA == scoreB -> 0.5
            else -> 0.0
        }

        // Simple Elo update
        val expectedA = 1.0 / (1.0 + Math.pow(10.0, ((teamB.rating - teamA.rating) / 400.0)))
        val k = 30 // K-factor
        val newRatingA = (teamA.rating + k * (outcomeA - expectedA)).roundToInt()
        val newRatingB = (teamB.rating + k * ((1.0 - outcomeA) - (1.0 - expectedA))).roundToInt()

        _teams.value = _teams.value.map {
            when (it.id) {
                teamA.id -> it.copy(rating = newRatingA)
                teamB.id -> it.copy(rating = newRatingB)
                else -> it
            }
        }
    }

    fun getLeaderboard(): List<Team> {
        return _teams.value.sortedByDescending { it.rating }
    }

    /**
     * Busca partidas ranqueadas entre equipas com rating dentro de uma diferença (threshold).
     * Se threshold = 100 -> procura equipas cujos ratings diferem <= 100.
     */
    fun findRankedMatchesForTeam(teamId: String, maxRatingDiff: Int = 100): List<Pair<Team, Team>> {
        val team = _teams.value.find { it.id == teamId } ?: return emptyList()
        val candidates = _teams.value.filter { it.id != teamId && kotlin.math.abs(it.rating - team.rating) <= maxRatingDiff }
        return candidates.map { Pair(team, it) }
    }

    /**
     * Procura partidas ranqueadas agendadas próximas a uma localização (haversine approx).
     */
    fun findNearbyMatches(lat: Double, lng: Double, radiusKm: Double = 10.0): List<Match> {
        return _matches.value.filter { match ->
            val d = distanceKm(lat, lng, match.location.lat, match.location.lng)
            d <= radiusKm
        }
    }

    private fun distanceKm(lat1: Double, lon1: Double, lat2: Double, lon2: Double): Double {
        val r = 6371.0 // km
        val dLat = Math.toRadians(lat2 - lat1)
        val dLon = Math.toRadians(lon2 - lon1)
        val a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
                Math.cos(Math.toRadians(lat1)) * Math.cos(Math.toRadians(lat2)) *
                Math.sin(dLon / 2) * Math.sin(dLon / 2)
        val c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))
        return r * c
    }

    // Helper: schedule random demo match near provided coords (used in UI)
    fun scheduleRandomNearbyMatch(refLat: Double, refLng: Double) {
        val teams = _teams.value
        if (teams.size < 2) return
        val t1 = teams.random()
        var t2 = teams.random()
        while (t2.id == t1.id) {
            t2 = teams.random()
        }
        val delta = Random.nextDouble(-0.03, 0.03)
        val match = Match(
            teamAId = t1.id,
            teamBId = t2.id,
            scheduledAt = Instant.now().plusSeconds(Random.nextLong(3600, 3600 * 24)),
            location = MatchLocation(refLat + delta, refLng + delta, "Campo Gerado")
        )
        createMatch(match)
    }
}