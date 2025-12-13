package com.example.amfootball.ui.viewModel.match

import com.example.amfootball.R
import com.example.amfootball.core.utils.FinishMatchConst
import com.example.amfootball.data.NetworkConnectivityObserver
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.remote.dtos.match.ResultMatchDto
import com.example.amfootball.domains.errors.ErrorMessage
import com.example.amfootball.domains.errors.formErrors.FinishMatchFormErrors
import com.example.amfootball.ui.viewModel.abstracts.FormsViewModel
import dagger.hilt.android.lifecycle.HiltViewModel
import javax.inject.Inject

//TODO: Falta apenas carregar o opponente
/**
 * ViewModel responsável pela lógica de negócio e gestão de estado do ecrã de Finalização de Partida (Reportar Resultado).
 *
 * Gere o estado do formulário de golos ([ResultMatchDto]), aplica validações síncronas de limites
 * e coordena a submissão para o serviço de backend.
 *
 * Herda de [FormsViewModel] para obter funcionalidades base de gestão de estado de formulário e erros.
 *
 * @property networkObserver Observador de conectividade para garantir operações online.
 * @property sessionManager Gestor de sessão para obter dados do utilizador.
 */
@HiltViewModel
class FinishMatchViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver,
    private val sessionManager: SessionManager
) : FormsViewModel<ResultMatchDto, FinishMatchFormErrors>(
    networkObserver = networkObserver,
    initialData = ResultMatchDto(),
    initialError = FinishMatchFormErrors()
) {

    //Initializar
    init {
        //TODO: Ver como vou sacar o que falta e claro carregar o opponente
        formState.value = ResultMatchDto(
            numGoalsTeam = 0,
            numGoalsOpponent = 0,
            idMatch = "ada",
            idTeam = "asd",
            idOpponent = "as",
        )
    }

    /**
     * Atualiza o número de golos marcados pela equipa do utilizador.
     *
     * Utiliza a função auxiliar [newNumGoals] para garantir que o valor se mantém dentro dos limites definidos
     * ([FinishMatchConst.MIN_GOALS] e [FinishMatchConst.MAX_GOALS]) antes de atualizar o estado.
     *
     * @param newNumGoalsTeam O novo número de golos (Int).
     */
    fun onNumGoalsTeamChange(newNumGoalsTeam: Int) {
        formState.value = formState.value.copy(numGoalsTeam = newNumGoals(newNumGoalsTeam))
    }

    /**
     * Atualiza o número de golos marcados pela equipa adversária.
     *
     * @param numGoalsOpponent O novo número de golos do adversário (Int).
     */
    fun onNumGoalsOponnetChange(numGoalsOpponent: Int) {
        formState.value = formState.value.copy(numGoalsOpponent = newNumGoals(numGoalsOpponent))
    }

    /**
     * Submete o resultado da partida para o backend.
     *
     * O fluxo de execução é:
     * 1. Executa a validação síncrona dos campos via [validateForm].
     * 2. Se a validação passar, executa o callback de sucesso [onSucess] (que tipicamente realiza a navegação).
     *
     * Nota: A lógica de chamada à API deve ser inserida aqui antes de chamar [onSucess].
     *
     * @param onSucess Callback (função lambda) a ser executado após uma submissão bem-sucedida.
     */
    fun onSubmitForm(onSucess: () -> Unit) {
        if (!validateForm()) {
            return
        }

        onSucess()
    }

    //Metodos privados
    /**
     * Validação global do formulário.
     *
     * Chama as funções de validação específicas (ex: [validateNumGoals]) e retorna o resultado agregado.
     *
     * @return `true` se todos os campos do formulário forem válidos.
     */
    override fun validateForm(): Boolean {
        var isValid = true

        if (!validateNumGoals()) {
            isValid = false
        }

        return isValid

    }

    /**
     * Validação síncrona dos campos de golos.
     *
     * Verifica se os golos da equipa e do adversário estão dentro dos limites definidos
     * ([FinishMatchConst.MIN_GOALS], [FinishMatchConst.MAX_GOALS]).
     *
     * Atualiza o estado [formErrors] com as mensagens de erro apropriadas se a validação falhar.
     *
     * @return `true` se ambos os campos de golos forem válidos, `false` caso contrário.
     */
    private fun validateNumGoals(): Boolean {
        val numGoalsTeam = formState.value.numGoalsTeam
        val numGoalsOpponent = formState.value.numGoalsOpponent

        var numGoalTeamError: ErrorMessage? = null
        var numGoalOpponentError: ErrorMessage? = null

        if (numGoalsTeam < FinishMatchConst.MIN_GOALS) {
            numGoalTeamError = ErrorMessage(
                messageId = R.string.error_min_goals_team,
                args = listOf(FinishMatchConst.MIN_GOALS)
            )
        } else if (numGoalsTeam > FinishMatchConst.MAX_GOALS) {
            numGoalTeamError = ErrorMessage(
                messageId = R.string.error_max_goals_team,
                args = listOf(FinishMatchConst.MAX_GOALS)
            )
        }

        if (numGoalsOpponent < FinishMatchConst.MIN_GOALS) {
            numGoalTeamError = ErrorMessage(
                messageId = R.string.error_min_goals_team,
                args = listOf(FinishMatchConst.MIN_GOALS)
            )
        } else if (numGoalsOpponent > FinishMatchConst.MAX_GOALS) {
            numGoalOpponentError = ErrorMessage(
                messageId = R.string.error_max_goals_team,
                args = listOf(FinishMatchConst.MAX_GOALS)
            )
        }

        formErrors.value = FinishMatchFormErrors(
            numGoalTeamError = numGoalTeamError,
            numGoalOpponentError = numGoalOpponentError
        )

        val isValid = listOf(numGoalTeamError, numGoalOpponentError).all {
            it == null
        }

        return isValid
    }

    /**
     * Função auxiliar que aplica limites mínimos e máximos ao número de golos (Clamping).
     * Garante que o número de golos nunca é negativo nem excede o máximo permitido ao atualizar o estado.
     *
     * @param newNumGoals O valor de golos proposto.
     * @return O valor de golos filtrado dentro dos limites definidos.
     */
    private fun newNumGoals(newNumGoals: Int): Int {
        var numGoals = FinishMatchConst.MIN_GOALS

        if (newNumGoals >= FinishMatchConst.MIN_GOALS && newNumGoals <= FinishMatchConst.MAX_GOALS) {
            numGoals = newNumGoals
        }

        return numGoals
    }
}