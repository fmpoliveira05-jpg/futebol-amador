package com.example.amfootball.ui.viewModel.lists

import androidx.lifecycle.SavedStateHandle
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.domains.enums.Position
import com.example.amfootball.domains.errors.ErrorMessage
import com.example.amfootball.domains.errors.filtersError.FilterPlayersErrors
import com.example.amfootball.data.filters.FilterListPlayer
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.NetworkConnectivityObserver
import com.example.amfootball.data.remote.services.PlayerService
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.abstracts.ListsViewModels
import com.example.amfootball.core.utils.GeneralConst
import com.example.amfootball.core.utils.PlayerConst
import com.example.amfootball.core.utils.UserConst
import com.example.amfootball.data.remote.dtos.player.InfoPlayerDto
import com.example.amfootball.domains.enums.UserRole
import com.example.amfootball.domains.enums.pages.ListPlayerMode
import com.example.amfootball.ui.navigation.objects.Arguments
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import javax.inject.Inject

//TODO: Falta a parte para este viewModel permiteit com que a página deia para a lista geral e para o de mandar convites de adesão
/**
 * ViewModel responsável pela lógica de negócio do ecrã de Listagem de Jogadores.
 *
 * Esta classe herda de [ListsViewModels] para obter automaticamente a gestão de lista (paginação,
 * cache, estado de loading), focando-se exclusivamente nas regras específicas de jogadores:
 * - Gestão de Filtros (Nome, Cidade, Idade, Posição, Altura).
 * - Validação de dados do formulário de pesquisa.
 * - Comunicação com o [PlayerplayerRepository].
 * - Navegação para detalhes do jogador.
 *
 * @property playerRepository Repositório para acesso aos dados dos jogadores via API.
 */
