package com.example.amfootball.ui.viewModel.team

import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.dtos.player.MemberTeamDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.filtersError.FilterMembersFilterError
import com.example.amfootball.data.filters.FilterMembersTeam
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.services.TeamService
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.abstracts.ListsViewModels
import com.example.amfootball.utils.UserConst
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import javax.inject.Inject

/**
 * ViewModel responsável pela gestão de estado e lógica de negócio do ecrã de Lista de Membros da Equipa.
 *
 * Utiliza a arquitetura MVVM com Hilt para injeção de dependências.
 *
 * Funcionalidades principais:
 * - **Carregamento de Dados:** Obtém a lista de membros da API.
 * - **Filtragem Híbrida:** Suporta filtragem via API (Online) e filtragem em memória (Offline).
 * - **Gestão de Membros:** Ações de promover, despromover e remover jogadores.
 * - **Gestão de Estado:** Controla Loading, Erros e Conectividade.
 *
 * @property networkObserver Observador para detetar alterações na conexão à internet.
 * @property sessionManager Gestor de sessão para acesso ao perfil do utilizador logado.
 * @property teamRepository Repositório para comunicação com a API de equipas.
 */
@HiltViewModel
class ListMembersViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver,
    private val sessionManager: SessionManager,
    private val teamRepository: TeamService
) : ListsViewModels<MemberTeamDto>(networkObserver = networkObserver) {

    /**
     * ID da equipa do utilizador autenticado.
     *
     * Obtido diretamente do perfil armazenado no [SessionManager].
     * Caso o perfil não esteja disponível, retorna uma string vazia.
     */
    private val teamId: String = sessionManager.getUserProfile()?.team?.id ?: ""

    /** Estado atual dos filtros aplicados pelo utilizador. */
    private val filterState: MutableStateFlow<FilterMembersTeam> =
        MutableStateFlow(FilterMembersTeam())
    val uiFilter: StateFlow<FilterMembersTeam> = filterState

    /** Estado que contém os erros de validação dos filtros (ex: idade mínima maior que máxima). */
    private val errorFilters: MutableStateFlow<FilterMembersFilterError> =
        MutableStateFlow(FilterMembersFilterError())
    val uiErrorFilters: StateFlow<FilterMembersFilterError> = errorFilters

    /** Lista de opções para o filtro de Tipo de Membro (ex: Jogador, Admin). */
    private val listTypeMember: MutableStateFlow<List<TypeMember?>> = MutableStateFlow(emptyList())
    val uiListTypeMember: StateFlow<List<TypeMember?>> = listTypeMember

    /** Lista de opções para o filtro de Posição em campo. */
    private val listPositions: MutableStateFlow<List<Position?>> = MutableStateFlow(emptyList())
    val uiListPositions: StateFlow<List<Position?>> = listPositions

    //Init
    init {
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


        if (!isOnline.value) {
            listState.value = filterListOffline(
                originalList = originalList,
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
        launchDataLoad {
            teamRepository.promotePlayer(teamId = teamId, playerPromoteId = playerId)
        }
    }

    //TODO: Validar se o utilizador tem autorização para aceder a cada um dos recursos (embora o botão não pode aparecer na mesma no forntend confirmar aqui também)
    /**
     * Despromove um Administrador a membro regular (Jogador).
     *
     * @param adminId O ID do administrador a despromover.
     */
    fun onDemoteMember(adminId: String) {
        launchDataLoad {
            val teams = teamRepository.demoteAdmin(teamId = teamId, adminDemoteId = adminId)
        }
    }

    //TODO: Validar se o utilizador tem autorização para aceder a cada um dos recursos (embora o botão não pode aparecer na mesma no forntend confirmar aqui também)
    /**
     * Remove (expulsa) um membro da equipa.
     *
     * @param playerId O ID do membro a remover.
     */
    fun onRemovePlayer(playerId: String) {
        launchDataLoad {
            teamRepository.removePlayerTeam(teamId = teamId, playerId = playerId)
        }
    }

    /**
     * Navega para o ecrã de detalhes do perfil do jogador.
     *
     * @param playerId O ID do jogador a visualizar.
     * @param navHostController O controlador de navegação.
     */
    fun onShowMoreInfo(playerId: String, navHostController: NavHostController) {
        navHostController.navigate("${Routes.UserRoutes.PROFILE.route}/${playerId}") {
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
        launchDataLoad {
            val teams = teamRepository.getListMembers(teamId = teamId, filterState.value)

            listState.value = teams
            if (filterState.value == FilterMembersTeam()) {
                originalList = teams
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
            val name =
                filters.name.isNullOrBlank() || item.name.contains(filters.name, ignoreCase = true)
            val typeMember = filters.typeMember == null || item.typeMember == filters.typeMember
            val minAge = filters.minAge == null || item.age >= filters.minAge
            val maxAge = filters.maxAge == null || item.age <= filters.maxAge
            val position = filters.position == null || item.position == filters.position

            name && typeMember && minAge && maxAge && position
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

        if (name != null && name.length > UserConst.MAX_NAME_LENGTH) {
            errorName = ErrorMessage(
                messageId = R.string.error_max_name_member,
                args = listOf(UserConst.MAX_NAME_LENGTH)
            )
        }

        var minAgeValid = true
        if (minAge != null && minAge < UserConst.MIN_AGE) {
            errorMinAge = ErrorMessage(
                messageId = R.string.error_min_age,
                args = listOf(UserConst.MIN_AGE)
            )
            minAgeValid = false
        }

        var maxAgeValid = true
        if (maxAge != null && maxAge > UserConst.MAX_AGE) {
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