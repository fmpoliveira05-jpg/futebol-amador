package com.example.amfootball.ui.viewModel.lists

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import androidx.navigation.NavHostController
import com.example.amfootball.data.filters.FilterListPlayer
import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.filtersError.FilterPlayersErrors
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.utils.UserConst
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.repository.PlayerRepository
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.ListsSizesConst
import com.example.amfootball.utils.NetworkUtils
import com.example.amfootball.utils.PlayerConst
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.combine
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

/**
 * ViewModel responsável pela gestão do ecrã de Listagem de Jogadores.
 *
 * Este ViewModel gere:
 * - O estado dos filtros de pesquisa (nome, idade, posição, etc.).
 * - A validação dos dados inseridos nos filtros.
 * - O carregamento da lista de jogadores da API com base nos filtros aplicados.
 * - A lógica de paginação local ("Mostrar Mais").
 * - A navegação para o perfil detalhado de um jogador.
 * - O envio de pedidos de adesão a equipas (para administradores).
 *
 * @property repository O repositório para aceder aos dados dos jogadores.
 */
@HiltViewModel
class ListPlayerViewModel @Inject constructor(
    private val repository: PlayerRepository
): ViewModel() {
    /**
     * Estado atual dos filtros aplicados pelo utilizador.
     * A UI observa este StateFlow para manter os campos de texto sincronizados.
     */
    private val filterState: MutableStateFlow<FilterListPlayer> = MutableStateFlow(FilterListPlayer())
    val uiFilters: StateFlow<FilterListPlayer> = filterState

    /**
     * Estado dos erros de validação dos filtros.
     * Contém mensagens de erro específicas para cada campo (ex: idade inválida).
     */
    private val filterErrorState: MutableStateFlow<FilterPlayersErrors> = MutableStateFlow(FilterPlayersErrors())
    val filterError: StateFlow<FilterPlayersErrors> = filterErrorState

    /**
     * Lista completa de jogadores carregada da API.
     * Mantém todos os dados recuperados, independentemente de quantos estão visíveis.
     */
    private val listState: MutableStateFlow<List<InfoPlayerDto>> = MutableStateFlow(emptyList())

    /**
     * Controlo de quantos itens devem ser exibidos inicialmente ou após clicar em "Mostrar Mais".
     */
    private val inicialSizeList = MutableStateFlow(value = ListsSizesConst.INICIAL_SIZE)

    /**
     * Lista fatiada para exibição na UI.
     *
     * Combina a [listState] (lista total) com o [inicialSizeList] (limite atual)
     * para retornar apenas o número de jogadores permitido pela paginação atual.
     */
    val uiList: StateFlow<List<InfoPlayerDto>> =
        combine(listState, inicialSizeList) { lista, numero ->
            lista.take(numero)
        }.stateIn(
            scope = viewModelScope,
            started = SharingStarted.Lazily,
            initialValue = emptyList()
        )

    /**
     * Controla a visibilidade do botão "Mostrar Mais".
     *
     * Verifica dinamicamente se o número de itens exibidos atualmente ([inicialSizeList])
     * é menor que o total de itens disponíveis na lista ([listState]).
     * Retorna `true` se houver mais itens para mostrar.
     */
    val showMoreButtonVisible: StateFlow<Boolean> =
        combine(listState, inicialSizeList) { listaCompleta, tamanhoAtual ->
            tamanhoAtual < listaCompleta.size
        }.stateIn(
            scope = viewModelScope,
            started = SharingStarted.Lazily,
            initialValue = false
        )

    /**
     * Lista de posições disponíveis para filtrar (incluindo opção 'null' para "Todas").
     * Usado para popular o Dropdown de posições.
     */
    private val listPositions: MutableStateFlow<List<Position?>> = MutableStateFlow(emptyList())
    val uiListPositions: StateFlow<List<Position?>> = listPositions

    /**
     * Estado geral da UI (Loading e Erros de Rede/API).
     */
    private val _uiState: MutableStateFlow<UiState> = MutableStateFlow(UiState(isLoading = true))
    val uiState: StateFlow<UiState> = _uiState

    init {
        loadingListPlayer()

        listPositions.value = listOf(
            null,
            Position.FORWARD,
            Position.MIDFIELDER,
            Position.DEFENDER,
            Position.GOALKEEPER
        )
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
     * Incrementa o limite de jogadores visíveis na lista.
     * Chamado quando o utilizador clica no botão "Mostrar Mais".
     */
    fun loadMorePlayers() {
        inicialSizeList.value = inicialSizeList.value.plus(ListsSizesConst.INCREMENT_SIZE)
    }

    /**
     * Aplica os filtros definidos e recarrega a lista de jogadores.
     *
     * Antes de fazer o pedido à API, executa [validateForm]. Se os dados forem inválidos,
     * o pedido é cancelado e os erros são mostrados na UI.
     */
    fun filterApply() {
        if(!validateForm()) {
            return
        }

        loadingListPlayer()
    }

    /**
     * Limpa todos os filtros e recarrega a lista completa de jogadores.
     */
    fun cleanFilters() {
        filterState.value = FilterListPlayer()
        loadingListPlayer()
    }

    /**
     * Envia um pedido de adesão (convite) para um jogador se juntar à equipa do utilizador autenticado.
     *
     * Nota: Esta funcionalidade destina-se apenas a administradores de equipa.
     *
     * @param idPlayer O ID do jogador a convidar.
     */
    fun sendMembershipRequest(idPlayer: String) {
        //TODO: Fazer pedido há API, para a Team mandar o pedido de adesão
    }

    /**
     * Navega para o perfil detalhado do jogador selecionado.
     */
    fun showMore(
        idPlayer: String,
        navHostController: NavHostController
    ) {
        navHostController.navigate("${Routes.UserRoutes.PROFILE.route}/${idPlayer}") {
            launchSingleTop = true
        }
    }

    /**
     * Tenta recarregar a lista de jogadores em caso de falha.
     */
    fun retry() {
        loadingListPlayer()
    }

    /**
     * Executa a chamada assíncrona à API para obter a lista de jogadores filtrada.
     * Gere os estados de Loading e Erro no [_uiState].
     *
     * Reinicia a paginação ([inicialSizeList]) para o valor padrão a cada nova pesquisa.
     */
    private fun loadingListPlayer() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, errorMessage = null) }
            inicialSizeList.value = ListsSizesConst.INICIAL_SIZE

            try {
                val response = repository.getListPlayer(filterState.value)

                if (response.isSuccessful && response.body() != null) {
                    val players = response.body()!!

                    listState.value = players
                    _uiState.update { it.copy(isLoading = false) }
                } else {
                    val errorRaw = response.errorBody()?.string()
                    val errorMsg = NetworkUtils.parseBackendError(errorRaw)
                        ?: "Erro desconhecido: ${response.code()}"

                    _uiState.update {
                        it.copy(isLoading = false, errorMessage = errorMsg)
                    }
                }
            } catch (e: Exception) {
                e.printStackTrace()
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem conexão: ${e.localizedMessage}")
                }
            }
        }
    }

    /**
     * Valida os dados introduzidos nos filtros.
     * Verifica limites de caracteres, intervalos de idade/altura e lógica (Min < Max).
     * Atualiza o [filterErrorState] com os resultados.
     *
     * @return `true` se todos os filtros forem válidos, `false` caso contrário.
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

        if(name != null && name.length > UserConst.MAX_NAME_LENGTH) {
            nameError = ErrorMessage(
                messageId = R.string.error_max_name_player,
                args = listOf(UserConst.MAX_NAME_LENGTH)
            )
        }

        if(city != null && city.length > GeneralConst.MAX_CITY_LENGTH) {
            cityError = ErrorMessage(
                messageId = R.string.error_max_city,
                args = listOf(GeneralConst.MAX_CITY_LENGTH)
            )
        }

        var isValidMinAge = true
        if(minAge != null) {
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
        if(maxAge != null ) {
            if(maxAge < UserConst.MIN_AGE) {
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

        if(isValidMinAge && isValidMaxAge && minAge != null && maxAge != null && minAge > maxAge) {
            minAgeError = ErrorMessage(
                messageId = R.string.error_min_age_greater_max,
            )

            maxAgeError = ErrorMessage(
                messageId = R.string.error_min_age_greater_max,
            )
        }

        var isValidMinSize = true
        if(minSize != null) {
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

        val isValid = listOf(nameError, cityError, minAgeError, maxAgeError, minSizeError, maxSizeError).all {
            it == null
        }

        return isValid
    }
}