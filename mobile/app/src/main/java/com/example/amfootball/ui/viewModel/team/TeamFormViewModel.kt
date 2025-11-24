package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.ViewModel
import androidx.lifecycle.SavedStateHandle
import com.example.amfootball.data.dtos.team.FormTeamDto
import javax.inject.Inject
import android.net.Uri
import android.util.Log
import androidx.lifecycle.viewModelScope
import androidx.navigation.NavHostController
import com.example.amfootball.data.errors.formErrors.TeamFormErros
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.PitchConst
import com.example.amfootball.utils.TeamConst
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.repository.TeamRepository
import com.example.amfootball.navigation.objects.Routes
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

//TODO: Testar tudo, quando tiver a rota com a autentificação e autorização funcional
/**
 * ViewModel responsável pela lógica de negócio do formulário de Equipas (Criação e Edição).
 *
 * Gere o estado do formulário, validação de dados, submissão para a API e feedback de UI (loading/erros).
 *
 * Funcionalidades principais:
 * - **Modo Híbrido:** Deteta automaticamente se é "Create" ou "Update" baseando-se na presença de `teamId`.
 * - **Validação:** Verifica regras de negócio (tamanhos mínimos/máximos, campos obrigatórios).
 * - **Gestão de Estado:** Mantém os dados do formulário ([uiFormState]) e erros ([uiErrors]) reativos.
 * - **Conectividade:** Impede submissões se o utilizador estiver offline.
 */
@HiltViewModel
class TeamFormViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver,
    private val teamRepository: TeamRepository,
    private val savedStateHandle: SavedStateHandle
) : ViewModel() {

    /**
     * ID da equipa recebido via navegação.
     * Se for nulo, estamos em modo de Criação. Se tiver valor, estamos em modo de Edição.
     */
    private val teamId: String? = savedStateHandle.get("teamId")

    /** Flag que indica se o ecrã está em modo de edição. */
    val isEditMode = teamId != null

    // --- ESTADOS DA UI ---

    /** Estado atual dos dados do formulário (Nome, Descrição, Pitch, etc.). */
    private val formState: MutableStateFlow<FormTeamDto> = MutableStateFlow(FormTeamDto())
    val uiFormState: StateFlow<FormTeamDto> = formState

    /** Estado dos erros de validação associados a cada campo do formulário. */
    private val errors: MutableStateFlow<TeamFormErros> = MutableStateFlow(TeamFormErros())
    val uiErrors: StateFlow<TeamFormErros> = errors

    /** Estado global da UI (Loading e Erros genéricos de API/Rede). */
    private val _uiState: MutableStateFlow<UiState> = MutableStateFlow(UiState(isLoading = false))
    val uiState: StateFlow<UiState> = _uiState

    /** Estado da conectividade de rede (Online/Offline). */
    private val _isOnline: MutableStateFlow<Boolean> = MutableStateFlow(networkObserver.isOnlineOneShot())
    val isOnlie: StateFlow<Boolean> = _isOnline

    // --- BLOCO DE INICIALIZAÇÃO ---
    init {
        observeNetworkChanges()

        if (isEditMode) {
            Log.d("TeamFormViewModel", "Modo Edição detetado: A carregar dados da equipa...")
            loadDataTeam()
        }
    }

    // --- SETTERS (Atualização dos campos) ---
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
        formState.value = formState.value.copy(pitch = formState.value.pitch.copy(name = name))
    }

    fun onAddressPitchChange(address: String) {
        formState.value = formState.value.copy(pitch = formState.value.pitch.copy(address = address))
    }


    // --- MÉTODOS PÚBLICOS (Ações) ---

    /**
     * Tenta submeter o formulário.
     *
     * 1. Executa a validação local dos campos.
     * 2. Se válido, chama [submitTeam] para enviar os dados à API.
     * 3. Se a API retornar sucesso, navega para a Home.
     *
     * @param navHostController Controlador de navegação para redirecionar após sucesso.
     */
    fun onSubmit(navHostController: NavHostController) {
        if(!validateFormValid()) {
            return
        }

        submitTeam()

        if (uiState.value.errorMessage != null) {
            return
        }

        navHostController.navigate(route = Routes.TeamRoutes.HOMEPAGE.route) {
            popUpTo(Routes.TeamRoutes.HOMEPAGE.route) { inclusive = true }
        }
    }

    /**
     * Carrega os dados da equipa para edição (apenas em modo [isEditMode]).
     *
     * Utiliza [teamRepository.getTeamToUpdate] para obter os dados já formatados para o formulário.
     */
    fun loadDataTeam() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, errorMessage = null) }

            if (isEditMode && !networkObserver.isOnlineOneShot()) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem internet. Verifique a sua conexão.")
                }
                return@launch
            }

            try {
                if(teamId != null) {
                    val team = teamRepository.getTeamToUpdate(teamId = teamId)

                    formState.value = team
                    _uiState.update { it.copy(isLoading = false) }
                }

            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = e.message)
                }
            }
        }
    }

    // --- MÉTODOS PRIVADOS ---

    /**
     * Envia os dados para a API (Create ou Update).
     *
     * Verifica a conectividade antes de tentar. Se for [isEditMode], chama update, caso contrário chama create.
     */
    private fun submitTeam() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, errorMessage = null) }

            if (!networkObserver.isOnlineOneShot()) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem internet. Verifique a sua conexão.")
                }
                return@launch
            }

            try {
                if (teamId != null && isEditMode) {
                    // UPDATE
                    val team = teamRepository.updateTeam(teamId = teamId, team = formState.value)

                    formState.value = team
                } else if (teamId == null && !isEditMode) {
                    // CREATE
                    teamRepository.createTeam(team = formState.value)
                } else {
                    // Estado inválido (ex: teamId null em edit mode)
                    _uiState.update {
                        it.copy(isLoading = false, errorMessage = "Operação Invalida")
                    }

                    return@launch
                }

                _uiState.update { it.copy(isLoading = false) }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = e.message)
                }
            }
        }
    }

    /**
     * Inicia a observação contínua do estado da conectividade de rede.
     * Atualiza [_isOnline] em tempo real.
     */
    private fun observeNetworkChanges() {
        viewModelScope.launch {
            networkObserver.observeConnectivity()
                .collect { isConnected ->
                    _isOnline.value = isConnected
                }
        }
    }

    /**
     * Valida todos os campos do formulário.
     *
     * Verifica regras de negócio como campos obrigatórios e limites de caracteres.
     * Atualiza o [uiErrors] com as mensagens correspondentes.
     *
     * @return `true` se todos os campos forem válidos, `false` caso contrário.
     */
    private fun validateFormValid(): Boolean {
        val name = formState.value.name
        val nameLength = name.length

        val description = formState.value.description ?: ""
        val descriptionLength = description.length

        val pitchName = formState.value.pitch.name
        val pitchNameLength = pitchName.length

        val pitchAddress = formState.value.pitch.address
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