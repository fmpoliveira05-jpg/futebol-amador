package com.example.amfootball.ui.viewModel.matchInvite

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
import com.example.amfootball.data.enums.Forms.MatchFormMode
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.repository.CalendarRepository
import com.example.amfootball.data.repository.TeamRepository
import com.example.amfootball.navigation.objects.Arguments
import com.example.amfootball.navigation.objects.Routes
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow

//TODO: Adaptar isto para depois a pagina também servir para PostPoneMatch
@HiltViewModel
class FormMatchInviteViewModel @Inject constructor(
    private val savedStateHandle: SavedStateHandle,
    private val calendarRepository: CalendarRepository,
    private val teamRepository: TeamRepository
) : ViewModel() {

    private val matchId: String? = savedStateHandle.get<String>(Arguments.MATCH_ID)
    private val matchInviteId: String? = savedStateHandle.get<String>(Arguments.MATCH_INVITE_ID)
    private val modeStr = savedStateHandle.get<String>(Arguments.FORM_MODE)

    val mode: MatchFormMode = try {
        if (modeStr != null) {
            MatchFormMode.valueOf(modeStr)
        }
        else {
            MatchFormMode.SEND
        }
    } catch (e: Exception) {
        MatchFormMode.SEND
    }

    private val formState: MutableStateFlow<MatchInviteDto> = MutableStateFlow(MatchInviteDto())
    val uiFormState: StateFlow<MatchInviteDto> = formState

    private val erros: MutableStateFlow<MatchInviteFormErros> = MutableStateFlow(MatchInviteFormErros())
    val uiErrorsForm: StateFlow<MatchInviteFormErros> = erros

    init {
        loadData()
    }

    //Metodos
    fun onGameDateChange(millis: Long) {
        val newDate = convertMillisToDate(millis)
        val current = formState.value

        formState.value = current.copy(
            gameDate = newDate
        )
    }

    fun onTimeGameChange(newTime: String) {
        val current = formState.value

        formState.value = current.copy(
            gameTime = newTime
        )
    }

    fun onLocalGameChange(isHome: Boolean) {
        formState.value = formState.value.copy(isHomeGame = isHome)
    }

    fun onSubmitForm(navHostController: NavHostController) {
        if(!isFormValid()) {
            return
        }

        val combinedDateTime = MatchInviteDto.combineToDateTime(
            formState.value.gameDate,
            formState.value.gameTime
        )

        when(modeStr) {
            MatchFormMode.NEGOCIATE.name -> {
                //TODO: Fazer o pedido ao endPoint de negociate
            }
            MatchFormMode.SEND.name -> {
                //TODO: Mandar pedido há API para ir buscar apenas o nome do Opponente
            }
            MatchFormMode.POSTPONE.name -> {
                //TODO: Fazer o pedido ao endPoint de postponeMatch
            }
            else -> {
                //Lançar exceção
            }
        }

        navHostController.navigate(Routes.TeamRoutes.CALENDAR.route) {
            popUpTo(0)
        }
    }

    //TODO: Implementar
    fun onCancelForm(navHostController: NavHostController, cancelReason: String) {
    }

    //Metodo Privados
    private fun loadData() {
        when(modeStr) {
            MatchFormMode.NEGOCIATE.name -> {
                //TODO: Fazer o pedido ao endPoint de negociate
            }
            MatchFormMode.SEND.name -> {
                //TODO: Mandar pedido há API para ir buscar apenas o nome do Opponente
            }
            MatchFormMode.CANCEL.name -> {
                //TODO: Fazer o pedido ao endPoint de cancelMatch
            }
            MatchFormMode.POSTPONE.name -> {
                //TODO: Fazer o pedido ao endPoint de postponeMatch
            }
            else -> {
                //Lançar exceção
            }
        }
    }

    //Trocar para um validateForm com mais verificações
    private fun isFormValid(): Boolean {
        val dateGame = formState.value.gameDate
        val timeGame = formState.value.gameTime

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