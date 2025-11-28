package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.SavedStateHandle
import androidx.navigation.NavHostController
import com.example.amfootball.data.filters.FilterMembersTeam
import com.example.amfootball.data.dtos.player.MemberTeamDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.filtersError.FilterMembersFilterError
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.utils.UserConst
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.repository.TeamRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

//TODO: Falta validar se o utilizador tem autorização para aceder a cada um dos recursos
/**
 * ViewModel responsável pela gestão de estado e lógica de negócio do ecrã de Lista de Membros da Equipa.
 *
 * Este ViewModel gere:
 * - O carregamento da lista de membros a partir da API.
 * - A filtragem da lista (suportando modo Online e Offline).
 * - A gestão de ações administrativas (Promover, Despromover, Expulsar).
 * - A monitorização do estado da rede para feedback em tempo real.
 *
 * @property networkObserver Observador de conectividade para detetar mudanças no estado da rede.
 * @property savedStateHandle Manipulador de estado para recuperar argumentos de navegação (ex: teamId).
 * @property teamRepository Repositório para acesso aos dados da equipa (API).
 */
@HiltViewModel
class ListMembersViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver,
    private val sessionManager: SessionManager,
    private val teamRepository: TeamRepository
): ViewModel() {

    /**
     * ID da equipa obtido através dos argumentos de navegação.
     * É fundamental para todas as operações deste ecrã.
     * @throws IllegalStateException Se o argumento "teamId" não for fornecido.
     */
    private val teamId: String = sessionManager.getUserProfile()?.idTeam ?: ""

    /** Estado atual dos filtros aplicados pelo utilizador. */
    private val filterState: MutableStateFlow<FilterMembersTeam> = MutableStateFlow(FilterMembersTeam())
    val uiFilter: StateFlow<FilterMembersTeam> = filterState

    /** Estado que contém os erros de validação dos filtros (ex: idade mínima maior que máxima). */
    private val errorFilters: MutableStateFlow<FilterMembersFilterError> = MutableStateFlow(FilterMembersFilterError())
    val uiErrorFilters: StateFlow<FilterMembersFilterError> = errorFilters

    /**
     * Estado da lista de membros a ser exibida na UI.
     * Esta lista pode ser o resultado direto da API ou uma versão filtrada localmente.
     */
    private val listMemberState: MutableStateFlow<List<MemberTeamDto>> = MutableStateFlow(emptyList<MemberTeamDto>())
    val uiList: StateFlow<List<MemberTeamDto>> = listMemberState

    /**
     * Cópia local da lista completa de membros obtida da API.
     * Usada como base para a filtragem offline para evitar perdas de dados ao aplicar filtros.
     */
    private var allMembersOriginal: List<MemberTeamDto> = emptyList()

    /** Lista de opções para o filtro de Tipo de Membro (ex: Jogador, Admin). */
    private val listTypeMember: MutableStateFlow<List<TypeMember?>> = MutableStateFlow(emptyList())
    val uiListTypeMember: StateFlow<List<TypeMember?>> = listTypeMember

    /** Lista de opções para o filtro de Posição em campo. */
    private val listPositions: MutableStateFlow<List<Position?>> = MutableStateFlow(emptyList())
    val uiListPositions: StateFlow<List<Position?>> = listPositions

    /** Estado global da UI, incluindo indicadores de carregamento (Loading) e mensagens de erro gerais. */
    private val _uiState: MutableStateFlow<UiState> = MutableStateFlow(UiState(isLoading = true))
    val uiState: StateFlow<UiState> = _uiState

    /** Estado de conectividade da rede (True = Online, False = Offline). */
    private val isConnected: MutableStateFlow<Boolean> = MutableStateFlow(networkObserver.isOnlineOneShot())
    val uiIsConnected: StateFlow<Boolean> = isConnected

    //Init
    init {
        observeNetworkChanges()
        loadListMember()
        loadListTypeMember()
        loadListPosition()
    }

    // --- SETTERS DE FILTROS ---

    /** Atualiza o filtro de tipo de membro selecionado. */
    fun onTypeMemberChange(typeMember: TypeMember?) {
        filterState.value = filterState.value.copy(typeMember = typeMember)
    }

    /** Atualiza o filtro de nome. */
    fun onNameChange(newName: String) {
        filterState.value = filterState.value.copy(name = newName)
    }

    /** Atualiza o filtro de idade mínima. */
    fun onMinAgeChange(newAge: Int?) {
        filterState.value = filterState.value.copy(minAge = newAge)
    }

    /** Atualiza o filtro de idade máxima. */
    fun onMaxAgeChange(newMaxAge: Int?) {
        filterState.value = filterState.value.copy(maxAge = newMaxAge)
    }

    /** Atualiza o filtro de posição. */
    fun onPositionChange(newPosition: Position?) {
        filterState.value = filterState.value.copy(position = newPosition)
    }

    /** Limpa todos os filtros, repondo o estado inicial. */
    fun onClearFilter() {
        filterState.value = FilterMembersTeam()
    }

    //Metodos
    //TODO: Perguntar ao setor se aqui tem logica filtrar sempre offline
    /**
     * Aplica os filtros definidos pelo utilizador.
     *
     * 1. Valida o formulário ([validateFilter]). Se inválido, aborta.
     * 2. Verifica a conectividade:
     * - **Offline:** Filtra a lista localmente ([filterListOffline]) usando [allMembersOriginal].
     * - **Online:** Solicita uma nova lista filtrada à API ([loadListMember]).
     */
    fun onApplyFilter() {
        if (!validateFilter()) {
            return
        }

        if (!isConnected.value) {
            listMemberState.value = filterListOffline(
                originalList = allMembersOriginal,
                filters = filterState.value
            )
        } else {
            loadListMember()
        }
    }

    //TODO: Validar se o utilizador tem autorização para aceder a cada um dos recursos (embora o botão não pode aparecer na mesma no forntend confirmar aqui também)
    /**
     * Promove um jogador a Administrador da equipa.
     *
     * - Verifica conectividade antes de iniciar.
     * - Atualiza o estado de Loading.
     * - Em caso de erro, atualiza a mensagem de erro no UiState.
     *
     * @param playerId O ID do jogador a promover.
     */
    fun onPromoteMember(playerId: String) {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true) }

            if (!networkObserver.isOnlineOneShot()) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem internet. Verifique a sua conexão.")
                }
                return@launch
            }

            try {
                val teams = teamRepository.promotePlayer(teamId = teamId, playerPromoteId = playerId)

                _uiState.update { it.copy(isLoading = false) }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = e.message)
                }
            }
        }
    }

    //TODO: Validar se o utilizador tem autorização para aceder a cada um dos recursos (embora o botão não pode aparecer na mesma no forntend confirmar aqui também)
    /**
     * Despromove um Administrador a membro regular (Jogador).
     *
     * @param adminId O ID do administrador a despromover.
     */
    fun onDemoteMember(adminId: String) {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true) }

            if (!networkObserver.isOnlineOneShot()) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem internet. Verifique a sua conexão.")
                }
                return@launch
            }

            try {
                val teams = teamRepository.demoteAdmin(teamId = teamId, adminDemoteId = adminId)

                _uiState.update { it.copy(isLoading = false) }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = e.message)
                }
            }
        }
    }

    //TODO: Validar se o utilizador tem autorização para aceder a cada um dos recursos (embora o botão não pode aparecer na mesma no forntend confirmar aqui também)
    /**
     * Remove (expulsa) um membro da equipa.
     *
     * @param playerId O ID do membro a remover.
     */
    fun onRemovePlayer(playerId: String) {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true) }

            if (!networkObserver.isOnlineOneShot()) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem internet. Verifique a sua conexão.")
                }
                return@launch
            }

            try {
                val teams = teamRepository.removePlayerTeam(teamId = teamId, playerId = playerId)

                _uiState.update { it.copy(isLoading = false) }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = e.message)
                }
            }
        }
    }

    /**
     * Navega para o ecrã de detalhes do perfil do jogador.
     *
     * @param playerId O ID do jogador a visualizar.
     * @param navHostController O controlador de navegação.
     */
    fun onShowMoreInfo(playerId: String, navHostController: NavHostController) {
        navHostController.navigate("${Routes.UserRoutes.PROFILE.route}/{${playerId}}") {
            launchSingleTop = true
        }
    }
    
    //Metodos privados
    /**
     * Carrega a lista de membros da API.
     *
     * Atualiza tanto a lista visível ([listMemberState]) quanto a cópia de segurança ([allMembersOriginal]).
     * Gere os estados de Loading e erros de rede/API.
     */
    private fun loadListMember() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true) }

            if (!networkObserver.isOnlineOneShot()) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem internet. Verifique a sua conexão.")
                }
                return@launch
            }

            try {
                val teams = teamRepository.getListMembers(teamId = teamId, filterState.value)

                listMemberState.value = teams
                allMembersOriginal = teams

                _uiState.update { it.copy(isLoading = false) }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = e.message)
                }
            }
        }
    }

    /** Inicializa a lista estática de tipos de membros para o filtro (Inclui 'null' para a opção "Todos"). */
    private fun loadListTypeMember() {
        listTypeMember.value = listOf(
            null,
            TypeMember.PLAYER,
            TypeMember.ADMIN_TEAM
        )    
    }

    /** Inicializa a lista estática de posições para o filtro (Inclui 'null' para a opção "Todas"). */
    private fun loadListPosition() {
        listPositions.value = listOf(
            null,
            Position.FORWARD,
            Position.MIDFIELDER,
            Position.DEFENDER,
            Position.GOALKEEPER
        )
    }

    /**
     * Realiza a filtragem local (em memória) da lista de membros.
     * Utilizado quando não há conexão à internet para permitir a pesquisa nos dados já carregados.
     *
     * @param originalList A lista completa de membros.
     * @param filters Os critérios a aplicar.
     * @return Uma nova lista contendo apenas os membros que correspondem a TODOS os critérios.
     */
    private fun filterListOffline(
        originalList: List<MemberTeamDto>,
        filters: FilterMembersTeam
    ): List<MemberTeamDto> {
        return originalList.filter { item ->
            val name = filters.name.isNullOrBlank() || item.name.contains(filters.name, ignoreCase = true)
            val typeMember = filters.typeMember == null || item.typeMember == filters.typeMember
            val minAge = filters.minAge == null || item.age >= filters.minAge
            val maxAge = filters.maxAge == null || item.age <= filters.maxAge
            val position = filters.position == null || item.position == filters.position

            name && typeMember && minAge && maxAge && position
        }
    }

    /**
     * Monitoriza as alterações de conectividade de rede em tempo real.
     * Atualiza o estado [isConnected] automaticamente.
     */
    private fun observeNetworkChanges() {
        viewModelScope.launch {
            networkObserver.observeConnectivity()
                .collect { isOnline ->
                    isConnected.value= isOnline
                }
        }
    }

    /**
     * Valida os critérios de filtro inseridos.
     *
     * Regras verificadas:
     * - Comprimento máximo do nome.
     * - Idade mínima e máxima permitida.
     * - Consistência de intervalo (Idade Mínima não pode ser maior que Idade Máxima).
     *
     * @return `true` se todos os dados forem válidos, `false` caso contrário (atualizando [errorFilters]).
     */
    private fun validateFilter(): Boolean {
        val name = filterState.value.name
        val minAge = filterState.value.minAge
        val maxAge = filterState.value.maxAge

        var errorName: ErrorMessage? = null
        var errorMinAge: ErrorMessage? = null
        var errorMaxAge: ErrorMessage? = null

        if(name != null && name.length > UserConst.MAX_NAME_LENGTH) {
            errorName = ErrorMessage(
                messageId = R.string.error_max_name_member,
                args = listOf(UserConst.MAX_NAME_LENGTH)
            )
        }

        var minAgeValid = true
        if(minAge != null && minAge < UserConst.MIN_AGE) {
            errorMinAge = ErrorMessage(
                messageId = R.string.error_min_age,
                args = listOf(UserConst.MIN_AGE)
            )
            minAgeValid = false
        }

        var maxAgeValid = true
        if(maxAge != null && maxAge > UserConst.MAX_AGE) {
            errorMaxAge = ErrorMessage(
                messageId = R.string.error_max_age,
                args = listOf(UserConst.MAX_AGE)
            )
            maxAgeValid = false
        }

        if (minAgeValid && maxAgeValid && maxAge != null && minAge != null) {
            if (minAge > maxAge) {
                errorMinAge = ErrorMessage(messageId = R.string.error_min_age_greater_max)
                errorMaxAge = ErrorMessage(messageId = R.string.error_max_age_minor_min)
            }
        }

        errorFilters.value = FilterMembersFilterError(
            nameError = errorName,
            minAgeError = errorMinAge,
            maxAgeError = errorMaxAge
        )

        val isValid = listOf(errorName, errorMinAge, errorMaxAge).all {
            it == null
        }

        return isValid
    }
}