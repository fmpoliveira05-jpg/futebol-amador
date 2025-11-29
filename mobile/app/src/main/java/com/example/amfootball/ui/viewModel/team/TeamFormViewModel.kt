package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.SavedStateHandle
import com.example.amfootball.data.dtos.team.FormTeamDto
import javax.inject.Inject
import android.net.Uri
import android.util.Log
import androidx.navigation.NavHostController
import com.example.amfootball.data.errors.formErrors.TeamFormErros
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.PitchConst
import com.example.amfootball.utils.TeamConst
import com.example.amfootball.R
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.repository.TeamRepository
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.abstracts.FormsViewModel
import dagger.hilt.android.lifecycle.HiltViewModel

//TODO: Testar tudo, quando tiver a rota com a autentificação e autorização funcional
/**
 * ViewModel responsável pela lógica de negócio do formulário de Equipas.
 *
 * Suporta dois modos de operação num único ecrã:
 * 1. **Criação:** Quando não é passado nenhum ID.
 * 2. **Edição:** Quando um `teamId` é passado via navegação.
 *
 * Herda de [FormsViewModel] para obter a gestão automática de estados de formulário,
 * validação base e tratamento de erros de rede.
 *
 * @property networkObserver Observador de conectividade (injetado no Pai).
 * @property teamRepository Repositório para operações CRUD de equipas.
 * @property savedStateHandle Recupera argumentos de navegação (ex: ID da equipa).
 */
@HiltViewModel
class TeamFormViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver,
    private val teamRepository: TeamRepository,
    private val savedStateHandle: SavedStateHandle
) : FormsViewModel<FormTeamDto, TeamFormErros>(
    networkObserver = networkObserver,
    initialData = FormTeamDto(),
    initialError = TeamFormErros()
) {

    /**
     * ID da equipa recebido via argumentos de navegação.
     * - `null`: Modo Criação.
     * - `String`: Modo Edição.
     */
    private val teamId: String? = savedStateHandle.get("teamId")

    /**
     * Propriedade computada que indica se o ViewModel está em modo de edição.
     * Útil para decidir entre chamar `createTeam` ou `updateTeam`.
     */
    val isEditMode: Boolean = !teamId.isNullOrBlank() && teamId != "null" && teamId != "{teamId}"

    init {
        if (isEditMode) {
            Log.d("TeamFormViewModel", "Modo Edição detetado: A carregar dados da equipa...")
            loadDataTeam()
        } else {
            stopLoading()
            Log.d("TeamFormViewModel", "Modo Criação: Formulário limpo")
        }
    }

    // ============================================================================================
    //  DATA BINDING (Setters)
    //  Métodos chamados pela UI para atualizar o estado do formulário conforme o user digita.
    // ============================================================================================

    /**
     * Atualiza o nome da equipa no estado do formulário.
     * @param name O novo nome inserido pelo utilizador.
     */
    fun onNameChange(name: String) {
        formState.value = formState.value.copy(name = name)
    }

    /**
     * Atualiza a descrição da equipa.
     * @param description A nova descrição (pode ser nula ou vazia).
     */
    fun onDescriptionChange(description: String?) {
        formState.value = formState.value.copy(description = description)
    }

    /**
     * Atualiza a URI da imagem selecionada (logótipo da equipa).
     * @param image A URI da imagem escolhida na galeria (ou null se removida).
     */
    fun onImageChange(image: Uri?) {
        formState.value = formState.value.copy(image = image)
    }

    /**
     * Atualiza o nome do estádio (Pitch).
     *
     * Nota: Como o [Pitch] é um objeto aninhado dentro do [FormTeamDto],
     * é necessário fazer uma cópia aninhada:
     * 1. Copia o estado principal.
     * 2. Dentro dele, copia o objeto 'pitch' alterando apenas o 'name'.
     */
    fun onNamePitchChange(name: String) {
        formState.value = formState.value.copy(pitch = formState.value.pitch.copy(name = name))
    }

    /**
     * Atualiza a morada/endereço do estádio (Pitch).
     * Também realiza uma atualização num objeto aninhado.
     */
    fun onAddressPitchChange(address: String) {
        formState.value = formState.value.copy(pitch = formState.value.pitch.copy(address = address))
    }

    // --- MÉTODOS PÚBLICOS (Ações) ---

    /**
     * Carrega os dados da equipa existente para preencher o formulário (apenas modo Edição).
     * Usa o [loadFormData] do Pai para gerir o estado de loading e erros.
     */
    fun loadDataTeam() {
        if (teamId == null)  {
            return
        }

        loadFormData {
            teamRepository.getTeamToUpdate(teamId = teamId)
        }
    }

    /**
     * Ação principal de submissão do formulário.
     *
     * Fluxo:
     * 1. Valida o formulário localmente via [validateForm].
     * 2. Verifica a internet.
     * 3. Decide se cria ou atualiza com base no [isEditMode].
     * 4. Em caso de sucesso, navega para a Homepage da equipa.
     *
     * @param navHostController Controlador para realizar a navegação após o sucesso.
     */
    fun onSubmit(navHostController: NavHostController) {
        submitForm(
            onSuccess = {
                navHostController.navigate(route = Routes.TeamRoutes.HOMEPAGE.route) {
                    popUpTo(Routes.TeamRoutes.HOMEPAGE.route) { inclusive = true }
                }
            },
            apiCall = {
                if (teamId != null && isEditMode) {
                    teamRepository.updateTeam(teamId = teamId, team = formState.value)
                } else {
                    teamRepository.createTeam(team = formState.value)
                }
            }
        )
    }

    /**
     * Implementação das regras de validação específicas para Equipas.
     *
     * É [protected] porque apenas a classe Pai ([FormsViewModel]) precisa de a chamar
     * internamente antes de submeter. Não deve ser acessível pela UI.
     *
     * Verifica:
     * - Campos obrigatórios (Nome, Estádio).
     * - Limites de caracteres (Mínimos e Máximos).
     *
     * @return `true` se não houver erros, `false` caso contrário.
     */
    override fun validateForm(): Boolean {
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

        formErrors.value = TeamFormErros(
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