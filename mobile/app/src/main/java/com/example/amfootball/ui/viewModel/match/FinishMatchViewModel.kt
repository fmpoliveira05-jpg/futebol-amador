package com.example.amfootball.ui.viewModel.match

import androidx.compose.runtime.State
import androidx.compose.runtime.derivedStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import com.example.amfootball.data.dtos.match.ResultMatchDto
import com.example.amfootball.utils.FinishMatchConst

class FinishMatchViewModel: ViewModel() {
    private val result = mutableStateOf<ResultMatchDto?>(null)

    //Getters
    val idMatch: State<String> = derivedStateOf {
        result.value?.idMatch.toString()
    }

    val idTeam: State<String> = derivedStateOf {
        result.value?.idTeam.toString()
    }

    val numGoalsTeam: State<Int> = derivedStateOf {
        result.value?.numGoals ?: 0
    }

    val idOpponent: State<String> = derivedStateOf {
        result.value?.idOpponent.toString()
    }

    val numGoalsOpponent: State<Int> = derivedStateOf {
        result.value?.numGoalsOpponent ?: 0
    }

    //Setters
    fun onNumGoalsTeamChange(newNumGoalsTeam: Int) {


        result.value = result.value?.copy(numGoals = NewNumGoals(newNumGoalsTeam))
    }

    fun onNumGoalsOponnetChange(numGoalsOpponent: Int) {
        result.value = result.value?.copy(numGoalsOpponent = NewNumGoals(numGoalsOpponent))
    }

    //Initializar
    init {
        //TODO: Ver o que fazer aqui
    }

    //Metodos
    fun onSubmitForm() {
        //TODO: Meter verificações

        result.value = ResultMatchDto(
            idMatch = idMatch.value,
            idTeam = idTeam.value,
            numGoals = numGoalsTeam.value,
            idOpponent = idOpponent.value,
            numGoalsOpponent = numGoalsOpponent.value
        )

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