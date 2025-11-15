package com.example.amfootball.ui.viewModel.match

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.example.amfootball.data.dtos.match.ResultMatchDto
import com.example.amfootball.utils.FinishMatchConst

class FinishMatchViewModel(): ViewModel() {
    private val resultState: MutableLiveData<ResultMatchDto?> = MutableLiveData<ResultMatchDto?>()
    val resutl: LiveData<ResultMatchDto?> = resultState

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
    fun onSubmitForm() {
        //TODO: Meter verificações

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
    }

    //Metodos privados
    private fun NewNumGoals(newNumGoals: Int): Int {
        var numGoals = FinishMatchConst.MIN_GOALS

        if (newNumGoals >= FinishMatchConst.MIN_GOALS && newNumGoals <= FinishMatchConst.MAX_GOALS) {
            numGoals = newNumGoals
        }

        return numGoals
    }
}