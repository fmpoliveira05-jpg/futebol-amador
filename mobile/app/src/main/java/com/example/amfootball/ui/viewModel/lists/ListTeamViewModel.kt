package com.example.amfootball.ui.viewModel.lists

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import androidx.navigation.NavHostController
import com.example.amfootball.data.UiState
import com.example.amfootball.data.filters.FiltersListTeam
import com.example.amfootball.data.dtos.team.ItemTeamInfoDto
import com.example.amfootball.data.dtos.rank.RankNameDto
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.repository.TeamRepository
import com.example.amfootball.navigation.objects.Routes
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject
import kotlin.text.ifEmpty
import com.example.amfootball.R
import com.example.amfootball.data.errors.filtersError.FilterTeamError
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.TeamConst

// TODO: Falta implementar a chamada à API no método memberShipRequest
/**
 * ViewModel responsável pela gestão do ecrã de listagem de equipas.
 *
 * Gere o estado da lista (loading, erros, dados), aplica filtros de pesquisa (online e offline),
 * valida os formulários de filtro e gere a navegação para detalhes e convites.
 *
 * Funcionalidades principais:
 * - Carregamento de dados da API com cache local (`allTeamOriginal`).
 * - Filtragem reativa e validação de dados (Min/Max, limites de caracteres).
 * - Gestão de conectividade (aviso de sem internet).
 */
