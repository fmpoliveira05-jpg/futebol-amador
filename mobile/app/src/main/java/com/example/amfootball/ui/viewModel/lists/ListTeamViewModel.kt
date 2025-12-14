package com.example.amfootball.ui.viewModel.lists

import androidx.lifecycle.SavedStateHandle
import com.example.amfootball.R
import com.example.amfootball.core.utils.Arguments
import com.example.amfootball.core.utils.GeneralConst
import com.example.amfootball.core.utils.TeamConst
import com.example.amfootball.data.NetworkConnectivityObserver
import com.example.amfootball.data.filters.FiltersListTeam
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.remote.dtos.rank.RankNameDto
import com.example.amfootball.data.remote.dtos.team.ItemTeamInfoDto
import com.example.amfootball.data.remote.services.TeamService
import com.example.amfootball.domains.enums.UserRole
import com.example.amfootball.domains.enums.pages.ListPlayerMode
import com.example.amfootball.domains.enums.pages.ListTeamMode
import com.example.amfootball.domains.errors.ErrorMessage
import com.example.amfootball.domains.errors.filtersError.FilterTeamError
import com.example.amfootball.ui.viewModel.abstracts.ListsViewModels
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import javax.inject.Inject

/**
 * ViewModel responsável pela gestão do ecrã de listagem de equipas.
 *
 * Esta classe herda de [ListsViewModels] para gerir automaticamente a paginação e o estado de carregamento,
 * focando-se na lógica de filtragem complexa (Pontos, Idade Média, Membros) e nas regras de negócio
 * baseadas no [ListTeamMode] (ex: listar todas as equipas vs. listar equipas para convite de jogo).
 *
 * @property networkObserver Observador de conectividade.
 * @property teamService Serviço de API para operações relacionadas com equipas.
 * @property savedStateHandle Fornece acesso ao estado da navegação, usado para obter o [ListTeamMode].
 * @property sessionManager Gestor da sessão do utilizador logado.
 */
