package com.example.amfootball.data.actions.filters

import com.example.amfootball.data.enums.match.MatchStatus
import com.example.amfootball.data.enums.match.TypeMatch

/**
 * Data class que agrupa todas as funções de callback (ações) necessárias para manipular
 * o estado de filtragem num ecrã de calendário de partidas.
 *
 * É o objeto principal de dependência passado da ViewModel para o ecrã de UI do filtro.
 *
 * @property onNameChange Callback para atualizar o filtro por nome (ex: nome da equipa adversária).
 * @property onMinDateGameChange Callback para atualizar a data mínima de jogo no filtro (em milissegundos).
 * @property onMaxDateGameChange Callback para atualizar a data máxima de jogo no filtro (em milissegundos).
 * @property onGameLocalChange Callback para atualizar o filtro de localização: true (Casa), false (Fora), ou null (Ambos).
 * @property onTypeMatchChange Callback para atualizar o filtro por tipo de partida: [TypeMatch] (Competitivo/Casual) ou null (Ambos).
 * @property onFinishMatch Callback para atualizar o filtro de estado: [MatchStatus] (ex: Agendado, Finalizado) ou null (Todos).
 * @property onButtonFilterActions Objeto que contém os callbacks para os botões de controlo (Aplicar e Limpar).
 */
data class FilterCalendarActions(
    val onNameChange: (String) -> Unit,
    val onMinDateGameChange: (Long) -> Unit,
    val onMaxDateGameChange: (Long) -> Unit,
    val onGameLocalChange: (Boolean?) -> Unit,
    val onTypeMatchChange: (TypeMatch?) -> Unit,
    val onFinishMatch: (Boolean?) -> Unit,
    val onButtonFilterActions: ButtonFilterActions
)