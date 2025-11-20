package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.ViewModel
import androidx.lifecycle.SavedStateHandle
import com.example.amfootball.data.dtos.team.FormTeamDto
import javax.inject.Inject
import android.net.Uri
import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.navigation.NavHostController
import com.example.amfootball.data.errors.formErrors.TeamFormErros
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.PitchConst
import com.example.amfootball.utils.TeamConst
import com.example.amfootball.R
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.navigation.Objects.Routes
import dagger.hilt.android.lifecycle.HiltViewModel

/**
 * Este viewModel é utilizado pelo create e update Team
 * */
@HiltViewModel
class TeamFormViewModel @Inject constructor(
    private val savedStateHandle: SavedStateHandle
) : ViewModel() {

    //Vai Busvar o parametro idTeam da rota, se tiver é update se não é create
    private val idTeam: String? = savedStateHandle.get("idTeam")
    val isEditMode = idTeam != null

    private val formState: MutableLiveData<FormTeamDto> = MutableLiveData(FormTeamDto())
    val uiFormState: LiveData<FormTeamDto> = formState

    //Para mandar erros
    private val errors: MutableLiveData<TeamFormErros> = MutableLiveData(TeamFormErros())
    val uiErrors: LiveData<TeamFormErros> = errors

    //Setters
    fun onNameChange(name: String) {
        formState.value = formState.value!!.copy(name = name)
    }

    fun onDescriptionChange(description: String?) {
        formState.value = formState.value!!.copy(description = description)
    }

    fun onImageChange(image: Uri?) {
        formState.value = formState.value!!.copy(image = image)
    }

    fun onNamePitchChange(name: String) {
        formState.value = formState.value!!.copy(
            pitch = formState.value!!.pitch.copy(name = name)
        )
    }

    fun onAddressPitchChange(address: String) {
        formState.value = formState.value!!.copy(
            pitch = formState.value!!.pitch.copy(address = address)
        )
    }

    //Initializar
    init {
        if (isEditMode) {
            //TODO: Carregar os dados da team
            formState.value = FormTeamDto.generateEditExampleTeam()
        }
    }

    //Metodos
    fun onSubmit(
        navHostController: NavHostController
    ) {
        if(!ValidateFormValid()) {
            return
        }

        if (isEditMode) {
            //TODO: Chamar metodo para editar a team
        } else {
            //TODO: Chamar metodo para criar a team
        }

        //Se tudo der bem
        navHostController.navigate(route = Routes.TeamRoutes.HOMEPAGE.route)
    }

    //Metodos privados
    private fun ValidateFormValid(): Boolean {
        val name = formState.value!!.name
        val nameLength = name.length

        val description = formState.value!!.description ?: ""
        val descriptionLength = description.length

        val pitchName = formState.value!!.pitch.name
        val pitchNameLength = pitchName.length

        val pitchAddress = formState.value!!.pitch.address
        val pitchAddressLength = pitchAddress.length

        var nameErr: ErrorMessage? = null
        var descErr: ErrorMessage? = null
        var pitchNameErr: ErrorMessage? = null
        var pitchAddrErr: ErrorMessage? = null

        if (name.isBlank()) {
            nameErr = ErrorMessage(
                messageId = R.string.mandatory_field
            )
        } else if (nameLength < TeamConst.MIN_NAME_LENGTH) {
            nameErr = ErrorMessage(
                messageId = R.string.error_min_name_team,
                args = listOf(TeamConst.MIN_NAME_LENGTH))
        } else if (nameLength > TeamConst.MAX_NAME_LENGTH) {
            nameErr = ErrorMessage(
                messageId = R.string.error_max_name_team,
                args = listOf(TeamConst.MAX_NAME_LENGTH)
            )
        }

        if (description.isNotBlank() && descriptionLength > TeamConst.MAX_DESCRIPTION_LENGTH) {
            descErr = ErrorMessage(
                messageId = R.string.error_max_description,
                args = listOf(TeamConst.MAX_DESCRIPTION_LENGTH)
            )
        }

        if (pitchName.isBlank()) {
            pitchNameErr = ErrorMessage(
                messageId = R.string.mandatory_field
            )
        } else if (pitchNameLength < PitchConst.MIN_NAME_LENGTH) {
            pitchNameErr = ErrorMessage(
                messageId = R.string.error_min_pitch_name,
                args = listOf(PitchConst.MIN_NAME_LENGTH)
            )
        } else if (pitchNameLength > PitchConst.MAX_NAME_LENGTH) {
            pitchNameErr = ErrorMessage(
                messageId = R.string.error_max_pitch_name,
                args = listOf(PitchConst.MAX_NAME_LENGTH)
            )
        }

        if (pitchAddress.isBlank()) {
            pitchAddrErr = ErrorMessage(
                messageId = R.string.mandatory_field
            )
        } else if (pitchAddressLength < GeneralConst.MIN_ADDRESS_LENGTH) {
            pitchAddrErr = ErrorMessage(
                messageId = R.string.error_min_address,
                args = listOf(GeneralConst.MIN_ADDRESS_LENGTH)
            )
        } else if (pitchAddressLength > GeneralConst.MAX_ADDRESS_LENGTH) {
            pitchAddrErr = ErrorMessage(
                messageId = R.string.error_max_address,
                args = listOf(GeneralConst.MAX_ADDRESS_LENGTH)
            )
        }

        errors.value = TeamFormErros(
            nameError = nameErr,
            descriptionError = descErr,
            pitchNameError = pitchNameErr,
            pitchAddressError = pitchAddrErr
        )

        val isValid = listOf(nameErr, descErr, pitchNameErr, pitchAddrErr).all {
            it == null
        }

        return isValid
    }

}