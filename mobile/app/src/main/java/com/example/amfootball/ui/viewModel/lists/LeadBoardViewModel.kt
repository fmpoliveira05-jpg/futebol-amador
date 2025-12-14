package com.example.amfootball.ui.viewModel.lists

import android.util.Log
import androidx.lifecycle.MutableLiveData
import androidx.navigation.NavHostController
import com.example.amfootball.data.NetworkConnectivityObserver
import com.example.amfootball.data.remote.dtos.leadboard.InfoTeamLeadboard
import com.example.amfootball.data.remote.dtos.leadboard.LeadboardDto
import com.example.amfootball.data.remote.services.TeamService
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.abstracts.ListsViewModels
import com.google.firebase.firestore.FirebaseFirestore
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.tasks.await
import javax.inject.Inject

/**
 * ViewModel responsável pela lógica de negócio e gestão de estado da Tabela de Classificação (Leaderboard).
 *
 * **Funcionalidade Central:** Implementa paginação local (Load More) da lista de equipas.
 * A lista completa é carregada uma vez no início, e apenas um subconjunto é exposto à UI.
 *
 * @property listTeam Armazena a lista completa de dados do Leaderboard ([LeadboardDto]), observável via [MutableLiveData].
 */
@HiltViewModel
class LeadBoardViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver,
    private val teamService: TeamService,
    private val db: FirebaseFirestore
) : ListsViewModels<InfoTeamLeadboard>(networkObserver = networkObserver) {

    /**
     * Navega para o ecrã de informações detalhadas da equipa.
     *
     * @param idTeam O ID da equipa cujos detalhes devem ser exibidos.
     * @param navHostController O controlador para gerir a navegação.
     */
    fun showInfoTeam(idTeam: String, navHostController: NavHostController) {
        navHostController.navigate(route = "${Routes.UserRoutes.PROFILE.route}/$idTeam") {
            launchSingleTop = true
        }
    }

    private val COLLECTION_NAME = "leaderboard_cache"

    init {
        loadLeaderboardData()
    }

    private fun loadLeaderboardData() {
        launchDataLoad(
            checkOnline = false,
            callApi = {
                if (networkObserver.isOnlineOneShot()) {
                    try {
                        val apiResult = teamService.getLeaderBoard()
                        listState.value = apiResult
                        saveToFirebase(apiResult)

                    } catch (e: Exception) {
                        Log.e("LeadBoardVM", "Erro na API, tentando Firebase: ${e.message}")
                        fetchFromFirebase()
                    }
                } else {
                    fetchFromFirebase()
                }
            }
        )
    }

    /**
     * Busca os dados armazenados no Firestore caso a API falhe ou não haja internet.
     */
    private suspend fun fetchFromFirebase() {
        try {

            val snapshot = db.collection(COLLECTION_NAME)
                .orderBy("position")
                .get()
                .await()
            if (!snapshot.isEmpty) {
                val cachedList = snapshot.documents.mapNotNull { doc ->
                    try {
                        InfoTeamLeadboard(
                            id = doc.getString("id") ?: "",
                            position = doc.getLong("position")?.toInt() ?: 0,
                            name = doc.getString("name") ?: "",
                            currentPoints = doc.getLong("currentPoints")?.toInt() ?: 0,
                            nameRank = doc.getString("nameRank") ?: "",
                            logoTeam = doc.getString("logoTeam")
                        )
                    } catch (e: Exception) {
                        null
                    }
                }
                listState.value = cachedList
            } else {
                Log.d("LeadBoardVM", "Cache vazio")
            }
        } catch (e: Exception) {
            Log.e("LeadBoardVM", "Erro ao ler do Firebase: ${e.message}")
        }
    }

    /**
     * Guarda a lista no Firestore.
     * Estratégia: Apagar tudo o que lá está (Batch Delete) e escrever os novos (Batch Write).
     */
    private suspend fun saveToFirebase(data: List<InfoTeamLeadboard>) {
        try {
            val batch = db.batch()
            val collectionRef = db.collection(COLLECTION_NAME)
            val oldDataSnapshot = collectionRef.get().await()

            for (document in oldDataSnapshot) {
                batch.delete(document.reference)
            }

            for (team in data) {
                val docRef = collectionRef.document(team.id)

                val teamMap = hashMapOf(
                    "id" to team.id,
                    "position" to team.position,
                    "name" to team.name,
                    "currentPoints" to team.currentPoints,
                    "nameRank" to team.nameRank,
                    "logoTeam" to team.logoTeam
                )

                batch.set(docRef, teamMap)
            }

            batch.commit().await()
            Log.d("LeadBoardVM", "Cache atualizado com sucesso no Firebase")

        } catch (e: Exception) {
            Log.e("LeadBoardVM", "Erro ao salvar no Firebase: ${e.message}")
        }
    }


}