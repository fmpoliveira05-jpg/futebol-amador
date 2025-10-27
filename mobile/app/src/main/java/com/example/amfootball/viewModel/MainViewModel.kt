package com.example.amfootball.viewModel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.amfootball.data.InMemoryRepository
import com.example.amfootball.data.Match
import com.example.amfootball.data.MatchResult
import com.example.amfootball.data.Team
import com.example.amfootball.data.User
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch

class MainViewModel(
    val repo: InMemoryRepository = InMemoryRepository()
) : ViewModel() {

    val users: StateFlow<List<User>> = repo.users.stateIn(viewModelScope, SharingStarted.Eagerly, emptyList())
    val teams: StateFlow<List<Team>> = repo.teams.stateIn(viewModelScope, SharingStarted.Eagerly, emptyList())
    val matches: StateFlow<List<Match>> = repo.matches.stateIn(viewModelScope, SharingStarted.Eagerly, emptyList())

    fun createUser(user: User) = viewModelScope.launch { repo.createUser(user) }
    fun createTeam(team: Team) = viewModelScope.launch { repo.createTeam(team) }
    fun createMatch(match: Match) = viewModelScope.launch { repo.createMatch(match) }
    fun recordResult(matchId: String, result: MatchResult) = viewModelScope.launch { repo.recordMatchResult(matchId, result) }

    fun getLeaderboard(): List<Team> = repo.getLeaderboard()
    fun findRankedMatchesForTeam(teamId: String, diff: Int) = repo.findRankedMatchesForTeam(teamId, diff)
    fun findNearbyMatches(lat: Double, lng: Double, radiusKm: Double) = repo.findNearbyMatches(lat, lng, radiusKm)
    fun scheduleRandomNearbyMatch(lat: Double, lng: Double) = viewModelScope.launch { repo.scheduleRandomNearbyMatch(lat, lng) }
}