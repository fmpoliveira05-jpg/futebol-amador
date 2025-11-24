package com.example.amfootball.ui.viewModel.match

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.match.ResultMatchDto
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.formErrors.FinishMatchFormErrors
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.utils.FinishMatchConst
import com.example.amfootball.R

class FinishMatchViewModel(): ViewModel() {
    private val resultState: MutableLiveData<ResultMatchDto?> = MutableLiveData<ResultMatchDto?>()
    val resutl: LiveData<ResultMatchDto?> = resultState

    private val resultErrorState: MutableLiveData<FinishMatchFormErrors> = MutableLiveData<FinishMatchFormErrors>()
    val resultError: LiveData<FinishMatchFormErrors> = resultErrorState

    //Setters
    fun onNumGoalsTeamChange(newNumGoalsTeam: Int) {
        resultState.value = resultState.value?.copy(numGoals = NewNumGoals(newNumGoalsTeam))
    }

    fun onNumGoalsOponnetChange(numGoalsOpponent: Int) {
        resultState.value = resultState.value?.copy(numGoalsOpponent = NewNumGoals(numGoalsOpponent))
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

    //Metodos
    //Sacar o id do parametro ou então buscar na url
    fun onSubmitForm(
        navHostController: NavHostController
    ) {
        if(!validateNumGoals()) {
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
    private fun validateNumGoals(): Boolean {
        val numGoalsTeam = resultState.value!!.numGoals
        val numGoalsOpponent = resultState.value!!.numGoalsOpponent

        var numGoalTeamError: ErrorMessage? = null
        var numGoalOpponentError: ErrorMessage? = null

        var validNumGoalsTeam = true
        if(numGoalsTeam < FinishMatchConst.MIN_GOALS) {
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
        if(numGoalsOpponent < FinishMatchConst.MIN_GOALS) {
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
    private fun NewNumGoals(newNumGoals: Int): Int {
        var numGoals = FinishMatchConst.MIN_GOALS

        if (newNumGoals >= FinishMatchConst.MIN_GOALS && newNumGoals <= FinishMatchConst.MAX_GOALS) {
            numGoals = newNumGoals
        }

        return numGoals
    }
}