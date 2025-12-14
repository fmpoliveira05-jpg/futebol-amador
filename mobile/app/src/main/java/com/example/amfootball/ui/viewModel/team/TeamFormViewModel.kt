package com.example.amfootball.ui.viewModel.team

import android.net.Uri
import androidx.lifecycle.SavedStateHandle
import com.example.amfootball.R
import com.example.amfootball.core.utils.Arguments
import com.example.amfootball.core.utils.CloudinaryManager
import com.example.amfootball.core.utils.GeneralConst
import com.example.amfootball.core.utils.PitchConst
import com.example.amfootball.core.utils.TeamConst
import com.example.amfootball.data.NetworkConnectivityObserver
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.remote.dtos.team.FormTeamDto
import com.example.amfootball.data.remote.services.TeamService
import com.example.amfootball.domains.errors.ErrorMessage
import com.example.amfootball.domains.errors.formErrors.TeamFormErros
import com.example.amfootball.ui.viewModel.abstracts.FormsViewModel
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.suspendCancellableCoroutine
import javax.inject.Inject
import kotlin.coroutines.resume
import kotlin.coroutines.resumeWithException

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
 * @property sessionManager Gestor de sessão local do utilizador.
 */
@HiltViewModel
class TeamFormViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver,
    private val teamRepository: TeamService,
    private val savedStateHandle: SavedStateHandle,
    private val sessionManager: SessionManager,
) : FormsViewModel<FormTeamDto, TeamFormErros>(
    networkObserver = networkObserver,
    initialData = FormTeamDto(),
    initialError = TeamFormErros()
) {

    /**
     * ID da equipa recebido via argumentos de navegação (Navigation Component).
     *
     * - Se for `null` ou inválido, assume-se que é uma **Criação**.
     * - Se for uma String válida, assume-se que é uma **Edição**.
     */
    private val teamId: String? = savedStateHandle.get(Arguments.TEAM_ID)

    /**
     * Propriedade computada que determina o modo de operação do formulário.
     *
     * Validações adicionais (`"null"`, `"{teamId}"`) são necessárias devido a comportamentos
     * específicos da injeção de argumentos no Jetpack Navigation em certos cenários.
     *
     * @return `true` se estiver em modo de edição, `false` caso contrário.
     */
    val isEditMode: Boolean = !teamId.isNullOrBlank() && teamId != "null" && teamId != "{${Arguments.TEAM_ID}}"

    /**
     * Bloco de inicialização.
     *
     * Se estiver em modo de edição, inicia imediatamente o carregamento dos dados da equipa.
     * Caso contrário, sinaliza que o carregamento "terminou" (pois o form começa vazio e pronto a usar).
     */
    init {
        if (isEditMode) {
            loadDataTeam()
        } else {
            stopLoading()
        }
    }

    /**
     * Atualiza o campo "Nome da Equipa".
     *
     * @param name O novo valor inserido pelo utilizador.
     */
    fun onNameChange(name: String) {
        formState.value = formState.value.copy(name = name)
    }

    /**
     * Atualiza o campo "Descrição".
     *
     * @param description A nova descrição. Pode ser nula ou vazia (opcional).
     */
    fun onDescriptionChange(description: String?) {
        formState.value = formState.value.copy(description = description)
    }

    /**
     * Atualiza o campo de Imagem (Logótipo).
     *
     * Recebe uma [String] que pode ser:
     * 1. Uma URL remota (ex: "https://...").
     * 2. Uma URI local convertida em String (ex: "content://media/...").
     *
     * @param image A string da imagem ou null se removida.
     */
    fun onImageChange(image: String?) {
        formState.value = formState.value.copy(image = image)
    }

    /**
     * Atualiza o campo "Nome do Campo/Estádio".
     *
     * Como o [PitchInfo] é um objeto aninhado dentro do DTO, utiliza-se o método [copy]
     * de forma recursiva para garantir a imutabilidade do estado.
     *
     * @param name O novo nome do campo.
     */
    fun onNamePitchChange(name: String) {
        formState.value = formState.value.copy(pitch = formState.value.pitch.copy(name = name))
    }

    /**
     * Atualiza o campo "Endereço do Campo".
     *
     * @param address O novo endereço físico do campo.
     */
    fun onAddressPitchChange(address: String) {
        formState.value =
            formState.value.copy(pitch = formState.value.pitch.copy(address = address))
    }

    // --- MÉTODOS PÚBLICOS (Ações) ---

    /**
     * Carrega os dados da equipa existente para preencher o formulário (apenas modo Edição).
     * Usa o [loadFormData] do Pai para gerir o estado de loading e erros.
     */
    fun loadDataTeam() {
        if (teamId == null) {
            return
        }

        loadFormData {
            teamRepository.getTeamToUpdate(teamId = teamId)
        }
    }

    /**
     * Executa a submissão do formulário (Criar ou Editar).
     *
     * Fluxo de Execução:
     * 1. **Verificação de Rede:** Se offline, exibe Toast e aborta.
     * 2. **Validação:** Executa [validateForm]. Se falhar, exibe erros nos inputs.
     * 3. **Chamada API:** Chama `createTeam` ou `updateTeam` baseado no [isEditMode].
     * 4. **Sucesso:** Invoca o callback [onSuccess] (geralmente navegação para trás).
     *
     * @param onSuccess Callback executado após a API retornar sucesso (código 200-299).
     */
    fun onSubmit(onSucess: () -> Unit) {
        if (!isNetworkAvailable()) {
            updateToast(R.string.toast_offline_edit_team)
            return
        }

        submitForm(
            onSuccess = onSucess,
            apiCall = {
                val finalImageUrl = saveImage()
                val finalTeamDto = formState.value.copy(image = finalImageUrl)

                if (teamId != null && isEditMode) {
                    teamRepository.updateTeam(teamId = teamId, team = finalTeamDto)
                } else {
                    val createdTeamDto = teamRepository.createTeam(team = finalTeamDto)

                    if (!createdTeamDto.id.isNullOrBlank()) {
                        sessionManager.updateTeamIdUser(teamId = createdTeamDto.id)
                        sessionManager.updateRoleMemberTeam(isAdmin = true)
                    } else {
                        updateToast(R.string.toast_error_create_team)
                    }
                }
            }
        )
    }

    /**
     * Ação de recarregar dados. Útil quando ocorre um erro de rede e o utilizador clica em "Tentar Novamente".
     * Apenas tem efeito em modo de edição.
     */
    fun retry() {
        if (isEditMode) {
            loadDataTeam()
        }
    }

    /**
     * Implementação das regras de validação síncronas do formulário.
     *
     * Este método é chamado automaticamente pelo [submitForm] da classe base.
     *
     * **Regras de Negócio:**
     * - **Nome da Equipa:** Obrigatório, min [TeamConst.MIN_NAME_LENGTH], max [TeamConst.MAX_NAME_LENGTH].
     * - **Descrição:** Opcional, mas se preenchida, max [TeamConst.MAX_DESCRIPTION_LENGTH].
     * - **Nome do Campo:** Obrigatório, min [PitchConst.MIN_NAME_LENGTH], max [PitchConst.MAX_NAME_LENGTH].
     * - **Endereço do Campo:** Obrigatório, min [GeneralConst.MIN_ADDRESS_LENGTH], max [GeneralConst.MAX_ADDRESS_LENGTH].
     *
     * @return `true` se todos os campos forem válidos, `false` caso contrário (atualizando `formErrors`).
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
                args = listOf(TeamConst.MIN_NAME_LENGTH)
            )
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

    /**
     * Realiza o upload da imagem para o Cloudinary se for uma imagem nova local.
     *
     * Utiliza [suspendCancellableCoroutine] para converter o callback do Cloudinary
     * numa função de suspensão que pode ser aguardada pelo fluxo do ViewModel.
     *
     * Lógica:
     * - Se a imagem for nula ou vazia, retorna null.
     * - Se a imagem já for um URL (começa por "http"), não faz upload e retorna o URL atual.
     * - Se for um URI local, faz upload e retorna o novo URL seguro (HTTPS).
     *
     * @return A URL da imagem hospedada ou null.
     * @throws Exception Caso o upload falhe.
     */
    private suspend fun saveImage(): String? {
        val currentImageString = formState.value.image

        if (currentImageString.isNullOrBlank() || currentImageString.startsWith("http")) {
            return currentImageString
        }

        return suspendCancellableCoroutine { continuation ->

            CloudinaryManager.uploadImage(
                uri = Uri.parse(currentImageString),
                onSuccess = { url ->
                    if (continuation.isActive) {
                        continuation.resume(url)
                    }
                },
                onError = { errorMsg ->
                    if (continuation.isActive) {
                        continuation.resumeWithException(Exception("Erro Cloudinary: $errorMsg"))
                    }
                }
            )
        }
    }
}