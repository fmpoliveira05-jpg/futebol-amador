package com.example.amfootball.ui.viewModel.match

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.match.MatchMakerInfo
import com.example.amfootball.navigation.objects.Routes

//TODO: FALTA TUDO
/**
 * ViewModel responsável pela lógica e estado do ecrã de "Matchmaker" (Procura de Jogo Competitivo).
 *
 * **Arquitetura Planeada:** Este ViewModel está desenhado para trabalhar em conjunto com um
 * **Background Service** (ou um Service de Long-Polling) que será o responsável por atualizar
 * o estado de emparelhamento em tempo real, notificando o ViewModel quando um adversário é encontrado
 * ou o local do jogo é definido.
 */
class MatchMakerViewModel : ViewModel() {
    /**
     * Estado interno mutável que guarda os dados da procura de jogo (Equipas, Campo, Status).
     * Inicialmente, deve conter a equipa do utilizador e o estado de "A procurar".
     */
    private val matchMakerState: MutableLiveData<MatchMakerInfo> = MutableLiveData<MatchMakerInfo>()

    /**
     * Estado público de leitura (Read-only) que a UI observa para renderizar
     * o ecrã de Matchmaker.
     */
    val matchMaker: LiveData<MatchMakerInfo> = matchMakerState

    init {
        //TODO: Não sei vem o que fazer aqui, talvez so ir buscar a equipa que entrou no lobby
        matchMakerState.value = MatchMakerInfo.createExampleWithOneTeam()
    }

    //TODO: Fazer pedido há API, para cancelar a procura
    /**
     * Envia um pedido à API para cancelar o processo de procura de jogo (Matchmaking).
     *
     * Após o envio do pedido de cancelamento (TODO), navega o utilizador de volta para a Home da Equipa.
     *
     * @param navHostController Controlador de navegação para o redirecionamento.
     */
    fun onCancelFind(navHostController: NavHostController) {
        navHostController.navigate(Routes.TeamRoutes.HOMEPAGE.route) {
            popUpTo(Routes.TeamRoutes.HOMEPAGE.route) {
                inclusive = true
            }
        }
    }
}