@HiltViewModel
class ListTeamViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver,
    private val teamService: TeamService,
    private val savedStateHandle: SavedStateHandle,
    private val sessionManager: SessionManager?
) : ListsViewModels<ItemTeamInfoDto>(networkObserver = networkObserver) {

    /** ID da equipa do utilizador logado, usado em modos como [ListTeamMode.LIST_TEAM_MATCH_INVITE]. */
    private val teamIdState: MutableStateFlow<String> = MutableStateFlow("")

    val teamId: StateFlow<String> = teamIdState.asStateFlow()

    /** ID do jogador logado, usado em modos como [ListTeamMode.LIST_TEAM_MEMBERSHIP_REQUEST]. */
    private val playerId: MutableStateFlow<String> = MutableStateFlow("")

    /** Nível de permissão do utilizador logado. */
    private val roleState: MutableStateFlow<UserRole> = MutableStateFlow(UserRole.PLAYER_WITHOUT_TEAM)

    val role: StateFlow<UserRole> = roleState.asStateFlow()

    /** Estado atual dos valores dos filtros inseridos pelo utilizador. */
    private val filterState: MutableStateFlow<FiltersListTeam> = MutableStateFlow(FiltersListTeam())
    val uiFilterState: StateFlow<FiltersListTeam> = filterState.asStateFlow()

    /** Estado dos erros de validação dos filtros (ex: mensagens vermelhas nos inputs). */
    private val filterErrorState: MutableStateFlow<FilterTeamError> = MutableStateFlow(FilterTeamError())

    val filterError: StateFlow<FilterTeamError> = filterErrorState.asStateFlow()

    /** Lista de Ranks disponíveis para seleção no filtro. */
    private val listRanks: MutableStateFlow<List<RankNameDto>> = MutableStateFlow(emptyList())
    var listRank: StateFlow<List<RankNameDto>> = listRanks.asStateFlow()

    private val modeStr = savedStateHandle.get<String>(Arguments.LIST_TEAM_MODE)

    /**
     * O modo de operação da página. Define o tipo de lista a ser carregada (Geral vs. Recrutamento).
     * Valor por defeito: [ListPlayerMode.PLAYER_LIST].
     */
    val mode: ListTeamMode = try {
        if (modeStr != null) {
            ListTeamMode.valueOf(modeStr)
        } else {
            ListTeamMode.LIST_TEAM
        }
    } catch (e: Exception) {
        ListTeamMode.LIST_TEAM
    }

    /**
     * Conjunto de IDs de equipas para as quais o utilizador já enviou um pedido de adesão
     * (durante a sessão atual).
     */
    private val _sentRequestIds = MutableStateFlow<Set<String>>(emptySet())
    val sentRequestIds = _sentRequestIds.asStateFlow()

    init {
        val profile = sessionManager?.getUserProfile()
        teamIdState.value = profile?.effectiveTeamId ?: ""
        playerId.value = profile?.loginResponseDto?.localId ?: ""
        roleState.value = profile?.role ?: UserRole.UNAUTHORIZED

        loadListTeam()

        listRanks.value = RankNameDto.generateExampleRanks()
    }

    // --- SETTERS (Atualizam o estado dos filtros) ---
    /** Atualiza o filtro de Nome. */
    fun onNameChange(name: String) {
        filterState.value = filterState.value.copy(name = name.ifEmpty { null })
    }

    /** Atualiza o filtro de Cidade. */
    fun onCityChange(city: String) {
        filterState.value = filterState.value.copy(city = city.ifEmpty { null })
    }

    /** Atualiza o filtro de Rank. */
    fun onRankChange(rank: String) {
        filterState.value = filterState.value.copy(rank = rank.ifEmpty { null })
    }

    /** Atualiza o filtro de Pontos Mínimos. */
    fun onMinPointChange(minPoint: Int?) {
        filterState.value = filterState.value.copy(minPoint = minPoint)
    }

    /** Atualiza o filtro de Pontos Máximos. */
    fun onMaxPointChange(maxPoint: Int?) {
        filterState.value = filterState.value.copy(maxPoint = maxPoint)
    }

    /** Atualiza o filtro de Idade Média Mínima. */
    fun onMinAgeChange(minAge: Int?) {
        filterState.value = filterState.value.copy(minAge = minAge)
    }

    /** Atualiza o filtro de Idade Média Máxima. */
    fun onMaxAgeChange(maxAge: Int?) {
        filterState.value = filterState.value.copy(maxAge = maxAge)
    }

    /** Atualiza o filtro de Número de Membros Mínimo. */
    fun onMinNumberMembersChange(minNumberMembers: Int?) {
        filterState.value = filterState.value.copy(minNumberMembers = minNumberMembers)
    }

    /** Atualiza o filtro de Número de Membros Máximo. */
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
        if (!validateForm()) {
            return
        }

        if (!networkObserver.isOnlineOneShot()) {
            listState.value = offlineFilterList(
                originalList = originalList,
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

        if (!networkObserver.isOnlineOneShot()) {
            listState.value = originalList
        } else {
            loadListTeam()
        }
    }

    /**
     * Envia um pedido de adesão (Membership) a uma equipa pelo jogador logado.
     *
     * Requer que o utilizador esteja no papel [UserRole.PLAYER_WITHOUT_TEAM].
     *
     * @param idTeam O ID da equipa alvo.
     */
    fun sendMemberShipRequest(idTeam: String) {
        val userProfile = sessionManager?.getUserProfile()

        if (userProfile == null) {
            updateToast(message = R.string.toast_autenticate_people)
            return
        }

        if (roleState.value != UserRole.PLAYER_WITHOUT_TEAM) {
            updateToast(message = R.string.toast_admin_only_send_membership_request)
            return
        }

        launchDataLoad {
            val playerId = userProfile.loginResponseDto?.localId ?: ""

            teamService.playerSendMembershipRequestToTeam(playerId = playerId, teamId = idTeam)

            _sentRequestIds.update { currentSet ->
                currentSet + idTeam
            }

            updateToast(message = R.string.toast_success_send_membershipRequest)
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
        launchDataLoad {
            var teams: List<ItemTeamInfoDto>
            val filter = filterState.value
            when (mode) {
                ListTeamMode.LIST_TEAM -> {
                    teams = teamService.getListTeam(filter)
                }
                ListTeamMode.LIST_TEAM_MATCH_INVITE -> {
                    teams = teamService.getListTeamMatchInvite(teamId = teamIdState.value, filter = filter)
                }
                ListTeamMode.LIST_TEAM_MEMBERSHIP_REQUEST -> {
                    teams = teamService.getListTeamMemberShipRequest(playerId = playerId.value, filter = filter)
                }
            }

            listState.value = teams
            if (filterState.value == FiltersListTeam()) {
                originalList = teams
            }
        }
    }

    /**
     * Filtra uma lista de equipas em memória com base nos filtros fornecidos.
     * Utiliza lógica "AND" (todos os critérios de filtro têm de ser verdadeiros) nos dados em cache.
     *
     * @param originalList A lista completa de equipas em cache.
     * @param filters Os critérios de filtro atuais.
     * @return Uma lista filtrada de [ItemTeamInfoDto].
     */
    private fun offlineFilterList(
        originalList: List<ItemTeamInfoDto>,
        filters: FiltersListTeam
    ): List<ItemTeamInfoDto> {
        return originalList.filter { item ->
            val matchesName =
                filters.name.isNullOrBlank() || item.name.contains(filters.name, ignoreCase = true)
            val matchesCity =
                filters.city.isNullOrBlank() || item.city.contains(filters.city, ignoreCase = true)
            val matchesRank = filters.rank.isNullOrBlank() || item.rank.name.contains(
                filters.rank,
                ignoreCase = true
            )

            val matchesMinPoint = filters.minPoint == null || item.points >= filters.minPoint
            val matchesMaxPoint = filters.maxPoint == null || item.points <= filters.maxPoint

            val matchesMinAge = filters.minAge == null || item.averageAge >= filters.minAge
            val matchesMaxAge = filters.maxAge == null || item.averageAge <= filters.maxAge

            val matchesMinMembers =
                filters.minNumberMembers == null || item.numberMembers >= filters.minNumberMembers
            val matchesMaxMembers =
                filters.maxNumberMembers == null || item.numberMembers <= filters.maxNumberMembers

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
            if (minPoint < TeamConst.MIN_NUMBER_POINTS) {
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
            minMembersError =
                ErrorMessage(messageId = R.string.error_min_number_members_greater_max)
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

        val isValid = listOf(
            nameError, cityError, minPointError, maxPointError, minAgeError,
            maxAgeError, minMembersError, maxMembersError
        ).all { it == null }

        return isValid
    }
}