@HiltViewModel
class ListPlayerViewModel @Inject constructor(
    private val savedStateHandle: SavedStateHandle,
    private val networkObserver: NetworkConnectivityObserver,
    private val playerRepository: PlayerService,
    private val sessionManager: SessionManager?,
) : ListsViewModels<InfoPlayerDto>(networkObserver = networkObserver) {
    private val teamId: MutableStateFlow<String> = MutableStateFlow("")

    /**
     * Estado interno mutável do Role do utilizador.
     */
    private val roleState: MutableStateFlow<UserRole> = MutableStateFlow(UserRole.PLAYER_WITHOUT_TEAM)

    /**
     * Fluxo público imutável que indica o nível de permissão do utilizador na equipa.
     *
     * A UI deve observar este estado para decidir quais cartões mostrar:
     * - [UserRole.ADMIN_TEAM]: Mostra tudo (Agendar, Gerir).
     * - [UserRole.MEMBER_TEAM]: Mostra apenas visualização (Calendário, Lista).
     *
     * Valor por defeito seguro: [UserRole.MEMBER_TEAM].
     */
    val role: StateFlow<UserRole> = roleState.asStateFlow()


    private val userIdState: MutableStateFlow<String> = MutableStateFlow("")


    val userId: StateFlow<String> = userIdState.asStateFlow()

    /**
     * Estado atual dos filtros aplicados pelo utilizador.
     * Observado pela UI para manter os campos de texto e seletores sincronizados.
     */
    private val filterState: MutableStateFlow<FilterListPlayer> = MutableStateFlow(FilterListPlayer())
    val uiFilters: StateFlow<FilterListPlayer> = filterState.asStateFlow()

    /**
     * Estado dos erros de validação dos filtros.
     * Se um filtro for inválido (ex: Idade Min > Max), este estado conterá a mensagem de erro.
     */
    private val filterErrorState: MutableStateFlow<FilterPlayersErrors> =
        MutableStateFlow(FilterPlayersErrors())
    val filterError: StateFlow<FilterPlayersErrors> = filterErrorState.asStateFlow()

    /**
     * Lista de posições disponíveis para preencher o Dropdown de filtro.
     * Inclui a opção `null` para representar "Todas as posições".
     */
    private val listPositions = MutableStateFlow(
        listOf(null, Position.FORWARD, Position.MIDFIELDER, Position.DEFENDER, Position.GOALKEEPER)
    )
    val uiListPositions: StateFlow<List<Position?>> = listPositions.asStateFlow()

    private val modeStr = savedStateHandle.get<String>(Arguments.LIST_PLAYER_MODE)

    val mode: ListPlayerMode = try {
        if (modeStr != null) {
            ListPlayerMode.valueOf(modeStr)
        } else {
            ListPlayerMode.PLAYER_LIST
        }
    } catch (e: Exception) {
        ListPlayerMode.PLAYER_LIST
    }

    private val _sentRequestIds = MutableStateFlow<Set<String>>(emptySet())
    val sentRequestIds = _sentRequestIds.asStateFlow()

    init {
        val profile = sessionManager?.getUserProfile()
        teamId.value = profile?.effectiveTeamId ?: ""
        roleState.value = profile?.role ?: UserRole.PLAYER_WITHOUT_TEAM
        userIdState.value = profile?.loginResponseDto?.localId ?: ""

        loadingListPlayer()
    }

    // --- Métodos de Atualização de Filtros (Data Binding) ---
    fun onNameChange(name: String) {
        filterState.value = filterState.value.copy(name = name)
    }

    fun onCityChange(city: String) {
        filterState.value = filterState.value.copy(city = city)
    }

    fun onMinAgeChange(minAge: Int?) {
        filterState.value = filterState.value.copy(minAge = minAge)
    }

    fun onMaxAgeChange(maxAge: Int?) {
        filterState.value = filterState.value.copy(maxAge = maxAge)
    }

    fun onPositionChange(position: Position?) {
        filterState.value = filterState.value.copy(position = position)
    }

    fun onMinSizeChange(minSize: Int?) {
        filterState.value = filterState.value.copy(minSize = minSize)
    }

    fun onMaxSizeChange(maxSize: Int?) {
        filterState.value = filterState.value.copy(maxSize = maxSize)
    }

    /**
     * Aplica os filtros definidos pelo utilizador.
     *
     * Fluxo:
     * 1. Valida os filtros via [validateForm]. Se inválido, aborta.
     * 2. Reinicia a paginação via [resetPagination].
     * 3. Se Online: Faz pedido à API via [loadingListPlayer].
     * 4. Se Offline: Filtra a [originalList] (cache) localmente via [filterOffline].
     */
    fun filterApply() {
        if (!validateForm()) {
            return
        }

        if (isNetworkAvailable()) {
            loadingListPlayer()
        } else {
            resetPagination()
            listState.value = filterOffline(originalList = originalList, filter = filterState.value)
        }
    }

    /**
     * Limpa todos os filtros e restaura a lista original.
     *
     * - Restaura o estado visual dos filtros para vazio.
     * - Limpa erros de validação.
     * - Reinicia a paginação.
     * - Se houver cache ([originalList]), restaura-a instantaneamente sem chamar a API.
     * - Se estiver online, pode optar por recarregar dados frescos.
     */
    fun cleanFilters() {
        filterState.value = FilterListPlayer()
        filterErrorState.value = FilterPlayersErrors()
        resetPagination()

        if (networkObserver.isOnlineOneShot()) {
            loadingListPlayer()
        } else {
            listState.value = originalList
        }
    }

    /**
     * Navega para o perfil detalhado do jogador selecionado.
     */
    fun showMore(idPlayer: String, navHostController: NavHostController) {
        navHostController.navigate("${Routes.UserRoutes.PROFILE.route}/${idPlayer}") {
            launchSingleTop = true
        }
    }

    /**
     * Envia um pedido de adesão (convite) para um jogador se juntar à equipa do utilizador.
     * @param idPlayer O ID do jogador a convidar.
     */
    fun sendMembershipRequest(idPlayer: String) {
        val dataUser = sessionManager?.getUserProfile()

        if (dataUser == null) {
            updateToast(message = R.string.toast_autenticate_people)
            return
        }

        if (teamId.value.isEmpty() || roleState.value != UserRole.ADMIN_TEAM) {
            updateToast(message = R.string.toast_admin_only_send_membership_request)
            return
        }

        launchDataLoad {
            playerRepository.sendMemberShipRequestToPlayer(teamId = teamId.value, idPlayer = idPlayer)

            _sentRequestIds.update { currentSet ->
                currentSet + idPlayer
            }

            updateToast(message = R.string.toast_success_send_membershipRequest)
        }
    }

    /**
     * Tenta recarregar a lista de jogadores em caso de falha.
     */
    fun retry() {
        loadingListPlayer()
    }

    /**
     * Carrega a lista de jogadores da API.
     *
     * Utiliza [launchDataLoad] do BaseViewModel para gerir automaticamente:
     * - Verificação de internet.
     * - Estado de Loading (`uiState.isLoading`).
     * - Tratamento de exceções (Try-Catch).
     *
     * Se os filtros estiverem vazios após o carregamento, atualiza a cache [originalList].
     */
    private fun loadingListPlayer() {
        launchDataLoad {
            val players = playerRepository.getListPlayer(teamId = teamId.value, mode = mode, filter = filterState.value)

            listState.value = players
            if (filterState.value == FilterListPlayer()) {
                originalList = players
            }
        }
    }

    /**
     * Filtra a lista de jogadores localmente (Modo Offline).
     * Aplica lógica "AND" para todos os campos (Nome E Cidade E Idade...).
     */
    private fun filterOffline(
        originalList: List<InfoPlayerDto>,
        filter: FilterListPlayer
    ): List<InfoPlayerDto> {
        return originalList.filter { item ->
            val name =
                filter.name.isNullOrBlank() || item.name.contains(filter.name, ignoreCase = true)
            val city =
                filter.city.isNullOrBlank() || item.address.contains(filter.city, ignoreCase = true)
            val minAge = filter.minAge == null || item.age >= filter.minAge
            val maxAge = filter.maxAge == null || item.age <= filter.maxAge
            val minSize = filter.minSize == null || item.heigth >= filter.minSize
            val maxSize = filter.maxSize == null || item.heigth <= filter.maxSize
            val position = filter.position == null || item.position == filter.position

            name && city && minAge && maxAge && minSize && maxSize && position
        }
    }

    /**
     * Valida os dados introduzidos no formulário de filtros.
     *
     * Verifica:
     * - Tamanho máximo de strings (Nome, Cidade).
     * - Intervalos lógicos (Idade Mínima <= Idade Máxima).
     * - Valores dentro dos limites permitidos (ex: Idade entre 16 e 100).
     *
     * @return `true` se todos os dados forem válidos, `false` caso contrário (atualizando [uiErrors]).
     */
    private fun validateForm(): Boolean {
        val name = filterState.value.name
        val city = filterState.value.city
        val minAge = filterState.value.minAge
        val maxAge = filterState.value.maxAge
        val minSize = filterState.value.minSize
        val maxSize = filterState.value.maxSize

        var nameError: ErrorMessage? = null
        var cityError: ErrorMessage? = null
        var minAgeError: ErrorMessage? = null
        var maxAgeError: ErrorMessage? = null
        var minSizeError: ErrorMessage? = null
        var maxSizeError: ErrorMessage? = null

        if (name != null && name.length > UserConst.MAX_NAME_LENGTH) {
            nameError = ErrorMessage(
                messageId = R.string.error_max_name_player,
                args = listOf(UserConst.MAX_NAME_LENGTH)
            )
        }

        if (city != null && city.length > GeneralConst.MAX_CITY_LENGTH) {
            cityError = ErrorMessage(
                messageId = R.string.error_max_city,
                args = listOf(GeneralConst.MAX_CITY_LENGTH)
            )
        }

        var isValidMinAge = true
        if (minAge != null) {
            if (minAge < UserConst.MIN_AGE) {
                minAgeError = ErrorMessage(
                    messageId = R.string.error_min_age,
                    args = listOf(UserConst.MIN_AGE)
                )
                isValidMinAge = false
            } else if (minAge > UserConst.MAX_AGE) {
                minAgeError = ErrorMessage(
                    messageId = R.string.error_max_age,
                    args = listOf(UserConst.MAX_AGE)
                )
                isValidMinAge = false
            }
        }

        var isValidMaxAge = true
        if (maxAge != null) {
            if (maxAge < UserConst.MIN_AGE) {
                maxAgeError = ErrorMessage(
                    messageId = R.string.error_min_age,
                    args = listOf(UserConst.MIN_AGE)
                )
                isValidMaxAge = false
            } else if (maxAge > UserConst.MAX_AGE) {
                maxAgeError = ErrorMessage(
                    messageId = R.string.error_max_age,
                    args = listOf(UserConst.MAX_AGE)
                )
                isValidMaxAge = false

            }
        }

        if (isValidMinAge && isValidMaxAge && minAge != null && maxAge != null && minAge > maxAge) {
            minAgeError = ErrorMessage(
                messageId = R.string.error_min_age_greater_max,
            )

            maxAgeError = ErrorMessage(
                messageId = R.string.error_min_age_greater_max,
            )
        }

        var isValidMinSize = true
        if (minSize != null) {
            if (minSize < PlayerConst.MIN_HEIGHT) {
                minSizeError = ErrorMessage(
                    messageId = R.string.error_min_size,
                    args = listOf(PlayerConst.MIN_HEIGHT)
                )
                isValidMinSize = false
            } else if (minSize > PlayerConst.MAX_HEIGHT) {
                minSizeError = ErrorMessage(
                    messageId = R.string.error_max_size,
                    args = listOf(PlayerConst.MAX_HEIGHT)
                )
                isValidMinSize = false
            }
        }

        var isValidMaxSize = true
        if (maxSize != null) {
            if (maxSize < PlayerConst.MIN_HEIGHT) {
                maxSizeError = ErrorMessage(
                    messageId = R.string.error_min_size,
                    args = listOf(PlayerConst.MIN_HEIGHT)
                )
                isValidMaxSize = false
            } else if (maxSize > PlayerConst.MAX_HEIGHT) {
                maxSizeError = ErrorMessage(
                    messageId = R.string.error_max_size,
                    args = listOf(PlayerConst.MAX_HEIGHT)
                )
                isValidMaxSize = false
            }
        }

        if (isValidMinSize && isValidMaxSize && minSize != null && maxSize != null && minSize > maxSize) {
            minSizeError = ErrorMessage(messageId = R.string.error_min_size_greater_max)
            maxSizeError = ErrorMessage(messageId = R.string.error_max_size_minor_min)
        }

        filterErrorState.value = FilterPlayersErrors(
            nameError = nameError,
            cityError = cityError,
            minAgeError = minAgeError,
            maxAgeError = maxAgeError,
            minSizeError = minSizeError,
            maxSizeError = maxSizeError
        )

        val isValid =
            listOf(nameError, cityError, minAgeError, maxAgeError, minSizeError, maxSizeError).all {
                it == null
            }

        return isValid
    }
}