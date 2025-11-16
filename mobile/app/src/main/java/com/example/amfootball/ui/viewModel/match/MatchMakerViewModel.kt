package com.example.amfootball.ui.viewModel.match

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.match.MatchMakerInfo
import com.example.amfootball.navigation.Objects.Routes

//Vai ter um BackGorundService por tras
//TODO: Depois adaptar o que for preciso por cuasa do BackGorundService
//TODO: Vai faltar metodos por causa do BackGroundService, depois implementar
class MatchMakerViewModel: ViewModel() {
    private val matchMakerState: MutableLiveData<MatchMakerInfo> = MutableLiveData<MatchMakerInfo>()
    val matchMaker: LiveData<MatchMakerInfo> = matchMakerState

    init {
        //TODO: Não sei vem o que fazer aqui, talvez so ir buscar a equipa que entrou no lobby
        matchMakerState.value = MatchMakerInfo.createExampleWithOneTeam()
    }

    //TODO: Fazer pedido há API, para cancelar a procura
    fun onCancelFind(navHostController: NavHostController) {
        navHostController.navigate(Routes.TeamRoutes.HOMEPAGE.route) {
            popUpTo(Routes.TeamRoutes.HOMEPAGE.route) {
                inclusive = true
            }
        }
    }
}