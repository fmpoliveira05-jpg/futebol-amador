package com.example.amfootball.ui.viewModel.matchInvite

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.errors.formErrors.MatchInviteFormErros
import java.time.Instant
import java.time.ZoneId
import java.time.format.DateTimeFormatter
import javax.inject.Inject
import com.example.amfootball.R
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.navigation.Objects.Routes

//TODO: Adaptar isto para depois a pagina também servir para PostPoneMatch
class FormMatchInviteViewModel @Inject constructor(
    private val savedStateHandle: SavedStateHandle
) : ViewModel() {

    private val idMatchInvite: String? = savedStateHandle.get("idMatchInvite")
    val isNegociate = idMatchInvite != null

    private val formState: MutableLiveData<MatchInviteDto> = MutableLiveData(MatchInviteDto())
    val uiFormState: LiveData<MatchInviteDto> = formState

    private val erros: MutableLiveData<MatchInviteFormErros> = MutableLiveData(MatchInviteFormErros())
    val uiErrorsForm: LiveData<MatchInviteFormErros> = erros

    init {
        if (isNegociate) {
            //TODO: Carregar os dados do MatchInvite do Backend
            formState.value = MatchInviteDto.genericForNegotiate()
        } else {
            //TODO: Mandar pedido há API para ir buscar apenas o nome do Opponente
            formState.value = MatchInviteDto.genericForCreate(nameOpponent = "Fenixs")
        }
    }

    //Metodos
    fun onGameDateChange(millis: Long) {
        val newDate = convertMillisToDate(millis)
        val current = formState.value!!

        formState.value = current.copy(
            gameDate = newDate
        )
    }

    fun onTimeGameChange(newTime: String) {
        val current = formState.value!!

        formState.value = current.copy(
            gameTime = newTime
        )
    }

    fun onLocalGameChange(isHome: Boolean) {
        formState.value = formState.value!!.copy(isHomeGame = isHome)
    }

    fun onSubmitForm(navHostController: NavHostController) {
        if(!IsFormValid()) {
            return
        }

        val combinedDateTime = MatchInviteDto.combineToDateTime(
            formState.value!!.gameDate,
            formState.value!!.gameTime
        )

        if (isNegociate) {
            //TODO: Fazer o pedido ao endPoint de negociate
        } else {
            //TODO: Fazer o pedido ao endPoint de sendMatchInvite
        }

        navHostController.navigate(Routes.TeamRoutes.CALENDAR.route) {
            popUpTo(0)
        }
    }

    //Trocar para um validateForm com mais verificações
    private fun IsFormValid(): Boolean {
        val dateGame = formState.value!!.gameDate
        val timeGame = formState.value!!.gameTime

        var errorDateGame: ErrorMessage? = null
        var errorTime: ErrorMessage? = null

        if (dateGame.isNullOrBlank()) {
            errorDateGame = ErrorMessage(
                messageId = R.string.mandatory_field
            )
        }

        if (timeGame.isNullOrBlank()) {
            errorTime = ErrorMessage(
                messageId = R.string.mandatory_field
            )
        }



        erros.value = MatchInviteFormErros(
            dateError = errorDateGame,
            timeError = errorTime
        )

        val isValid = listOf(errorDateGame, errorTime).all {
            it == null
        }

        return isValid
    }

    private fun convertMillisToDate(millis: Long): String {
        val formatter = DateTimeFormatter.ofPattern("dd/MM/yyyy")
        return Instant.ofEpochMilli(millis)
            .atZone(ZoneId.systemDefault()) // Usa o fuso horário do sistema.
            .toLocalDate()
            .format(formatter)
    }
}