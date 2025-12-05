package com.example.amfootball.ui.viewModel.lists

import androidx.compose.runtime.State
import androidx.compose.runtime.derivedStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.leadboard.LeadboardDto
import com.example.amfootball.navigation.objects.Routes

//TODO: Falta conexão com o Backend e extender com o ListsViewModel
/**
 * ViewModel responsável pela lógica de negócio e gestão de estado da Tabela de Classificação (Leaderboard).
 *
 * **Funcionalidade Central:** Implementa paginação local (Load More) da lista de equipas.
 * A lista completa é carregada uma vez no início, e apenas um subconjunto é exposto à UI.
 *
 * @property listTeam Armazena a lista completa de dados do Leaderboard ([LeadboardDto]), observável via [MutableLiveData].
 */
class LeadBoardViewModel : ViewModel() {
    private val listTeam: MutableLiveData<List<LeadboardDto>> =
        MutableLiveData(emptyList())

    /**
     * Estado mutável que controla quantos itens devem ser exibidos na UI (usado para paginação).
     * Inicialmente definido como 10.
     */
    private val inicialSizeList = mutableStateOf(value = 10)

    //Getters
    /**
     * Estado computado que retorna o subconjunto atual da lista de equipas.
     *
     * Este valor é recalculado sempre que [listTeam] ou [inicialSizeList] muda,
     * implementando o efeito de "paginação".
     *
     * @return [State] contendo a porção visível da lista (ex: itens 0 a 10).
     */
    val inicialList: State<List<LeadboardDto>> = derivedStateOf {
        listTeam.value!!.take(inicialSizeList.value)
    }

    /**
     * Estado computado que determina se o botão "Carregar Mais" (Load More) deve ser exibido.
     *
     * O botão só é visível se o número de itens atualmente exibidos ([inicialSizeList])
     * for menor do que o número total de equipas carregadas.
     *
     * @return [State] contendo `true` se houver mais itens para carregar.
     */
    val showMoreButton: State<Boolean> = derivedStateOf {
        inicialSizeList.value < listTeam.value!!.size
    }

    //Inicializer
    init {
        //TODO: Depois adaptar para ir buscar ao FireBase
        listTeam.value = LeadboardDto.Companion.generateLeadboardExample()
    }

    //Metodo
    /**
     * Aumenta o número de itens exibidos em 10.
     *
     * Disparado pelo evento "Carregar Mais" (Load More) da UI. Isto provoca a reavaliação
     * de [inicialList] e [showMoreButton].
     */
    fun loadMoreTeams() {
        inicialSizeList.value = inicialSizeList.value.plus(10)
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