@HiltViewModel
class ListTeamViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver,
    private val teamRepository: TeamRepository,
): ViewModel() {

    /** Estado atual da lista de equipas visível na UI (pode estar filtrada). */
    private val listState: MutableStateFlow<List<ItemTeamInfoDto>> = MutableStateFlow(emptyList())
    val listTeams: StateFlow<List<ItemTeamInfoDto>> = listState

    /** Cache local da lista completa original vinda da API. Usada para filtrar sem novas chamadas de rede. */
    private var allTeamOriginal: List<ItemTeamInfoDto> = emptyList()

    /** Estado atual dos valores dos filtros inseridos pelo utilizador. */
    private val filterState: MutableStateFlow<FiltersListTeam> = MutableStateFlow(FiltersListTeam())
    val uiFilterState: StateFlow<FiltersListTeam> = filterState

    /** Estado dos erros de validação dos filtros (ex: mensagens vermelhas nos inputs). */
    private val filterErrorState: MutableStateFlow<FilterTeamError> = MutableStateFlow(FilterTeamError())
    val filterError: StateFlow<FilterTeamError> = filterErrorState

    /** Lista de Ranks disponíveis para seleção no filtro. */
    private val listRanks: MutableStateFlow<List<RankNameDto>> = MutableStateFlow(emptyList())
    var listRank: StateFlow<List<RankNameDto>> = listRanks

    /** Monitoriza o estado da conexão à internet. */
    private val _isOnline = MutableStateFlow(networkObserver.isOnlineOneShot())
    val isOnline: StateFlow<Boolean> = _isOnline

    /** Estado geral da UI (Loading e Erros genéricos de Rede/API). */
    private val _uiState: MutableStateFlow<UiState> = MutableStateFlow(UiState(isLoading = true))
    val uiState: StateFlow<UiState> = _uiState

    init {
        observeNetworkChanges()
        loadListTeam()

        //Por enquanto esta lista vai ser estatica
        listRanks.value = RankNameDto.generateExampleRanks()
    }

    // --- SETTERS (Atualizam o estado dos filtros) ---
    fun onNameChange(name: String) {
        filterState.value = filterState.value.copy(name = name.ifEmpty {null})
    }

    fun onCityChange(city: String) {
        filterState.value = filterState.value.copy(city = city.ifEmpty {null})
    }

    fun onRankChange(rank: String) {
        filterState.value = filterState.value.copy(rank = rank.ifEmpty {null})
    }

    fun onMinPointChange(minPoint: Int?) {
        filterState.value = filterState.value.copy(minPoint = minPoint)
    }

    fun onMaxPointChange(maxPoint: Int?) {
        filterState.value = filterState.value.copy(maxPoint = maxPoint)
    }

    fun onMinAgeChange(minAge: Int?) {
        filterState.value = filterState.value.copy(minAge = minAge)
    }

    fun onMaxAgeChange(maxAge: Int?) {
        filterState.value = filterState.value.copy(maxAge = maxAge)
    }

    fun onMinNumberMembersChange(minNumberMembers: Int?) {
        filterState.value = filterState.value.copy(minNumberMembers = minNumberMembers)
    }

    fun onMaxNumberMembersChange(maxNumberMembers: Int?) {
        filterState.value = filterState.value.copy(maxNumberMembers = maxNumberMembers)
    }

    // --- MÉTODOS PÚBLICOS ---
    /**
     * Aplica os filtros definidos pelo utilizador.
     *
     * 1. Executa a validação [validateForm]. Se falhar, interrompe e mostra erros.
     * 2. Se estiver Offline, filtra localmente a lista atual para garantir performance e funcionamento sem rede.
     * 3. Se estiver Online, recarrega a lista da API (opção atual).
     */
    fun applyFilters() {
        if(!validateForm()) {
            return
        }

        if(!networkObserver.isOnlineOneShot()) {
            listState.value = offlineFilterList(
                originalList = allTeamOriginal,
                filters = filterState.value
            )
        } else {
            loadListTeam()
        }
    }

    /**
     * Limpa todos os filtros ativos e restaura a lista original.
     */
    fun clearFilters() {
        filterState.value = FiltersListTeam()

        if(!networkObserver.isOnlineOneShot()) {
           listState.value = allTeamOriginal
        } else {
            loadListTeam()
        }
    }

    /**
     * Navega para o ecrã de envio de convite de jogo.
     */
    fun sendMatchInvite(idTeam: String, navHostController: NavHostController) {
        navHostController.navigate(route = "${Routes.TeamRoutes.SEND_MATCH_INVITE.route}/${idTeam}") {
            launchSingleTop = true
        }
    }

    /**
     * Envia um pedido de adesão (Membership) a uma equipa.
     */
    fun sendMemberShipRequest(idTeam: String, navHostController: NavHostController) {
        //TODO: Executar chamada há API para o sendMemberShipRequest
    }

    /**
     * Navega para o perfil detalhado da equipa.
     */
    fun showMore(idTeam: String, navHostController: NavHostController,
    ) {
        navHostController.navigate(route = "${Routes.TeamRoutes.TEAM_PROFILE.route}/${idTeam}") {
            launchSingleTop = true
        }
    }

    fun retry() {
        loadListTeam()
    }

    //Metodos privados
    /**
     * Carrega a lista de equipas.
     *
     * Verifica a conexão à internet:
     * - Se Offline: Atualiza o estado de erro e não tenta fazer o pedido.
     * - Se Online: Tenta obter os dados do repositório, atualiza a lista visível e guarda uma cópia em [allTeamOriginal].
     */
    private fun loadListTeam() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, errorMessage = null) }

            if (!networkObserver.isOnlineOneShot()) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem internet. Verifique a sua conexão.")
                }
                return@launch
            }

            try {
                val teams = teamRepository.getListTeam(filterState.value)

                listState.value = teams
                allTeamOriginal = teams
                _uiState.update { it.copy(isLoading = false) }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = e.message)
                }
            }
        }
    }

    /**
     * Inicia a observação contínua do estado da conectividade de rede.
     *
     * Lança uma corrotina no [viewModelScope] que subscreve o fluxo (Flow) do [networkObserver].
     * Sempre que o estado da rede altera (Online <-> Offline), a variável [_isOnline] é atualizada
     * automaticamente, permitindo que a UI reaja em tempo real (ex: exibir/ocultar banner de "Sem Internet").
     */
    private fun observeNetworkChanges() {
        viewModelScope.launch {
            networkObserver.observeConnectivity()
                .collect { isConnected ->
                    _isOnline.value = isConnected
                }
        }
    }

    /**
     * Filtra uma lista de equipas em memória com base nos filtros fornecidos.
     * Utiliza lógica "AND" (todos os critérios têm de ser verdadeiros).
     */
    private fun offlineFilterList(
        originalList: List<ItemTeamInfoDto>,
        filters: FiltersListTeam
    ): List<ItemTeamInfoDto> {
        return originalList.filter { item ->
            val matchesName = filters.name.isNullOrBlank() || item.name.contains(filters.name, ignoreCase = true)
            val matchesCity = filters.city.isNullOrBlank() || item.city.contains(filters.city, ignoreCase = true)
            val matchesRank = filters.rank.isNullOrBlank() || item.rank.name.contains(filters.rank, ignoreCase = true)

            val matchesMinPoint = filters.minPoint == null || item.points >= filters.minPoint
            val matchesMaxPoint = filters.maxPoint == null || item.points <= filters.maxPoint

            val matchesMinAge = filters.minAge == null || item.averageAge >= filters.minAge
            val matchesMaxAge = filters.maxAge == null || item.averageAge <= filters.maxAge

            val matchesMinMembers = filters.minNumberMembers == null || item.numberMembers >= filters.minNumberMembers
            val matchesMaxMembers = filters.maxNumberMembers == null || item.numberMembers <= filters.maxNumberMembers

            matchesName && matchesCity && matchesRank &&
                    matchesMinPoint && matchesMaxPoint &&
                    matchesMinAge && matchesMaxAge &&
                    matchesMinMembers && matchesMaxMembers
        }
    }

    /**
     * Valida os dados introduzidos nos filtros.
     * Verifica:
     * - Limites de caracteres (Nome, Cidade).
     * - Intervalos válidos (Min < Max) para Pontos, Idade e Membros.
     * - Valores mínimos absolutos (não negativos).
     *
     * Atualiza o [filterErrorState] com as mensagens de erro correspondentes.
     * @return true se tudo for válido, false se houver erros.
     */
    private fun validateForm(): Boolean {
        val current = filterState.value

        val name = current.name
        val city = current.city
        val minPoint = current.minPoint
        val maxPoint = current.maxPoint
        val minAge = current.minAge
        val maxAge = current.maxAge
        val minMembers = current.minNumberMembers
        val maxMembers = current.maxNumberMembers

        var nameError: ErrorMessage? = null
        var cityError: ErrorMessage? = null
        var minPointError: ErrorMessage? = null
        var maxPointError: ErrorMessage? = null
        var minAgeError: ErrorMessage? = null
        var maxAgeError: ErrorMessage? = null
        var minMembersError: ErrorMessage? = null
        var maxMembersError: ErrorMessage? = null

        if (name != null && name.length > TeamConst.MAX_NAME_LENGTH) {
            nameError = ErrorMessage(
                messageId = R.string.error_max_name_team,
                args = listOf(TeamConst.MAX_NAME_LENGTH)
            )
        }
        if (city != null && city.length > GeneralConst.MAX_CITY_LENGTH) {
            cityError = ErrorMessage(
                messageId = R.string.error_max_city,
                args = listOf(GeneralConst.MAX_CITY_LENGTH)
            )
        }

        var isValidMinPoint = true
        if (minPoint != null) {
            if(minPoint < TeamConst.MIN_NUMBER_POINTS) {
                minPointError = ErrorMessage(
                    messageId = R.string.error_min_number_points,
                    args = listOf(TeamConst.MIN_NUMBER_POINTS)
                )
                isValidMinPoint = false
            } else if (minPoint > TeamConst.MAX_NUMBER_POINTS) {
                minPointError = ErrorMessage(
                    messageId = R.string.error_max_city,
                    args = listOf(TeamConst.MAX_NUMBER_POINTS)
                )
                isValidMinPoint = false
            }
        }

        var isValidMaxPoint = true
        if (maxPoint != null) {
            if (maxPoint < TeamConst.MIN_NUMBER_POINTS) {
                maxPointError = ErrorMessage(
                    messageId = R.string.error_min_number_points,
                    args = listOf(TeamConst.MIN_NUMBER_POINTS)
                )
                isValidMaxPoint = false
            } else if (maxPoint > TeamConst.MAX_NUMBER_POINTS) {
                maxPointError = ErrorMessage(
                    messageId = R.string.error_max_number_points,
                    args = listOf(TeamConst.MAX_NUMBER_POINTS)
                )
                isValidMaxPoint = false
            }
        }

        if (isValidMinPoint && isValidMaxPoint && minPoint != null && maxPoint != null && minPoint > maxPoint) {
            minPointError = ErrorMessage(messageId = R.string.error_min_number_points_greater_max)
            maxPointError = ErrorMessage(messageId = R.string.error_max_number_points_minor_min)
        }

        var isValidMinAge = true
        if (minAge != null) {
            if (minAge < TeamConst.MIN_AVERAGE_AGE) {
                minAgeError = ErrorMessage(
                    messageId = R.string.error_min_age,
                    args = listOf(TeamConst.MIN_AVERAGE_AGE)
                )
                isValidMinAge = false
            } else if (minAge > TeamConst.MAX_AVERAGE_AGE) {
                minAgeError = ErrorMessage(
                    messageId = R.string.error_max_age,
                    args = listOf(TeamConst.MAX_AVERAGE_AGE)
                )
                isValidMinAge = false
            }
        }

        var isValidMaxAge = true
        if (maxAge != null) {
            if (maxAge < TeamConst.MIN_AVERAGE_AGE) {
                maxAgeError = ErrorMessage(
                    messageId = R.string.error_min_age,
                    args = listOf(TeamConst.MIN_AVERAGE_AGE)
                )
                isValidMaxAge = false
            } else if (maxAge > TeamConst.MAX_AVERAGE_AGE) {
                maxAgeError = ErrorMessage(
                    messageId = R.string.error_max_age,
                    args = listOf(TeamConst.MAX_AVERAGE_AGE)
                )
                isValidMaxAge = false
            }
        }

        if (isValidMinAge && isValidMaxAge && minAge != null && maxAge != null && minAge > maxAge) {
            minAgeError = ErrorMessage(messageId = R.string.error_min_age_greater_max)
            maxAgeError = ErrorMessage(messageId = R.string.error_min_age_greater_max)
        }

        var isValidMinMembers = true
        if (minMembers != null) {
            if (minMembers < TeamConst.MIN_MEMBERS) {
                minMembersError = ErrorMessage(
                    messageId = R.string.error_min_number_members,
                    args = listOf(TeamConst.MIN_MEMBERS)
                )
                isValidMinMembers = false
            } else if (minMembers > TeamConst.MAX_MEMBERS) {
                minMembersError = ErrorMessage(
                    messageId = R.string.error_max_number_members,
                    args = listOf(TeamConst.MAX_MEMBERS)
                )
                isValidMinMembers = false
            }
        }

        var isValidMaxMembers = true
        if (maxMembers != null) {
            if (maxMembers < TeamConst.MIN_MEMBERS) {
                maxMembersError = ErrorMessage(
                    messageId = R.string.error_min_number_members,
                    args = listOf(TeamConst.MIN_MEMBERS)
                )
                isValidMaxMembers = false
            } else if (maxMembers > TeamConst.MAX_MEMBERS) {
                maxMembersError = ErrorMessage(
                    messageId = R.string.error_max_number_members,
                    args = listOf(TeamConst.MAX_MEMBERS)
                )
                isValidMaxMembers = false
            }
        }

        if (isValidMinMembers && isValidMaxMembers && minMembers != null && maxMembers != null && minMembers > maxMembers) {
            minMembersError = ErrorMessage(messageId = R.string.error_min_number_members_greater_max)
            maxMembersError = ErrorMessage(messageId = R.string.error_max_number_members_minor_min)
        }

        filterErrorState.value = FilterTeamError(
            nameError = nameError,
            cityError = cityError,
            minPointError = minPointError,
            maxPointError = maxPointError,
            minAgeError = minAgeError,
            maxAgeError = maxAgeError,
            minNumberMembersError = minMembersError,
            maxNumberMembersError = maxMembersError
        )

        val isValid = listOf(nameError, cityError, minPointError, maxPointError, minAgeError,
            maxAgeError, minMembersError, maxMembersError).all { it == null }

        return isValid
    }
}