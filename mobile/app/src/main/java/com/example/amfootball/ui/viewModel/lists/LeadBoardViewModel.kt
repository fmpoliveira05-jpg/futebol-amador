package com.example.amfootball.ui.viewModel.lists

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
import javax.inject.Inject

//TODO: Falta conexão com o Backend e extender com o ListsViewModel
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

    //Inicializer
    init {
        //TODO: Depois adaptar para ir buscar ao FireBase


        launchDataLoad(
            callApi = {
                listState.value = teamService.getLeaderBoard()
            },
            checkOnline = true
        )
    }

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
}