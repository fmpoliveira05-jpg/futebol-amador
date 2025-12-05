package com.example.amfootball.data.actions.itemsList

import androidx.navigation.NavHostController

/**
 * Data class que agrupa as funções de callback (ações) que podem ser executadas
 * em um item de partida listado num calendário ou lista.
 *
 * Estas ações gerenciam o ciclo de vida e o estado do jogo (iniciar, finalizar, adiar, cancelar).
 *
 * @property onCancelMatch Callback para cancelar uma partida. Recebe o ID da partida (String) e o [NavHostController] para navegação.
 * @property onPostPoneMatch Callback para adiar uma partida. Recebe o ID da partida (String) e o [NavHostController] para navegação (ex: abrir formulário de adiamento).
 * @property onStartMatch Callback para iniciar uma partida (mudar o estado para "Em Andamento"). Recebe o ID da partida (String).
 * @property onFinishMatch Callback para finalizar uma partida (registar o resultado). Recebe o ID da partida (String) e o [NavHostController] para navegação (ex: abrir formulário de resultado).
 */
data class ItemsCalendarActions(
    val onCancelMatch: (String, NavHostController) -> Unit,
    val onPostPoneMatch: (String, NavHostController) -> Unit,
    val onStartMatch: (String) -> Unit,
    val onFinishMatch: (String, NavHostController) -> Unit,
)
