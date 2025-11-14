package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.ViewModel
import androidx.lifecycle.SavedStateHandle
import com.example.amfootball.data.dtos.team.FormTeamDto
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject
import android.net.Uri
import androidx.navigation.NavHostController
import com.example.amfootball.data.errors.TeamFormErros
import com.example.amfootball.navigation.objects.navBar.RoutesNavBarTeam
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.PitchConst
import com.example.amfootball.utils.TeamConst
import com.example.amfootball.R
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

    private val formState = MutableStateFlow(FormTeamDto())
    val uiFormState: StateFlow<FormTeamDto> = formState.asStateFlow()

    //Para mandar erros
    private val errors = MutableStateFlow(TeamFormErros())
    val uiErrors: StateFlow<TeamFormErros> = errors.asStateFlow()


    //Setters
    fun onNameChange(name: String) {
        formState.value = formState.value.copy(name = name)
    }

    fun onDescriptionChange(description: String?) {
        formState.value = formState.value.copy(description = description)
    }

    fun onImageChange(image: Uri?) {
        formState.value = formState.value.copy(image = image)
    }

    fun onNamePitchChange(name: String) {
        formState.value = formState.value.copy(
            pitch = formState.value.pitch.copy(name = name)
        )
    }

    fun onAddressPitchChange(address: String) {
        formState.value = formState.value.copy(
            pitch = formState.value.pitch.copy(address = address)
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
        //TODO: Chamar metodo para validar cada um dos campos
        if(!ValidateFormValid()) {
            return
        }

        if (isEditMode) {
            //TODO: Chamar metodo para editar a team
        } else {
            //TODO: Chamar metodo para criar a team
        }

        //Se tudo der bem
        navHostController.navigate(route = RoutesNavBarTeam.HOME_PAGE_TEAM)
    }

    //Metodos privados
    private fun ValidateFormValid(): Boolean {
        val name = formState.value.name
        val nameLength = name.length

        val description = formState.value.description ?: ""
        val descriptionLength = description.length

        val pitchName = formState.value.pitch.name
        val pitchNameLength = pitchName.length

        val pitchAddress = formState.value.pitch.address
        val pitchAddressLength = pitchAddress.length

        var nameErr: Int? = null
        var descErr: Int? = null
        var pitchNameErr: Int? = null
        var pitchAddrErr: Int? = null

        if (name.isBlank()) {
            nameErr = R.string.mandatory_field
        } else if (nameLength < TeamConst.MIN_NAME_LENGTH) {
            nameErr = R.string.error_min_name_team
        } else if (nameLength > TeamConst.MAX_NAME_LENGTH) {
            nameErr = R.string.error_max_name_team
        }

        if (description.isNotBlank() && descriptionLength > TeamConst.MAX_DESCRIPTION_LENGTH) {
            descErr = R.string.error_max_description
        }

        if (pitchName.isBlank()) {
            pitchNameErr = R.string.mandatory_field
        } else if (pitchNameLength < PitchConst.MIN_NAME_LENGTH) {
            pitchNameErr = R.string.error_min_pitch_name
        } else if (pitchNameLength > PitchConst.MAX_NAME_LENGTH) {
            pitchNameErr = R.string.error_max_pitch_name
        }

        if (pitchAddress.isBlank()) {
            pitchAddrErr = R.string.mandatory_field
        } else if (pitchAddressLength < GeneralConst.MIN_ADDRESS_LENGTH) {
            pitchAddrErr = R.string.error_min_address
        } else if (pitchAddressLength > GeneralConst.MAX_ADDRESS_LENGTH) {
            pitchAddrErr = R.string.error_max_address
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