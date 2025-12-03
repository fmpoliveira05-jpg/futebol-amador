package com.example.amfootball.ui.viewModel.match

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.dtos.match.ResultMatchDto
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.formErrors.FinishMatchFormErrors
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.utils.FinishMatchConst

//TODO: Falta a conexão com o Backend
/**
 * ViewModel responsável pela lógica de negócio e gestão de estado do ecrã de Finalização de Partida (Reportar Resultado).
 *
 * Gere o estado do formulário de golos, aplica validações síncronas e coordena a submissão
 * para o serviço de backend.
 */
class FinishMatchViewModel() : ViewModel() {
    /**
     * Estado interno mutável contendo o DTO do resultado da partida (Golos, IDs).
     */
    private val resultState: MutableLiveData<ResultMatchDto?> = MutableLiveData<ResultMatchDto?>()

    /**
     * Estado público de leitura dos dados do resultado.
     * Observado pela UI para preencher os campos de input.
     */
    val resutl: LiveData<ResultMatchDto?> = resultState

    /**
     * Estado interno mutável contendo os erros de validação do formulário.
     */
    private val resultErrorState: MutableLiveData<FinishMatchFormErrors> = MutableLiveData<FinishMatchFormErrors>()

    /**
     * Estado público de leitura dos erros.
     * Observado pela UI para exibir mensagens de erro nos inputs.
     */
    val resultError: LiveData<FinishMatchFormErrors> = resultErrorState

    //Setters
    /**
     * Atualiza o número de golos marcados pela equipa do utilizador.
     *
     * Utiliza [NewNumGoals] para garantir que o valor se mantém dentro dos limites definidos
     * ([FinishMatchConst.MIN_GOALS] e [FinishMatchConst.MAX_GOALS]) antes de atualizar o estado.
     *
     * @param newNumGoalsTeam O novo número de golos (Int).
     */
    fun onNumGoalsTeamChange(newNumGoalsTeam: Int) {
        resultState.value = resultState.value?.copy(numGoals = NewNumGoals(newNumGoalsTeam))
    }

    /**
     * Atualiza o número de golos marcados pela equipa adversária.
     *
     * @param numGoalsOpponent O novo número de golos do adversário (Int).
     */
    fun onNumGoalsOponnetChange(numGoalsOpponent: Int) {
        resultState.value =
            resultState.value?.copy(numGoalsOpponent = NewNumGoals(numGoalsOpponent))
    }

    //Initializar
    init {
        //TODO: Ver como vou sacar o que falta e claro carregar o opponente
        resultState.value = ResultMatchDto(
            numGoals = 0,
            numGoalsOpponent = 0,
            idMatch = "ada",
            idTeam = "asd",
            idOpponent = "as",
        )
    }

    /**
     * Submete o resultado da partida para o backend.
     *
     * O fluxo de submissão planeado é:
     * 1. Executa a validação síncrona [validateNumGoals].
     * 2. Se for válido, constrói o DTO final.
     * 3. **TODO:** Envia o resultado para o endpoint da API.
     * 4. **TODO:** Após sucesso, verifica se o Hub em tempo real indicou que o resultado do adversário também foi submetido.
     * 5. Navega para o calendário.
     *
     * @param navHostController Controlador de navegação para redirecionamento após submissão.
     */
    fun onSubmitForm(navHostController: NavHostController) {
        if (!validateNumGoals()) {
            return
        }

        /*
        resultState.value = ResultMatchDto(
            idMatch = resultState.value!!.idMatch,
            idTeam = idTeam.value,
            numGoals = numGoalsTeam.value,
            idOpponent = idOpponent.value,
            numGoalsOpponent = numGoalsOpponent.value
        )
        * */
        //TODO: Enviar para o endPoint da API
        //TODO: Meter um if que caso o hub diga que a partida foi finalizada então volta para o calendario,
        // se não é ativado um loading

        //TODO: Meter para ir para esta rota apenas depois depois de ambos os admins submeterem o formulario
        navHostController.navigate(Routes.TeamRoutes.CALENDAR.route) {
            popUpTo(Routes.TeamRoutes.CALENDAR.route) {
                inclusive = true
            }
        }
    }

    //Metodos privados
    /**
     * Validação síncrona dos campos de golos.
     *
     * Verifica se os golos da equipa e do adversário estão dentro dos limites ([MIN_GOALS], [MAX_GOALS]).
     *
     * @return `true` se ambos os campos forem válidos, `false` caso contrário.
     */
    private fun validateNumGoals(): Boolean {
        val numGoalsTeam = resultState.value!!.numGoals
        val numGoalsOpponent = resultState.value!!.numGoalsOpponent

        var numGoalTeamError: ErrorMessage? = null
        var numGoalOpponentError: ErrorMessage? = null

        var validNumGoalsTeam = true
        if (numGoalsTeam < FinishMatchConst.MIN_GOALS) {
            numGoalTeamError = ErrorMessage(
                messageId = R.string.error_min_goals_team,
                args = listOf(FinishMatchConst.MIN_GOALS)
            )
            validNumGoalsTeam = false
        } else if (numGoalsTeam > FinishMatchConst.MAX_GOALS) {
            numGoalTeamError = ErrorMessage(
                messageId = R.string.error_max_goals_team,
                args = listOf(FinishMatchConst.MAX_GOALS)
            )
            validNumGoalsTeam = false
        }

        var validNumGoalsOpponent = true
        if (numGoalsOpponent < FinishMatchConst.MIN_GOALS) {
            numGoalTeamError = ErrorMessage(
                messageId = R.string.error_min_goals_team,
                args = listOf(FinishMatchConst.MIN_GOALS)
            )
            validNumGoalsOpponent = false
        } else if (numGoalsOpponent > FinishMatchConst.MAX_GOALS) {
            numGoalOpponentError = ErrorMessage(
                messageId = R.string.error_max_goals_team,
                args = listOf(FinishMatchConst.MAX_GOALS)
            )
            validNumGoalsOpponent = false
        }

        resultErrorState.value = FinishMatchFormErrors(
            numGoalTeamError = numGoalTeamError,
            numGoalOpponentError = numGoalOpponentError
        )

        val isValid = listOf(numGoalTeamError, numGoalOpponentError).all {
            it == null
        }

        return isValid
    }

    /**
     * Função auxiliar que aplica limites mínimos e máximos ao número de golos.
     * Garante que o número de golos nunca é negativo nem excede o máximo permitido.
     *
     * @param newNumGoals O valor de golos proposto.
     * @return O valor de golos dentro dos limites definidos.
     */
    private fun NewNumGoals(newNumGoals: Int): Int {
        var numGoals = FinishMatchConst.MIN_GOALS

        if (newNumGoals >= FinishMatchConst.MIN_GOALS && newNumGoals <= FinishMatchConst.MAX_GOALS) {
            numGoals = newNumGoals
        }

        return numGoals
    }
}