package com.example.amfootball.ui.viewModel

import androidx.lifecycle.viewModelScope
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.utils.ListsSizesConst
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.combine
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.flow.update

/**
 * ViewModel Abstrato e Genérico para gestão de listas paginadas.
 *
 * Esta classe encapsula toda a lógica repetitiva de gestão de listas, incluindo:
 * - Estado da lista de dados ([listState]).
 * - Cache para limpeza de filtros ([originalList]).
 * - Lógica de paginação automática ([uiList], [showMoreButtonVisible]).
 * - Incremento de páginas ([loadMoreItems]).
 *
 * Deve ser herdada por qualquer ViewModel que exiba uma lista infinita (ex: Jogadores, Equipas, Jogos).
 *
 * @param T O tipo de objeto contido na lista (ex: InfoPlayerDto).
 * @param networkObserver Observador de conectividade (passado ao [BaseViewModel]).
 */
abstract class ListsViewModels<T>(
    private val networkObserver: NetworkConnectivityObserver,
): BaseViewModel(networkObserver = networkObserver) {

    /**
     * Lista completa de dados carregados (da API ou filtrados).
     * É [protected] para permitir que as subclasses atualizem os dados após um pedido à API.
     */
    protected val listState: MutableStateFlow<List<T>> = MutableStateFlow(emptyList())

    /**
     * Cache da lista original carregada inicialmente (sem filtros).
     * Usada para restaurar a lista completa instantaneamente ao limpar filtros ou filtrar offline.
     * É [protected] e [var] para ser inicializada pela subclasse.
     */
    protected var originalList: List<T> = emptyList()

    /**
     * Contador do número máximo de itens a exibir na UI atualmente.
     * Inicia com [ListsSizesConst.INICIAL_SIZE] e incrementa conforme o utilizador pede "Mais".
     */
    protected val inicialSizeList = MutableStateFlow(value = ListsSizesConst.INICIAL_SIZE)

    /**
     * Lista final pronta para a UI.
     *
     * Combina automaticamente o [listState] (dados totais) com o [inicialSizeList] (limite atual)
     * para retornar apenas a "fatia" visível dos dados.
     * Reage a mudanças em qualquer um dos estados.
     */
    val uiList: StateFlow<List<T>> =
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
     * Retorna `true` se houver mais itens na lista total ([listState]) do que os que estão
     * a ser mostrados atualmente ([inicialSizeList]).
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
     * Ação da UI: Carregar mais itens.
     * Incrementa o limite de paginação em [ListsSizesConst.INCREMENT_SIZE],
     * fazendo com que [uiList] emita uma nova lista maior.
     */
    protected fun loadMoreItems() {
        inicialSizeList.update { it + ListsSizesConst.INCREMENT_SIZE }
    }

    /**
     * Helper Protegido: Reiniciar Paginação.
     *
     * Deve ser chamado pelas subclasses sempre que:
     * 1. Um novo filtro é aplicado.
     * 2. A lista é recarregada da API.
     * 3. Os filtros são limpos.
     *
     * Garante que a visualização volta ao topo/início.
     */
    protected fun resetPagination() {
        inicialSizeList.value = ListsSizesConst.INICIAL_SIZE
    }
}