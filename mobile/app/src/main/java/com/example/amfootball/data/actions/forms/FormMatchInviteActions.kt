package com.example.amfootball.data.actions.forms

import androidx.navigation.NavHostController

/**
 * Data class que agrupa as ações lambda necessárias para o formulário de envio de convite de partida.
 *
 * É tipicamente usado para passar as funções de modificação de estado de data, hora e localização,
 * juntamente com a ação de submissão, da ViewModel para o componente Composable.
 *
 * @property onGameDateChange Callback para atualizar a data do jogo selecionada (em milissegundos).
 * @property onTimeGameChange Callback para atualizar a hora do jogo selecionada (formato "HH:mm").
 * @property onLocalGameChange Callback para definir se a partida será jogada em casa (true) ou fora (false).
 * @property onSubmitForm Callback executado ao submeter o formulário. Recebe o [NavHostController] para permitir a navegação após o sucesso.
 */
data class FormMatchInviteActions(
    val onGameDateChange: (newDate: Long) -> Unit,
    val onTimeGameChange: (newTime: String) -> Unit,
    val onLocalGameChange: (isHome: Boolean) -> Unit,
    val onSubmitForm: (navHostController: NavHostController) -> Unit
)