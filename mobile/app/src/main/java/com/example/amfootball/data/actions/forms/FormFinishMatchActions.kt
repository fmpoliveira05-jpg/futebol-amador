package com.example.amfootball.data.actions.forms

import androidx.navigation.NavHostController

/**
 * Data class que agrupa as ações lambda necessárias para o formulário de registo
 * do resultado final de uma partida.
 *
 * É tipicamente usado para passar as funções de modificação de estado e a ação de submissão
 * da ViewModel para o componente Composable.
 *
 * @property onNumGoalsTeamChange Callback para atualizar o número de golos marcados pela equipa local (própria).
 * @property onNumGoalsOpponentChange Callback para atualizar o número de golos marcados pela equipa adversária.
 * @property onSubmitForm Callback executado ao submeter o formulário. Recebe o [NavHostController] para permitir a navegação após o sucesso.
 */
data class FormFinishMatchActions(
    val onNumGoalsTeamChange: (newNumGoalsTeam: Int) -> Unit,
    val onNumGoalsOpponentChange: (numGoalsOpponent: Int) -> Unit,
    val onSubmitForm: (NavHostController) -> Unit,
)
